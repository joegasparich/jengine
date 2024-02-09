using System.Numerics;
using Raylib_cs;

namespace JEngine;

public enum InputType {
    CameraLeft,
    CameraRight,
    CameraUp,
    CameraDown,
    CameraZoomIn,
    CameraZoomOut,
    Undo, // TODO: combos
    Pause,
    NormalSpeed,
    FastSpeed,
    FasterSpeed,
    IncreaseBrushSize,
    DecreaseBrushSize,
    RotateClockwise,
    RotateCounterClockwise
}

public enum InputEventType {
    Key,
    MouseButton,
    MouseScroll,
    Input
}

public class InputEvent {
    public InputEventType type;
    public KeyboardKey?   keyDown   = null;
    public KeyboardKey?   keyUp     = null;
    public MouseButton?   mouseDown = null;
    public MouseButton?   mouseUp   = null;
    public InputType?     inputDown = null;
    public InputType?     inputUp   = null;
    public Vector2        mousePos;
    public Vector2        mouseWorldPos;
    public float          mouseScroll;
    public bool           consumed = false;
    
    public InputEvent(InputEventType type) {
        this.type = type;
    }

    public void Consume() {
        consumed = true;
    }
}

public class InputManager {
    // Constants
    private static readonly int MouseButtonNull = -1;
    private static readonly int MouseButtonMax  = (int)MouseButton.Back;
    private static readonly int KeyMax          = (int)KeyboardKey.KeyboardMenu;
    
    // Collections
    private Dictionary<KeyboardKey, InputType[]> inputs = new() {
        // Default inputs
        {KeyboardKey.W, new[] {InputType.CameraUp}},
        {KeyboardKey.A, new[] {InputType.CameraLeft}},
        {KeyboardKey.S, new[] {InputType.CameraDown}},
        {KeyboardKey.D, new[] {InputType.CameraRight}},
        {KeyboardKey.Up, new[] {InputType.CameraUp}},
        {KeyboardKey.Left, new[] {InputType.CameraLeft}},
        {KeyboardKey.Down, new[] {InputType.CameraDown}},
        {KeyboardKey.Right, new[] {InputType.CameraRight}},
    };

    // State
    private InputEvent currentEvent;

    public void ProcessInput() {
        // Key events
        for (int k = 0; k < KeyMax; k++) {
            var key = (KeyboardKey)k;
            if (!Raylib.IsKeyPressed(key) && !Raylib.IsKeyReleased(key) && !Raylib.IsKeyDown(key))
                continue;

            // Raw Keys
            var evt = new InputEvent(InputEventType.Key);
            evt.keyDown       = Raylib.IsKeyPressed(key) ? key : null;
            evt.keyUp         = Raylib.IsKeyReleased(key) ? key : null;
            evt.mousePos      = Raylib.GetMousePosition();
            evt.mouseWorldPos = Find.Renderer.ScreenToWorldPos(evt.mousePos);

            FireInputEvent(evt);
            
            // Inputs
            if (!inputs.ContainsKey(key)) continue;
            
            foreach (var input in inputs[key]) {
                evt = new InputEvent(InputEventType.Input);
                evt.inputDown       = Raylib.IsKeyPressed(key) ? input : null;
                evt.inputUp         = Raylib.IsKeyReleased(key) ? input : null;
                evt.mousePos        = Raylib.GetMousePosition();
                evt.mouseWorldPos   = Find.Renderer.ScreenToWorldPos(evt.mousePos);
                FireInputEvent(evt);
            }
        }
        
        // Mouse events
        for (int mb = 0; mb < MouseButtonMax; mb++) {
            var mouseButton = (MouseButton)mb;
            if (!Raylib.IsMouseButtonPressed(mouseButton) && !Raylib.IsMouseButtonReleased(mouseButton) && !Raylib.IsMouseButtonDown(mouseButton))
                continue;

            var evt = new InputEvent(InputEventType.MouseButton);
            evt.mouseDown     = Raylib.IsMouseButtonPressed(mouseButton) ? mouseButton : null;
            evt.mouseUp       = Raylib.IsMouseButtonReleased(mouseButton) ? mouseButton : null;
            evt.mousePos      = Raylib.GetMousePosition();
            evt.mouseWorldPos = Find.Renderer.ScreenToWorldPos(evt.mousePos);

            FireInputEvent(evt);
        }

        if (Raylib.GetMouseWheelMove() != 0) {
            var evt = new InputEvent(InputEventType.MouseScroll);
            evt.mouseScroll   = Raylib.GetMouseWheelMove();
            evt.mousePos      = Raylib.GetMousePosition();
            evt.mouseWorldPos = Find.Renderer.ScreenToWorldPos(evt.mousePos);

            FireInputEvent(evt);
        }
    }

    public void FireInputEvent(InputEvent evt) {
        currentEvent = evt;
        
        Find.Game.OnInput(evt);
        // Messenger::fire(EventType::InputEvent);
    }

    public void RegisterInput(InputType input, KeyboardKey key) {
        if (!inputs.ContainsKey(key))
            inputs[key] = Array.Empty<InputType>();

        inputs[key] = inputs[key].Append(input).ToArray();
    }

    public bool IsKeyHeld(KeyboardKey key) {
        return Raylib.IsKeyDown(key);
    }
    
    public bool IsMouseHeld(MouseButton button) {
        return Raylib.IsMouseButtonDown(button);
    }
    
    public bool IsInputHeld(InputType input) {
        // Prob replace with reverse dictionary
        foreach (var (key, ins) in inputs) {
            if (ins.Contains(input))
                return Raylib.IsKeyDown(key);
        }

        return false;
    }
    
    public Vector2 GetMousePos() {
        return Raylib.GetMousePosition();
    }
    
    public Vector2 GetMouseWorldPos() {
        return Find.Renderer.ScreenToWorldPos(GetMousePos());
    }
    
    public InputEvent GetCurrentEvent() {
        return currentEvent;
    }
}