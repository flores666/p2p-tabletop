using System.Numerics;
using ImGuiNET;
using P2PVTT.Events;

namespace P2PVTT.Components;

public class TokenInspectorComponent
{
    private uint _tokenTextureId;
    private string _tokenName = null!;

    public void Render(int width, int height, int x, int y)
    {
        ImGui.SetNextWindowPos(new Vector2(x, y), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(width, height), ImGuiCond.Always);

        ImGui.Begin(
            "Right Bar",
            ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                | ImGuiWindowFlags.NoResize
                | ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoMove
        );

        ImGui.Text("Token Inspector");
        ImGui.Separator();

        if (_tokenTextureId != 0)
        {
            ImGui.Image(new IntPtr(_tokenTextureId), new Vector2(100, 100));
            ImGui.SameLine();
            if (!string.IsNullOrEmpty(_tokenName))
            {
                ImGui.Text(_tokenName);
            }
        }

        ImGui.End();
    }

    public void HandleTokenCreatedEvent(object? sender, TokenCreatedEvent e)
    {
        _tokenTextureId = e.TextureId;
        _tokenName = e.TokenName;
    }
}
