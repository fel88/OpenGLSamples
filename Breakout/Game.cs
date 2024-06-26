﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;

namespace Breakout
{
    // Game holds all game-related state and functionality.
    // Combines all game-related data into a single class for
    // easy access to each of the components and manageability.
    public class Game
    {

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

        // Game-related State data
        SpriteRenderer Renderer;
        GameObject Player;
        BallObject Ball;
        ParticleGenerator Particles;
        PostProcessor Effects;
        List<PowerUp> PowerUps = new List<PowerUp>();
        float ShakeTime = 0.0f;


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
                        {
                            box.Destroyed = true;
                            SpawnPowerUps(box);
                        }
                        else
                        {
                            // if block is solid, enable shake effect
                            ShakeTime = 0.05f;
                            Effects.Shake = true;
                        }
                        // collision resolution
                        Direction dir = collision.Direction;
                        var diff_vector = (collision).Diff;

                        if (!(Ball.PassThrough && !box.IsSolid)) // don't do collision resolution on non-solid bricks if pass-through is activated
                        {
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
                                if (dir == Direction.UP)
                                    Ball.Position.Y -= penetration; // move ball back up
                                else
                                    Ball.Position.Y += penetration; // move ball back down
                            }
                        }
                    }
                }
            }

            // also check collisions on PowerUps and if so, activate them
            foreach (var powerUp in PowerUps)
            {
                if (!powerUp.Destroyed)
                {
                    // first check if powerup passed bottom edge, if so: keep as inactive and destroy
                    if (powerUp.Position.Y >= Height)
                        powerUp.Destroyed = true;

                    if (CheckCollision(Player, powerUp))
                    {   // collided with player, now activate powerup
                        ActivatePowerUp(powerUp);
                        powerUp.Destroyed = true;
                        powerUp.Activated = true;
                    }
                }
            }

            // check collisions for player pad (unless stuck)
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
                Ball.Velocity = (Ball.Velocity.Normalized()) * (oldVelocity.Length); // keep speed consistent over both axes (multiply by length of old velocity, so total strength is not changed)                                                                                            // fix sticky paddle
                Ball.Velocity.Y = -1.0f * Math.Abs(Ball.Velocity.Y);
                //Ball.Velocity.Y = -Ball.Velocity.Y;
                //Ball.Velocity = (Ball.Velocity.Normalized()) * (oldVelocity.Length);
            }
        }
        void ActivatePowerUp(PowerUp powerUp)
        {
            if (powerUp.Type == "speed")
            {
                Ball.Velocity *= 1.2f;
            }
            else if (powerUp.Type == "sticky")
            {
                Ball.Sticky = true;
                Player.Color = new Vector3(1.0f, 0.5f, 1.0f);
            }
            else if (powerUp.Type == "pass-through")
            {
                Ball.PassThrough = true;
                Ball.Color = new Vector3(1.0f, 0.5f, 0.5f);
            }
            else if (powerUp.Type == "pad-size-increase")
            {
                Player.Size.X += 50;
            }
            else if (powerUp.Type == "confuse")
            {
                if (!Effects.Chaos)
                    Effects.Confuse = true; // only activate if chaos wasn't already active
            }
            else if (powerUp.Type == "chaos")
            {
                if (!Effects.Confuse)
                    Effects.Chaos = true;
            }
        }


        // initialize game state (load all shaders/textures/levels)
        public void Init()
        {
            // load shaders
            ResourceManager.LoadShader("sprite.vs", "sprite.frag", null, "sprite");
            ResourceManager.LoadShader("particle.vs", "particle.frag", null, "particle");
            ResourceManager.LoadShader("post_processing.vs", "post_processing.frag", null, "postprocessing");
            // configure shaders
            var projection = Matrix4.CreateOrthographicOffCenter(0, (float)(Width),
                    (float)(Height), 0, -1.0f, 1.0f);

            ResourceManager.GetShader("sprite").use().SetInteger("image", 0);
            ResourceManager.GetShader("sprite").SetMatrix4("projection", projection);
            ResourceManager.GetShader("particle").use().SetInteger("sprite", 0);
            ResourceManager.GetShader("particle").SetMatrix4("projection", projection);


            // load textures

            ResourceManager.LoadTexture("background.jpg", false, "background");
            ResourceManager.LoadTexture("awesomeface.png", true, "face");
            ResourceManager.LoadTexture("block.png", false, "block");
            ResourceManager.LoadTexture("block_solid.png", false, "block_solid");
            ResourceManager.LoadTexture("paddle.png", true, "paddle");
            ResourceManager.LoadTexture("particle.png", true, "particle");

            ResourceManager.LoadTexture("powerup_speed.png", true, "powerup_speed");
            ResourceManager.LoadTexture("powerup_sticky.png", true, "powerup_sticky");
            ResourceManager.LoadTexture("powerup_increase.png", true, "powerup_increase");
            ResourceManager.LoadTexture("powerup_confuse.png", true, "powerup_confuse");
            ResourceManager.LoadTexture("powerup_chaos.png", true, "powerup_chaos");
            ResourceManager.LoadTexture("powerup_passthrough.png", true, "powerup_passthrough");
            // set render-specific controls

            // set render-specific controls
            Renderer = new SpriteRenderer(ResourceManager.GetShader("sprite"));
            Particles = new ParticleGenerator(ResourceManager.GetShader("particle"), ResourceManager.GetTexture("particle"), 500);
            Effects = new PostProcessor(ResourceManager.GetShader("postprocessing"), Width, Height);
            // load levels
            GameLevel one = new GameLevel(); one.Load("one.lvl", Width, Height / 2);
            GameLevel two = new GameLevel(); two.Load("two.lvl", Width, Height / 2);
            GameLevel three = new GameLevel(); three.Load("three.lvl", Width, Height / 2);
            GameLevel four = new GameLevel(); four.Load("four.lvl", Width, Height / 2);
            Levels.Add(one);
            Levels.Add(two);
            Levels.Add(three);
            Levels.Add(four);
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
            Player.Position = new Vector2(Width / 2.0f - PLAYER_SIZE.X / 2.0f, Height - PLAYER_SIZE.Y);
            Ball.Reset(Player.Position + new Vector2(PLAYER_SIZE.X / 2.0f - BALL_RADIUS, -(BALL_RADIUS * 2.0f)), INITIAL_BALL_VELOCITY);
            // also disable all active powerups
            Effects.Chaos = Effects.Confuse = false;
            Ball.PassThrough = Ball.Sticky = false;
            Player.Color = new Vector3(1.0f);
            Ball.Color = new Vector3(1.0f);
        }

        public void Update(float dt)
        {
            // update objects
            Ball.Move(dt, Width);
            // check for collisions
            DoCollisions();
            // update particles
            Particles.Update(dt, Ball, 2, new Vector2(Ball.Radius / 2.0f));
            // update PowerUps
            UpdatePowerUps(dt);

            if (ShakeTime > 0.0f)
            {
                ShakeTime -= dt;
                if (ShakeTime <= 0.0f)
                    Effects.Shake = false;
            }

            // check loss condition            
            if (Ball.Position.Y >= Height) // did ball reach bottom edge?
            {
                ResetLevel();
                ResetPlayer();
            }

        }


        void UpdatePowerUps(float dt)
        {
            foreach (var powerUp in PowerUps)
            {
                powerUp.Position += powerUp.Velocity * dt;
                if (powerUp.Activated)
                {
                    powerUp.Duration -= dt;

                    if (powerUp.Duration <= 0.0f)
                    {
                        // remove powerup from list (will later be removed)
                        powerUp.Activated = false;
                        // deactivate effects
                        if (powerUp.Type == "sticky")
                        {
                            if (!IsOtherPowerUpActive(PowerUps, "sticky"))
                            {   // only reset if no other PowerUp of type sticky is active
                                Ball.Sticky = false;
                                Player.Color = new Vector3(1.0f);
                            }
                        }
                        else if (powerUp.Type == "pass-through")
                        {
                            if (!IsOtherPowerUpActive(PowerUps, "pass-through"))
                            {   // only reset if no other PowerUp of type pass-through is active
                                Ball.PassThrough = false;
                                Ball.Color = new Vector3(1.0f);
                            }
                        }
                        else if (powerUp.Type == "confuse")
                        {
                            if (!IsOtherPowerUpActive(PowerUps, "confuse"))
                            {   // only reset if no other PowerUp of type confuse is active
                                Effects.Confuse = false;
                            }
                        }
                        else if (powerUp.Type == "chaos")
                        {
                            if (!IsOtherPowerUpActive(PowerUps, "chaos"))
                            {   // only reset if no other PowerUp of type chaos is active
                                Effects.Chaos = false;
                            }
                        }
                    }
                }
            }
            // Remove all PowerUps from vector that are destroyed AND !activated (thus either off the map or finished)
            // Note we use a lambda expression to remove each PowerUp which is destroyed and not activated
            PowerUps.RemoveAll(z => z.Destroyed && !z.Activated);
        }

        private bool IsOtherPowerUpActive(List<PowerUp> powerUps, string type)
        {
            // Check if another PowerUp of the same type is still active
            // in which case we don't disable its effect (yet)
            foreach (var powerUp in powerUps)
            {
                if (powerUp.Activated)
                    if (powerUp.Type == type)
                        return true;
            }
            return false;
        }

        bool ShouldSpawn(int chance)
        {
            int random = GLHelpers.rand() % chance;
            return random == 0;
        }
        void SpawnPowerUps(GameObject block)
        {
            if (ShouldSpawn(75)) // 1 in 75 chance
                PowerUps.Add(new PowerUp("speed", new Vector3(0.5f, 0.5f, 1.0f), 0.0f, block.Position, ResourceManager.GetTexture("powerup_speed")));
            if (ShouldSpawn(75))
                PowerUps.Add(new PowerUp("sticky", new Vector3(1.0f, 0.5f, 1.0f), 20.0f, block.Position, ResourceManager.GetTexture("powerup_sticky")));
            if (ShouldSpawn(75))
                PowerUps.Add(new PowerUp("pass-through", new Vector3(0.5f, 1.0f, 0.5f), 10.0f, block.Position, ResourceManager.GetTexture("powerup_passthrough")));
            if (ShouldSpawn(75))
                PowerUps.Add(new PowerUp("pad-size-increase", new Vector3(1.0f, 0.6f, 0.4f), 0.0f, block.Position, ResourceManager.GetTexture("powerup_increase")));
            if (ShouldSpawn(15)) // Negative powerups should spawn more often
                PowerUps.Add(new PowerUp("confuse", new Vector3(1.0f, 0.3f, 0.3f), 15.0f, block.Position, ResourceManager.GetTexture("powerup_confuse")));
            if (ShouldSpawn(15))
                PowerUps.Add(new PowerUp("chaos", new Vector3(0.9f, 0.25f, 0.25f), 15.0f, block.Position, ResourceManager.GetTexture("powerup_chaos")));
        }

        public void Render()
        {
            if (State != GameState.GAME_ACTIVE)
                return;

            Effects.BeginRender();
            // draw background
            Renderer.DrawSprite(ResourceManager.GetTexture("background"),
                new Vector2(0.0f, 0.0f), new Vector2(Width, Height), 0.0f);
            // draw level
            Levels[Level].Draw(Renderer);
            // draw player
            Player.Draw(Renderer);
            // draw PowerUps
            foreach (var powerUp in PowerUps)
                if (!powerUp.Destroyed)
                    powerUp.Draw(Renderer);

            // draw particles	
            Particles.Draw();
            // draw ball
            Ball.Draw(Renderer);

            Effects.EndRender();
            Effects.Render(GLHelpers.glfwGetTime());
        }
    }
}

