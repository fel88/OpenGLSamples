using OpenTK;

namespace Breakout
{
    // BallObject holds the state of the Ball object inheriting
    // relevant state data from GameObject. Contains some extra
    // functionality specific to Breakout's ball object that
    // were too specific for within GameObject alone.
    public class BallObject : GameObject
    {

        // ball state	
        public float Radius;
        public bool Stuck;
        // constructor(s)
        public BallObject()
        {
            Radius = (12.5f); Stuck = (true);
        }
        public BallObject(Vector2 pos, float radius, Vector2 velocity, Texture2D sprite) : base(pos, new Vector2(radius * 2.0f, radius * 2.0f), sprite, new Vector3(1.0f), velocity)

        {
            Radius = (radius);
            Stuck = (true);
        }

        // moves the ball, keeping it constrained within the window bounds (except bottom edge); returns new position
        public Vector2 Move(float dt, int window_width)
        {
            // if not stuck to player board
            if (!Stuck)
            {
                // move the ball
                Position += Velocity * dt;
                // check if outside window bounds; if so, reverse velocity and restore at correct position
                if (Position.X <= 0.0f)
                {
                    Velocity.X = -Velocity.X;
                    Position.X = 0.0f;
                }
                else if (Position.X + Size.X >= window_width)
                {
                    Velocity.X = -Velocity.X;
                    Position.X = window_width - Size.X;
                }
                if (Position.Y <= 0.0f)
                {
                    Velocity.Y = -Velocity.Y;
                    Position.Y = 0.0f;
                }

            }
            return Position;
        }
        // resets the ball to original state with given position and velocity
        public void Reset(Vector2 position, Vector2 velocity)
        {
            Position = position;
            Velocity = velocity;
            Stuck = true;
        }
    }


}

