using OpenTK;
using System.Drawing;

namespace Breakout
{
    // Container object for holding all state relevant for a single
    // game object entity. Each object in the game likely needs the
    // minimal of state as described within GameObject.
    public class GameObject
    {

        // object state
        public Vector2 Position, Size, Velocity;
        public Vector3 Color;
        public float Rotation;
        public bool IsSolid;
        public bool Destroyed;
        // render state
        Texture2D Sprite;
        


        // constructor(s)
        public GameObject()
        {
            Position = new Vector2(0.0f, 0.0f);
            Size = new Vector2(1.0f, 1.0f);
            Velocity = new Vector2(0.0f);
            Color = new Vector3(1.0f);
            Rotation = (0.0f);
            Sprite = new Texture2D();
            IsSolid = (false);
            Destroyed = (false);

        }

        public GameObject(Vector2 playerPos, Vector2 pLAYER_SIZE, Texture2D texture2D)
        {
            Position = playerPos;
            Size = pLAYER_SIZE;
            Sprite = texture2D;
            Color = new Vector3(1);
            Velocity = new Vector2(0, 0);
        }

        public GameObject(Vector2 playerPos, Vector2 pLAYER_SIZE, Texture2D texture2D, Vector3 color) : this(playerPos, pLAYER_SIZE, texture2D)
        {
            Color = color;
        }

        public GameObject(Vector2 pos, Vector2 size, Texture2D sprite, Vector3 color, Vector2 velocity)
        {
            Position = pos;
            Size = (size);
            Velocity = (velocity);
            Color = (color);
            Rotation = (0.0f);
            Sprite = (sprite);
            IsSolid = (false);
            Destroyed = (false);
        }

        // draw sprite
        public virtual void Draw(SpriteRenderer renderer)
        {
            renderer.DrawSprite(Sprite, Position, Size, Rotation, Color);
        }
    }
}

