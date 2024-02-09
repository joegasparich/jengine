using System.Numerics;
using Raylib_cs;

namespace JEngine.util; 

// Most of these functions are just copied from here but with a zPos parameter added
// https://github.com/raysan5/raylib/blob/e5d332dea23e65f66e7e7b279dc712afeb9404c9/src/rshapes.c
public static class Draw {
    // Draw a line  (Vector version)
    public static void DrawLineV3D(Vector2 startPos, Vector2 endPos, Color color, float zPos)
    {
        Rlgl.CheckRenderBatchLimit(10);
        Rlgl.Begin(DrawMode.Lines);
        Rlgl.Color4ub(color.R, color.G, color.B, color.A);
        Rlgl.Vertex3f(startPos.X, startPos.Y, zPos);
        Rlgl.Vertex3f(endPos.X,   endPos.Y,   zPos);
        Rlgl.End();
    }
    
    // Draw lines sequence
    public static void DrawLineStrip3D(Vector2[] points, Color color, float zPos)
    {
        if (points.Length >= 2)
        {
            Rlgl.CheckRenderBatchLimit(points.Length);
            Rlgl.Begin(DrawMode.Lines);
            Rlgl.Color4ub(color.R, color.G, color.B, color.A);

            for (var i = 0; i < points.Length - 1; i++)
            {
                Rlgl.Vertex3f(points[i].X, points[i].Y, zPos);
                Rlgl.Vertex3f(points[i + 1].X, points[i + 1].Y, zPos);
            }
            Rlgl.End();
        }
    }

    // Draw a color-filled rectangle with pro parameters
    public static void DrawRectanglePro3D(Rectangle rec, Vector2 origin, float rotation, Color color, float zPos)
    {
        Vector2 topLeft;
        Vector2 topRight;
        Vector2 bottomLeft;
        Vector2 bottomRight;

        // Only calculate rotation if needed
        if (rotation == 0.0f)
        {
            var x = rec.X - origin.X;
            var y = rec.Y - origin.Y;
            topLeft = new Vector2( x, y );
            topRight = new Vector2( x + rec.Width, y );
            bottomLeft = new Vector2( x, y + rec.Height );
            bottomRight = new Vector2( x + rec.Width, y + rec.Height );
        }
        else
        {
            var sinRotation = MathF.Sin(JMath.DegToRad(rotation));
            var cosRotation = MathF.Cos(JMath.DegToRad(rotation));
            var x = rec.X;
            var y = rec.Y;
            var dx = -origin.X;
            var dy = -origin.Y;

            topLeft.X = x + dx*cosRotation - dy*sinRotation;
            topLeft.Y = y + dx*sinRotation + dy*cosRotation;

            topRight.X = x + (dx + rec.Width)*cosRotation - dy*sinRotation;
            topRight.Y = y + (dx + rec.Width)*sinRotation + dy*cosRotation;

            bottomLeft.X = x + dx*cosRotation - (dy + rec.Height)*sinRotation;
            bottomLeft.Y = y + dx*sinRotation + (dy + rec.Height)*cosRotation;

            bottomRight.X = x + (dx + rec.Width)*cosRotation - (dy + rec.Height)*sinRotation;
            bottomRight.Y = y + (dx + rec.Width)*sinRotation + (dy + rec.Height)*cosRotation;
        }

        Rlgl.CheckRenderBatchLimit(10);
        Rlgl.Begin(DrawMode.Triangles);

        Rlgl.Color4ub(color.R, color.G, color.B, color.A);

        Rlgl.Vertex3f(topLeft.X, topLeft.Y, zPos);
        Rlgl.Vertex3f(bottomLeft.X, bottomLeft.Y, zPos);
        Rlgl.Vertex3f(topRight.X, topRight.Y, zPos);

        Rlgl.Vertex3f(topRight.X, topRight.Y, zPos);
        Rlgl.Vertex3f(bottomLeft.X, bottomLeft.Y, zPos);
        Rlgl.Vertex3f(bottomRight.X, bottomRight.Y, zPos);

        Rlgl.End();
    }

    // Draw a color-filled rectangle (Vector version)
    // NOTE: On OpenGL 3.3 and ES2 we use QUADS to avoid drawing order issues
    public static void DrawRectangleV3D(Vector2 position, Vector2 size, Color color, float zPos) {
        DrawRectanglePro3D(new Rectangle(position.X, position.Y, size.X, size.Y), new Vector2(0.0f, 0.0f), 0.0f, color, zPos);
    }

    // Draw a triangle fan defined by points
    // NOTE: First vertex provided is the center, shared by all triangles
    // By default, following vertex should be provided in counter-clockwise order
    public static void DrawTriangleFan3D(Vector2[] points, Color color, float zPos)
    {
        if (points.Length >= 3)
        {
            Rlgl.CheckRenderBatchLimit((points.Length - 2)*4);
            
            Rlgl.Begin(DrawMode.Quads);
            Rlgl.Color4ub(color.R, color.G, color.B, color.A);

            for (var i = 1; i < points.Length - 1; i++)
            {
                Rlgl.Vertex3f(points[0].X, points[0].Y, zPos);
                Rlgl.Vertex3f(points[i].X, points[i].Y, zPos);
                Rlgl.Vertex3f(points[i + 1].X, points[i + 1].Y, zPos);
                Rlgl.Vertex3f(points[i + 1].X, points[i + 1].Y, zPos);
            }
            Rlgl.End();
        }
    }
    
    public static void DrawTexturePro3D(
        Texture2D texture,
        Rectangle sourceRect,
        Rectangle destRect,
        Vector3 origin,
        float rotation,
        float posZ,
        Color tint
    ) {
        // Check if texture if valid
        if (texture.Id <= 0) 
            return;

        var flipX = false;

        if (sourceRect.Width < 0) {
            flipX = true; 
            sourceRect.Width *= -1;
        }
        if (sourceRect.Height < 0) 
            sourceRect.Y -= sourceRect.Height;
        
        Rlgl.CheckRenderBatchLimit(4);
        
        Rlgl.SetTexture(texture.Id);
        Rlgl.PushMatrix();
        Rlgl.Translatef(destRect.X, destRect.Y, 0);
        Rlgl.Rotatef(rotation, 0.0f, 0.0f, 1.0f);
        Rlgl.Translatef(-origin.X, -origin.Y, -origin.Z);
        
        Rlgl.Begin(DrawMode.Quads);
        Rlgl.Color4ub(tint.R, tint.G, tint.B, tint.A);
        Rlgl.Normal3f(0.0f, 0.0f, 1.0f); // Normal vector pointing towards viewer
        
        // Bottom-left corner for texture and quad
        if (flipX) 
            Rlgl.TexCoord2f((sourceRect.X + sourceRect.Width) / texture.Width, sourceRect.Y / texture.Height);
        else 
            Rlgl.TexCoord2f(sourceRect.X / texture.Width, sourceRect.Y / texture.Height);
        Rlgl.Vertex3f(0.0f, 0.0f, posZ);

        // Bottom-right corner for texture and quad
        if (flipX) 
            Rlgl.TexCoord2f((sourceRect.X + sourceRect.Width) / texture.Width, (sourceRect.Y + sourceRect.Height) / texture.Height);
        else 
            Rlgl.TexCoord2f(sourceRect.X / texture.Width, (sourceRect.Y + sourceRect.Height) / texture.Height);
        Rlgl.Vertex3f(0.0f, destRect.Height, posZ);

        // Top-right corner for texture and quad
        if (flipX) 
            Rlgl.TexCoord2f(sourceRect.X / texture.Width, (sourceRect.Y + sourceRect.Height) / texture.Height);
        else 
            Rlgl.TexCoord2f((sourceRect.X + sourceRect.Width) / texture.Width, (sourceRect.Y + sourceRect.Height) / texture.Height);
        Rlgl.Vertex3f(destRect.Width, destRect.Height, posZ);

        // Top-left corner for texture and quad
        if (flipX) 
            Rlgl.TexCoord2f(sourceRect.X / texture.Width, sourceRect.Y / texture.Height);
        else 
            Rlgl.TexCoord2f((sourceRect.X + sourceRect.Width) / texture.Width, sourceRect.Y / texture.Height);
        Rlgl.Vertex3f(destRect.Width, 0.0f, posZ);
        
        Rlgl.End();
        Rlgl.PopMatrix();
        Rlgl.SetTexture(0);
    }

    // Get average of list of points
    public static Vector2 Average(this List<Vector2> points)
    {
        var average = new Vector2(0, 0);
        for (var i = 0; i < points.Count; i++)
        {
            average.X += points[i].X;
            average.Y += points[i].Y;
        }
        average.X /= points.Count;
        average.Y /= points.Count;
        return average;
    }
}