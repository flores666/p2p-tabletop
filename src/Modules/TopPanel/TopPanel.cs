using System.Numerics;
using ImGuiNET;
using Modules.TopPanel.TokenLoader;
using P2PVTT.Services;
using Silk.NET.OpenGL;

namespace P2PVTT.Modules.TopPanel;

public class Panel : IDisposable
{
    private const string DemoPopupName = "Demo";
    public Loader _tokenLoaderModule;

    public Panel(GL gl, TextureLoader tl)
    {
        _tokenLoaderModule = new Loader(gl, tl);
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
        _tokenLoaderModule.Render(550, 600);

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

    public void Dispose()
    {
        _tokenLoaderModule?.Dispose();
    }
}
