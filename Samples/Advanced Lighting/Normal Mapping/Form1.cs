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

namespace pbr
{
    public partial class Form1 : Form
    {
        private void Form1_Load(object sender, EventArgs e)
        {
            mf = new MessageFilter();
            Application.AddMessageFilter(mf);
        }
        MessageFilter mf = null;

        Label label1;
        bool dynamicLight = true;
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
            if (keyData == Keys.L)
            {
                dynamicLight = !dynamicLight;
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
            label1 = new Label() { AutoSize = true };
            glControl.Controls.Add(label1);
            label1.BackColor = Color.FromArgb(25, 25, 25);
            label1.ForeColor = Color.White;
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
            camera.ProcessMouseMovement(xoffset, yoffset);
        }

        float lastX = 800.0f / 2.0f;
        float lastY = 600.0f / 2.0f;
        bool firstMouse = true;
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            camera.ProcessMouseScroll(e.Delta / 120);
            var projection = Matrix4.CreatePerspectiveFieldOfView((float)Camera.radians(camera.Zoom), (float)SCR_WIDTH / (float)SCR_HEIGHT, 0.1f, 100.0f);
            shader.use();
            shader.setMat4("projection", projection);
        }


        GLControl glControl;
        Shader shader;
        bool first = true;
        const int SCR_WIDTH = 1280;
        const int SCR_HEIGHT = 720;
        Vector3 lightPos;
        private void Gl_Paint(object sender, PaintEventArgs e)
        {
            if (!glControl.Context.IsCurrent)
            {
                glControl.MakeCurrent();
            }
            if (first)
            {
                shader = new Shader("4.normal_mapping.vs", "4.normal_mapping.fs");
                GL.Viewport(0, 0, SCR_WIDTH, SCR_HEIGHT);

                // shader configuration
                // --------------------
                shader.use();
                shader.setInt("diffuseMap", 0);
                shader.setInt("normalMap", 1);

                // lighting info
                // -------------
                lightPos = new Vector3(0.5f, 1.0f, 0.3f);

                first = false;
                loadTextures();
                var projection = Matrix4.CreatePerspectiveFieldOfView((float)Camera.radians(camera.Zoom), (float)SCR_WIDTH / (float)SCR_HEIGHT, 0.1f, 100.0f);
                shader.use();
                shader.setMat4("projection", projection);

            }
            Redraw();


            label1.Text = "Dynamic light: " + (dynamicLight ? "On" : "Off");

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

        void loadTextures()
        {
            // load textures
            // -------------
            diffuseMap = loadTexture(ReadResourceBmp("brickwall.jpg"));
            normalMap = loadTexture(ReadResourceBmp("brickwall_normal.jpg"));
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

        int diffuseMap;
        int normalMap;


        Camera camera = new Camera(new Vector3(0.0f, 0.0f, 3.0f));
        void Redraw()
        {
            GL.Enable(EnableCap.DepthTest);

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);





            // configure view/projection matrices
            var projection = Matrix4.CreatePerspectiveFieldOfView((float)Camera.radians(camera.Zoom), (float)SCR_WIDTH / (float)SCR_HEIGHT, 0.1f, 100.0f);
            var view = camera.GetViewMatrix();
            shader.use();
            shader.setMat4("projection", projection);
            shader.setMat4("view", view);



            // render normal-mapped quad
            var model = Matrix4.Identity;
            //model = Matrix4.CreateFromAxisAngle( glm::rotate(model, glm::radians((float)glfwGetTime() * -10.0f), glm::normalize(glm::vec3(1.0, 0.0, 1.0))); // rotate the quad to show normal mapping from multiple directions
            model = Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 1).Normalized(), (float)((glfwGetTime() * (-10f)) * Math.PI / 180f));
            shader.setMat4("model", model);
            shader.setVec3("viewPos", camera.Position);
            shader.setVec3("lightPos", lightPos);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, diffuseMap);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, normalMap);
            renderQuad();

            // render light source (simply re-renders a smaller plane at the light's position for debugging/visualization)
            model = Matrix4.Identity;

            model = Matrix4.CreateTranslation(lightPos)*model;
            model = Matrix4.CreateScale(new Vector3(0.1f))*model;
            shader.setMat4("model", model);
            renderQuad();
        }



        float timeTemp = 0;
        private float glfwGetTime()
        {
            if (dynamicLight)
                timeTemp += 0.01f;
            if (timeTemp > 3) { timeTemp = 0; }
            return timeTemp;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            glControl.Invalidate();

        }

        // renders a 1x1 quad in NDC with manually calculated tangent vectors
        // ------------------------------------------------------------------
        int quadVAO = 0;
        int quadVBO;
        void renderQuad()
        {
            if (quadVAO == 0)
            {
                // positions
                Vector3 pos1 = new Vector3(-1.0f, 1.0f, 0.0f);
                Vector3 pos2 = new Vector3(-1.0f, -1.0f, 0.0f);
                Vector3 pos3 = new Vector3(1.0f, -1.0f, 0.0f);
                Vector3 pos4 = new Vector3(1.0f, 1.0f, 0.0f);
                // texture coordinates
                Vector2 uv1 = new Vector2(0.0f, 1.0f);
                Vector2 uv2 = new Vector2(0.0f, 0.0f);
                Vector2 uv3 = new Vector2(1.0f, 0.0f);
                Vector2 uv4 = new Vector2(1.0f, 1.0f);
                // normal vector
                Vector3 nm = new Vector3(0.0f, 0.0f, 1.0f);

                // calculate tangent/bitangent vectors of both triangles
                Vector3 tangent1, bitangent1;
                Vector3 tangent2, bitangent2;
                // triangle 1
                // ----------
                Vector3 edge1 = pos2 - pos1;
                Vector3 edge2 = pos3 - pos1;
                Vector2 deltaUV1 = uv2 - uv1;
                Vector2 deltaUV2 = uv3 - uv1;

                float f = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);

                tangent1.X = f * (deltaUV2.Y * edge1.X - deltaUV1.Y * edge2.X);
                tangent1.Y = f * (deltaUV2.Y * edge1.Y - deltaUV1.Y * edge2.Y);
                tangent1.Z = f * (deltaUV2.Y * edge1.Z - deltaUV1.Y * edge2.Z);

                bitangent1.X = f * (-deltaUV2.X * edge1.X + deltaUV1.X * edge2.X);
                bitangent1.Y = f * (-deltaUV2.X * edge1.Y + deltaUV1.X * edge2.Y);
                bitangent1.Z = f * (-deltaUV2.X * edge1.Z + deltaUV1.X * edge2.Z);

                // triangle 2
                // ----------
                edge1 = pos3 - pos1;
                edge2 = pos4 - pos1;
                deltaUV1 = uv3 - uv1;
                deltaUV2 = uv4 - uv1;

                f = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);

                tangent2.X = f * (deltaUV2.Y * edge1.X - deltaUV1.Y * edge2.X);
                tangent2.Y = f * (deltaUV2.Y * edge1.Y - deltaUV1.Y * edge2.Y);
                tangent2.Z = f * (deltaUV2.Y * edge1.Z - deltaUV1.Y * edge2.Z);


                bitangent2.X = f * (-deltaUV2.X * edge1.X + deltaUV1.X * edge2.X);
                bitangent2.Y = f * (-deltaUV2.X * edge1.Y + deltaUV1.X * edge2.Y);
                bitangent2.Z = f * (-deltaUV2.X * edge1.Z + deltaUV1.X * edge2.Z);


                float[] quadVertices = {
            // positions            // normal         // texcoords  // tangent                          // bitangent
            pos1.X, pos1.Y, pos1.Z, nm.X, nm.Y, nm.Z, uv1.X, uv1.Y, tangent1.X, tangent1.Y, tangent1.Z, bitangent1.X, bitangent1.Y, bitangent1.Z,
            pos2.X, pos2.Y, pos2.Z, nm.X, nm.Y, nm.Z, uv2.X, uv2.Y, tangent1.X, tangent1.Y, tangent1.Z, bitangent1.X, bitangent1.Y, bitangent1.Z,
            pos3.X, pos3.Y, pos3.Z, nm.X, nm.Y, nm.Z, uv3.X, uv3.Y, tangent1.X, tangent1.Y, tangent1.Z, bitangent1.X, bitangent1.Y, bitangent1.Z,

            pos1.X, pos1.Y, pos1.Z, nm.X, nm.Y, nm.Z, uv1.X, uv1.Y, tangent2.X, tangent2.Y, tangent2.Z, bitangent2.X, bitangent2.Y, bitangent2.Z,
            pos3.X, pos3.Y, pos3.Z, nm.X, nm.Y, nm.Z, uv3.X, uv3.Y, tangent2.X, tangent2.Y, tangent2.Z, bitangent2.X, bitangent2.Y, bitangent2.Z,
            pos4.X, pos4.Y, pos4.Z, nm.X, nm.Y, nm.Z, uv4.X, uv4.Y, tangent2.X, tangent2.Y, tangent2.Z, bitangent2.X, bitangent2.Y, bitangent2.Z
        };
                // configure plane VAO
                GL.GenVertexArrays(1, out quadVAO);
                GL.GenBuffers(1, out quadVBO);
                GL.BindVertexArray(quadVAO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, quadVBO);
                var arr1 = quadVertices.ToArray();
                GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), arr1, BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 14 * sizeof(float), 0);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 14 * sizeof(float), (3 * sizeof(float)));
                GL.EnableVertexAttribArray(2);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 14 * sizeof(float), (6 * sizeof(float)));
                GL.EnableVertexAttribArray(3);
                GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 14 * sizeof(float), (8 * sizeof(float)));
                GL.EnableVertexAttribArray(4);
                GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 14 * sizeof(float), (11 * sizeof(float)));
            }
            GL.BindVertexArray(quadVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);

        }


    }

}

