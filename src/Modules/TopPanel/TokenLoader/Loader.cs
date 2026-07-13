using System.Numerics;
using System.Text;
using ImGuiNET;
using P2PVTT.Modules;
using P2PVTT.Modules.Events;
using P2PVTT.Modules.Partial;
using P2PVTT.Services;
using Silk.NET.OpenGL;

namespace Modules.TopPanel.TokenLoader;

public class Loader : IDisposable
{
    private readonly byte[] Search = new byte[256];
    private int _windowWidth;
    private int _windowHeight;
    private uint _textureId = 0;
    private string _currentImagePath = null!;
    private GL _gl;
    private readonly TextureLoader _texLoader;

    private Vector2 FramePadding => ImGui.GetStyle().FramePadding;

    public event EventHandler<TokenImagePickedEvent>? TokenImagePicked;

    public Loader(GL gl, TextureLoader texLoader)
    {
        _gl = gl;
        _texLoader = texLoader;

        var home = Encoding.UTF8.GetBytes(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        );

        for (var i = 0; i < home.Length; i++)
        {
            Search[i] = home[i];
        }

        TokenImagePicked += (object? _, TokenImagePickedEvent _) =>
        {
            ImGui.CloseCurrentPopup();
        };
        TokenImagePicked += ChangeTokenImage;
    }

    public void Render(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;

        var buttonPressed = ImGui.Button("Load Token");
        RenderTokenCreatorPopup(buttonPressed, width, height);
    }

    private void RenderTokenCreatorPopup(bool openPopup, int width, int height)
    {
        if (openPopup)
        {
            ImGui.OpenPopup(Constants.SharedPopupName);
        }

        if (ImGui.IsPopupOpen(Constants.SharedPopupName))
        {
            ImGui.SetNextWindowPos(Constants.Center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new Vector2(width, height));
        }

        if (ImGui.BeginPopup(Constants.SharedPopupName))
        {
            var childWidth = (int)((width * 0.4) - FramePadding.X * 3);
            var childHeight = (int)((height * 0.35) - FramePadding.Y * 2);

            RenderImageChild(childWidth, childHeight);
            ImGui.EndPopup();
        }
    }

    private void RenderImageChild(int width, int height)
    {
        ImGui.BeginChild("Image", new Vector2(width, height), ImGuiChildFlags.Borders);

        ImGui.Text("TOKEN IMAGE");

        var choosingButtonText = "Choose image";
        var buttonHeight = ImGui.CalcTextSize(choosingButtonText).Y + FramePadding.Y * 2;
        var buttonWidth = width / 2 - FramePadding.X * 3;
        var radius = (int)(width * 0.65);

        var targetPreviewX =
            (int)(ImGui.GetContentRegionAvail().X * 0.5 - radius / 2) + FramePadding.X * 2;

        ImGui.SetCursorPosX(targetPreviewX);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 180);
        ImGui.BeginChild(
            "Image preview",
            new Vector2(radius, radius),
            ImGuiChildFlags.Borders,
            ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar
        );
        if (_textureId != 0)
        {
            var size = new Vector2(radius, radius);
            var min = ImGui.GetCursorScreenPos() - new Vector2(FramePadding.X * 2);
            var max = min + size;

            ImGui
                .GetWindowDrawList()
                .AddImageRounded(
                    new IntPtr(_textureId),
                    min,
                    max,
                    Vector2.Zero,
                    Vector2.One,
                    uint.MaxValue,
                    180,
                    ImDrawFlags.RoundCornersAll
                );

            ImGui.Dummy(size);
        }
        ImGui.EndChild();
        ImGui.PopStyleVar();

        var targetY = (int)(height - FramePadding.Y * 3) - buttonHeight;
        ImGui.SetCursorPosY(targetY);
        var chooseImagePressed = ImGui.Button(
            choosingButtonText,
            new Vector2(buttonWidth, buttonHeight)
        );

        RenderImagePickerPopup(
            chooseImagePressed,
            Math.Min(_windowWidth, 350),
            Math.Min(_windowHeight, 450)
        );

        ImGui.SameLine();

        if (ImGui.Button("Remove", new Vector2(buttonWidth, buttonHeight)))
        {
            DeleteTexture();
        }

        ImGui.EndChild();
    }

    private void RenderImagePickerPopup(bool openPopup, int width, int height)
    {
        var popupTitle = "choose picture";
        if (openPopup)
        {
            ImGui.OpenPopup(popupTitle);
        }

        if (ImGui.IsPopupOpen(popupTitle))
        {
            ImGui.SetNextWindowPos(Constants.Center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new Vector2(width, height));
        }

        if (ImGui.BeginPopup(popupTitle))
        {
            ImGui.Text("Choose picture to load");

            ImGui.SetNextItemWidth(width - ImGui.GetStyle().DisplayWindowPadding.X);

            ImGui.PushID(popupTitle + "input search");
            ImGui.InputText(
                "##",
                Search,
                (uint)Search.Length,
                ImGuiInputTextFlags.ElideLeft
                    | ImGuiInputTextFlags.EscapeClearsAll
                    | ImGuiInputTextFlags.CharsNoBlank
            );
            ImGui.PopID();

            var searchString = Encoding.UTF8.GetString(Search[..GetSearchLength()]);
            ImGui.SetNextItemWidth(width);
            if (
                ImGui.TreeNodeEx(
                    string.IsNullOrEmpty(searchString) ? "##" : searchString,
                    ImGuiTreeNodeFlags.DefaultOpen
                )
            )
            {
                DirectoryTreeNode.Render(
                    searchString,
                    FilePickerFlags.Images,
                    (fileName) =>
                    {
                        TokenImagePicked?.Invoke(this, new TokenImagePickedEvent(fileName));
                    }
                );
                ImGui.TreePop();
            }

            ImGui.EndPopup();
        }
    }

    private void LoadTokenTextureOnce(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
        {
            if (_textureId != 0)
            {
                DeleteTexture();
            }

            return;
        }

        if (_currentImagePath != imagePath)
        {
            DeleteTexture();
        }

        if (_textureId == 0)
        {
            var image = ImageLoader.LoadImageRgba(imagePath);
            _textureId = _texLoader.Load(image);
            _currentImagePath = imagePath;
        }
    }

    private int GetSearchLength()
    {
        int size = 0;

        for (var i = 0; i < Search.Length; i++)
        {
            if (Search[i] != 0)
                size++;
            else
                break;
        }

        return size;
    }

    public void ChangeTokenImage(object? sender, TokenImagePickedEvent e)
    {
        LoadTokenTextureOnce(e.Path);
    }

    public void Dispose()
    {
        if (_textureId != 0)
        {
            DeleteTexture();
        }

        TokenImagePicked -= ChangeTokenImage;
    }

    private void DeleteTexture()
    {
        _gl.DeleteTexture(_textureId);
        _textureId = 0;
    }
}
