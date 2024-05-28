using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Skybox
{
    public partial class Form1 : Form
    {
        private void Form1_Load(object sender, EventArgs e)
        {
            mf = new MessageFilter();
            Application.AddMessageFilter(mf);
        }

        MessageFilter mf = null;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var deltaTime = 0.010f;
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            if (keyData == Keys.W)
            {
                camera.ProcessKeyboard(Camera_Movement.FORWARD, deltaTime);
                return true;
            }
            if (keyData == Keys.S)
            {
                camera.ProcessKeyboard(Camera_Movement.BACKWARD, deltaTime);
                return true;
            }
            if (keyData == Keys.A)
            {
                camera.ProcessKeyboard(Camera_Movement.LEFT, deltaTime);
                return true;
            }
            if (keyData == Keys.D)
            {
                camera.ProcessKeyboard(Camera_Movement.RIGHT, deltaTime);
                return true;
            }

            return false;
        }
        public Form1()
        {
            InitializeComponent();

            MouseWheel += Form1_MouseWheel;
            glControl = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 4), 3, 3, OpenTK.Graphics.GraphicsContextFlags.Default);
            glControl.MouseMove += GlControl_MouseMove;
            glControl.Paint += Gl_Paint;
            Controls.Add(glControl);
            glControl.Dock = DockStyle.Fill;
            Width = SCR_WIDTH;
            Height = SCR_HEIGHT;
        }

        private void GlControl_MouseMove(object sender, MouseEventArgs e)
        {
            var cur = PointToClient(Cursor.Position);

            var xpos = cur.X;
            var ypos = cur.Y;

            if (firstMouse)
            {
                lastX = xpos;
                lastY = ypos;
                firstMouse = false;
            }

            float xoffset = xpos - lastX;
            float yoffset = lastY - ypos; // reversed since y-coordinates go from bottom to top

            lastX = xpos;
            lastY = ypos;
            camera.ProcessMouseMovement(xoffset, yoffset);
        }

        float lastX = 800.0f / 2.0f;
        float lastY = 600.0f / 2.0f;
        bool firstMouse = true;


        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            camera.ProcessMouseScroll(e.Delta / 120);
        }

        // cube VAO
        int cubeVAO, cubeVBO;
        GLControl glControl;

        int quadVAO, quadVBO;

        bool first = true;
        const int SCR_WIDTH = 1280;
        const int SCR_HEIGHT = 720;
        int planeVAO, planeVBO;

        int cubeTexture;
        int floorTexture;
        int textureColorbuffer;

        void init()
        {
            // build and compile shaders
            // -------------------------
            shader = new Shader("5.2.framebuffers.vs", "5.2.framebuffers.fs");
            screenShader = new Shader("5.2.framebuffers_screen.vs", "5.2.framebuffers_screen.fs");
            // set up vertex data (and buffer(s)) and configure vertex attributes
            // ------------------------------------------------------------------
            float[] cubeVertices = {
        // positions          // texture Coords
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
         0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
         0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
        -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

        -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
        -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
        -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

         0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
         0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
         0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
         0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
         0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
         0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
         0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
        -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
    };
            float[] planeVertices = {
        // positions          // texture Coords 
         5.0f, -0.5f,  5.0f,  2.0f, 0.0f,
        -5.0f, -0.5f,  5.0f,  0.0f, 0.0f,
        -5.0f, -0.5f, -5.0f,  0.0f, 2.0f,

         5.0f, -0.5f,  5.0f,  2.0f, 0.0f,
        -5.0f, -0.5f, -5.0f,  0.0f, 2.0f,
         5.0f, -0.5f, -5.0f,  2.0f, 2.0f
    };
            float[] quadVertices = { // vertex attributes for a quad that fills the entire screen in Normalized Device Coordinates. NOTE that this plane is now much smaller and at the top of the screen
        // positions   // texCoords
        -0.3f,  1.0f,  0.0f, 1.0f,
        -0.3f,  0.7f,  0.0f, 0.0f,
         0.3f,  0.7f,  1.0f, 0.0f,

        -0.3f,  1.0f,  0.0f, 1.0f,
         0.3f,  0.7f,  1.0f, 0.0f,
         0.3f,  1.0f,  1.0f, 1.0f
    };

            GL.GenVertexArrays(1, out cubeVAO);
            GL.GenBuffers(1, out cubeVBO);
            GL.BindVertexArray(cubeVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, cubeVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * cubeVertices.Length, cubeVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (3 * sizeof(float)));
            // plane VAO
            
            GL.GenVertexArrays(1, out planeVAO);
            GL.GenBuffers(1, out planeVBO);
            GL.BindVertexArray(planeVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, planeVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * planeVertices.Length, planeVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (3 * sizeof(float)));
            // screen quad VAO
            
            GL.GenVertexArrays(1, out quadVAO);
            GL.GenBuffers(1, out quadVBO);
            GL.BindVertexArray(quadVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, quadVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * quadVertices.Length, quadVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (2 * sizeof(float)));

            // load textures
            // -------------
            cubeTexture = loadTexture("container.jpg");
            floorTexture = loadTexture("metal.png");

            // shader configuration
            // --------------------
            shader.use();
            shader.setInt("texture1", 0);

            screenShader.use();
            screenShader.setInt("screenTexture", 0);

            // framebuffer configuration
            // -------------------------
            
            GL.GenFramebuffers(1, out framebuffer);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            // create a color attachment texture
            
            GL.GenTextures(1, out textureColorbuffer);
            GL.BindTexture(TextureTarget.Texture2D, textureColorbuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, SCR_WIDTH, SCR_HEIGHT, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedInt, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, textureColorbuffer, 0);
            // create a renderbuffer object for depth and stencil attachment (we won't be sampling these)
            int rbo;
            GL.GenRenderbuffers(1, out rbo);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, SCR_WIDTH, SCR_HEIGHT); // use a single renderbuffer object for both a depth AND stencil buffer.
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, rbo); // now actually attach it
                                                                                                                                                           // now that we actually created the framebuffer and added all attachments we want to check if it is actually complete now
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                //cout << "ERROR::FRAMEBUFFER:: Framebuffer is not complete!" << endl;             
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        }
        private void Gl_Paint(object sender, PaintEventArgs e)
        {
            if (!glControl.Context.IsCurrent)
                glControl.MakeCurrent();

            GL.Viewport(0, 0, glControl.Width, glControl.Height);

            if (first)
                init();

            Redraw();
            glControl.SwapBuffers();
        }

        public static Bitmap ReadResourceBmp(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fr1 = assembly.GetManifestResourceNames().First(z => z.Contains(resourceName));

            using (Stream stream = assembly.GetManifestResourceStream(fr1))
            {
                return Bitmap.FromStream(stream) as Bitmap;
            }
        }

        private int loadTexture(string path)
        {
            return loadTexture(ReadResourceBmp(path));
        }

        private int loadTexture(Bitmap bitmap)
        {
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            int textureID = GL.GenTexture();
            GL.GenTextures(1, out textureID);

            GL.BindTexture(TextureTarget.Texture2D, textureID);

            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var format = PixelInternalFormat.Rgba;
            var format2 = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;

            var ps = Image.GetPixelFormatSize(bitmap.PixelFormat);
            if (ps == 24)
            {
                format = PixelInternalFormat.Rgb;
                format2 = OpenTK.Graphics.OpenGL.PixelFormat.Bgr;

            }
            if (ps == 8)
            {
                format = PixelInternalFormat.R8;
                format2 = OpenTK.Graphics.OpenGL.PixelFormat.Red;
            }
            GL.TexImage2D(TextureTarget.Texture2D, 0, format, data.Width, data.Height, 0,
                format2, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            return textureID;
        }

        // loads a cubemap texture from 6 individual texture faces
        // order:
        // +X (right)
        // -X (left)
        // +Y (top)
        // -Y (bottom)
        // +Z (front) 
        // -Z (back)
        // -------------------------------------------------------
        int loadCubemap(string[] faces)
        {
            int textureID;
            GL.GenTextures(1, out textureID);
            GL.BindTexture(TextureTarget.TextureCubeMap, textureID);

            for (int i = 0; i < faces.Length; i++)
            {
                var bitmap = ReadResourceBmp(faces[i]);
                BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, bitmap.PixelFormat);
                var format = PixelInternalFormat.Rgba;
                var format2 = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;

                var ps = Image.GetPixelFormatSize(bitmap.PixelFormat);
                if (ps == 24)
                {
                    format = PixelInternalFormat.Rgb;
                    format2 = OpenTK.Graphics.OpenGL.PixelFormat.Bgr;

                }
                if (ps == 8)
                {
                    format = PixelInternalFormat.R8;
                    format2 = OpenTK.Graphics.OpenGL.PixelFormat.Red;
                }
                //unsigned char* data = stbi_load(faces[i].c_str(), &width, &height, &nrChannels, 0);
                //if (data)
                {
                    GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, format, data.Width, data.Height, 0,
                format2, PixelType.UnsignedByte, data.Scan0);


                }
                //    else
                {
                    //std::cout << "Cubemap texture failed to load at path: " << faces[i] << std::endl;
                    //stbi_image_free(data);
                }
            }
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);


            return textureID;
        }

        int framebuffer;


        Shader shader;
        Shader screenShader;

        Camera camera = new Camera(new Vector3(0.0f, 0.0f, 3.0f));
        void Redraw()
        {
            GL.Enable(EnableCap.DepthTest);

            // first render pass: mirror texture.
            // bind to framebuffer and draw to color texture as we normally 
            // would, but with the view camera reversed.
            // bind to framebuffer and draw scene as we normally would to color texture 
            // ------------------------------------------------------------------------
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.Enable(EnableCap.DepthTest); // enable depth testing (is disabled for rendering screen-space quad)


            GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            shader.use();
            Matrix4 model = Matrix4.Identity;
            camera.Yaw += 180.0f; // rotate the camera's yaw 180 degrees around
            camera.ProcessMouseMovement(0, 0, false); // call this to make sure it updates its camera vectors, note that we disable pitch constrains for this specific case (otherwise we can't reverse camera's pitch values)
            var view = camera.GetViewMatrix();
            camera.Yaw -= 180.0f; // reset it back to its original orientation
            camera.ProcessMouseMovement(0, 0, true);
            var projection =Matrix4.CreatePerspectiveFieldOfView((float)(camera.Zoom*Math.PI/180f), (float)SCR_WIDTH / (float)SCR_HEIGHT, 0.1f, 100.0f);
            shader.setMat4("view", view);
            shader.setMat4("projection", projection);
            // cubes
            GL.BindVertexArray(cubeVAO);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, cubeTexture);
            var tr = Matrix4.CreateTranslation(-1, 0, -1);
            model = model * tr;
            shader.setMat4("model", model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            model = Matrix4.Identity;

            tr = Matrix4.CreateTranslation(2, 0, 0);
            model = tr * model;

            shader.setMat4("model", model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            // floor
            GL.BindVertexArray(planeVAO);
            GL.BindTexture(TextureTarget.Texture2D, floorTexture);
            shader.setMat4("model", Matrix4.Identity);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);

            // second render pass: draw as normal
            // ----------------------------------
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit| ClearBufferMask.DepthBufferBit);

            model = Matrix4.Identity;
            view = camera.GetViewMatrix();
            shader.setMat4("view", view);

            // cubes
            GL.BindVertexArray(cubeVAO);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, cubeTexture);

            tr = Matrix4.CreateTranslation(-1, 0, -1);
            model = tr * model;
            shader.setMat4("model", model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            model = Matrix4.Identity;
            tr = Matrix4.CreateTranslation(2, 0, 0);
            model = tr * model;
            shader.setMat4("model", model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            // floor
            GL.BindVertexArray(planeVAO);
            GL.BindTexture(TextureTarget.Texture2D, floorTexture);
            shader.setMat4("model", Matrix4.Identity);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);

            // now draw the mirror quad with screen texture
            // --------------------------------------------
            GL.Disable(EnableCap.DepthTest); // disable depth test so screen-space quad isn't discarded due to depth test.

            screenShader.use();
            GL.BindVertexArray(quadVAO);
            GL.BindTexture(TextureTarget.Texture2D, textureColorbuffer);   // use the color attachment texture as the texture of the quad plane
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);



        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            glControl.Invalidate();
        }
    }

}

