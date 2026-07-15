using System.Numerics;
using Components.TopPanel.TokenLoader;
using ImGuiNET;
using P2PVTT.Services;
using Silk.NET.OpenGL;

namespace P2PVTT.Components.MainMenu;

public class MainMenuComponent
{
    private const string DemoPopupName = "Demo";
    public readonly TokenLoaderComponent TokenLoader;

    public MainMenuComponent(GL gl, TextureLoader tl)
    {
        TokenLoader = new TokenLoaderComponent(gl, tl);
    }

    public void Render(int width, int height, int x, int y)
    {
        ImGui.SetNextWindowPos(new Vector2(x, y), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(width, height), ImGuiCond.Always);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8, 0));
        ImGui.Begin(
            "Top Panel",
            ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                | ImGuiWindowFlags.NoResize
                | ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoMove
        );

        ImGui.SetCursorPosY(height * 0.5f - UsefulVars.PredictedButtonHeight * 0.5f);
        RenderDemoWindow();

        ImGui.SameLine();

        ImGui.SetCursorPosY(height * 0.5f - UsefulVars.PredictedButtonHeight * 0.5f);
        TokenLoader.Render(550, 600);

        ImGui.End();

        ImGui.PopStyleVar();
    }

    private void RenderDemoWindow()
    {
        if (ImGui.Button("Show Demo"))
        {
            ImGui.OpenPopup(DemoPopupName);
        }

        if (ImGui.IsPopupOpen(DemoPopupName))
        {
            ImGui.SetNextWindowPos(UsefulVars.Center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        }

        if (ImGui.BeginPopup(DemoPopupName))
        {
            ImGui.ShowDemoWindow();
            ImGui.EndPopup();
        }
    }
}
