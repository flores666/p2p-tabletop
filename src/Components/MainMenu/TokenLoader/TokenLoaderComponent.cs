using System.Numerics;
using System.Text;
using ImGuiNET;
using P2PVTT.Components;
using P2PVTT.Components.Partial;
using P2PVTT.Events;
using P2PVTT.Services;
using Silk.NET.OpenGL;

namespace Components.TopPanel.TokenLoader;

public class TokenLoaderComponent
{
    private uint _textureId = 0;
    private string _currentImagePath = null!;
    private byte[] _tokenName = new byte[128];
    private byte[] _tokenType = new byte[128];
    private byte[] _tokenSize = new byte[128];

    private readonly byte[] Search = new byte[256];
    private int _windowWidth;
    private int _windowHeight;
    private GL _gl;
    private readonly TextureLoader _texLoader;
    private readonly int _customPadding = 8;

    private Vector2 FramePadding => ImGui.GetStyle().FramePadding;

    private event EventHandler<TokenImagePickedEvent>? _tokenImagePicked;
    public event EventHandler<TokenCreatedEvent>? TokenCreated;

    public TokenLoaderComponent(GL gl, TextureLoader texLoader)
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

        AddEventHandlers();
    }

    public void Render(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;

        var buttonPressed = ImGui.Button("Load Token");

        RenderTokenCreatorPopup(buttonPressed, width, height);
    }

    private void AddEventHandlers()
    {
        _tokenImagePicked += (object? _, TokenImagePickedEvent _) =>
        {
            ImGui.CloseCurrentPopup();
        };

        TokenCreated += (object? _, TokenCreatedEvent _) =>
        {
            ImGui.CloseCurrentPopup();
        };

        _tokenImagePicked += ChangeTokenImage;
    }

    private void RenderTokenCreatorPopup(bool openPopup, int width, int height)
    {
        if (openPopup)
        {
            ImGui.OpenPopup(UsefulVars.SharedPopupName);
        }

        if (ImGui.IsPopupOpen(UsefulVars.SharedPopupName))
        {
            ImGui.SetNextWindowPos(UsefulVars.Center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new Vector2(width, height));
        }

        if (ImGui.BeginPopup(UsefulVars.SharedPopupName))
        {
            var imagesChildWidth = (int)((width * 0.4) - FramePadding.X * 3);
            var imagesChildHeight = (int)((height * 0.35) - FramePadding.Y * 2);

            ImGui.Text("Add Token");
            ImGui.TextDisabled("Create token to place on the map");

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + _customPadding);

            RenderImageChild(imagesChildWidth, imagesChildHeight);
            ImGui.SameLine();
            RenderInputsChild((int)((width * 0.6) - FramePadding.X * 3), imagesChildHeight);

            RenderFinalButtons();

            ImGui.EndPopup();
        }
    }

    private void RenderFinalButtons()
    {
        if (ImGui.Button("Cancel"))
        {
            DeleteTexture();
            ImGui.CloseCurrentPopup();
        }

        ImGui.SameLine();

        if (ImGui.Button("Create Token"))
        {
            TokenCreated?.Invoke(
                null,
                new TokenCreatedEvent(Guid.NewGuid(), _tokenName, _textureId)
            );
        }
    }

    private void RenderInputsChild(int width, int height)
    {
        ImGui.BeginChild("Inputs", new Vector2(width, height), ImGuiChildFlags.Borders);

        ImGui.PushItemWidth(-1);

        ImGui.Text("TOKEN NAME*");
        ImGui.InputText(
            "##n",
            _tokenName,
            (uint)_tokenName.Length,
            ImGuiInputTextFlags.ElideLeft | ImGuiInputTextFlags.EscapeClearsAll
        );

        ImGui.Text("TOKEN TYPE");
        ImGui.InputText(
            "##t",
            _tokenType,
            (uint)_tokenType.Length,
            ImGuiInputTextFlags.ElideLeft | ImGuiInputTextFlags.EscapeClearsAll
        );

        ImGui.Text("TOKEN SIZE (CELLS)");
        ImGui.InputText(
            "##s",
            _tokenSize,
            (uint)_tokenSize.Length,
            ImGuiInputTextFlags.ElideLeft | ImGuiInputTextFlags.EscapeClearsAll
        );

        ImGui.PopItemWidth();

        ImGui.EndChild();
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
            ImGui.SetNextWindowPos(UsefulVars.Center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
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
                DirectoryTreeNodeComponent.Render(
                    searchString,
                    FilePickerFlags.Images,
                    (fileName) =>
                    {
                        _tokenImagePicked?.Invoke(this, new TokenImagePickedEvent(fileName));
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

    private void ChangeTokenImage(object? sender, TokenImagePickedEvent e)
    {
        LoadTokenTextureOnce(e.Path);
    }

    private void DeleteTexture()
    {
        _gl.DeleteTexture(_textureId);
        _textureId = 0;
    }
}
