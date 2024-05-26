using OpenTK;
using System;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;

namespace stencil
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            gl = new GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 1, 8));
            KeyPreview = true;
            if (gl.Context.GraphicsMode.Samples == 0)
            {
                gl = new GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 1, 8));
            }

            gl.Dock = DockStyle.Fill;

            gl.MouseMove += Gl_MouseMove;
            gl.BorderStyle = BorderStyle.FixedSingle;
            gl.MouseWheel += Gl_MouseWheel;
            KeyDown += Form1_KeyDown;
            gl.KeyDown += Gl_KeyDown;
            Controls.Add(gl);
            gl.Dock = DockStyle.Fill;
        }

        bool firstMouse = true;
        float lastX;
        float lastY;

        private void Gl_MouseMove(object sender, MouseEventArgs e)
        {
            var xpos = Cursor.Position.X;
            var ypos = Cursor.Position.Y;
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

        private void Gl_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            float delta = 0.1f;
            if (e.KeyCode == Keys.W)
            {
                camera.ProcessKeyboard(Camera.Camera_Movement.FORWARD, delta);
            }
            if (e.KeyCode == Keys.S)
            {
                camera.ProcessKeyboard(Camera.Camera_Movement.BACKWARD, delta);
            }
            if (e.KeyCode == Keys.A)
            {
                camera.ProcessKeyboard(Camera.Camera_Movement.LEFT, delta);
            }
            if (e.KeyCode == Keys.D)
            {
                camera.ProcessKeyboard(Camera.Camera_Movement.RIGHT, delta);
            }
        }

        private void Gl_MouseWheel(object sender, MouseEventArgs e)
        {
            camera.ProcessMouseScroll(e.Delta / 100f);
        }

        int VBO, VAO;        
        GLControl gl;
        Shader shader;
        
        // cube VAO        
        uint cubeTexture;
        uint floorTexture;
        Camera camera = new Camera(new Vector3(0.0f, 0.0f, 3.0f));

        public void init()
        {
            // configure global opengl state
            // -----------------------------
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Enable(EnableCap.StencilTest);
            GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);

            /*
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Always); // always pass the depth test (same effect as glDisable(GL_DEPTH_TEST))
            */


            // build and compile shaders
            // -------------------------
            shader = new Shader("9.1.geometry_shader.vs", "9.1.geometry_shader.fs", "9.1.geometry_shader.gs");

            // set up vertex data (and buffer(s)) and configure vertex attributes
            // ------------------------------------------------------------------
            float []points = {
        -0.5f,  0.5f, 1.0f, 0.0f, 0.0f, // top-left
         0.5f,  0.5f, 0.0f, 1.0f, 0.0f, // top-right
         0.5f, -0.5f, 0.0f, 0.0f, 1.0f, // bottom-right
        -0.5f, -0.5f, 1.0f, 1.0f, 0.0f  // bottom-left
    };
             
            GL.GenBuffers(1, out VBO);
            GL.GenBuffers(1, out VAO);
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float)*points.Length, points, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (2 * sizeof(float)));
            GL.BindVertexArray(0);



        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var deltaTime = 0.010f;
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            

            return false;
        }

        bool first = true;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (first)
            {
                lastX = (float)(gl.Width / 2.0);
                lastY = (float)(gl.Height / 2.0);
                init();
                first = false;
            }
            GL.Viewport(0, 0, gl.Width, gl.Height);



            // render
            // ------
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit); // don't forget to clear the stencil buffer!

            // draw points
            shader.use();
            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType.Points, 0, 4);


            gl.SwapBuffers();
        }
    }

}