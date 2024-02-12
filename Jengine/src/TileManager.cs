using System.Numerics;
using JEngine.util;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace JEngine;

public class TileManager
{
    class TileGrid {
        public GraphicData TileSet;

        private int[] tiles;
        private int   width;
        private int   height;

        public int[] Tiles => tiles;
        public int   Width => width;
        public int   Height => height;

        public TileGrid(GraphicData tileSet, int width, int height) {
            TileSet = tileSet;
            this.width = width;
            this.height = height;
            tiles = new int[width * height];
        }

        public int TileIndexAt(int x, int y) {
            return tiles[y * width + x];
        }
    }

    private List<TileGrid> layers = new();
    private List<Texture2D> layerTextures = new();

    public void LoadLDTKLevel(string path, int levelIndex) {
        layers.Clear();
        layerTextures.Clear();

        var json = Find.AssetManager.GetJson(path);

        var level = json["levels"][levelIndex];
        foreach (JObject layer in level["layerInstances"]) {
            var tileSetPath = layer["__tilesetRelPath"].Value<string>();
            var tileSetData = json["defs"]["tilesets"].Where(ts => ts["relPath"].Value<string>() == tileSetPath).First();
            var tileSet = new GraphicData();
            tileSet.SetSpritesheet(tileSetPath.Substring(tileSetPath.IndexOf("textures/") + 9), tileSetData["tileGridSize"].Value<int>(), tileSetData["tileGridSize"].Value<int>());

            var grid = new TileGrid(tileSet, layer["__cWid"].Value<int>(), layer["__cHei"].Value<int>());

            foreach (var tile in layer["autoLayerTiles"]) {
                var pos = new IntVec2(x: tile["px"][0].Value<int>() / 16, y: tile["px"][1].Value<int>() / 16);
                var index = pos.Y * grid.Width + pos.X;
                grid.Tiles[index] = tile["t"].Value<int>();
            }

            layers.Add(grid);
            layerTextures.Add(CreateTexture(grid));
        }
    }

    private Texture2D CreateTexture(TileGrid grid) {
        var texture = Raylib.LoadRenderTexture(grid.Width * grid.TileSet.CellWidth, grid.Height * grid.TileSet.CellHeight);
        Raylib.BeginTextureMode(texture);
        Raylib.ClearBackground(Colour.Transparent);

        for (var y = 0; y < grid.Height; y++) {
            for (var x = 0; x < grid.Width; x++) {
                var pos   = new Vector2(x * grid.TileSet.CellWidth, (grid.Height - y - 1) * grid.TileSet.CellHeight);
                var index = grid.TileIndexAt(x, y);

                grid.TileSet.Draw(pos, index: index, flipY: true, now: true);
            }
        }

        Raylib.EndTextureMode();

        // var img = Raylib.LoadImageFromTexture(texture.Texture);
        // Raylib.ExportImage(img, "test.png");

        return texture.Texture;
    }

    public void Render() {
        for (var i = 0; i < layers.Count; i++) {
            Raylib.DrawTexture(layerTextures[i], 0, 0, Colour.White);
        }
    }
}