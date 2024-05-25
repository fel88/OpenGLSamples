using System;
using System.Collections.Generic;
using System.Linq;

namespace AsteroidField
{
    public class ObjParseContext
    {
        public List<VertexInfo> Vertices = new List<VertexInfo>();
        public List<ObjIndexedTriangle> Triangles = new List<ObjIndexedTriangle>();
        public List<int[]> Faces = new List<int[]>();

        internal int AddVertex(VertexInfo vinf, int posIdx, int normalIdx, int textureIdx)
        {
            for (int i = 0; i < Triangles.Count; i++)
            {
                if (Triangles[i].IsEquals(posIdx, normalIdx, textureIdx))
                {
                    return i;
                }
            }

            Vertices.Add(vinf);
            Triangles.Add(new ObjIndexedTriangle() { PosIdx = posIdx, NormalIdx = normalIdx, TextureIdx = textureIdx });
            return Triangles.Count - 1;
        }
    }

}

