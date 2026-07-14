using System.Numerics;
using ImGuiNET;
using Silk.NET.OpenGL;

namespace P2PVTT.Components.Scene;

public class SceneComponent
{
    private readonly GL _gl;

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

        ImGui.End();
    }
}
