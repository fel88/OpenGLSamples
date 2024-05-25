using System;
using OpenTK;

namespace AsteroidFieldSlow
{
    public class Camera
    {
        // camera Attributes
        public Vector3 Position;
        Vector3 Front;
        Vector3 Up;
        Vector3 Right;
        Vector3 WorldUp;
        // euler Angles
        float Yaw;
        float Pitch;
        // camera options
        float MovementSpeed;
        float MouseSensitivity;
        public float Zoom;

        const float YAW = -90.0f;
        const float PITCH = 0.0f;
        const float SPEED = 2.5f;
        const float SENSITIVITY = 0.1f;
        const float ZOOM = 45.0f;
        public Camera(Vector3 position)
        {
            Vector3 up = new Vector3(0, 1, 0);
            Position = position;
            WorldUp = up;
            Yaw = YAW;
            Zoom = ZOOM;
            Pitch = PITCH;
            MovementSpeed = SPEED;
            MouseSensitivity = SENSITIVITY;

            updateCameraVectors();
        }
        // calculates the front vector from the Camera's (updated) Euler Angles
        void updateCameraVectors()
        {
            // calculate the new Front vector
            Vector3 front;
            front.X = (float)(Math.Cos(radians(Yaw)) * Math.Cos(radians(Pitch)));
            front.Y = (float)Math.Sin(radians(Pitch));
            front.Z = (float)Math.Sin(radians(Yaw)) * (float)Math.Cos(radians(Pitch));

            Front = front.Normalized();
            // also re-calculate the Right and Up vector
            Right = (Vector3.Cross(Front, WorldUp)).Normalized();  // normalize the vectors, because their length gets closer to 0 the more you look up or down which results in slower movement.
            Up = (Vector3.Cross(Right, Front)).Normalized();
        } // returns the view matrix calculated using Euler Angles and the LookAt Matrix

        public static double radians(float yaw)
        {
            return yaw * Math.PI / 180f;
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);

        }
        // processes input received from a mouse scroll-wheel event. Only requires input on the vertical wheel-axis
        public void ProcessMouseScroll(float yoffset)
        {
            Zoom -= (float)yoffset;
            if (Zoom < 1.0f)
                Zoom = 1.0f;
            if (Zoom > 120)
                Zoom = 120;
        }   
        // processes input received from any keyboard-like input system. Accepts input parameter in the form of camera defined ENUM (to abstract it from windowing systems)
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

        // processes input received from a mouse input system. Expects the offset value in both the x and y direction.
        public void ProcessMouseMovement(float xoffset, float yoffset, bool constrainPitch = true)
        {
            xoffset *= MouseSensitivity;
            yoffset *= MouseSensitivity;

            Yaw += xoffset;
            Pitch += yoffset;

            // make sure that when pitch is out of bounds, screen doesn't get flipped
            if (constrainPitch)
            {
                if (Pitch > 89.0f)
                    Pitch = 89.0f;
                if (Pitch < -89.0f)
                    Pitch = -89.0f;
            }

            // update Front, Right and Up Vectors using the updated Euler angles
            updateCameraVectors();
        }

    }
}

