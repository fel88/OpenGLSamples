using OpenTK;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Keys = System.Windows.Forms.Keys;

namespace Breakout
{
    public partial class Form1 : Form
    {



        // timing
        float deltaTime = 0.0f;
        float lastFrame = 0.0f;

        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;


        private const int WM_SYSKEYDOWN = 0x0104; // For system keys (e.g., Alt + key)


        public class GlobalMessageFilter : IMessageFilter
        {

            public bool PreFilterMessage(ref Message m)
            {
                bool release = false;
                bool press = false;

                if (m.Msg == WM_KEYDOWN) // 0x100 is WM_KEYDOWN
                {
                    press = true;
                }
                if (m.Msg == WM_KEYUP)
                {
                    release = true;

                }

                var key = (int)m.WParam;
                if (key >= 0 && key < 1024)
                {
                    if (press)
                        Breakout.Keys[key] = true;
                    else if (release)
                        Breakout.Keys[key] = false;
                }

                return false;
            }
        }




        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var deltaTime = (float)this.deltaTime;
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
            
            Application.AddMessageFilter(new GlobalMessageFilter());
            //glControl = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 4), 3, 3, OpenTK.Graphics.GraphicsContextFlags.Default);
            glControl = new GLControl(new GLControlSettings()
            {
                NumberOfSamples = 8
            });

            glControl.Paint += Gl_Paint;
            Controls.Add(glControl);
            glControl.Dock = DockStyle.Fill;


            Width = SCREEN_WIDTH;
            Height = SCREEN_HEIGHT;
        }



        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {

            var d = camera.Position.Normalized();
            camera.Position -= d * (e.Delta / 120f);
            //camera.ProcessMouseScroll(e.Delta / 120);
        }

        GLControl glControl;


        bool first = true;




        // The Width of the screen
        const int SCREEN_WIDTH = 800;
        // The height of the screen
        const int SCREEN_HEIGHT = 600;

        static Game Breakout = new Game(SCREEN_WIDTH, SCREEN_HEIGHT);
        void init()
        {

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // initialize game
            // ---------------
            Breakout.Init();



            first = false;
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
                Text = "Breakout. FPS: " + Math.Round(lastFps, 2);
                cntr = 0;
            }
            cntr++;
            sw.Restart();
            Redraw();
            glControl.SwapBuffers();
        }

        Camera camera = new Camera(new Vector3(0.0f, 0, 155));
        void Redraw()
        {
            var currentFrame = GLHelpers.glfwGetTime();
            deltaTime = currentFrame - lastFrame;
            lastFrame = currentFrame;

            // manage user input
            // -----------------
            Breakout.ProcessInput(deltaTime);

            // update game state
            // -----------------
            Breakout.Update(deltaTime);


            GL.ClearColor(0.0f, 0.0f, 0.0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // render
            // ------

            Breakout.Render();


        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            glControl.Invalidate();
        }
    }
}

