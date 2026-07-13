using System.Numerics;
using ImGuiNET;

namespace P2PVTT.Modules;

public class TokenInspector
{
    private uint _characterTexture;
    private string _currentImagePath = null!;

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
}
