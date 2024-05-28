using OpenTK;

namespace Breakout
{
    // PowerUp inherits its state and rendering functions from
    // GameObject but also holds extra information to state its
    // active duration and whether it is activated or not. 
    // The type of PowerUp is stored as a string.
    public class PowerUp :  GameObject 
{
        // The size of a PowerUp block
        readonly Vector2 POWERUP_SIZE = new Vector2(60.0f, 20.0f);
        // Velocity a PowerUp block has when spawned
        readonly Vector2 VELOCITY = new Vector2(0.0f, 150.0f);

        // powerup state
        public string Type;
    public float Duration;
    public bool Activated;
        // constructor
        public PowerUp(string type, Vector3 color, float duration, Vector2 position, Texture2D texture)
            : base(position,texture)            
        {
            Duration = duration;
            Type = type;
            Position = position;
            Color = color;
            Size = POWERUP_SIZE;
            Velocity = VELOCITY;
            
            //position, POWERUP_SIZE, texture, color, VELOCITY), Type(type), Duration(duration), Activated() { }
        }        
};
}

