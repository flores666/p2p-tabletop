using System.Numerics;
using System.Text;
using ImGuiNET;
using P2PVTT.Modules;
using P2PVTT.Modules.Events;
using P2PVTT.Modules.Partial;

namespace Modules.TopPanel.TokenLoader;

public class Loader

{
    private readonly byte[] Search = new byte[256];

    public event EventHandler<TokenImagePickedEvent>? TokenImagePicked;

    public Loader()
    {
        var home = Encoding.UTF8.GetBytes(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        );

        for (var i = 0; i < home.Length; i++)
        {
            Search[i] = home[i];
        }
    }

    public void Render(int width, int height)
    {
        if (ImGui.Button("Load Token"))
        {
            ImGui.OpenPopup(Constants.SharedPopupName);
        }

        if (ImGui.IsPopupOpen(Constants.SharedPopupName))
        {
            ImGui.SetNextWindowPos(Constants.Center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new Vector2(width, height));
        }

        if (ImGui.BeginPopup(Constants.SharedPopupName))
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
