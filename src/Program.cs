using System.Drawing;
using P2PVTT.Modules;
using P2PVTT.Modules.TopPanel;
using P2PVTT.Services;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

internal class Program
{
    private static readonly int WindowWidth = 1280;
    private static readonly int WindowHeight = 720;
    private static readonly string Title = "Virtual Table App";

    private static IWindow _window = null!;
    private static ImGuiController _controller = null!;
    private static GL _gl = null!;

    private static IInputContext _inputContext = null!;
    private static TokenInspector _tokenInspector = null!;
    private static Panel _topPanel = null!;
    private static TextureLoader _texLoader = null!;

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
        _topPanel.TokenLoaderModule.TokenImagePicked -= _tokenInspector.ChangeTokenImage;

        _tokenInspector?.Dispose();
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
        _texLoader = new TextureLoader(_gl);
        _tokenInspector = new TokenInspector(_gl, _texLoader);

        _topPanel = new Panel();
        _topPanel.TokenLoaderModule.TokenImagePicked += _tokenInspector.ChangeTokenImage;
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

        var tpHeight = (int)(WindowHeight * 0.05);
        _topPanel.Render(WindowWidth, tpHeight, 0, 0);

        var tiWidth = (int)(WindowWidth * 0.2);
        _tokenInspector.Render(tiWidth, WindowHeight, WindowWidth - tiWidth, tpHeight);

        _controller.Render();
    }
}
