using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace Breakout
{
    public class SpriteRenderer : IDisposable
    {

        // Constructor (inits shaders/shapes)
        public SpriteRenderer(Shader shader)
        {
            this.shader = shader;
            this.initRenderData();
        }
        // Destructor
        ~SpriteRenderer()
        {

        }

        // Renders a defined quad textured with given sprite

        public void DrawSprite(Texture2D texture, Vector2 position, Vector2 size, float rotate, Vector3? color=null)
        {
            // prepare transformations
            shader.use();

            var model = Matrix4.Identity;
            model = Matrix4.CreateTranslation(new Vector3(position.X, position.Y, 0)) * model;
            //model = glm::translate(model, glm::vec3(position, 0.0f));  // first translate (transformations are: scale happens first, then rotation, and then final translation happens; reversed order)

            model = Matrix4.CreateTranslation(new Vector3(0.5f * size.X, 0.5f * size.Y, 0)) * model;
            //model = glm::translate(model, glm::vec3(0.5f * size.x, 0.5f * size.y, 0.0f)); // move origin of rotation to center of quad
            model = Matrix4.CreateFromAxisAngle(new Vector3(0.0f, 0.0f, 1), (float)(rotate * Math.PI / 180f)) * model;

            //model = glm::rotate(model, glm::radians(rotate), glm::vec3(0.0f, 0.0f, 1.0f)); // then rotate
            //model = glm::translate(model, glm::vec3(-0.5f * size.x, -0.5f * size.y, 0.0f)); // move origin back
            model = Matrix4.CreateTranslation(new Vector3(-0.5f * size.X, -0.5f * size.Y, 0)) * model;

            //model = glm::scale(model, glm::vec3(size, 1.0f)); // last scale

            model = Matrix4.CreateScale(new Vector3(size.X, size.Y, 1)) * model;

            shader.SetMatrix4("model", model);
            if (color != null)
            {
                // render textured quad
                shader.SetVector3f("spriteColor", color.Value);
            }

            GL.ActiveTexture(TextureUnit.Texture0);
            texture.Bind();

            GL.BindVertexArray(quadVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);
        }

        // Render state
        private Shader shader;
        private int quadVAO;
        // Initializes and configures the quad's buffer and vertex attributes
        private void initRenderData()
        {
            // configure VAO/VBO
            int VBO;
            float[] vertices =  { 
        // pos      // tex
        0.0f, 1.0f, 0.0f, 1.0f,
        1.0f, 0.0f, 1.0f, 0.0f,
        0.0f, 0.0f, 0.0f, 0.0f,

        0.0f, 1.0f, 0.0f, 1.0f,
        1.0f, 1.0f, 1.0f, 1.0f,
        1.0f, 0.0f, 1.0f, 0.0f
    };

            GL.GenVertexArrays(1, out quadVAO);
            GL.GenBuffers(1, out VBO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(quadVAO);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            GL.DeleteVertexArrays(1, new[] { this.quadVAO });
        }

        
    }

}

