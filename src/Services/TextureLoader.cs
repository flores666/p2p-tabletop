using Silk.NET.OpenGL;
using StbImageSharp;

namespace P2PVTT.Services;

public class TextureLoader
{
    private readonly GL _gl;

    public TextureLoader(GL gl)
    {
        _gl = gl;
    }

    public uint Load(ImageResult image)
    {
        try
        {
            var texId = _gl.GenTexture();
            _gl.BindTexture(GLEnum.Texture2D, texId);

            var linear = (int)GLEnum.Linear;
            _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, ref linear);
            _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, ref linear);

            var clamp = (int)GLEnum.ClampToEdge;
            _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, ref clamp);
            _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, ref clamp);

            _gl.TexImage2D(
                GLEnum.Texture2D,
                0,
                (int)GLEnum.Rgba,
                (uint)image.Width,
                (uint)image.Height,
                0,
                GLEnum.Rgba,
                GLEnum.UnsignedByte,
                image.Data
            );

            _gl.BindTexture(GLEnum.Texture2D, 0);

            return texId;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            return 0;
        }
    }
}
