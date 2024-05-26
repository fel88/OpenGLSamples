using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenGLSamples
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
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            
            return false;
        }
        public Form1()
        {
            InitializeComponent();

            MouseWheel += Form1_MouseWheel;
            glControl = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 4), 3, 3, OpenTK.Graphics.GraphicsContextFlags.ForwardCompatible);


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
            //var xOffset = Width / 2 - cur.X;
            //var yOffset = Height / 2 - cur.Y;

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
        }

        float lastX = 800.0f / 2.0f;
        float lastY = 600.0f / 2.0f;
        bool firstMouse = true;
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
                        
        }

        GLControl glControl;
        Shader ourShader;

        bool first = true;
        const int SCR_WIDTH = 800;
        const int SCR_HEIGHT = 600;
        int VBO, VAO, EBO;
        int texture1, texture2;
        private void Gl_Paint(object sender, PaintEventArgs e)
        {
            if (!glControl.Context.IsCurrent)
            {
                glControl.MakeCurrent();
            }
            if (first)
            {
                // build and compile our shader zprogram
                // ------------------------------------
                ourShader = new Shader("5.1.transform.vs", "5.1.transform.fs");
                // set up vertex data (and buffer(s)) and configure vertex attributes
                // ------------------------------------------------------------------
                float[] vertices = {
        // positions          // texture coords
         0.5f,  0.5f, 0.0f,   1.0f, 1.0f, // top right
         0.5f, -0.5f, 0.0f,   1.0f, 0.0f, // bottom right
        -0.5f, -0.5f, 0.0f,   0.0f, 0.0f, // bottom left
        -0.5f,  0.5f, 0.0f,   0.0f, 1.0f  // top left 
    };
                int[] indices = {
        0, 1, 3, // first triangle
        1, 2, 3  // second triangle
    };

                GL.GenVertexArrays(1, out VAO);
                GL.GenBuffers(1, out VBO);
                GL.GenBuffers(1, out EBO);

                GL.BindVertexArray(VAO);

                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float)* vertices.Length, vertices, BufferUsageHint.StaticDraw);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
                GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int)* indices.Length, indices, BufferUsageHint.StaticDraw);

                // position attribute
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);
                // texture coord attribute
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (3 * sizeof(float)));
                GL.EnableVertexAttribArray(1);


                // load and create a texture 
                // -------------------------

                // texture 1
                // ---------
              
                // load image, create texture and generate mipmaps
                texture1 = loadTexture(ReadResourceBmp("container.jpg"));
                texture2 = loadTexture(ReadResourceBmp("awesomeface.png"));
                

                // tell opengl for each sampler to which texture unit it belongs to (only has to be done once)
                // -------------------------------------------------------------------------------------------
                ourShader.use();
                ourShader.setInt("texture1", 0);
                ourShader.setInt("texture2", 1);


                first = false;



            }
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
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
        float glfwGetTime()
        {
            return (float)DateTime.Now.Subtract(startTime).TotalSeconds;
        }
        DateTime startTime = DateTime.Now;

        
        void Redraw()
        {
            // render
            // ------

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // bind textures on corresponding texture units
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, texture2);

            // create transformations
            Matrix4 transform = Matrix4.Identity; // make sure to initialize matrix to identity matrix first
            //transform = glm::translate(transform, glm::vec3(0.5f, -0.5f, 0.0f));
            transform = Matrix4.CreateTranslation(0.5f, -0.5f, 0.0f) * transform;
            //transform = glm::rotate(transform, (float)glfwGetTime(), glm::vec3(0.0f, 0.0f, 1.0f));
            transform = Matrix4.CreateRotationZ((float)glfwGetTime()) * transform;

            // get matrix's uniform location and set matrix
            ourShader.use();
            //int transformLoc = GL.GetUniformLocation(ourShader.ID, "transform");
            //GL.UniformMatrix4(transformLoc, 1, false, ref transform);
            ourShader.setMat4("transform",transform);
            // render container
            GL.BindVertexArray(VAO);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }
                
        private void timer1_Tick(object sender, EventArgs e)
        {
            glControl.Invalidate();

        }
    }
}

