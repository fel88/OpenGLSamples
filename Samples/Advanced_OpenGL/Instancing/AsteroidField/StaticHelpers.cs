using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Mathematics;

namespace AsteroidField
{
    public static class StaticHelpers
    {
        public static double ParseDouble(this string z)
        {
            return double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture);
        }

        public static float ParseFloat(this string z)
        {
            return float.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture);
        }

        public static Vector3d ToVector3d(this Vector3 v)
        {
            return new Vector3d(v.X, v.Y, v.Z);
        }

        public static Vector2d ToVector2d(this PointF v)
        {
            return new Vector2d(v.X, v.Y);
        }

        public static Vector3 ToVector3(this Vector3d v)
        {
            return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
        }

        public static Vector2 ToVector2(this Vector2d v)
        {
            return new Vector2((float)v.X, (float)v.Y);
        }

        public static double DistTo(this PointF p, PointF p2)
        {
            return Math.Sqrt(Math.Pow(p.X - p2.X, 2) + Math.Pow(p.Y - p2.Y, 2));
        }

        public static bool ShowQuestion(string v, string title)
        {
            return MessageBox.Show(v, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}

