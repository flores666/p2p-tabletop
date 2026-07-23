using System.Numerics;
using ImGuiNET;
using Silk.NET.OpenGL;

namespace P2PVTT.Components.Scene;

public sealed class SceneComponent : IDisposable
{
    private readonly GL _gl;

    private const string _fragmentShaderCode = """
            #version 330 core

            out vec4 FragColor;

            void main()
            {
                // RGBA format (Red, Green, Blue, Alpha)
                FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f); 
            }
        """;
    private const string _vertexShaderCode = """
        #version 330 core

        // Input vertex position data from Vertex Buffer Object (VBO)
        layout (location = 0) in vec3 aPos;

        void main() {
            // Convert vec3 position to vec4 clip space coordinates
            gl_Position = vec4(aPos, 1.0);
        }
        """;

    private uint _fragmentShader;
    private uint _shaderProgram;
    private uint _sceneFramebuffer;
    private uint _sceneColorTexture;
    private uint _sceneRenderBufferObject;

    private bool _disposed;
    private uint _frameWidth;
    private uint _frameHeight;
    private uint _vertexShader;
    private uint _vbo;
    private uint _vao;
    private uint _ebo;

    public SceneComponent(GL gl)
    {
        _gl = gl;
    }

    public void Render(int width, int height, int x, int y)
    {
        ImGui.SetNextWindowPos(new Vector2(x, y), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(width, height), ImGuiCond.Always);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        bool isVisible = ImGui.Begin(
            "OpenGL scene",
            ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                | ImGuiWindowFlags.NoResize
                | ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoMove
        );

        ImGui.PopStyleVar();

        if (isVisible)
        {
            var availableSize = ImGui.GetContentRegionAvail();
            if (availableSize.X > 0 && availableSize.Y > 0)
            {
                InitializeFrame((uint)availableSize.X, (uint)availableSize.Y);
                DrawScene((uint)availableSize.X, (uint)availableSize.Y);

                ImGui.Image(
                    (nint)_sceneColorTexture,
                    availableSize,
                    new Vector2(0.0f, 1.0f),
                    new Vector2(1.0f, 0.0f)
                );
            }
        }

        ImGui.End();
    }

    private void DrawScene(uint width, uint height)
    {
        _gl.UseProgram(_shaderProgram);

        _gl.BindFramebuffer(GLEnum.Framebuffer, _sceneFramebuffer);
        _gl.Viewport(0, 0, width, height);

        _gl.ClearColor(0f, 0f, 0f, 0f);
        _gl.Clear(
            ClearBufferMask.ColorBufferBit
                | ClearBufferMask.DepthBufferBit
                | ClearBufferMask.StencilBufferBit
        );

        _gl.BindVertexArray(_vao);

        unsafe
        {
            _gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, null);
        }

        _gl.BindVertexArray(0);
        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);
    }

    private void InitializeShaders()
    {
        _fragmentShader = _gl.CreateShader(GLEnum.FragmentShader);
        _gl.ShaderSource(_fragmentShader, _fragmentShaderCode);
        _gl.CompileShader(_fragmentShader);

        var shaderLog = _gl.GetShaderInfoLog(_fragmentShader);
        if (!string.IsNullOrEmpty(shaderLog))
            Console.WriteLine("SHADER LOG: " + shaderLog);

        _vertexShader = _gl.CreateShader(GLEnum.VertexShader);
        _gl.ShaderSource(_vertexShader, _vertexShaderCode);
        _gl.CompileShader(_vertexShader);

        shaderLog = _gl.GetShaderInfoLog(_vertexShader);
        if (!string.IsNullOrEmpty(shaderLog))
            Console.WriteLine("SHADER LOG: " + shaderLog);

        _shaderProgram = _gl.CreateProgram();
        _gl.AttachShader(_shaderProgram, _fragmentShader);
        _gl.AttachShader(_shaderProgram, _vertexShader);

        _gl.LinkProgram(_shaderProgram);

        _gl.GetProgram(_shaderProgram, GLEnum.LinkStatus, out int linkStatus);

        Console.WriteLine($"Program link status: {linkStatus}");
        var pLog = _gl.GetProgramInfoLog(_shaderProgram);
        if (!string.IsNullOrEmpty(pLog))
            Console.WriteLine($"Program log: {pLog}");

        _gl.DeleteShader(_fragmentShader);
        _gl.DeleteShader(_vertexShader);
    }

    private void CreateShapes()
    {
        float[] vertices =
        {
            0.5f,
            0.5f,
            0.0f, // Vertex 0: Top-Right
            0.5f,
            -0.5f,
            0.0f, // Vertex 1: Bottom-Right
            -0.5f,
            -0.5f,
            0.0f, // Vertex 2: Bottom-Left
            -0.5f,
            0.5f,
            0.0f, // Vertex 3: Top-Left
        };

        uint[] indices =
        {
            0,
            1,
            3, // First Triangle (Top-Right, Bottom-Right, Top-Left)
            1,
            2,
            3, // Second Triangle (Bottom-Right, Bottom-Left, Top-Left)
        };

        _gl.GenVertexArrays(1, out _vao);
        _gl.GenBuffers(1, out _vbo);
        _gl.GenBuffers(1, out _ebo);

        _gl.BindVertexArray(_vao);

        _gl.BindBuffer(GLEnum.ArrayBuffer, _vbo);
        _gl.BufferData<float>(
            GLEnum.ArrayBuffer,
            sizeof(float) * (uint)vertices.Length,
            vertices.AsSpan(),
            GLEnum.StaticDraw
        );

        _gl.GetBufferParameter(GLEnum.ArrayBuffer, GLEnum.BufferSize, out int vboSize);
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, _ebo);
        _gl.BufferData<uint>(
            GLEnum.ElementArrayBuffer,
            sizeof(uint) * (uint)indices.Length,
            indices.AsSpan(),
            GLEnum.StaticDraw
        );

        _gl.GetBufferParameter(GLEnum.ElementArrayBuffer, GLEnum.BufferSize, out int eboSize);

        Console.WriteLine($"VBO size: {vboSize}");
        Console.WriteLine($"EBO size: {eboSize}");

        _gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), nint.Zero);
        _gl.EnableVertexAttribArray(0);

        _gl.BindVertexArray(0);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
    }

    private void InitializeFrame(uint width, uint height)
    {
        if (_sceneFramebuffer == 0)
        {
            CreateFramebuffer(width, height);
            InitializeShaders();
            CreateShapes();

            _frameWidth = width;
            _frameHeight = height;
        }

        if (_frameWidth != width || _frameHeight != height)
        {
            ResizeFramebuffer(width, height);

            _frameWidth = width;
            _frameHeight = height;
        }
    }

    private void ResizeFramebuffer(uint width, uint height)
    {
        _gl.BindFramebuffer(GLEnum.Framebuffer, _sceneFramebuffer);
        _gl.BindRenderbuffer(GLEnum.Renderbuffer, _sceneRenderBufferObject);
        _gl.BindTexture(GLEnum.Texture2D, _sceneColorTexture);

        _gl.TexImage2D(
            GLEnum.Texture2D,
            0,
            InternalFormat.Rgba,
            width,
            height,
            0,
            GLEnum.Rgba,
            GLEnum.UnsignedByte,
            Array.Empty<byte>()
        );

        _gl.RenderbufferStorage(GLEnum.Renderbuffer, GLEnum.Depth24Stencil8, width, height);

        if (_gl.CheckFramebufferStatus(GLEnum.Framebuffer) != GLEnum.FramebufferComplete)
        {
            throw new Exception("Framebuffer is not complete");
        }

        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);
        _gl.BindTexture(GLEnum.Texture2D, 0);
        _gl.BindRenderbuffer(GLEnum.Renderbuffer, 0);
    }

    private void CreateFramebuffer(uint width, uint height)
    {
        _gl.GenFramebuffers(1, out _sceneFramebuffer);
        _gl.BindFramebuffer(GLEnum.Framebuffer, _sceneFramebuffer);

        _gl.GenTextures(1, out _sceneColorTexture);
        _gl.BindTexture(GLEnum.Texture2D, _sceneColorTexture);

        _gl.TexImage2D(
            GLEnum.Texture2D,
            0,
            InternalFormat.Rgba,
            width,
            height,
            0,
            GLEnum.Rgba,
            GLEnum.UnsignedByte,
            Array.Empty<byte>()
        );

        var linear = (int)GLEnum.Linear;
        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, ref linear);
        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, ref linear);

        _gl.FramebufferTexture2D(
            GLEnum.Framebuffer,
            GLEnum.ColorAttachment0,
            GLEnum.Texture2D,
            _sceneColorTexture,
            0
        );

        _gl.GenRenderbuffers(1, out _sceneRenderBufferObject);
        _gl.BindRenderbuffer(GLEnum.Renderbuffer, _sceneRenderBufferObject);
        _gl.RenderbufferStorage(GLEnum.Renderbuffer, GLEnum.Depth24Stencil8, width, height);
        _gl.FramebufferRenderbuffer(
            GLEnum.Framebuffer,
            GLEnum.DepthStencilAttachment,
            GLEnum.Renderbuffer,
            _sceneRenderBufferObject
        );

        if (_gl.CheckFramebufferStatus(GLEnum.Framebuffer) != GLEnum.FramebufferComplete)
        {
            throw new Exception("Framebuffer is not complete");
        }

        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        if (_sceneFramebuffer != 0)
            _gl.DeleteFramebuffer(_sceneFramebuffer);

        if (_sceneColorTexture != 0)
            _gl.DeleteTexture(_sceneColorTexture);

        if (_fragmentShader != 0)
            _gl.DeleteShader(_fragmentShader);

        if (_vertexShader != 0)
            _gl.DeleteShader(_vertexShader);

        if (_shaderProgram != 0)
            _gl.DeleteProgram(_shaderProgram);

        if (_sceneRenderBufferObject != 0)
            _gl.DeleteRenderbuffer(_sceneRenderBufferObject);

        if (_vao != 0)
            _gl.DeleteVertexArray(_vao);

        if (_ebo != 0)
            _gl.DeleteBuffer(_ebo);

        if (_vbo != 0)
            _gl.DeleteBuffer(_vbo);

        _sceneFramebuffer = 0;
        _sceneRenderBufferObject = 0;
        _sceneColorTexture = 0;
        _shaderProgram = 0;
        _fragmentShader = 0;
        _vertexShader = 0;
        _vbo = 0;
        _vao = 0;
        _ebo = 0;

        _disposed = true;
    }
}
