using System.Numerics;
using Raylib_cs;
using JEngine.util;

namespace JEngine;

public class Camera {
    // State
    private Camera3D camera;
    public  float    Zoom = 1f;

    public Camera3D Cam      => camera;
    public Vector2  Position => camera.Position.Xy();
    public Vector2  WorldPos
    {
        get => Position / Find.Config.WorldScalePx;
        set => JumpTo(value);
    }

    public Camera() {
        camera = new Camera3D {
            Projection = CameraProjection.Orthographic,
            FovY       = Game.DefaultScreenHeight / Zoom,
            Position   = new Vector3(0, 0,  (int)Depth.Camera),
            Up         = new Vector3(0, -1, 0)
        };
    }

    public void OnInput(InputEvent evt) {

    }

    public void JumpTo(Vector2 worldPos) {
        var camPos = worldPos * Find.Config.WorldScalePx;
        camera.Position.X = camPos.X;
        camera.Position.Y = camPos.Y;
        camera.Target     = camera.Position with { Z = 0 };

        camera.FovY = Find.Game.ScreenHeight / Zoom;
    }

    public void OnScreenResized() {
        camera.FovY = Find.Game.ScreenHeight / Zoom;
    }
}