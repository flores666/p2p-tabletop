using ImGuiNET;

namespace P2PVTT.Modules.Partial;

public enum FilePickerFlags
{
    All,
    Images,
};

public static class DirectoryTreeNode
{
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

    public static void Render(
        string path,
        FilePickerFlags flag = FilePickerFlags.All,
        Action<string>? filePickedCallback = null
    )
    {
        var directories = Array.Empty<string>();
        var files = Array.Empty<string>();

        GetDirectoriesAndFiles(path, flag, out directories, out files);

        foreach (var item in directories)
        {
            if (ImGui.TreeNode(item[(item.LastIndexOf('/') + 1)..]))
            {
                Render(item, flag, filePickedCallback);
                ImGui.TreePop();
            }
        }

        foreach (var file in files)
        {
            ImGui.PushID(file);
            if (ImGui.Button(file[(file.LastIndexOf('/') + 1)..]))
            {
                if (filePickedCallback != null)
                    filePickedCallback(file);
            }
            ImGui.PopID();
        }
    }

    private static void GetDirectoriesAndFiles(
        string path,
        FilePickerFlags flag,
        out string[] dirs,
        out string[] files
    )
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
                .FilterFiles(ImageExtensions, flag)
                .ToArray();
        }
        catch (DirectoryNotFoundException)
        {
            dirs = Directory.EnumerateDirectories(rest, "*", options).ToArray();
            files = Directory
                .EnumerateFiles(rest, "*", options)
                .FilterFiles(ImageExtensions, flag)
                .ToArray();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}

file static class FilesEnumerationExtensions
{
    public static IEnumerable<string> FilterFiles(
        this IEnumerable<string> e,
        HashSet<string> exts,
        FilePickerFlags flag
    )
    {
        if (flag == FilePickerFlags.Images)
        {
            return e.Where(w => exts.Any(a => w.EndsWith(a, StringComparison.OrdinalIgnoreCase)));
        }

        return e;
    }
}
