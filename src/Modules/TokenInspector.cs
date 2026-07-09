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

    private bool _texLoaded;

    public TokenInspector(GL gl, TextureLoader texLoader)
    {
        _gl = gl;
        _texLoader = texLoader;
    }

    public void Render(int width, int height, int x, int y, string imagePath)
    {
        LoadTextureOnce(imagePath);

        if (_characterTexture <= 0)
            throw new ArgumentException("Character texture cannot be <= 0");

        ImGui.SetNextWindowPos(new Vector2(x, y), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(width, height), ImGuiCond.Always);

        ImGui.Begin(
            "Right Bar",
            ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove
        );

        ImGui.Text("Token Inspector");
        ImGui.Separator();

        ImGui.Image(new IntPtr(_characterTexture), new Vector2(100, 100));

        ImGui.End();
    }

    private void LoadTextureOnce(string imagePath)
    {
        if (!_texLoaded)
        {
            var image = ImageLoader.LoadImageRgba(imagePath);
            _texLoaded = true;
            _characterTexture = _texLoader.Load(image);
        }
    }

    public void Dispose()
    {
        if (_characterTexture != 0)
        {
            _gl.DeleteTexture(_characterTexture);
        }
    }
}
