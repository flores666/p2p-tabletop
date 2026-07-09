using StbImageSharp;

namespace P2PVTT.Services;

public static class ImageLoader
{
    public static ImageResult LoadImageRgba(string path)
    {
        using var stream = new FileStream(
            path,
            new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.Read,
                BufferSize = 64 * 1024,
                Options = FileOptions.SequentialScan,
            }
        );
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        return image;
    }
}
