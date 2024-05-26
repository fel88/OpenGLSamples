using OpenTK;
using System;

namespace stencil
{
    // An abstract camera class that processes input and calculates the corresponding Euler Angles, Vectors and Matrices for use in OpenGL
    public class Camera
    {
        public enum Camera_Movement
        {
            FORWARD,
            BACKWARD,
            LEFT,
            RIGHT
        };

        // Default camera values
        const float YAW = -90.0f;
        const float PITCH = 0.0f;
        const float SPEED = 2.5f;
        const float SENSITIVITY = 0.1f;
        const float ZOOM = 45.0f;

        // Camera Attributes
        public Vector3 Position;
        public Vector3 Front;
        public Vector3 Up;
        public Vector3 Right;
        public Vector3 WorldUp;
        // Euler Angles
        float Yaw;
        float Pitch;
        // Camera options
        float MovementSpeed;
        float MouseSensitivity;
        public float Zoom;

        // Constructor with vectors
        public Camera(Vector3? position = null,
            Vector3? up = null, float yaw = YAW, float pitch = PITCH)

        {
            if (position == null) position = Vector3.Zero;
            if (up == null) up = Vector3.UnitY;
            Front = -Vector3.UnitZ;
            MovementSpeed = SPEED;
            MouseSensitivity = SENSITIVITY;
            Zoom = ZOOM;
            Position = position.Value;
            WorldUp = up.Value;
            Yaw = yaw;
            Pitch = pitch;
            updateCameraVectors();
        }

        // Constructor with scalar values
        public Camera(float posX, float posY, float posZ, float upX, float upY, float upZ, float yaw, float pitch)
        {
            Front = new Vector3(0.0f, 0.0f, -1.0f);
            MovementSpeed = SPEED;
            MouseSensitivity = SENSITIVITY;
            Zoom = ZOOM;
            Position = new Vector3(posX, posY, posZ);
            WorldUp = new Vector3(upX, upY, upZ);
            Yaw = yaw;
            Pitch = pitch;
            updateCameraVectors();
        }

        // Returns the view matrix calculated using Euler Angles and the LookAt Matrix
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }

        // Processes input received from any keyboard-like input system. Accepts input parameter in the form of camera defined ENUM (to abstract it from windowing systems)
        public void ProcessKeyboard(Camera_Movement direction, float deltaTime)
        {
            float velocity = MovementSpeed * deltaTime;
            if (direction == Camera_Movement.FORWARD)
                Position += Front * velocity;
            if (direction == Camera_Movement.BACKWARD)
                Position -= Front * velocity;
            if (direction == Camera_Movement.LEFT)
                Position -= Right * velocity;
            if (direction == Camera_Movement.RIGHT)
                Position += Right * velocity;
        }

        // Processes input received from a mouse input system. Expects the offset value in both the x and y direction.
        public void ProcessMouseMovement(float xoffset, float yoffset, bool constrainPitch = true)
        {
            xoffset *= MouseSensitivity;
            yoffset *= MouseSensitivity;

            Yaw += xoffset;
            Pitch += yoffset;

            // Make sure that when pitch is out of bounds, screen doesn't get flipped
            if (constrainPitch)
            {
                if (Pitch > 89.0f)
                    Pitch = 89.0f;
                if (Pitch < -89.0f)
                    Pitch = -89.0f;
            }

            // Update Front, Right and Up Vectors using the updated Euler angles
            updateCameraVectors();
        }

        // Processes input received from a mouse scroll-wheel event. Only requires input on the vertical wheel-axis
        public void ProcessMouseScroll(float yoffset)
        {
            if (Zoom >= 1.0f && Zoom <= 45.0f)
                Zoom -= yoffset;
            if (Zoom <= 1.0f)
                Zoom = 1.0f;
            if (Zoom >= 45.0f)
                Zoom = 45.0f;
        }

        public static double torad(double f)
        {
            return f * Math.PI / 180;
        }

        void updateCameraVectors()
        {
            // Calculate the new Front vector
            Vector3 front = new Vector3();

            front.X = (float)(Math.Cos(torad(Yaw)) * Math.Cos(torad(Pitch)));
            front.Y = (float)Math.Sin(torad(Pitch));
            front.Z = (float)(Math.Sin(torad(Yaw)) * Math.Cos(torad(Pitch)));
            Front = front.Normalized();
            // Also re-calculate the Right and Up vector
            Right = Vector3.Normalize(Vector3.Cross(Front, WorldUp));  // Normalize the vectors, because their length gets closer to 0 the more you look up or down which results in slower movement.
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
    };

}