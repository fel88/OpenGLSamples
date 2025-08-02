using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK;
using OpenTK.Mathematics;

namespace AsteroidField
{
    public class ObjLoader
    {
        public ObjParseContext Load(Stream stream)
        {
            var rr = new StreamReader(stream).ReadToEnd();
            return Parse(rr);
        }

        public ObjParseContext Load(string path)
        {
            var ln = File.ReadAllLines(path).Where(z => !z.Trim().StartsWith("#")).ToArray();
            return Parse(ln);
        }

        public ObjParseContext Parse(string text)
        {
            var ln = text.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries).ToArray(); ;
            return Parse(ln);
        }

        public ObjParseContext Parse(string[] ln)
        {
            ObjParseContext ret = new ObjParseContext();

            List<SimpleMesh> meshes = new List<SimpleMesh>();
            SimpleMesh mh = new SimpleMesh();
            meshes.Add(mh);

            List<Vector3d> vv = new List<Vector3d>();
            List<Vector3d> vn = new List<Vector3d>();
            List<Vector2d> vt = new List<Vector2d>();
            foreach (var l in ln)
            {
                if (l.StartsWith("o "))
                {
                    if (mh.Triangles.Count == 0)
                    {
                        meshes.Remove(mh);
                    }
                    mh = new SimpleMesh();
                    meshes.Add(mh);
                    //vv = new List<Vector3d>();

                }
                else
                    if (l.StartsWith("v "))
                {
                    var spl = l.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(StaticHelpers.ParseDouble).ToArray();
                    vv.Add(new Vector3d() { X = spl[0], Y = spl[1], Z = spl[2] });
                }
                else
                    if (l.StartsWith("vn "))
                {
                    var spl = l.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(StaticHelpers.ParseDouble).ToArray();
                    vn.Add(new Vector3d() { X = spl[0], Y = spl[1], Z = spl[2] });
                }
                else
                    if (l.StartsWith("vt "))
                {
                    var spl = l.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(StaticHelpers.ParseDouble).ToArray();
                    vt.Add(new Vector2d() { X = spl[0], Y = spl[1] });
                }
                else if (l.StartsWith("f "))
                {
                    TriangleInfo t = new TriangleInfo();
                    if (l.Contains("//"))// vert//normal
                    {

                    }
                    else if (l.Contains("/"))//vert/vtext/normal
                    {
                        var sk = l.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Skip(1);
                        var spl = sk.Select(zz =>
                        {
                            return int.Parse(zz.Split('/')[0]);
                        }).ToArray();

                        var spln = sk.Select(zz =>
                        {
                            return int.Parse(zz.Split('/')[2]);
                        }).ToArray();

                        var splt = sk.Select(zz =>
                        {
                            return int.Parse(zz.Split('/')[1]);
                        }).ToArray();

                        t.Vertices = new VertexInfo[spl.Length];
                        List<int> inds = new List<int>();
                        for (int k = 0; k < spl.Length; k++)
                        {                            
                            t.Vertices[k] = new VertexInfo() { Position = vv[spl[k] - 1], Normal = vn[spln[k] - 1], Texture = vt[splt[k] - 1] };
                            var ind = ret.AddVertex(t.Vertices[k], spl[k] - 1, spln[k] - 1, splt[k] - 1);
                            inds.Add(ind);
                        }
                        ret.Faces.Add(inds.ToArray());
                    }
                    else
                    {
                        var spl = l.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(int.Parse).ToArray();

                        t.Vertices = new VertexInfo[spl.Length];
                        for (int k = 0; k < spl.Length; k++)
                        {
                            int zz = spl[k] - 1;
                            t.Vertices[k] = new VertexInfo() { Position = vv[zz] };
                        }
                    }
                    mh.Triangles.Add(t);
                }
            }
            return ret;
        }
    }
}

