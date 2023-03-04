using OpenTK.Graphics.OpenGL;
using System.Reflection;

namespace stencil
{
    public static class Helpers
    {
        public static uint loadTexture(string txt)
        {
            var asm = Assembly.GetAssembly(typeof(Helpers));
            var bmp = ResourceFile.GetBitmap(txt, asm);
            uint textureID;
            GL.GenTextures(1, out textureID);

            GL.BindTexture(TextureTarget.Texture2D, textureID);
            var data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly,
               bmp.PixelFormat);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, bmp.Width, bmp.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, data.Scan0);


            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            const int GL_REPEAT = 0x2901;
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, new int[] { GL_REPEAT });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, new int[] { GL_REPEAT });

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);





            bmp.UnlockBits(data);

            return textureID;
        }
    }
}
