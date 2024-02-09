using System.Numerics;
using Raylib_cs;
using JEngine.util;

namespace JEngine;

public class Camera {
    // Constants
    private const float CameraSpeed    = 2f;
    private const float CameraZoomRate = 0.005f;
    private const float MinZoom        = 0.5f;
    private const float MaxZoom        = 10f;
    
    // State
    private Camera3D camera;
    private float    zoom = 1;
    private Vector2? dragStart;
    private Vector2? dragCameraOrigin;

    public Camera3D Cam      => camera;
    public Vector2  Position => camera.Position.XY();
    public float    Zoom     => zoom;

    public Camera() {
        camera = new Camera3D {
            Projection = CameraProjection.Orthographic,
            FovY       = Find.Game.ScreenHeight / zoom,
            Position   = new Vector3(0, 0,  (int)Depth.Camera),
            Up         = new Vector3(0, -1, 0)
        };

        dragStart        = Find.Game.input.GetMousePos();
        dragCameraOrigin = camera.Target.XY();
    }

    public void OnInput(InputEvent evt) {

    }

    public void JumpTo(Vector2 worldPos) {
        var camPos = worldPos * Game.WorldScale;
        camera.Position.X = camPos.X;
        camera.Position.Y = camPos.Y;
        camera.FovY       = Find.Game.ScreenHeight / zoom;
        camera.Target     = camera.Position with { Z = 0 };
    }

    public void OnScreenResized() {
        camera.FovY = Find.Game.ScreenHeight / zoom;
    }
}