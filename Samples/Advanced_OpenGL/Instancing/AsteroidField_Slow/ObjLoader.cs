using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK;

namespace AsteroidFieldSlow
{
    public class ObjLoader
    {
        public SimpleMesh[] Load(Stream stream)
        {
            var rr = new StreamReader(stream).ReadToEnd();
            return Parse(rr);
        }

        public SimpleMesh[] Load(string path)
        {
            var ln = File.ReadAllLines(path).Where(z => !z.Trim().StartsWith("#")).ToArray();
            return Parse(ln);
        }

        public SimpleMesh[] Parse(string text)
        {
            var ln = text.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries).ToArray(); ;
            return Parse(ln);
        }

        public SimpleMesh[] Parse(string[] ln)
        {
            List<SimpleMesh> ret = new List<SimpleMesh>();
            SimpleMesh mh = new SimpleMesh();
            ret.Add(mh);

            List<Vector3d> vv = new List<Vector3d>();
            List<Vector3d> vn = new List<Vector3d>();
            foreach (var l in ln)
            {
                if (l.StartsWith("o "))
                {
                    if (mh.Triangles.Count == 0)
                    {
                        ret.Remove(mh);
                    }
                    mh = new SimpleMesh();
                    ret.Add(mh);
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
                else if (l.StartsWith("f "))
                {
                    TriangleInfo t = new TriangleInfo();
                    if (l.Contains("//"))// vert//normal
                    {

                    }
                    else if (l.Contains("/"))//vert/vtext/normal
                    {

                        var spl = l.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(zz =>
                        {
                            return int.Parse(zz.Split('/')[0]);
                        }).ToArray();
                        var spln = l.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(zz =>
                        {
                            return int.Parse(zz.Split('/')[2]);
                        }).ToArray();
                        t.Vertices = new VertexInfo[spl.Length];
                        for (int k = 0; k < spl.Length; k++)
                        {
                            int zz = spl[k] - 1;
                            t.Vertices[k] = new VertexInfo() { Position = vv[zz], Normal = vn[spln[k] - 1] };
                        }
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
            return ret.ToArray();
        }
    }
}

