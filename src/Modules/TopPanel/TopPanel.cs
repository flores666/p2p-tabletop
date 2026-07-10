using System.Numerics;
using System.Text;
using ImGuiNET;
using P2PVTT.Modules.Events;
using P2PVTT.Services;

namespace P2PVTT.Modules.TopPanel;

public class Panel
{
    private const string UniversalPopupName = "popup";
    private const string DemoPopupName = "Demo";

    private readonly byte[] Search = new byte[256];

    public Panel()
    {
        var home = Encoding.UTF8.GetBytes(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        );
        for (var i = 0; i < home.Length; i++)
        {
            Search[i] = home[i];
        }
    }

    private readonly Vector2 Center = ImGui.GetMainViewport().GetCenter();

    public event EventHandler<TokenImagePickedEvent>? TokenImagePicked;

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
        RenderTokenLoader(700, 500);

        ImGui.End();
    }

    private void RenderTokenLoader(int width, int height)
    {
        if (ImGui.Button("Load Token"))
        {
            ImGui.OpenPopup(UniversalPopupName);
        }

        if (ImGui.IsPopupOpen(UniversalPopupName))
        {
            ImGui.SetNextWindowPos(Center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new Vector2(width, height));
        }

        if (ImGui.BeginPopup(UniversalPopupName))
        {
            ImGui.Text("Choose picture to load");

            ImGui.SetNextItemWidth(width - ImGui.GetStyle().DisplayWindowPadding.X);

            ImGui.PushID("input search");
            ImGui.InputText(
                "##",
                Search,
                (uint)Search.Length,
                ImGuiInputTextFlags.ElideLeft
                    | ImGuiInputTextFlags.EscapeClearsAll
                    | ImGuiInputTextFlags.CharsNoBlank
            );
            ImGui.PopID();

            var searchString = Encoding.UTF8.GetString(Search[..GetSearchLength()]);
            ImGui.SetNextItemWidth(width);
            if (
                ImGui.TreeNodeEx(
                    string.IsNullOrEmpty(searchString) ? "##" : searchString,
                    ImGuiTreeNodeFlags.DefaultOpen
                )
            )
            {
                DirectoryTreeNode.Render(
                    searchString,
                    FilePickerFlags.Images,
                    (fileName) =>
                    {
                        TokenImagePicked?.Invoke(this, new TokenImagePickedEvent(fileName));
                    }
                );
                ImGui.TreePop();
            }

            ImGui.EndPopup();
        }
    }

    private void RenderDemoWindow()
    {
        if (ImGui.Button("Show Demo"))
        {
            ImGui.OpenPopup(DemoPopupName);
        }

        if (ImGui.IsPopupOpen(DemoPopupName))
        {
            ImGui.SetNextWindowPos(Center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        }

        if (ImGui.BeginPopup(DemoPopupName))
        {
            ImGui.ShowDemoWindow();
            ImGui.EndPopup();
        }
    }

    private int GetSearchLength()
    {
        int size = 0;

        for (var i = 0; i < Search.Length; i++)
        {
            if (Search[i] != 0)
                size++;
            else
                break;
        }

        return size;
    }
}
