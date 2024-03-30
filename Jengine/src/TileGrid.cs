using System.Numerics;
using Jengine.util;
using JEngine.util;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace JEngine;

public class TileGrid
{
    public Graphic TileSet;

    private int[] tiles;
    public  int   Width      { get; }
    public  int   Height     { get; }
    public  int   TileWidth  { get; }
    public  int   TileHeight { get; }

    public TileGrid(Graphic tileSet, int width, int height, int tileWidth = -1, int tileHeight = -1) {
        TileSet = tileSet;
        Width = width;
        Height = height;
        tiles = new int[width * height];
        
        TileWidth = tileWidth == -1 ? tileSet.CellWidth : tileWidth;
        TileHeight = tileHeight == -1 ? tileSet.CellHeight : tileHeight;
    }

    public int GetTileAt(int x, int y) {
        return tiles[y * Width + x];
    }
    
    public void SetTileAt(IntVec2 pos, int index) {
        tiles[pos.Y * Width + pos.X] = index;
    }

    public RenderTex CreateTexture(int slice = -1, bool randomiseRotation = false) {
        var isSlice = slice != -1;
        
        var textureHeight = isSlice ? TileSet.CellHeight : Height * TileHeight;
        var texture       = new RenderTex(Width * TileWidth, textureHeight);
        Raylib.BeginTextureMode(texture);
        Raylib.ClearBackground(Colour.Transparent);

        var startY = isSlice ? slice : 0;
        var endY   = isSlice ? slice + 1 : Height;

        for (var y = startY; y < endY; y++) {
            for (var x = 0; x < Width; x++) {
                var pos = new Vector2(x * TileWidth, (Height - y - 1) * TileHeight);
                
                if (isSlice)
                    pos.Y = 0;
                
                var index = GetTileAt(x, y);
                
                var flipX = randomiseRotation && Rand.Bool();
                var flipY = !randomiseRotation || Rand.Bool();

                TileSet.Draw(pos, index: index, flipX: flipX, flipY: flipY, now: true);
            }
        }

        Raylib.EndTextureMode();
        
        // var img = Raylib.LoadImageFromTexture(texture.Texture);
        // Raylib.ExportImage(img, $"test{slice}.png");

        return texture;
    }
    
    public static List<TileGrid> LoadLdtkLevel(string path, int levelIndex) {
        var json = Find.AssetManager.GetJson(path);

        if (json == null)
            return null;
        
        var layers = new List<TileGrid>();

        var level = json["levels"][levelIndex];
        foreach (JObject layer in level["layerInstances"]) {
            var tileSetPath = layer["__tilesetRelPath"].Value<string>();
            var tileSetData = json["defs"]["tilesets"].Where(ts => ts["relPath"].Value<string>() == tileSetPath).First();
            var tileSet     = new Graphic();
            tileSet.SetSpritesheet(tileSetPath.Substring(tileSetPath.IndexOf("textures/") + 9), tileSetData["tileGridSize"].Value<int>(), tileSetData["tileGridSize"].Value<int>());

            var grid = new TileGrid(tileSet, layer["__cWid"].Value<int>(), layer["__cHei"].Value<int>());

            foreach (var tile in layer["autoLayerTiles"]) {
                var pos   = new IntVec2(x: tile["px"][0].Value<int>() / 16, y: tile["px"][1].Value<int>() / 16);
                var index = pos.Y * grid.Width + pos.X;
                grid.tiles[index] = tile["t"].Value<int>();
            }

            layers.Add(grid);
        }
        
        return layers;
    }
}