using System.Numerics;
using ImGuiNET;
using Silk.NET.OpenGL;

namespace P2PVTT.Components.Scene;

public sealed class SceneComponent : IDisposable
{
    private readonly GL _gl;

    private const string _fragmentShaderCode =
        @"#version 330 core

// Output variable for the final color of the fragment
out vec4 FragColor;

void main()
{
    // RGBA format (Red, Green, Blue, Alpha)
    FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f); 
}

";
    private uint _fragmentShader;
    private uint _shaderProgram;
    private uint _sceneFramebuffer;
    private uint _sceneColorTexture;
    private uint _sceneRenderBufferObject;

    private bool _initialized;
    private bool _disposed;

    public SceneComponent(GL gl)
    {
        _gl = gl;
    }

    public void Render(int width, int height, int x, int y)
    {
        ImGui.SetNextWindowPos(new Vector2(x, y), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(width, height), ImGuiCond.Always);

        InitializeFrame((uint)width, (uint)height);

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
                var sceneWidth = Math.Max(1, (int)availableSize.X);
                var sceneHeight = Math.Max(1, (int)availableSize.Y);

                DrawScene((uint)sceneWidth, (uint)sceneHeight);

                ImGui.Image(
                    (nint)_sceneColorTexture,
                    availableSize,
                    new Vector2(0.0f, 1.0f),
                    new Vector2(1.0f, 0.0f)
                );

                //_gl.UseProgram(0);
            }
        }

        ImGui.End();
    }

    private void DrawScene(uint width, uint height)
    {
        _gl.BindFramebuffer(GLEnum.Framebuffer, _sceneFramebuffer);
        _gl.UseProgram(_shaderProgram);

        var vertices = new List<float>();
        var cx = 0f;
        var cy = 0f;
        var radius = 0.5f;
        var segments = 100;

        vertices.Add(cx);
        vertices.Add(cy);
        vertices.Add(0f);

        for (var i = 0; i <= segments; i++)
        {
            var theta = 2 * Math.PI * (float)i / (float)segments;
            var x = radius * (float)Math.Cos(theta);
            var y = radius * (float)Math.Sin(theta);

            vertices.Add(x + cx);
            vertices.Add(y + cy);
            vertices.Add(0f);
        }

        var vao = _gl.GenVertexArray();
        _gl.GenBuffers(1, out uint vbo);

        _gl.BindVertexArray(vao);
        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        _gl.BufferData<float>(
            GLEnum.ArrayBuffer,
            (uint)vertices.Count * sizeof(float),
            vertices.ToArray().AsSpan(),
            GLEnum.StaticDraw
        );

        _gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), 0);
        _gl.EnableVertexAttribArray(0);

        _gl.BindVertexArray(vao);

        _gl.DrawArrays(GLEnum.TriangleFan, 0, (uint)segments + 2);

        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);
    }

    private void InitializeShader()
    {
        _fragmentShader = _gl.CreateShader(GLEnum.FragmentShader);
        _gl.ShaderSource(_fragmentShader, _fragmentShaderCode);
        _gl.CompileShader(_fragmentShader);

        var shaderLog = _gl.GetShaderInfoLog(_fragmentShader);
        if (!string.IsNullOrEmpty(shaderLog))
            Console.WriteLine("SHADER LOG: " + shaderLog);

        _shaderProgram = _gl.CreateProgram();
        _gl.AttachShader(_shaderProgram, _fragmentShader);

        _gl.LinkProgram(_shaderProgram);

        _gl.DeleteShader(_fragmentShader);
    }

    private void InitializeFrame(uint width, uint height)
    {
        _gl.Viewport(0, 0, width, height);

        if (_initialized)
            return;

        _sceneFramebuffer = _gl.GenFramebuffer();
        _gl.BindFramebuffer(GLEnum.Framebuffer, _sceneFramebuffer);

        _sceneColorTexture = _gl.GenTexture();
        _gl.BindTexture(GLEnum.Texture2D, _sceneColorTexture);

        _gl.TexImage2D(
            GLEnum.Texture2D,
            0,
            InternalFormat.Rgb,
            width,
            height,
            0,
            GLEnum.Rgb,
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

        _gl.GenRenderbuffers(_sceneRenderBufferObject);
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

        InitializeShader();

        _initialized = true;
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

        if (_shaderProgram != 0)
            _gl.DeleteProgram(_shaderProgram);

        if (_sceneRenderBufferObject != 0)
            _gl.DeleteRenderbuffer(_sceneRenderBufferObject);

        _sceneFramebuffer = 0;
        _sceneRenderBufferObject = 0;
        _sceneColorTexture = 0;
        _shaderProgram = 0;
        _fragmentShader = 0;

        _disposed = true;
    }
}
