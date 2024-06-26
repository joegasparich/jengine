using System.Numerics;
using Jengine.util;
using Raylib_cs;

namespace JEngine.util; 

// Most of these functions are just copied from here but with a zPos parameter added
// https://github.com/raysan5/raylib/blob/e5d332dea23e65f66e7e7b279dc712afeb9404c9/src/rshapes.c
public static class Drawing {
    private static Texture2D texShapes = new() {
        Id = 1,
        Width = 1,
        Height = 1,
        Mipmaps = 1,
        Format = PixelFormat.UncompressedR8G8B8A8
    }; // Texture used on shapes drawing (white pixel loaded by rlgl)
    private static Rectangle texShapesRect = new(0, 0, 1, 1); // Texture rectangle to draw shapes

    // Draw a line  (Vector version)
    public static void DrawLine(Vector2 startPos, Vector2 endPos, Color color, float zPos)
    {
        Rlgl.CheckRenderBatchLimit(10);
        Rlgl.Begin(DrawMode.Lines);
        Rlgl.Color4ub(color.R, color.G, color.B, color.A);
        Rlgl.Vertex3f(startPos.X, startPos.Y, zPos);
        Rlgl.Vertex3f(endPos.X,   endPos.Y,   zPos);
        Rlgl.End();
    }
    
    // Draw lines sequence
    public static void DrawLineStrip(Vector2[] points, Color color, float zPos)
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

    public static void DrawRectangle(Vector2 position, Vector2 size, Color color, float zPos) {
        DrawRectangle(new Rectangle(position.X, position.Y, size.X, size.Y), new Vector2(0.0f, 0.0f), 0.0f, color, zPos);
    }

    public static void DrawRectangle(Rectangle rec, Vector2 origin, float rotation, Color color, float zPos)
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
            var sinRotation = MathF.Sin(JMath.DegToRad * rotation);
            var cosRotation = MathF.Cos(JMath.DegToRad * rotation);
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

        Rlgl.SetTexture(texShapes.Id);

        Rlgl.Begin(DrawMode.Quads);

        Rlgl.Normal3f(0.0f, 0.0f, 1.0f);
        Rlgl.Color4ub(color.R, color.G, color.B, color.A);

        Rlgl.TexCoord2f(texShapesRect.X/texShapes.Width, texShapesRect.Y/texShapes.Height);
        Rlgl.Vertex3f(topLeft.X, topLeft.Y, zPos);

        Rlgl.TexCoord2f(texShapesRect.X/texShapes.Width, (texShapesRect.Y + texShapesRect.Height)/texShapes.Height);
        Rlgl.Vertex3f(bottomLeft.X, bottomLeft.Y, zPos);

        Rlgl.TexCoord2f((texShapesRect.X + texShapesRect.Width)/texShapes.Width, (texShapesRect.Y + texShapesRect.Height)/texShapes.Height);
        Rlgl.Vertex3f(bottomRight.X, bottomRight.Y, zPos);

        Rlgl.TexCoord2f((texShapesRect.X + texShapesRect.Width)/texShapes.Width, texShapesRect.Y/texShapes.Height);
        Rlgl.Vertex3f(topRight.X, topRight.Y, zPos);

        Rlgl.End();

        Rlgl.SetTexture(0);
    }

    public static void DrawRectangleOutline(Vector2 position, Vector2 size, Color color, float zPos) {
        DrawRectangleOutline(new Rectangle(position.X, position.Y, size.X, size.Y), color, zPos);
    }

    public static void DrawRectangleOutline(Rectangle rectangle, Color color, float zPos) {
        DrawLine(new Vector2(rectangle.X, rectangle.Y), new Vector2(rectangle.X + rectangle.Width, rectangle.Y), color, zPos);
        DrawLine(new Vector2(rectangle.X + rectangle.Width, rectangle.Y), new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height), color, zPos);
        DrawLine(new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height), new Vector2(rectangle.X, rectangle.Y + rectangle.Height), color, zPos);
        DrawLine(new Vector2(rectangle.X, rectangle.Y + rectangle.Height), new Vector2(rectangle.X, rectangle.Y), color, zPos);
    }

    // Draw a triangle fan defined by points
    // NOTE: First vertex provided is the center, shared by all triangles
    // By default, following vertex should be provided in counter-clockwise order
    public static void DrawTriangleFan(Vector2[] points, Color color, float zPos)
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

    public static void DrawCircle(Vector2 center, float radius, Color color, float zPos) {
        DrawCircleSector(center, radius, 0, 360, 36, color, zPos);
    }

    // Draw a piece of a circle
    public static void DrawCircleSector(Vector2 center, float radius, float startAngle, float endAngle, int segments, Color color, float zPos) {
        if (radius <= 0.0f)
            radius = 0.1f;  // Avoid div by zero

        // Function expects (endAngle > startAngle)
        if (endAngle < startAngle)
            (startAngle, endAngle) = (endAngle, startAngle);

        var minSegments = (int)MathF.Ceiling((endAngle - startAngle) / 90);

        if (segments < minSegments) {
            // Calculate the maximum angle between segments based on the error rate (usually 0.5f)
            var th = MathF.Acos(2 * MathF.Pow(1 - 0.5f / radius, 2) - 1);
            segments = (int)((endAngle - startAngle) * MathF.Ceiling(2 * MathF.PI / th) / 360);

            if (segments <= 0)
                segments = minSegments;
        }

        var stepLength = (endAngle - startAngle) / segments;
        var angle = startAngle;

        Rlgl.SetTexture(texShapes.Id);

        Rlgl.Begin(DrawMode.Quads);

        // NOTE: Every QUAD actually represents two segments
        for (var i = 0; i < segments / 2; i++) {
            Rlgl.Color4ub(color.R, color.G, color.B, color.A);

            Rlgl.TexCoord2f(texShapesRect.X / texShapes.Width, texShapesRect.Y / texShapes.Height);
            Rlgl.Vertex3f(center.X, center.Y, zPos);

            Rlgl.TexCoord2f((texShapesRect.X + texShapesRect.Width) / texShapes.Width, texShapesRect.Y / texShapes.Height);
            Rlgl.Vertex3f(center.X + MathF.Cos(JMath.DegToRad * (angle + stepLength * 2.0f)) * radius, center.Y + MathF.Sin(JMath.DegToRad * (angle + stepLength * 2.0f)) * radius, zPos);

            Rlgl.TexCoord2f((texShapesRect.X + texShapesRect.Width) / texShapes.Width, (texShapesRect.Y + texShapesRect.Height) / texShapes.Height);
            Rlgl.Vertex3f(center.X + MathF.Cos(JMath.DegToRad * (angle + stepLength)) * radius, center.Y + MathF.Sin(JMath.DegToRad * (angle + stepLength)) * radius, zPos);

            Rlgl.TexCoord2f(texShapesRect.X / texShapes.Width, (texShapesRect.Y + texShapesRect.Height) / texShapes.Height);
            Rlgl.Vertex3f(center.X + MathF.Cos(JMath.DegToRad * angle) * radius, center.Y + MathF.Sin(JMath.DegToRad * angle) * radius, zPos);

            angle += (stepLength * 2.0f);
        }

        // NOTE: In case number of segments is odd, we add one last piece to the cake
        if (segments % 2 == 1) {
            Rlgl.Color4ub(color.R, color.G, color.B, color.A);

            Rlgl.TexCoord2f(texShapesRect.X / texShapes.Width, texShapesRect.Y / texShapes.Height);
            Rlgl.Vertex3f(center.X, center.Y, zPos);

            Rlgl.TexCoord2f((texShapesRect.X + texShapesRect.Width) / texShapes.Width, (texShapesRect.Y + texShapesRect.Height) / texShapes.Height);
            Rlgl.Vertex3f(center.X + MathF.Cos(JMath.DegToRad * (angle + stepLength)) * radius, center.Y + MathF.Sin(JMath.DegToRad * (angle + stepLength)) * radius, zPos);

            Rlgl.TexCoord2f(texShapesRect.X / texShapes.Width, (texShapesRect.Y + texShapesRect.Height) / texShapes.Height);
            Rlgl.Vertex3f(center.X + MathF.Cos(JMath.DegToRad * angle) * radius, center.Y + MathF.Sin(JMath.DegToRad * angle) * radius, zPos);

            Rlgl.TexCoord2f((texShapesRect.X + texShapesRect.Width) / texShapes.Width, texShapesRect.Y / texShapes.Height);
            Rlgl.Vertex3f(center.X, center.Y, zPos);
        }

        Rlgl.End();

        Rlgl.SetTexture(0);
    }

    // Draw circle outline (Vector version)
    public static void DrawCircleLines(Vector2 center, float radius, Color color, float zPos)
    {
        Rlgl.Begin(DrawMode.Lines);
        Rlgl.Color4ub(color.R, color.G, color.B, color.A);

        // NOTE: Circle outline is drawn pixel by pixel every degree (0 to 360)
        for (var i = 0; i < 360; i += 10) {
            Rlgl.Vertex3f(center.X + MathF.Cos(JMath.DegToRad * i) * radius, center.Y + MathF.Sin(JMath.DegToRad * i) * radius, zPos);
            Rlgl.Vertex3f(center.X + MathF.Cos(JMath.DegToRad * (i + 10)) * radius, center.Y + MathF.Sin(JMath.DegToRad * (i + 10)) * radius, zPos);
        }

        Rlgl.End();
    }

    public static void DrawTexture(
        Tex      tex,
        Vector2  position,
        float?   rotation = null,
        Vector2? scale    = null,
        Vector2? origin   = null,
        float?   posZ     = null,
        Color?   tint     = null
    ) {
        rotation ??= 0;
        scale ??= new Vector2(1, 1);
        origin ??= new Vector2(0, 0);
        posZ ??= 0;
        tint ??= Color.White;

        DrawTexture(
            tex,
            new Rectangle(0, 0, tex.Width, tex.Height),
            new Rectangle(position.X, position.Y, tex.Width * scale.Value.X, tex.Height * scale.Value.Y),
            origin.Value,
            rotation.Value,
            posZ.Value,
            tint.Value
        );
    }
    
    public static void DrawTexture(
        Tex       tex,
        Rectangle sourceRect,
        Rectangle destRect,
        Vector2   origin,
        float     rotation,
        float     posZ,
        Color     tint
    ) {
        Texture2D texture = tex;
        
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
        Rlgl.Translatef(-origin.X, -origin.Y, 0);
        
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
    public static Vector2 Average(this List<Vector2> points) {
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