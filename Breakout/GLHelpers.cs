using System;

namespace Breakout
{
    public static class GLHelpers
    {
        public static DateTime startTime = DateTime.Now;
        public static float glfwGetTime()
        {
            return (float)DateTime.Now.Subtract(startTime).TotalSeconds;
        }
    }
}

