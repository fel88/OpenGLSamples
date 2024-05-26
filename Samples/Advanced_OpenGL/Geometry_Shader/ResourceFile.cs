using System.Drawing;
using System.Linq;
using System.Reflection;
using System.IO;

namespace stencil
{
    public static class ResourceFile
    {
        public static byte[] GetBinaryFile(string name, Assembly assembly = null)
        {

            if (assembly == null)
            {
                assembly = Assembly.GetEntryAssembly();
            }


            var nms = assembly.GetManifestResourceNames();

            var nfr = nms.First(z => z.ToLower().Contains(name.ToLower()));

            name = nfr;
            byte[] ret = null;
            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                var len = stream.Length;
                ret = new byte[len];
                stream.Read(ret, 0, (int)len);
            }

            return ret;
        }

        public static string GetFileText(string name, Assembly assembly = null)
        {

            if (assembly == null)
            {
                assembly = Assembly.GetEntryAssembly();
            }


            var nms = assembly.GetManifestResourceNames();
            string ret = "";
            var nfr = nms.First(z => z.ToLower().Contains(name.ToLower()));

            name = nfr;
            using (Stream stream = assembly.GetManifestResourceStream(name))
            using (StreamReader reader = new StreamReader(stream))
            {
                ret = reader.ReadToEnd();
            }
            return ret;
        }

        public static Bitmap GetBitmap(string name, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetEntryAssembly();
            }


            var nms = assembly.GetManifestResourceNames();

            var nfr = nms.First(z => z.ToLower().Contains(name.ToLower()));

            name = nfr;
            Bitmap ret = null;
            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                ret = Bitmap.FromStream(stream) as Bitmap;
            }
            return ret;
        }
    }
}
