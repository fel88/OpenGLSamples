using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Windows.Forms;

namespace Breakout
{
    // Game holds all game-related state and functionality.
    // Combines all game-related data into a single class for
    // easy access to each of the components and manageability.
    public class Game
    {
        SpriteRenderer Renderer;
        GameObject Player;

        // game state
        public GameState State;
        public bool[] Keys = new bool[1024];
        public int Width, Height;
        List<GameLevel> Levels = new List<GameLevel>();
        int Level;
        // Initial velocity of the Ball
        Vector2 INITIAL_BALL_VELOCITY = new Vector2(100.0f, -350.0f);
        // Radius of the ball object
        const float BALL_RADIUS = 12.5f;

        BallObject Ball;

        // constructor/destructor
        public Game(int width, int height)
        {
            Width = width;
            Height = height;
            State = GameState.GAME_ACTIVE;
        }

        // Initial size of the player paddle
        readonly Vector2 PLAYER_SIZE = new Vector2(100.0f, 20.0f);
        // Initial velocity of the player paddle
        const float PLAYER_VELOCITY = (500.0f);
        float clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
        public class Collision
        {
            public Collision(bool b, Direction dir, Vector2 v)
            {
                Fired = b;
                Direction = dir;
                Diff = v;
            }
            public Direction Direction;
            public bool Fired;
            public Vector2 Diff;
        }
        Direction VectorDirection(Vector2 target)
        {
            Vector2[] compass = {
        new Vector2(0.0f, 1.0f),	// up
        new Vector2(1.0f, 0.0f),	// right
        new Vector2(0.0f, -1.0f),	// down
        new Vector2(-1.0f, 0.0f)	// left
    };
            float max = 0.0f;
            int best_match = -1;
            for (int i = 0; i < 4; i++)
            {
                float dot_product = Vector2.Dot(target.Normalized(), compass[i]);
                if (dot_product > max)
                {
                    max = dot_product;
                    best_match = i;
                }
            }
            return (Direction)best_match;
        }
        public enum Direction
        {
            UP,
            RIGHT,
            DOWN,
            LEFT
        }
        Collision CheckCollision(BallObject one, GameObject two) // AABB - Circle collision
        {
            // get center point circle first 
            Vector2 center = one.Position + new Vector2(one.Radius);
            // calculate AABB info (center, half-extents)
            Vector2 aabb_half_extents = new Vector2(two.Size.X / 2.0f, two.Size.Y / 2.0f);
            Vector2 aabb_center = new Vector2(
                two.Position.X + aabb_half_extents.X,
        two.Position.Y + aabb_half_extents.Y
    );
            // get difference vector between both centers
            Vector2 difference = center - aabb_center;
            Vector2 clamped = clamp(difference, -aabb_half_extents, aabb_half_extents);
            // add clamped value to AABB_center and we get the value of box closest to circle
            Vector2 closest = aabb_center + clamped;
            // retrieve vector between center circle and closest point AABB and check if length <= radius
            difference = closest - center;

            if ((difference.Length) < one.Radius)
                return new Collision(true, VectorDirection(difference), difference);
            else
                return new Collision(false, Direction.UP, new Vector2(0.0f, 0.0f));
        }

        private Vector2 clamp(Vector2 v, Vector2 min, Vector2 max)
        {
            return new Vector2(clamp(v.X, min.X, max.X), clamp(v.Y, min.Y, max.Y));
        }

        bool CheckCollision(GameObject one, GameObject two) // AABB - AABB collision
        {
            // collision x-axis?
            bool collisionX = one.Position.X + one.Size.X >= two.Position.X &&
                two.Position.X + two.Size.X >= one.Position.X;
            // collision y-axis?
            bool collisionY = one.Position.Y + one.Size.Y >= two.Position.Y &&
                two.Position.Y + two.Size.Y >= one.Position.Y;
            // collision only if on both axes
            return collisionX && collisionY;
        }
        void DoCollisions()
        {
            foreach (var box in Levels[Level].Bricks)
            {
                if (!box.Destroyed)
                {
                    Collision collision = CheckCollision(Ball, box);
                    if (collision.Fired) // if collision is true
                    {
                        // destroy block if not solid
                        if (!box.IsSolid)
                            box.Destroyed = true;
                        // collision resolution
                        Direction dir = collision.Direction;
                        var diff_vector = (collision).Diff;
                        if (dir == Direction.LEFT || dir == Direction.RIGHT) // horizontal collision
                        {
                            Ball.Velocity.X = -Ball.Velocity.X; // reverse horizontal velocity
                                                                  // relocate
                            float penetration = Ball.Radius - Math.Abs(diff_vector.X);
                            if (dir == Direction.LEFT)
                                Ball.Position.X += penetration; // move ball to right
                            else
                                Ball.Position.X -= penetration; // move ball to left;
                        }
                        else // vertical collision
                        {
                            Ball.Velocity.Y = -Ball.Velocity.Y; // reverse vertical velocity
                                                                  // relocate
                            float penetration = Ball.Radius - Math.Abs(diff_vector.Y);
                            if (dir == Direction.UP )
                                Ball.Position.Y -= penetration; // move ball back up
                            else
                                Ball.Position.Y += penetration; // move ball back down
                        }
                    }
                }
            }
            Collision result = CheckCollision(Ball, Player);
            if (!Ball.Stuck && result.Fired)
            {
                // check where it hit the board, and change velocity based on where it hit the board
                float centerBoard = Player.Position.X + Player.Size.X / 2.0f;
                float distance = (Ball.Position.X + Ball.Radius) - centerBoard;
                float percentage = distance / (Player.Size.X / 2.0f);
                // then move accordingly
                float strength = 2.0f;
                Vector2 oldVelocity = Ball.Velocity;
                Ball.Velocity.X = INITIAL_BALL_VELOCITY.X * percentage * strength;
                Ball.Velocity.Y = -Ball.Velocity.Y;
                Ball.Velocity = (Ball.Velocity.Normalized()) * (oldVelocity.Length);
            }
        }
        // initialize game state (load all shaders/textures/levels)
        public void Init()
        {

            // load shaders
            ResourceManager.LoadShader("sprite.vs", "sprite.frag", null, "sprite");
            // configure shaders
            var projection = Matrix4.CreateOrthographicOffCenter(0, (float)(Width),
                    (float)(Height), 0, -1.0f, 1.0f);
            ResourceManager.GetShader("sprite").use().SetInteger("image", 0);
            ResourceManager.GetShader("sprite").SetMatrix4("projection", projection);
            // set render-specific controls
            Renderer = new SpriteRenderer(ResourceManager.GetShader("sprite"));
            // load textures

            ResourceManager.LoadTexture("background.jpg", false, "background");
            ResourceManager.LoadTexture("awesomeface.png", true, "face");
            ResourceManager.LoadTexture("block.png", false, "block");
            ResourceManager.LoadTexture("block_solid.png", false, "block_solid");
            ResourceManager.LoadTexture("paddle.png", true, "paddle");

            // load levels
            GameLevel one = new GameLevel(); one.Load("one.lvl", Width, Height / 2);
            /*GameLevel two; two.Load("levels/two.lvl", this->Width, this->Height / 2);
            GameLevel three; three.Load("levels/three.lvl", this->Width, this->Height / 2);
            GameLevel four; four.Load("levels/four.lvl", this->Width, this->Height / 2);*/
            Levels.Add(one);
            /*this->Levels.push_back(two);
            this->Levels.push_back(three);
            this->Levels.push_back(four);*/
            Level = 0;

            // configure game objects
            Vector2 playerPos = new Vector2(Width / 2.0f - PLAYER_SIZE.X / 2.0f, Height - PLAYER_SIZE.Y);
            Player = new GameObject(playerPos, PLAYER_SIZE, ResourceManager.GetTexture("paddle"));

            //ball init
            Vector2 ballPos = playerPos + new Vector2(PLAYER_SIZE.X / 2.0f - BALL_RADIUS,
                                            -BALL_RADIUS * 2.0f);
            Ball = new BallObject(ballPos, BALL_RADIUS, INITIAL_BALL_VELOCITY,
                ResourceManager.GetTexture("face"));
        }
        // game loop
        public void ProcessInput(float dt)
        {

            if (State != GameState.GAME_ACTIVE)
                return;

            float velocity = PLAYER_VELOCITY * dt;
            // move playerboard
            if (Keys[(int)System.Windows.Forms.Keys.A])
            {
                if (Player.Position.X >= 0.0f)
                {
                    Player.Position.X -= velocity;
                    if (Ball.Stuck)
                        Ball.Position.X -= velocity;
                }
            }
            if (Keys[(int)System.Windows.Forms.Keys.D])
            {
                if (Player.Position.X <= Width - Player.Size.X)
                {
                    Player.Position.X += velocity;
                    if (Ball.Stuck)
                        Ball.Position.X += velocity;
                }
            }
            if (Keys[(int)System.Windows.Forms.Keys.Space])
            {
                Ball.Stuck = false;
            }


        }
        void ResetLevel()
        {
            if (Level == 0)
                Levels[0].Load("one.lvl", Width, Height / 2);
            else if (Level == 1)
                Levels[1].Load("two.lvl", Width, Height / 2);
            else if (Level == 2)
                Levels[2].Load("three.lvl", Width, Height / 2);
            else if (Level == 3)
                Levels[3].Load("four.lvl", Width, Height / 2);
        }

        void ResetPlayer()
        {
            // reset player/ball stats
            Player.Size = PLAYER_SIZE;
            Player.Position = new Vector2(Width / 2.0f - PLAYER_SIZE.X / 2.0f, Height - PLAYER_SIZE.Y   );
            Ball.Reset(Player.Position + new Vector2(PLAYER_SIZE.X / 2.0f - BALL_RADIUS, -(BALL_RADIUS * 2.0f)), INITIAL_BALL_VELOCITY);
        }
        public void Update(float dt)
        {
            // update objects
            Ball.Move(dt, Width);
            // check for collisions
            DoCollisions();
            if (Ball.Position.Y >= Height) // did ball reach bottom edge?
            {
                ResetLevel();
                ResetPlayer();
            }
        }

        public void Render()
        {
            if (State != GameState.GAME_ACTIVE) return;

            // draw background
            Renderer.DrawSprite(ResourceManager.GetTexture("background"),
                new Vector2(0.0f, 0.0f), new Vector2(Width, Height), 0.0f);
            // draw level
            Levels[Level].Draw(Renderer);
            // draw player
            Player.Draw(Renderer);
            Ball.Draw(Renderer);

        }
    }


}

