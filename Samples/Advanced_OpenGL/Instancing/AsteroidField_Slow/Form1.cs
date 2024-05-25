using System;
using System.Diagnostics;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace AsteroidFieldSlow
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
            var deltaTime = (float)this.deltaTime;
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
            if (keyData == Keys.Q)
            {
                camera.ProcessKeyboard(Camera_Movement.TOP, deltaTime);
                return true;
            }
            if (keyData == Keys.E)
            {
                camera.ProcessKeyboard(Camera_Movement.DOWN, deltaTime);
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

            /*if (firstMouse)
            {
                lastX = xpos;
                lastY = ypos;
                firstMouse = false;
            }*/

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
            var d = camera.Position.Normalized();
            camera.Position -= d *( e.Delta / 120f);
            //camera.ProcessMouseScroll(e.Delta / 120);
        }


        GLControl glControl;

        Shader shader;

        bool first = true;
        const int SCR_WIDTH = 800;
        const int SCR_HEIGHT = 600;

        Matrix4[] modelMatrices;
        Model rock, planet;
        Random random;
        void init()
        {
            rock = new Model("rock.zip");
            planet = new Model("planet.zip");

            shader = new Shader("10.2.instancing.vs", "10.2.instancing.fs");
            // generate a large list of semi-random model transformation matrices
            // ------------------------------------------------------------------

            modelMatrices = new Matrix4[amount];
            random = new Random(Guid.NewGuid().GetHashCode());

            float radius = 50.0f;
            float offset = 2.5f;
            for (int i = 0; i < amount; i++)
            {
                var model = Matrix4.Identity;
                //glm::mat4 model = glm::mat4(1.0f);
                // 1. translation: displace along circle with 'radius' in range [-offset, offset]
                float angle = (float)i / (float)amount * 360.0f;
                float displacement = (rand() % (int)(2 * offset * 100)) / 100.0f - offset;
                float x = sin(angle) * radius + displacement;
                displacement = (rand() % (int)(2 * offset * 100)) / 100.0f - offset;
                float y = displacement * 0.4f; // keep height of asteroid field smaller compared to width of x and z
                displacement = (rand() % (int)(2 * offset * 100)) / 100.0f - offset;
                float z = cos(angle) * radius + displacement;
                var tr = Matrix4.CreateTranslation(x, y, z);
                model = tr * model;

                // 2. scale: Scale between 0.05 and 0.25f
                float scale = (float)((rand() % 20) / 100.0 + 0.05);
                var sc = Matrix4.CreateScale(scale);
                model = sc * model;

                // 3. rotation: add random rotation around a (semi)randomly picked rotation axis vector
                float rotAngle = (float)((rand() % 360));
                var rt = Matrix4.CreateFromAxisAngle(new Vector3(0.4f, 0.6f, 0.8f), rotAngle);
                model = rt * model;

                // 4. now add to list of matrices
                modelMatrices[i] = model;
            }



            first = false;
        }

        private float cos(float angle)
        {
            return (float)Math.Cos(angle);
        }

        private float sin(float angle)
        {
            return (float)Math.Sin(angle);
        }

        private int rand()
        {
            return random.Next();
        }

        double lastFps = 0;
        Stopwatch sw = new Stopwatch();
        int cntr = 0;
        private void Gl_Paint(object sender, PaintEventArgs e)
        {
            if (!glControl.Context.IsCurrent)
                glControl.MakeCurrent();

            GL.Viewport(0, 0, glControl.Width, glControl.Height);

            if (first)
                init();
            sw.Stop();
            if (sw.ElapsedMilliseconds > 0)
            {
                var newFps = 1000f / sw.ElapsedMilliseconds;
                lastFps = lastFps * 0.9 + newFps * 0.1;
            }
            if (cntr > 10)
            {
                Text = "FPS: " + Math.Round(lastFps, 2);
                cntr = 0;
            }
            cntr++;
            sw.Restart();
            Redraw();
            glControl.SwapBuffers();
        }


        int amount = 1000;

        Camera camera = new Camera(new Vector3(0.0f, 3, 55.0f));

        // timing
        double deltaTime = 0.0f;
        double lastFrame = 0.0f;
        DateTime startTime = new DateTime();
        double glfwGetTime()
        {
            return DateTime.Now.Subtract(startTime).TotalSeconds;
        }
        void Redraw()
        {
            GL.Enable(EnableCap.DepthTest);
            var currentFrame = glfwGetTime();
            deltaTime = currentFrame - lastFrame;
            lastFrame = currentFrame;

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // configure transformation matrices
            //glm::mat4 projection = glm::perspective(glm::radians(45.0f), (float)SCR_WIDTH / (float)SCR_HEIGHT, 0.1f, 1000.0f);
            //var projection = Matrix4.CreatePerspectiveFieldOfView((float)Camera.radians(camera.Zoom), (float)glControl.Width / (float)glControl.Height, 0.1f, 1000.0f);
            var projection = Matrix4.CreatePerspectiveFieldOfView((float)(45*Math.PI/180), (float)glControl.Width / (float)glControl.Height, 0.1f, 1000.0f);

            //glm::mat4 view = camera.GetViewMatrix(); ;
            var view = camera.GetViewMatrix();
            shader.use();
            shader.setMat4("model", Matrix4.Identity);

            shader.setMat4("projection", projection);
            shader.setMat4("view", view);

            // draw planet
            var model = Matrix4.Identity;

            var tr = Matrix4.CreateTranslation(0.0f, -3.0f, 0.0f);
            model = tr * model;
            var sc = Matrix4.CreateScale(4.0f, 4.0f, 4.0f);
            model = sc * model;
            shader.setMat4("model", model);
            planet.Draw(shader);

            // draw meteorites
            for (int i = 0; i < amount; i++)
            {
                shader.setMat4("model", modelMatrices[i]);
                rock.Draw(shader);
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            glControl.Invalidate();
        }
    }
}

