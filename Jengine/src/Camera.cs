using System.Numerics;
using Raylib_cs;
using JEngine.util;

namespace JEngine;

public class Camera {
    // State
    private Camera3D camera;
    public  float    zoom = 1f;

    public Camera3D Cam      => camera;
    public Vector2  Position => camera.Position.XY();
    public Vector2  WorldPos
    {
        get => Position / Find.Config.worldScalePx;
        set => JumpTo(value);
    }

    public Camera() {
        camera = new Camera3D {
            Projection = CameraProjection.Orthographic,
            FovY       = Game.DefaultScreenHeight / zoom,
            Position   = new Vector3(0, 0,  (int)Depth.Camera),
            Up         = new Vector3(0, -1, 0)
        };
    }

    public void OnInput(InputEvent evt) {

    }

    public void JumpTo(Vector2 worldPos) {
        var camPos = worldPos * Find.Config.worldScalePx;
        camera.Position.X = camPos.X;
        camera.Position.Y = camPos.Y;
        camera.Target     = camera.Position with { Z = 0 };

        camera.FovY = Find.Game.ScreenHeight / zoom;
    }

    public void OnScreenResized() {
        camera.FovY = Find.Game.ScreenHeight / zoom;
    }
}