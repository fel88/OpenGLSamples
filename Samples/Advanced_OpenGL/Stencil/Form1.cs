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


        uint planeVAO, planeVBO;
        GLControl gl;
        Shader shader;
        Shader shaderSingleColor;
        // cube VAO
        uint cubeVAO, cubeVBO;
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
            shader = new Shader("2.stencil_testing.vs", "2.stencil_testing.fs");
            shaderSingleColor = new Shader("2.stencil_testing.vs", "2.stencil_single_color.fs");

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
        // positions          // texture Coords (note we set these higher than 1 (together with GL_REPEAT as texture wrapping mode). this will cause the floor texture to repeat)
         5.0f, -0.5f,  5.0f,  2.0f, 0.0f,
        -5.0f, -0.5f,  5.0f,  0.0f, 0.0f,
        -5.0f, -0.5f, -5.0f,  0.0f, 2.0f,

         5.0f, -0.5f,  5.0f,  2.0f, 0.0f,
        -5.0f, -0.5f, -5.0f,  0.0f, 2.0f,
         5.0f, -0.5f, -5.0f,  2.0f, 2.0f
    };



            GL.GenVertexArrays(1, out cubeVAO);
            GL.GenBuffers(1, out cubeVBO);
            GL.BindVertexArray(cubeVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, cubeVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * cubeVertices.Length, cubeVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (IntPtr)0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (IntPtr)(3 * sizeof(float)));
            GL.BindVertexArray(0);
            // plane VAO

            GL.GenVertexArrays(1, out planeVAO);
            GL.GenBuffers(1, out planeVBO);
            GL.BindVertexArray(planeVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, planeVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * planeVertices.Length, planeVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (IntPtr)0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (IntPtr)(3 * sizeof(float)));
            GL.BindVertexArray(0);

            // load textures
            // -------------
            try
            {
                cubeTexture = Helpers.loadTexture("marble.jpg");
                floorTexture = Helpers.loadTexture("metal.png");
            }
            catch (Exception ex)
            {

            }

            // shader configuration
            // --------------------
            shader.use();
            shader.SetInt("texture1", 0);
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
            //GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //shader.use();
            //var model = Matrix4.Identity;
            //var view = camera.GetViewMatrix();
            //var projection = Matrix4.CreatePerspectiveFieldOfView((float)Camera.torad(camera.Zoom),
            //    (float)gl.Width / (float)gl.Height,
            //    0.1f, 100.0f);
            //shader.SetMat4("view", view);
            //shader.SetMat4("projection", projection);
            //// cubes
            //GL.BindVertexArray(cubeVAO);
            //GL.ActiveTexture(TextureUnit.Texture0);
            //GL.BindTexture(TextureTarget.Texture2D, cubeTexture);
            ////model = glm::translate(model, glm::vec3(-1.0f, 0.0f, -1.0f));
            //shader.SetMat4("model", model);
            //GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            //model = Matrix4.Identity;
            ////model = glm::translate(model, glm::vec3(2.0f, 0.0f, 0.0f));
            //shader.SetMat4("model", model);
            //GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            //// floor
            //GL.BindVertexArray(planeVAO);
            //GL.BindTexture(TextureTarget.Texture2D, floorTexture);
            //shader.SetMat4("model", Matrix4.Identity);
            //GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            //GL.BindVertexArray(0);

            // render
            // ------
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit); // don't forget to clear the stencil buffer!

            // set uniforms
            shaderSingleColor.use();
            var model = Matrix4.Identity;

            //glm::mat4 model = glm::mat4(1.0f);
            var view = camera.GetViewMatrix();
            var projection = Matrix4.CreatePerspectiveFieldOfView((float)Camera.torad(camera.Zoom), (float)gl.Width / (float)gl.Height, 0.1f, 100.0f);

            shaderSingleColor.SetMat4("view", view);
            shaderSingleColor.SetMat4("projection", projection);

            shader.use();
            shader.SetMat4("view", view);
            shader.SetMat4("projection", projection);

            // draw floor as normal, but don't write the floor to the stencil buffer, we only care about the containers. We set its mask to 0x00 to not write to the stencil buffer.
            GL.StencilMask(0x00);
            // floor
            GL.BindVertexArray(planeVAO);
            GL.BindTexture(TextureTarget.Texture2D, floorTexture);
            shader.SetMat4("model", Matrix4.Identity);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);

            // 1st. render pass, draw objects as normal, writing to the stencil buffer
            // --------------------------------------------------------------------
            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilMask(0xFF);
            // cubes
            GL.BindVertexArray(cubeVAO);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, cubeTexture);
            model = model * Matrix4.CreateTranslation(-1, 0, -1);
            shader.SetMat4("model", model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            model = Matrix4.Identity;
            model = model * Matrix4.CreateTranslation(2, 0, 0);
            shader.SetMat4("model", model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            // 2nd. render pass: now draw slightly scaled versions of the objects, this time disabling stencil writing.
            // Because the stencil buffer is now filled with several 1s. The parts of the buffer that are 1 are not drawn, thus only drawing 
            // the objects' size differences, making it look like borders.
            // -----------------------------------------------------------------------------------------------------------------------------
            GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
            GL.StencilMask(0x00);
            GL.Disable(EnableCap.DepthTest);
            shaderSingleColor.use();
            float scale = 1.1f;
            // cubes
            GL.BindVertexArray(cubeVAO);
            GL.BindTexture(TextureTarget.Texture2D, cubeTexture);
            model = Matrix4.Identity;
            model = Matrix4.CreateTranslation(-1, 0, -1) * model;
            model = Matrix4.CreateScale(scale, scale, scale) * model;

            //model = glm::translate(model, glm::vec3(-1.0f, 0.0f, -1.0f));
            //model = glm::scale(model, glm::vec3(scale, scale, scale));
            shaderSingleColor.SetMat4("model", model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            model = Matrix4.Identity;
            model = Matrix4.CreateTranslation(2, 0, 0) * model;
            model = Matrix4.CreateScale(scale, scale, scale) * model;


            //model = glm::translate(model, glm::vec3(2.0f, 0.0f, 0.0f));
            //model = glm::scale(model, glm::vec3(scale, scale, scale));
            shaderSingleColor.SetMat4("model", model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.BindVertexArray(0);
            GL.StencilMask(0xFF);
            GL.Enable(EnableCap.DepthTest);

            gl.SwapBuffers();
        }
    }

}