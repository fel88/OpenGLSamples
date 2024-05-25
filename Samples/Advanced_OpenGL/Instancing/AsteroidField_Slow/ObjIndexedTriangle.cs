namespace AsteroidFieldSlow
{
    public class ObjIndexedTriangle
    {
        public int PosIdx;
        public int NormalIdx;
        public int TextureIdx;

        internal bool IsEquals(int posIdx, int normalIdx, int textureIdx)
        {
            return posIdx == PosIdx && normalIdx == NormalIdx && textureIdx == TextureIdx;
        }
    }

}

