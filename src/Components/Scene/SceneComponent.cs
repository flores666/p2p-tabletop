using System.Numerics;
using ImGuiNET;
using Silk.NET.OpenGL;

namespace P2PVTT.Components.Scene;

public class SceneComponent
{
    private readonly GL _gl;
    private uint _sceneTexture;

    public SceneComponent(GL gl)
    {
        _gl = gl;
    }

    public void Render(int width, int height, int x, int y)
    {
        ImGui.SetNextWindowPos(new Vector2(x, y), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(width, height), ImGuiCond.Always);

        ImGui.Begin(
            "OpenGL scene",
            ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                | ImGuiWindowFlags.NoResize
                | ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoMove
        );

        if (_sceneTexture != 0)
        {
            ImGui.Image(new IntPtr(_sceneTexture), new Vector2(width, height));
        }

        ImGui.End();
    }

    private void InitDemoSceneOnce()
    {
        var frameBuffer = _gl.GenFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);

        var textureColorBuffer = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, textureColorBuffer);
    }
}
