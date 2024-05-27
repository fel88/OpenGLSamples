using OpenTK;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Breakout
{
    /// GameLevel holds all Tiles as part of a Breakout level and 
    /// hosts functionality to Load/render levels from the harddisk.
    public class GameLevel
    {

        // level state
        public List<GameObject> Bricks = new List<GameObject>();
        // constructor
        public GameLevel() { }
        // loads level from file
        public void Load(string file, int levelWidth, int levelHeight)
        {
            // clear old data
            Bricks.Clear();
            // load from file

            string line;
            var lns = ResourcesHelper.ReadResourceTxt(file).Split(new char[] { '\r', '\n', '\t' }, System.StringSplitOptions.RemoveEmptyEntries).Select(z => z.Trim()).Where(z => z.Length > 0).ToArray();
            List<int[]> tileData = new List<int[]>();
            foreach (var item in lns)
            {
                var row = item.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                tileData.Add(row.ToArray());
            }

            if (tileData.Count > 0)
                init(tileData.ToArray(), levelWidth, levelHeight);

        }
        // render level
        public void Draw(SpriteRenderer renderer)
        {
            foreach (var tile in Bricks)
                if (!tile.Destroyed)
                    tile.Draw(renderer);
        }
        // check if the level is completed (all non-solid tiles are destroyed)
        public bool IsCompleted()
        {
            foreach (var tile in Bricks)
                if (!tile.IsSolid && !tile.Destroyed)
                    return false;
            return true;
        }

        // initialize level from tile data
        private void init(int[][] tileData, int levelWidth, int levelHeight)
        {
            // calculate dimensions
            int height = tileData.Length;
            int width = tileData[0].Length; // note we can index vector at [0] since this function is only called if height > 0
            float unit_width = levelWidth / (float)(width), unit_height = levelHeight / height;
            // initialize level tiles based on tileData		
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    // check block type from level data (2D level array)
                    if (tileData[y][x] == 1) // solid
                    {
                        Vector2 pos = new Vector2(unit_width * x, unit_height * y);
                        Vector2 size = new Vector2(unit_width, unit_height);
                        GameObject obj = new GameObject(pos, size, ResourceManager.GetTexture("block_solid"), new OpenTK.Vector3(0.8f, 0.8f, 0.7f));
                        obj.IsSolid = true;
                        Bricks.Add(obj);
                    }
                    else if (tileData[y][x] > 1)    // non-solid; now determine its color based on level data
                    {
                        Vector3 color = new Vector3(1.0f); // original: white
                        if (tileData[y][x] == 2)
                            color = new Vector3(0.2f, 0.6f, 1.0f);
                        else if (tileData[y][x] == 3)
                            color = new Vector3(0.0f, 0.7f, 0.0f);
                        else if (tileData[y][x] == 4)
                            color = new Vector3(0.8f, 0.8f, 0.4f);
                        else if (tileData[y][x] == 5)
                            color = new Vector3(1.0f, 0.5f, 0.0f);

                        Vector2 pos = new Vector2(unit_width * x, unit_height * y);
                        Vector2 size = new Vector2(unit_width, unit_height);
                        Bricks.Add(new GameObject(pos, size, ResourceManager.GetTexture("block"), color));
                    }
                }
            }
        }
    };
}

