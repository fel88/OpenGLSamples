using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace AsteroidField
{
    public class Model
    {
        // model data 
        public List<Texture> textures_loaded = new List<Texture>();    // stores all the textures loaded so far, optimization to make sure textures aren't loaded more than once.
        public List<Mesh> meshes = new List<Mesh>();
        public Model(string path, bool gamma = false)
        {
            gammasCorrection = gamma;
            loadModel(path);
        }

        private void loadModel(string path)
        {
            ObjLoader obj = new ObjLoader();
            var bb = ResourcesHelper.ReadResource(path);

            using (MemoryStream memoryStream = new MemoryStream(bb))
            using (ZipArchive zipArchive = new ZipArchive(memoryStream))
            {
                foreach (var zipEntry in zipArchive.Entries)
                {
                    if (zipEntry.Name.EndsWith(".png"))
                    {
                        using (var stream = zipEntry.Open())
                        {
                            var bmp = (Bitmap)Image.FromStream(stream);
                            textures_loaded.Add(new Texture() { id = loadTexture(bmp), path = zipEntry.FullName, type = "texture_diffuse" });
                        }
                    }
                }

                foreach (var zipEntry in zipArchive.Entries)
                {
                    if (zipEntry.Name.EndsWith(".obj"))
                    {
                        using (var stream = zipEntry.Open())
                        {
                            var ctx = obj.Load(stream);
                            var vrts = ctx.Vertices.Select(z => new Mesh.Vertex()
                            {
                                Position = z.Position.ToVector3(),
                                Normal = z.Normal.ToVector3(),
                                TexCoords = z.Texture.ToVector2(),
                            });
                            meshes.Add(new Mesh(vrts.ToArray(), ctx.Faces.SelectMany(z => z).ToArray(), textures_loaded.ToArray()));
                        }
                    }
                }

            }


            //todo: obj loader here
            //new Mesh()
        }

        bool gammasCorrection;
        private int loadTexture(Bitmap bitmap)
        {
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            int textureID = GL.GenTexture();
            GL.GenTextures(1, out textureID);

            GL.BindTexture(TextureTarget.Texture2D, textureID);

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
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            return textureID;
        }

        internal void Draw(Shader shader)
        {
            for (int i = 0; i < meshes.Count; i++)
                meshes[i].Draw(shader);

        }
    }
}

