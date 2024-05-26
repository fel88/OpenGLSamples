using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Breakout
{
    public static class ResourceManager
    {
        static Dictionary<string, Shader> Shaders = new Dictionary<string, Shader>();
        static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

        // loads (and generates) a shader program from file loading vertex, fragment (and geometry) shader's source code. If gShaderFile is not nullptr, it also loads a geometry shader
        public static Shader LoadShader(string vShaderFile, string fShaderFile, string gShaderFile, string name)
        {
            Shader ret = new Shader(vShaderFile, fShaderFile);
            Shaders.Add(name, ret);
            return ret;
        }

        // retrieves a stored sader
        public static Shader GetShader(string name)
        {
            return Shaders[name];

        }

        // loads (and generates) a texture from file
        public static Texture2D LoadTexture(string file, bool alpha, string name)
        {

            Textures[name] = loadTextureFromFile(file, alpha);
            return Textures[name];

           
        }

       
        // retrieves a stored texture
        public static Texture2D GetTexture(string name)
        {
            return Textures[name];
        }
        // properly de-allocates all loaded resources
        static void Clear() { }

        // private constructor, that is we do not want any actual resource manager objects. Its members and functions should be publicly available (static).

        // loads and generates a shader from file
        private static Shader loadShaderFromFile(string vShaderFile, string fShaderFile, string gShaderFile = null)
        {
            return null;
        }
        // loads a single texture from file
        private static Texture2D loadTextureFromFile(string file, bool alpha)
        {
            Texture2D texture = new Texture2D();
            if (alpha)
            {
                texture.Internal_Format = PixelInternalFormat.Rgba;
                texture.Image_Format = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
            }

            var tt = texture.Load(ResourcesHelper.ReadResourceBmp(file));
            //texture.Generate(width, height, data);
            return texture;
        }


    }


}

