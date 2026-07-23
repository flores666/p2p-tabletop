using System.Numerics;
using ImGuiNET;
using P2PVTT;
using P2PVTT.Components;
using P2PVTT.Components.MainMenu;
using P2PVTT.Components.Scene;
using P2PVTT.GameEngine;
using P2PVTT.Services;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

internal class Program
{
    private static readonly int _halfMargin = 3;
    private static readonly string Title = "Virtual Table App";

    private static readonly Vector4 AppBackground = Rgba(7, 7, 6);

    private static IWindow _window = null!;
    private static ImGuiController _controller = null!;
    private static GL _gl = null!;
    private static IInputContext _inputContext = null!;

    private static TokenInspectorComponent _tokenInspector = null!;
    private static MainMenuComponent _topPanel = null!;
    private static SceneComponent _scene = null!;
    private static MainLayout _layout;
    private static TextureLoader _texLoader = null!;
    private static GameState _gameState = null!;

    private static void Main(string[] args)
    {
        var wOptions = WindowOptions.Default with
        {
            Title = Title,
            WindowClass = "floating-windows", // if you using Hyprland, create window rule for this class
            Size = new Vector2D<int>(
                (int)AppWindowState.WindowWidth,
                (int)AppWindowState.WindowHeight
            ),
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
        _scene.Dispose();
        _controller?.Dispose();
        _inputContext?.Dispose();
        _gl?.Dispose();
    }

    private static void OnFrameBufferSize(Vector2D<int> d)
    {
        //_gl.Viewport(d);

        AppWindowState.WindowWidth = (uint)d.X;
        AppWindowState.WindowHeight = (uint)d.Y;

        _layout = CalculateMainLayout();
    }

    private static void OnLoad()
    {
        _inputContext = _window.CreateInput();

        foreach (var kb in _inputContext.Keyboards)
        {
            kb.KeyDown += OnKeyDown;
        }

        _gameState = new GameState();
        _controller = new ImGuiController(_gl = _window.CreateOpenGL(), _window, _inputContext);

        ApplyTheme();
        _layout = CalculateMainLayout();

        _texLoader = new TextureLoader(_gl);

        _topPanel = new MainMenuComponent(_gl, _texLoader);
        _tokenInspector = new TokenInspectorComponent();
        _scene = new SceneComponent(_gl);

        // When scene is implemented delete next row. TokenInspectorComponent
        // displays info only when token picked from scene
        _topPanel.TokenLoader.TokenCreated += _tokenInspector.HandleTokenCreatedEvent;
        _topPanel.TokenLoader.TokenCreated += _gameState.HandleTokenCreatedEvent;

        _gl.ClearColor(AppBackground.X, AppBackground.Y, AppBackground.Z, AppBackground.W);

        Console.WriteLine($"OpenGL version: {_gl.GetStringS(GLEnum.Version)}");
        Console.WriteLine($"GLSL version:   {_gl.GetStringS(GLEnum.ShadingLanguageVersion)}");
        Console.WriteLine($"Renderer:       {_gl.GetStringS(GLEnum.Renderer)}");
        Console.WriteLine($"Vendor:         {_gl.GetStringS(GLEnum.Vendor)}");
    }

    private static void OnKeyDown(IKeyboard keyboard, Key key, int arg3) { }

    private static void OnUpdate(double deltaTime) { }

    private static void OnRender(double deltaTime)
    {
        _controller.Update((float)deltaTime);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _gl.Viewport(0, 0, AppWindowState.WindowWidth, AppWindowState.WindowHeight);
        _gl.Clear(
            ClearBufferMask.ColorBufferBit
                | ClearBufferMask.DepthBufferBit
                | ClearBufferMask.StencilBufferBit
        );

        _topPanel.Render(
            _layout.TopPanel.Width,
            _layout.TopPanel.Height,
            _layout.TopPanel.X,
            _layout.TopPanel.Y
        );

        _tokenInspector.Render(
            _layout.TokenInspector.Width,
            _layout.TokenInspector.Height,
            _layout.TokenInspector.X,
            _layout.TokenInspector.Y
        );

        _scene.Render(_layout.Scene.Width, _layout.Scene.Height, _layout.Scene.X, _layout.Scene.Y);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _gl.Viewport(0, 0, AppWindowState.WindowWidth, AppWindowState.WindowHeight);

        _controller.Render();
    }

    private static Vector4 Rgba(int r, int g, int b, int a = 255)
    {
        return new Vector4(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    private static void ApplyTheme()
    {
        var style = ImGui.GetStyle();
        var colors = style.Colors;

        style.WindowRounding = 5f;
        style.ChildRounding = 4f;
        style.FrameRounding = 4f;
        style.PopupRounding = 5f;
        style.ScrollbarRounding = 4f;
        style.GrabRounding = 4f;

        style.WindowBorderSize = 1f;
        style.ChildBorderSize = 1f;
        style.FrameBorderSize = 1f;
        style.PopupBorderSize = 1f;

        style.WindowPadding = new Vector2(10, 10);
        style.FramePadding = new Vector2(8, 5);
        style.ItemSpacing = new Vector2(8, 6);
        style.ItemInnerSpacing = new Vector2(6, 4);
        style.ScrollbarSize = 12f;

        // Backgrounds
        colors[(int)ImGuiCol.WindowBg] = Rgba(18, 17, 15, 255);
        colors[(int)ImGuiCol.ChildBg] = Rgba(20, 19, 17, 255);
        colors[(int)ImGuiCol.PopupBg] = Rgba(18, 17, 15, 250);
        colors[(int)ImGuiCol.MenuBarBg] = Rgba(12, 11, 10, 255);

        // Main borders — muted, not gold
        colors[(int)ImGuiCol.Border] = Rgba(55, 49, 40, 185);
        colors[(int)ImGuiCol.BorderShadow] = Rgba(0, 0, 0, 0);

        // Text
        colors[(int)ImGuiCol.Text] = Rgba(226, 216, 194, 255);
        colors[(int)ImGuiCol.TextDisabled] = Rgba(128, 118, 96, 255);

        // Inputs / frames
        colors[(int)ImGuiCol.FrameBg] = Rgba(15, 14, 13, 255);
        colors[(int)ImGuiCol.FrameBgHovered] = Rgba(28, 25, 21, 255);
        colors[(int)ImGuiCol.FrameBgActive] = Rgba(38, 32, 24, 255);

        // Buttons
        colors[(int)ImGuiCol.Button] = Rgba(22, 20, 17, 255);
        colors[(int)ImGuiCol.ButtonHovered] = Rgba(42, 34, 24, 255);
        colors[(int)ImGuiCol.ButtonActive] = Rgba(68, 50, 28, 255);

        // Headers / selectable rows
        colors[(int)ImGuiCol.Header] = Rgba(35, 27, 16, 255);
        colors[(int)ImGuiCol.HeaderHovered] = Rgba(58, 42, 22, 255);
        colors[(int)ImGuiCol.HeaderActive] = Rgba(78, 55, 27, 255);

        // Separators — very subtle
        colors[(int)ImGuiCol.Separator] = Rgba(43, 38, 31, 180);
        colors[(int)ImGuiCol.SeparatorHovered] = Rgba(116, 87, 43, 220);
        colors[(int)ImGuiCol.SeparatorActive] = Rgba(184, 135, 52, 255);

        // Accent controls
        colors[(int)ImGuiCol.CheckMark] = Rgba(212, 175, 55, 255);
        colors[(int)ImGuiCol.SliderGrab] = Rgba(165, 119, 45, 255);
        colors[(int)ImGuiCol.SliderGrabActive] = Rgba(212, 175, 55, 255);

        // Tabs
        colors[(int)ImGuiCol.Tab] = Rgba(18, 16, 13, 255);
        colors[(int)ImGuiCol.TabHovered] = Rgba(50, 37, 21, 255);
        colors[(int)ImGuiCol.TabSelected] = Rgba(36, 28, 18, 255);

        // Title bars
        colors[(int)ImGuiCol.TitleBg] = Rgba(11, 10, 9, 255);
        colors[(int)ImGuiCol.TitleBgActive] = Rgba(16, 14, 11, 255);
        colors[(int)ImGuiCol.TitleBgCollapsed] = Rgba(11, 10, 9, 255);

        // Scrollbars
        colors[(int)ImGuiCol.ScrollbarBg] = Rgba(10, 9, 8, 255);
        colors[(int)ImGuiCol.ScrollbarGrab] = Rgba(44, 39, 32, 255);
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = Rgba(65, 55, 42, 255);
        colors[(int)ImGuiCol.ScrollbarGrabActive] = Rgba(90, 70, 42, 255);

        // Resize grip
        colors[(int)ImGuiCol.ResizeGrip] = Rgba(70, 58, 40, 70);
        colors[(int)ImGuiCol.ResizeGripHovered] = Rgba(130, 96, 48, 140);
        colors[(int)ImGuiCol.ResizeGripActive] = Rgba(212, 175, 55, 180);

        // Tables
        colors[(int)ImGuiCol.TableHeaderBg] = Rgba(21, 18, 14, 255);
        colors[(int)ImGuiCol.TableBorderStrong] = Rgba(55, 49, 40, 185);
        colors[(int)ImGuiCol.TableBorderLight] = Rgba(40, 35, 29, 150);
        colors[(int)ImGuiCol.TableRowBg] = Rgba(0, 0, 0, 0);
        colors[(int)ImGuiCol.TableRowBgAlt] = Rgba(255, 255, 255, 6);

        // Selection / navigation
        colors[(int)ImGuiCol.NavWindowingHighlight] = Rgba(212, 175, 55, 160);
        colors[(int)ImGuiCol.TextSelectedBg] = Rgba(120, 84, 32, 110);
    }

    private readonly record struct UiRect(int Width, int Height, int X, int Y);

    private readonly record struct MainLayout(UiRect TopPanel, UiRect Scene, UiRect TokenInspector);

    private static MainLayout CalculateMainLayout()
    {
        var margin = _halfMargin;

        var topPanelX = margin;
        var topPanelY = margin;
        var topPanelWidth = (int)AppWindowState.WindowWidth - margin * 2;

        var tokenInspectorWidth = (int)(AppWindowState.WindowWidth * 0.2);
        var topPanelHeight = (int)(AppWindowState.WindowHeight * 0.05);

        var contentY = topPanelY + topPanelHeight + margin;
        var contentHeight = (int)AppWindowState.WindowHeight - topPanelHeight - margin * 3;

        var sceneX = margin;
        var sceneY = contentY;
        var sceneWidth = (int)AppWindowState.WindowWidth - tokenInspectorWidth - margin * 2;
        var sceneHeight = contentHeight;

        var inspectorX = sceneX + sceneWidth + margin;
        var inspectorY = contentY;
        var inspectorWidth = tokenInspectorWidth - margin;
        var inspectorHeight = contentHeight;

        return new MainLayout(
            TopPanel: new UiRect(
                Width: topPanelWidth,
                Height: topPanelHeight,
                X: topPanelX,
                Y: topPanelY
            ),
            Scene: new UiRect(Width: sceneWidth, Height: sceneHeight, X: sceneX, Y: sceneY),
            TokenInspector: new UiRect(
                Width: inspectorWidth,
                Height: inspectorHeight,
                X: inspectorX,
                Y: inspectorY
            )
        );
    }
}
