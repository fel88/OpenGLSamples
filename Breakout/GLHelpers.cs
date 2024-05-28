using System;

namespace Breakout
{
    public static class GLHelpers
    {
        public static int rand()
        {
            return random.Next();
        }

        static Random random = new Random(Guid.NewGuid().GetHashCode());
        public static DateTime startTime = DateTime.Now;
        public static float glfwGetTime()
        {
            return (float)DateTime.Now.Subtract(startTime).TotalSeconds;
        }
    }
}

