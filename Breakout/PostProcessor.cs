using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Breakout
{
    // PostProcessor hosts all PostProcessing effects for the Breakout
    // Game. It renders the game on a textured quad after which one can
    // enable specific effects by enabling either the Confuse, Chaos or 
    // Shake boolean. 
    // It is required to call BeginRender() before rendering the game
    // and EndRender() after rendering the game for the class to work.
    public class PostProcessor
    {
        // state
        public Shader PostProcessingShader;
        Texture2D Texture;
        public int Width, Height;
        // options
        public bool Confuse, Chaos, Shake;

        // constructor
        public PostProcessor(Shader shader, int width, int height)
        {
            PostProcessingShader = (shader);
            Texture = new Texture2D();
            Width = (width);
            Height = (height);
            Confuse = (false); Chaos = (false);
            Shake = (false);

            // initialize renderbuffer/framebuffer object
            GL.GenFramebuffers(1, out MSFBO);
            GL.GenFramebuffers(1, out FBO);
            GL.GenRenderbuffers(1, out RBO);
            // initialize renderbuffer storage with a multisampled color buffer (don't need a depth/stencil buffer)
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, MSFBO);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, 4, RenderbufferStorage.Rgb8, width, height); // allocate storage for render buffer object
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, RBO); // attach MS render buffer object to framebuffer
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                //std::cout << "ERROR::POSTPROCESSOR: Failed to initialize MSFBO" << std::endl;
            }
            // also initialize the FBO/texture to blit multisampled color-buffer to; used for shader operations (for postprocessing effects)
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            Texture.Generate(width, height, null);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Texture.ID, 0); // attach texture to framebuffer as its color attachment
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                //std::cout << "ERROR::POSTPROCESSOR: Failed to initialize FBO" << std::endl;
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            // initialize render data and uniforms
            initRenderData();
            PostProcessingShader.SetInteger("scene", 0, true);
            float offset = 1.0f / 300.0f;
            float[] offsets = {
                -offset,  offset,  // top-left
 0.0f,    offset  ,  // top-center
        offset,  offset  ,  // top-right
        -offset,  0.0f   ,  // center-left
     0.0f,    0.0f    ,  // center-center
       offset,  0.0f    ,  // center - right
      -offset, -offset  ,  // bottom-left
       0.0f,   -offset  ,  // bottom-center
      offset, -offset     // bottom-right    
            };
            GL.Uniform2(GL.GetUniformLocation(PostProcessingShader.ID, "offsets"), 9, offsets);
            int[] edge_kernel = {
        -1, -1, -1,
        -1,  8, -1,
        -1, -1, -1
    };
            GL.Uniform1(GL.GetUniformLocation(PostProcessingShader.ID, "edge_kernel"), 9, edge_kernel);
            float[] blur_kernel = {
        1.0f / 16.0f, 2.0f / 16.0f, 1.0f / 16.0f,
        2.0f / 16.0f, 4.0f / 16.0f, 2.0f / 16.0f,
        1.0f / 16.0f, 2.0f / 16.0f, 1.0f / 16.0f
    };
            GL.Uniform1(GL.GetUniformLocation(PostProcessingShader.ID, "blur_kernel"), 9, blur_kernel);
        }

        // prepares the postprocessor's framebuffer operations before rendering the game
        public void BeginRender()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, MSFBO);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        // should be called after rendering the game, so it stores all the rendered data into a texture object
        public void EndRender()
        {
            // now resolve multisampled color-buffer into intermediate FBO to store to texture
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, MSFBO);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FBO);
            GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); // binds both READ and WRITE framebuffer to default framebuffer
        }

        // renders the PostProcessor texture quad (as a screen-encompassing large sprite)
        public void Render(float time)
        {
            // set uniforms/options
            PostProcessingShader.use();
            PostProcessingShader.SetFloat("time", time);
            PostProcessingShader.SetInteger("confuse", Confuse);
            PostProcessingShader.SetInteger("chaos", Chaos);
            PostProcessingShader.SetInteger("shake", Shake);
            // render textured quad
            GL.ActiveTexture(TextureUnit.Texture0);
            Texture.Bind();
            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);
        }

        // render state
        private int MSFBO, FBO; // MSFBO = Multisampled FBO. FBO is regular, used for blitting MS color-buffer to texture
        private int RBO; // RBO is used for multisampled color buffer
        private int VAO;
        // initialize quad for rendering postprocessing texture
        private void initRenderData()
        {
            // configure VAO/VBO
            int VBO;
            float[] vertices = {
        // pos        // tex
        -1.0f, -1.0f, 0.0f, 0.0f,
         1.0f,  1.0f, 1.0f, 1.0f,
        -1.0f,  1.0f, 0.0f, 1.0f,

        -1.0f, -1.0f, 0.0f, 0.0f,
         1.0f, -1.0f, 1.0f, 0.0f,
         1.0f,  1.0f, 1.0f, 1.0f
    };
            GL.GenVertexArrays(1, out VAO);
            GL.GenBuffers(1, out VBO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(VAO);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
    };
}

