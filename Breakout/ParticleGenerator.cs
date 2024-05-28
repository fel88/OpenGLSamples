using OpenTK;
using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Breakout
{
    // ParticleGenerator acts as a container for rendering a large number of 
    // particles by repeatedly spawning and updating particles and killing 
    // them after a given amount of time.
    public class ParticleGenerator
    {

        // constructor
        public ParticleGenerator(Shader shader, Texture2D texture, int amount)
        {
            this.shader = shader;
            this.texture = texture;
            this.amount = amount;

            init();

        }
        // update all particles
        public void Update(float dt, GameObject gobject, int newParticles, Vector2? offset = null)
        {
            if (offset == null)
                offset = new Vector2(0, 0);

            // add new particles 
            for (int i = 0; i < newParticles; ++i)
            {
                int unusedParticle = firstUnusedParticle();
                respawnParticle(particles[unusedParticle], gobject, offset);
            }
            // update all particles
            for (int i = 0; i < amount; ++i)
            {
                Particle p = particles[i];
                p.Life -= dt; // reduce life
                if (p.Life > 0.0f)
                {   // particle is alive, thus update
                    p.Position -= p.Velocity * dt;
                    p.Color.W -= dt * 2.5f;
                }
            }

        }
        // render all particles
        public void Draw()
        {
            // use additive blending to give it a 'glow' effect
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
            shader.use();
            foreach (var particle in particles)
            {
                if (particle.Life > 0.0f)
                {
                    shader.SetVector2f("offset", particle.Position);
                    shader.SetVector4f("color", particle.Color);
                    texture.Bind();
                    GL.BindVertexArray(VAO);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                    GL.BindVertexArray(0);
                }
            }
            // don't forget to reset to default blending mode
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        // state
        private List<Particle> particles = new List<Particle>();
        int amount;
        // render state
        Shader shader;
        Texture2D texture;
        int VAO;
        // initializes buffer and vertex attributes
        void init()
        {
            // set up mesh and attribute properties
            int VBO;
            float[] particle_quad = {
        0.0f, 1.0f, 0.0f, 1.0f,
        1.0f, 0.0f, 1.0f, 0.0f,
        0.0f, 0.0f, 0.0f, 0.0f,

        0.0f, 1.0f, 0.0f, 1.0f,
        1.0f, 1.0f, 1.0f, 1.0f,
        1.0f, 0.0f, 1.0f, 0.0f
    };
            GL.GenVertexArrays(1, out VAO);
            GL.GenBuffers(1, out VBO);
            GL.BindVertexArray(VAO);
            // fill mesh buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * particle_quad.Length, particle_quad, BufferUsageHint.StaticDraw);
            // set mesh attributes
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.BindVertexArray(0);

            // create this->amount default particle instances
            for (int i = 0; i < amount; ++i)
                particles.Add(new Particle());

        }
        // stores the index of the last particle used (for quick access to next dead particle)
        int lastUsedParticle = 0;
        // returns the first Particle index that's currently unused e.g. Life <= 0.0f or 0 if no particle is currently inactive
        int firstUnusedParticle()
        {
            // first search from last used particle, this will usually return almost instantly
            for (int i = lastUsedParticle; i < amount; ++i)
            {
                if (particles[i].Life <= 0.0f)
                {
                    lastUsedParticle = i;
                    return i;
                }
            }
            // otherwise, do a linear search
            for (int i = 0; i < lastUsedParticle; ++i)
            {
                if (particles[i].Life <= 0.0f)
                {
                    lastUsedParticle = i;
                    return i;
                }
            }
            // all particles are taken, override the first one (note that if it repeatedly hits this case, more particles should be reserved)
            lastUsedParticle = 0;
            return 0;
        }
        
        private int rand()
        {
            return GLHelpers.rand();
        }
        // respawns particle
        void respawnParticle(Particle particle, GameObject _object, Vector2? offset = null)
        {
            if (offset == null)
                offset = new Vector2(0, 0);

            float random = ((rand() % 100) - 50) / 10.0f;
            float rColor = 0.5f + ((rand() % 100) / 100.0f);
            particle.Position = _object.Position + new Vector2(random) + offset.Value;
            particle.Color = new Vector4(rColor, rColor, rColor, 1.0f);
            particle.Life = 1.0f;
            particle.Velocity = _object.Velocity * 0.1f;
        }
    }

}

