using System.Numerics;
using Raylib_cs;
using JEngine.util;

namespace JEngine;

public class Camera {
    // State
    private Camera3D camera;
    private float    zoom = 1;

    public Camera3D Cam      => camera;
    public Vector2  Position => camera.Position.XY();
    public float    Zoom     => zoom;

    public Camera() {
        camera = new Camera3D {
            Projection = CameraProjection.Orthographic,
            FovY       = Find.Renderer.resolution.Y / zoom,
            Position   = new Vector3(0, 0,  (int)Depth.Camera),
            Up         = new Vector3(0, -1, 0)
        };
    }

    public void OnInput(InputEvent evt) {

    }

    public void JumpTo(Vector2 worldPos) {
        var camPos = worldPos;
        camera.Position.X = camPos.X;
        camera.Position.Y = camPos.Y;
        camera.FovY       = Find.Renderer.resolution.Y / zoom;
        camera.Target     = camera.Position with { Z = 0 };
    }

    public void OnScreenResized() {
        camera.FovY = Find.Renderer.resolution.Y / zoom;
    }
}