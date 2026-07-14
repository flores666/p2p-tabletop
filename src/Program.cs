using System.Drawing;
using P2PVTT.Components;
using P2PVTT.Components.MainMenu;
using P2PVTT.GameEngine;
using P2PVTT.Services;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

internal class Program
{
    private static int _windowWidth = 1280;
    private static int _windowHeight = 720;
    private static readonly string Title = "Virtual Table App";

    private static int _tokenInspectorWidth = (int)(_windowWidth * 0.2);
    private static int _topPanelHeight = (int)(_windowHeight * 0.05);

    private static IWindow _window = null!;
    private static ImGuiController _controller = null!;
    private static GL _gl = null!;
    private static IInputContext _inputContext = null!;

    private static TokenInspectorComponent _tokenInspector = null!;
    private static MainMenuComponent _topPanel = null!;
    private static TextureLoader _texLoader = null!;
    private static GameState _gameState = null!;

    private static void Main(string[] args)
    {
        var wOptions = WindowOptions.Default with
        {
            Title = Title,
            WindowClass = "floating-windows", // if you using Hyprland, create window rule for this class
            Size = new Vector2D<int>(_windowWidth, _windowHeight),
            WindowState = WindowState.Normal,
            WindowBorder = WindowBorder.Resizable,
            FramesPerSecond = 60,
            UpdatesPerSecond = 60,
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
        _topPanel?.Dispose();
        _controller?.Dispose();
        _inputContext?.Dispose();
        _gl?.Dispose();
    }

    private static void OnFrameBufferSize(Vector2D<int> d)
    {
        _gl.Viewport(d);

        _windowWidth = d.X;
        _windowHeight = d.Y;

        _tokenInspectorWidth = (int)(_windowWidth * 0.2);
        _topPanelHeight = (int)(_windowHeight * 0.05);
    }

    private static void OnLoad()
    {
        _inputContext = _window.CreateInput();

        foreach (var kb in _inputContext.Keyboards)
        {
            kb.KeyDown += OnKeyDown;
        }

        _gameState = new GameState();
        _tokenInspector = new TokenInspectorComponent();
        _controller = new ImGuiController(_gl = _window.CreateOpenGL(), _window, _inputContext);
        _texLoader = new TextureLoader(_gl);
        _topPanel = new MainMenuComponent(_gl, _texLoader);

        // When scene is implemented delete next row. TokenInspectorComponent
        // displays info only when token picked from scene
        _topPanel.TokenLoader.TokenCreated += _tokenInspector.HandleTokenCreatedEvent;
        _topPanel.TokenLoader.TokenCreated += _gameState.HandleTokenCreatedEvent;
    }

    private static void OnKeyDown(IKeyboard keyboard, Key key, int arg3) { }

    private static void OnUpdate(double deltaTime) { }

    private static void OnRender(double deltaTime)
    {
        _controller.Update((float)deltaTime);

        _gl.ClearColor(Color.FromArgb(255, 255, 255, 255));
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        _topPanel.Render(_windowWidth, _topPanelHeight, 0, 0);
        _tokenInspector.Render(
            _tokenInspectorWidth,
            _windowHeight,
            _windowWidth - _tokenInspectorWidth,
            _topPanelHeight
        );

        _controller.Render();
    }
}
