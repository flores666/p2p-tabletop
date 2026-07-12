using System.Numerics;
using ImGuiNET;
using Modules.TopPanel.TokenLoader;

namespace P2PVTT.Modules.TopPanel;

public class Panel
{
    private const string DemoPopupName = "Demo";
    public Loader TokenLoaderModule { get; private set; }

    public Panel()
    {
        TokenLoaderModule = new Loader();
    }

    public void Render(int width, int height, int x, int y)
    {
        ImGui.SetNextWindowPos(new Vector2(x, y), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(width, height), ImGuiCond.Always);

        ImGui.Begin(
            "Top Panel",
            ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove
        );

        RenderDemoWindow();
        ImGui.SameLine();
        TokenLoaderModule.Render(700, 500);

        ImGui.End();
    }

    private void RenderDemoWindow()
    {
        if (ImGui.Button("Show Demo"))
        {
            ImGui.OpenPopup(DemoPopupName);
        }

        if (ImGui.IsPopupOpen(DemoPopupName))
        {
            ImGui.SetNextWindowPos(Constants.Center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        }

        if (ImGui.BeginPopup(DemoPopupName))
        {
            ImGui.ShowDemoWindow();
            ImGui.EndPopup();
        }
    }
}
