using OpenTK;
using System;

namespace Breakout
{
    // Game holds all game-related state and functionality.
    // Combines all game-related data into a single class for
    // easy access to each of the components and manageability.
    public class Game
    {
        SpriteRenderer Renderer;

        // game state
        public GameState State;
        public bool[] Keys = new bool[1024];
        public int Width, Height;
        // constructor/destructor
        public Game(int width, int height)
        {
            Width = width;
            Height = height;
            State = GameState.GAME_ACTIVE;
        }

        // initialize game state (load all shaders/textures/levels)
        public void Init()
        {

            // load shaders
            ResourceManager.LoadShader("sprite.vs", "sprite.frag", null, "sprite");
            // configure shaders
            var projection = Matrix4.CreateOrthographicOffCenter(0,(float)(Width),
                    (float)(Height), 0,-1.0f, 1.0f);
            ResourceManager.GetShader("sprite").use().SetInteger("image", 0);
            ResourceManager.GetShader("sprite").SetMatrix4("projection", projection);
            // set render-specific controls
            Renderer = new SpriteRenderer(ResourceManager.GetShader("sprite"));
            // load textures
            ResourceManager.LoadTexture("awesomeface.png", true, "face");

        }
        // game loop
        public void ProcessInput(float dt) { }
        public void Update(float dt) { }
        public void Render()
        {
            Renderer.DrawSprite(ResourceManager.GetTexture("face"), new Vector2(200.0f, 200.0f), new Vector2(300.0f, 400.0f), 45.0f, new Vector3(0.0f, 1.0f, 0.0f));
        }
    }


}

