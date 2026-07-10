using System.Numerics;
using ImGuiNET;
using P2PVTT.Services;
using Silk.NET.OpenGL;

namespace P2PVTT.Modules;

public class TokenInspector : IDisposable
{
    private readonly GL _gl;
    private TextureLoader _texLoader;

    private uint _characterTexture;
    private string _currentImagePath = null!;

    public TokenInspector(GL gl, TextureLoader texLoader)
    {
        _gl = gl;
        _texLoader = texLoader;
    }

    public void Render(int width, int height, int x, int y)
    {
        ImGui.SetNextWindowPos(new Vector2(x, y), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(width, height), ImGuiCond.Always);

        ImGui.Begin(
            "Right Bar",
            ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove
        );

        ImGui.Text("Token Inspector");
        ImGui.Separator();

        if (_characterTexture != 0)
        {
            ImGui.Image(new IntPtr(_characterTexture), new Vector2(100, 100));
        }

        ImGui.End();
    }

    private void LoadTokenTextureOnce(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
        {
            if (_characterTexture != 0)
            {
                DeleteTexture();
            }

            return;
        }

        if (_currentImagePath != imagePath)
        {
            DeleteTexture();
        }

        if (_characterTexture == 0)
        {
            var image = ImageLoader.LoadImageRgba(imagePath);
            _characterTexture = _texLoader.Load(image);
            _currentImagePath = imagePath;
        }
    }

    public void Dispose()
    {
        if (_characterTexture != 0)
        {
            DeleteTexture();
        }
    }

    private void DeleteTexture()
    {
        _gl.DeleteTexture(_characterTexture);
        _characterTexture = 0;
    }

    public void ChangeTokenImage(object? sender, TokenImagePickedEvent e)
    {
        LoadTokenTextureOnce(e.Path);
    }
}
