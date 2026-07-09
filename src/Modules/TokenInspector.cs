using System.Numerics;
using ImGuiNET;
using Silk.NET.OpenGL;

namespace P2PVTT.Modules;

public class TokenInspector : IDisposable
{
    private readonly GL _gl;
    private readonly uint _characterTexture;

    public TokenInspector(GL gl, uint charTex)
    {
        _gl = gl;
        _characterTexture = charTex;
    }

    public void Render(int width, int height, int x, int y)
    {
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

    public void Dispose()
    {
        if (_characterTexture != 0)
        {
            _gl.DeleteTexture(_characterTexture);
        }
    }
}
