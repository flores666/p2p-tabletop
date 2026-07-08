using System.Drawing;
using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using StbImageSharp;

internal class Program
{
    private static IWindow _window = null!;
    private static readonly int WindowWidth = 1280;
    private static readonly string Title = "Virtual Table App";
    private static readonly int WindowHeight = 720;
    private static ImGuiController _controller = null!;
    private static GL _gl = null!;
    public static IInputContext _inputContext = null!;

    private static uint _characterTexture;
    private static int _characterWidth;
    private static int _characterHeight;

    private static void Main(string[] args)
    {
        var wOptions = WindowOptions.Default with
        {
            Title = Title,
            WindowClass = "floating-windows",
            Size = new Vector2D<int>(WindowWidth, WindowHeight),
            WindowState = WindowState.Normal,
            WindowBorder = WindowBorder.Resizable,
        };

        _window = Window.Create(wOptions);

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.FramebufferResize += OnFrameBufferSize;
        _window.Closing += OnWindowClosing;

        _window.Run();
        _window.Dispose();
    }

    private static void OnWindowClosing()
    {
        if (_characterTexture != 0)
        {
            _gl.DeleteTexture(_characterTexture);
        }
        _controller?.Dispose();
        _inputContext?.Dispose();
        _gl?.Dispose();
    }

    private static void OnFrameBufferSize(Vector2D<int> d)
    {
        _gl.Viewport(d);
    }

    private static void OnLoad()
    {
        _inputContext = _window.CreateInput();

        foreach (var kb in _inputContext.Keyboards)
        {
            kb.KeyDown += OnKeyDown;
        }

        _controller = new ImGuiController(_gl = _window.CreateOpenGL(), _window, _inputContext);
        _characterTexture = LoadTexture(
            "assets/character.jpg",
            out _characterWidth,
            out _characterHeight
        );
    }

    private static void OnKeyDown(IKeyboard keyboard, Key key, int arg3)
    {
        if (key == Key.Escape)
            _window.Close();
    }

    private static void OnUpdate(double deltaTime) { }

    private static void OnRender(double deltaTime)
    {
        _controller.Update((float)deltaTime);

        _gl.ClearColor(Color.FromArgb(255, 255, 255, 255));
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        RenderTokenInspector();
        ImGui.ShowDemoWindow();

        _controller.Render();
    }

    private static void RenderTokenInspector()
    {
        int width = (int)(WindowWidth * 0.2);
        ImGui.SetNextWindowPos(new Vector2(WindowWidth - width, 0), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(width, WindowHeight), ImGuiCond.Always);

        ImGui.Begin(
            "Right Bar",
            ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove
        );

        ImGui.Text("Token Inspector");
        ImGui.Separator();

        ImGui.Image(new IntPtr(_characterTexture), new Vector2(100, 100));

        ImGui.End();
    }

    private static uint LoadTexture(string path, out int width, out int height)
    {
        try
        {
            var bytes = File.ReadAllBytes(path);
            var image = ImageResult.FromMemory(bytes, ColorComponents.RedGreenBlueAlpha);

            width = image.Width;
            height = image.Height;

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
                (uint)width,
                (uint)height,
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
            width = -1;
            height = -1;

            return 0;
        }
    }
}
