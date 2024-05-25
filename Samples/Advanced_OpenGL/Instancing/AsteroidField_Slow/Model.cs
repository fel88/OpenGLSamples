using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace AsteroidFieldSlow
{
    public class Model
    {

        // model data 
        List<Texture> textures_loaded = new List<Texture>();    // stores all the textures loaded so far, optimization to make sure textures aren't loaded more than once.
        List<Mesh> meshes = new List<Mesh>();
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
                            meshes.Add(new Mesh(vrts.ToArray(), ctx.Faces.SelectMany(z => z).ToArray(), new Texture[0]));
                        }
                    }
                }
            }


            //todo: obj loader here
            //new Mesh()
        }

        bool gammasCorrection;


        internal void Draw(Shader shader)
        {
            for (int i = 0; i < meshes.Count; i++)
                meshes[i].Draw(shader);

        }
    }
}

