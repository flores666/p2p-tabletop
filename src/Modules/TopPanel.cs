using System.Buffers;
using System.Numerics;
using System.Text;
using ImGuiNET;

namespace P2PVTT.Modules;

public class TopPanel
{
    private const string UniversalPopupName = "popup";
    private const string DemoPopupName = "Demo";

    private readonly byte[] Search = new byte[256];

    public TopPanel()
    {
        var home = Encoding.UTF8.GetBytes(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        );
        for (var i = 0; i < home.Length; i++)
        {
            Search[i] = home[i];
        }
    }

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".gif",
        ".bmp",
        ".svg",
        ".ico",
    };
    private static readonly EnumerationOptions options = new EnumerationOptions
    {
        RecurseSubdirectories = false,
        AttributesToSkip = FileAttributes.Hidden | FileAttributes.System,
        IgnoreInaccessible = true,
        MatchCasing = MatchCasing.CaseInsensitive,
    };

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
        RenderLoadTokenWindow(700, 500);

        ImGui.End();
    }

    private void RenderLoadTokenWindow(int width, int height)
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
                RenderDirectoryTreeNode(searchString);
                ImGui.TreePop();
            }

            ImGui.EndPopup();
        }
    }

    private void RenderDirectoryTreeNode(string path)
    {
        var directories = Array.Empty<string>();
        var files = Array.Empty<string>();

        GetDirectoriesAndFiles(path, out directories, out files);
        if (directories.Length <= 0)
            return;

        foreach (var item in directories)
        {
            if (ImGui.TreeNode(item[(item.LastIndexOf('/') + 1)..]))
            {
                RenderDirectoryTreeNode(item);
                ImGui.TreePop();
            }
        }

        foreach (var file in files)
        {
            if (ImGui.Button(file[(file.LastIndexOf('/') + 1)..]))
            {
                TokenImagePicked?.Invoke(this, new TokenImagePickedEvent(file));
            }
        }
    }

    private void GetDirectoriesAndFiles(string path, out string[] dirs, out string[] files)
    {
        dirs = Array.Empty<string>();
        files = Array.Empty<string>();

        if (string.IsNullOrEmpty(path))
            return;

        var directory = Path.GetFileName(Path.TrimEndingDirectorySeparator(path));
        var rest = path[..path.LastIndexOf(directory)];

        try
        {
            dirs = Directory.EnumerateDirectories(path, "*", options).ToArray();
            files = Directory
                .EnumerateFiles(path, "*", options)
                .Where(w =>
                    ImageExtensions.Any(a => w.EndsWith(a, StringComparison.OrdinalIgnoreCase))
                )
                .ToArray();
        }
        catch (DirectoryNotFoundException)
        {
            dirs = Directory.EnumerateDirectories(rest, "*", options).ToArray();
            files = Directory
                .EnumerateFiles(path, "*", options)
                .Where(w =>
                    ImageExtensions.Any(a => w.EndsWith(a, StringComparison.OrdinalIgnoreCase))
                )
                .ToArray();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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
