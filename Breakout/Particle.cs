using OpenTK;

namespace Breakout
{
    // Represents a single particle and its state
    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector4 Color;
        public float Life;

        public Particle()
        {
            Position = new Vector2(0.0f);
            Velocity = new Vector2(0.0f);
            Color = new Vector4(1.0f);
            Life = (0.0f);
        }

    };

}

