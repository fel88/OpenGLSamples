using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing.Imaging;
using System.Drawing;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace Breakout
{
    public class Texture2D
    {
        // holds the ID of the texture object, used for all texture operations to reference to this particular texture
        public int ID;
        // texture image dimensions
        public int Width, Height; // width and height of loaded image in pixels
                                  // texture Format
        public PixelInternalFormat Internal_Format; // format of texture object
        public PixelFormat Image_Format; // format of loaded image
                                         // texture configuration
        public TextureWrapMode Wrap_S; // wrapping mode on S axis
        public TextureWrapMode Wrap_T; // wrapping mode on T axis
        public TextureMinFilter Filter_Min; // filtering mode if texture pixels < screen pixels
        public TextureMagFilter Filter_Max; // filtering mode if texture pixels > screen pixels
                               // constructor (sets default texture modes)
        public Texture2D()
        {
            Width = (0);
            Height = (0);

            Internal_Format = PixelInternalFormat.Rgb;
            Image_Format = PixelFormat.Rgb;
            Wrap_S = TextureWrapMode.Repeat;
            Wrap_T = TextureWrapMode.Repeat;
            Filter_Min = TextureMinFilter.Linear;
            Filter_Max = TextureMagFilter.Linear;            
            GL.GenTextures(1, out ID);
        }

        // generates texture from image data
        public void Generate(int width, int height, byte[] data)
        {
            /*Width = width;
            Height = height;
            // create Texture
            GL.BindTexture(TextureTarget.Texture2D, ID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, Internal_Format, width, height, 0, Image_Format, PixelType.UnsignedByte, data);

            // set Texture wrap and filter modes           

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)Filter_Min);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)Filter_Max);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)Wrap_S);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)Wrap_T);

            // unbind texture
            GL.BindTexture(TextureTarget.Texture2D, 0);*/
        }
        public int Load(Bitmap bitmap)
        {
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            Width = bitmap.Width;
            Height = bitmap.Height;
            //int textureID = GL.GenTexture();
            GL.GenTextures(1, out ID);

            GL.BindTexture(TextureTarget.Texture2D, ID);

            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var format = PixelInternalFormat.Rgba;
            var format2 = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;

            var ps = Image.GetPixelFormatSize(bitmap.PixelFormat);
            if (ps == 24)
            {
                format = PixelInternalFormat.Rgb;
                format2 = OpenTK.Graphics.OpenGL.PixelFormat.Bgr;

            }
            if (ps == 8)
            {
                format = PixelInternalFormat.R8;
                format2 = OpenTK.Graphics.OpenGL.PixelFormat.Red;
            }
            GL.TexImage2D(TextureTarget.Texture2D, 0, format, data.Width, data.Height, 0,
                format2, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            /*GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);*/
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)Filter_Min);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)Filter_Max);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)Wrap_S);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)Wrap_T);
            return ID;
        }

        // binds the texture as the current active GL_TEXTURE_2D texture object            
        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, ID);
        }
    }
}