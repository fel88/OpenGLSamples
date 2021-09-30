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

        private void Gl_Paint(object sender, PaintEventArgs e)
        {            
            if (!glControl.Context.IsCurrent)
            {
                glControl.MakeCurrent();
            }
            if (first)
            {
                shader = new Shader("1.2.pbr.vs", "1.2.pbr.fs");
                GL.Viewport(0, 0, SCR_WIDTH, SCR_HEIGHT);

                shader.use();
                shader.setInt("albedoMap", 0);
                shader.setInt("normalMap", 1);
                shader.setInt("metallicMap", 2);
                shader.setInt("roughnessMap", 3);
                shader.setInt("aoMap", 4);
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

        Vector3[] lightPositions = {
        new Vector3(0.0f, 0.0f, 10.0f),
    };
        Vector3[] lightColors = {
        new Vector3(150.0f, 150.0f, 150.0f),
    };
        int nrRows = 7;
        int nrColumns = 7;
        float spacing = 2.5f;
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
            // load PBR material textures
            // --------------------------
            albedo = loadTexture(ReadResourceBmp("albedo.png"));
            normal = loadTexture(ReadResourceBmp("normal.png"));
            metallic = loadTexture(ReadResourceBmp("metallic.png"));
            roughness = loadTexture(ReadResourceBmp("roughness.png"));
            ao = loadTexture(ReadResourceBmp("ao.png"));
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

        int albedo;
        int normal;
        int ao;
        int roughness;
        int metallic;

        Camera camera = new Camera(new Vector3(0.0f, 0.0f, 3.0f));
        void Redraw()
        {

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);


            shader.use();
            var view = camera.GetViewMatrix();
            shader.setMat4("view", view);
            shader.setVec3("camPos", camera.Position);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, albedo);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, normal);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, metallic);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, roughness);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, ao);

            // render rows*column number of spheres with material properties defined by textures (they all have the same material properties)
            Matrix4 model = Matrix4.Identity;
            for (int row = 0; row < nrRows; ++row)
            {
                for (int col = 0; col < nrColumns; ++col)
                {
                    model = Matrix4.Identity;
                    var tr = Matrix4.CreateTranslation(new Vector3(
                        (float)(col - (nrColumns / 2)) * spacing,
                        (float)(row - (nrRows / 2)) * spacing,
                        0.0f
                    ));
                    model = model * tr;
                    shader.setMat4("model", model);
                    renderSphere();
                }
            }

            // render light source (simply re-render sphere at light positions)
            // this looks a bit off as we use the same shader, but it'll make their positions obvious and 
            // keeps the codeprint small.
            for (int i = 0; i < lightPositions.Length; ++i)
            {
                var newPos = lightPositions[i] + new Vector3((float)(Math.Sin(glfwGetTime() * 5.0f) * 5.0f), 0.0f, 0.0f);
                //newPos = lightPositions[i];
                shader.setVec3("lightPositions[" + i + "]", newPos);
                shader.setVec3("lightColors[" + i + "]", lightColors[i]);

                model = Matrix4.Identity;
                model = Matrix4.CreateTranslation(newPos) * model;
                var sc = Matrix4.CreateScale(0.5f);
                model = sc * model;
                shader.setMat4("model", model);
                renderSphere();
            }


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

        // renders (and builds at first invocation) a sphere
        // -------------------------------------------------
        uint sphereVAO = 0;
        int indexCount;
        void renderSphere()
        {
            if (sphereVAO == 0)
            {
                GL.GenVertexArrays(1, out sphereVAO);


                uint vbo, ebo;
                GL.GenBuffers(1, out vbo);
                GL.GenBuffers(1, out ebo);

                List<Vector3> positions = new List<Vector3>();
                List<Vector2> uv = new List<Vector2>();
                List<Vector3> normals = new List<Vector3>();
                List<int> indices = new List<int>(); ;

                const int X_SEGMENTS = 64;
                const int Y_SEGMENTS = 64;

                for (uint x = 0; x <= X_SEGMENTS; ++x)
                {
                    for (uint y = 0; y <= Y_SEGMENTS; ++y)
                    {
                        float xSegment = (float)x / (float)X_SEGMENTS;
                        float ySegment = (float)y / (float)Y_SEGMENTS;
                        var xPos = (float)(Math.Cos(xSegment * 2.0f * Math.PI) * Math.Sin(ySegment * Math.PI));
                        var yPos = (float)(Math.Cos(ySegment * Math.PI));
                        var zPos = (float)(Math.Sin(xSegment * 2.0f * Math.PI) * Math.Sin(ySegment * Math.PI));

                        positions.Add(new Vector3(xPos, yPos, zPos));
                        uv.Add(new Vector2(xSegment, ySegment));
                        normals.Add(new Vector3(xPos, yPos, zPos));
                    }
                }

                bool oddRow = false;
                for (uint y = 0; y < Y_SEGMENTS; ++y)
                {
                    if (!oddRow) // even rows: y == 0, y == 2; and so on
                    {
                        for (int x = 0; x <= X_SEGMENTS; ++x)
                        {
                            indices.Add((int)(y * (X_SEGMENTS + 1) + x));
                            indices.Add((int)((y + 1) * (X_SEGMENTS + 1) + x));
                        }
                    }
                    else
                    {
                        for (int x = X_SEGMENTS; x >= 0; --x)
                        {
                            indices.Add((int)((y + 1) * (X_SEGMENTS + 1) + x));
                            indices.Add((int)(y * (X_SEGMENTS + 1) + x));
                        }
                    }
                    oddRow = !oddRow;
                }
                indexCount = indices.Count;

                List<float> data = new List<float>(); ;
                for (int i = 0; i < positions.Count; ++i)
                {
                    data.Add(positions[i].X);
                    data.Add(positions[i].Y);
                    data.Add(positions[i].Z);
                    if (normals.Count > 0)
                    {
                        data.Add(normals[i].X);
                        data.Add(normals[i].Y);
                        data.Add(normals[i].Z);
                    }
                    if (uv.Count > 0)
                    {
                        data.Add(uv[i].X);
                        data.Add(uv[i].Y);
                    }
                }
                GL.BindVertexArray(sphereVAO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                var arr1 = data.ToArray();
                GL.BufferData<float>(BufferTarget.ArrayBuffer, data.Count * sizeof(float), arr1, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
                var arr2 = indices.ToArray();
                GL.BufferData<int>(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), arr2, BufferUsageHint.StaticDraw);
                int stride = (3 + 2 + 3) * sizeof(float);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, (3 * sizeof(float)));
                GL.EnableVertexAttribArray(2);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, (6 * sizeof(float)));
            }

            GL.BindVertexArray(sphereVAO);
            GL.DrawElements(BeginMode.TriangleStrip, indexCount, DrawElementsType.UnsignedInt, 0);
        }
    }

}

