using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
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
        }
        // game loop
        public void ProcessInput(float dt)
        {

            if (State == GameState.GAME_ACTIVE)
            {
                float velocity = PLAYER_VELOCITY * dt;
                // move playerboard
                if (Keys[(int)System.Windows.Forms.Keys.A])
                {
                    if (Player.Position.X >= 0.0f)
                        Player.Position.X -= velocity;
                }
                if (Keys[(int)System.Windows.Forms.Keys.D])
                {
                    if (Player.Position.X <= Width - Player.Size.X)
                        Player.Position.X += velocity;
                }
            }

        }
        public void Update(float dt) { }

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

        }
    }


}

