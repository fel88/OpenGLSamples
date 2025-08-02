using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using OpenTK;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

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
            //glControl = new GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 4), 3, 3, OpenTK.Graphics.GraphicsContextFlags.ForwardCompatible);
            glControl = new GLControl();


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
            /*shader.use();
            shader.setMat4("projection", projection);*/
        }



        GLControl glControl;
        Shader lightingShader;
        Shader lightCubeShader;
        bool first = true;
        const int SCR_WIDTH = 800;
        const int SCR_HEIGHT = 600;
        Vector3 lightPos = new Vector3(1.2f, 1.0f, 2.0f);
        int SHADOW_WIDTH = 1024, SHADOW_HEIGHT = 1024;
        int lightCubeVAO;
        int VBO, cubeVAO;

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
                lightingShader = new Shader("3.1.materials.vs", "3.1.materials.fs");
                lightCubeShader = new Shader("3.1.light_cube.vs", "3.1.light_cube.fs");

                GL.Viewport(0, 0, SCR_WIDTH, SCR_HEIGHT);

                // set up vertex data (and buffer(s)) and configure vertex attributes
                // ------------------------------------------------------------------
                float[] vertices = {
         -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
         0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
         0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
         0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
        -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,

        -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
         0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
         0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
         0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
        -0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,

        -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,
        -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
        -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
        -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
        -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f,
        -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,

         0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,
         0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
         0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
         0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
         0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,
         0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,

        -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,
         0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,
         0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
         0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,

        -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,
         0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,
         0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
         0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
        -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
        -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f
    };

                // first, configure the cube's VAO (and VBO)                

                GL.GenVertexArrays(1, out cubeVAO);
                GL.GenBuffers(1, out VBO);


                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);


                GL.BindVertexArray(cubeVAO);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (3 * sizeof(float)));
                GL.EnableVertexAttribArray(1);

                // second, configure the light's VAO (VBO stays the same; the vertices are the same for the light object which is also a 3D cube)

                GL.GenVertexArrays(1, out lightCubeVAO);
                GL.BindVertexArray(lightCubeVAO);

                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                // note that we update the lamp's position attribute's stride to reflect the updated buffer data
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);



                first = false;



            }
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

        static DateTime startTime = DateTime.Now;
        static float glfwGetTime() => (float)DateTime.Now.Subtract(startTime).TotalSeconds;

        Camera camera = new Camera(new Vector3(0.0f, 0.0f, 3.0f));
        void Redraw()
        {
            GL.Enable(EnableCap.DepthTest);

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // be sure to activate shader when setting uniforms/drawing objects
            lightingShader.use();
            lightingShader.setVec3("light.position", lightPos);
            lightingShader.setVec3("viewPos", camera.Position);

            // light properties
            Vector3 lightColor = new Vector3();
            lightColor.X = (float)Math.Sin(glfwGetTime() * 2.0);
            lightColor.Y = (float)Math.Sin(glfwGetTime() * 0.7);
            lightColor.Z = (float)Math.Sin(glfwGetTime() * 1.3);
            Vector3 diffuseColor = lightColor * new Vector3(0.5f); // decrease the influence
            Vector3 ambientColor = diffuseColor * new Vector3(0.2f); // low influence
            lightingShader.setVec3("light.ambient", ambientColor);
            lightingShader.setVec3("light.diffuse", diffuseColor);
            lightingShader.setVec3("light.specular", 1.0f, 1.0f, 1.0f);

            // material properties
            lightingShader.setVec3("material.ambient", 1.0f, 0.5f, 0.31f);
            lightingShader.setVec3("material.diffuse", 1.0f, 0.5f, 0.31f);
            lightingShader.setVec3("material.specular", 0.5f, 0.5f, 0.5f); // specular lighting doesn't have full effect on this object's material
            lightingShader.setFloat("material.shininess", 32.0f);


            // view/projection transformations
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)((camera.Zoom) * Math.PI / 180f), (float)SCR_WIDTH / (float)SCR_HEIGHT, 0.1f, 100.0f);
            Matrix4 view = camera.GetViewMatrix();
            lightingShader.setMat4("projection", projection);
            lightingShader.setMat4("view", view);

            // world transformation
            Matrix4 model = Matrix4.Identity;
            lightingShader.setMat4("model", model);

            // render the cube
            GL.BindVertexArray(cubeVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);


            // also draw the lamp object(s)
            lightCubeShader.use();
            lightCubeShader.setMat4("projection", projection);
            lightCubeShader.setMat4("view", view);

            model = Matrix4.Identity;

            model = Matrix4.CreateTranslation(lightPos);
            model = Matrix4.CreateScale(0.2f) * model; // a smaller cube
            lightCubeShader.setMat4("model", model);

            // we now draw as many light bulbs as we have point lights.
            GL.BindVertexArray(lightCubeVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }

        float timeTemp = 0;


        private void timer1_Tick(object sender, EventArgs e)
        {
            glControl.Invalidate();
        }
    }
}

