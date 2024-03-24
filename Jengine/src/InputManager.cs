using System.Numerics;
using Raylib_cs;

namespace JEngine;

public enum InputEventType {
    Key,
    MouseButton,
    MouseScroll,
    Input
}

public class InputEvent {
    public InputEventType Type;
    public KeyboardKey?   KeyDown;
    public KeyboardKey?   KeyUp;
    public MouseButton?   MouseDown;
    public MouseButton?   MouseUp;
    // public InputType?     inputDown;
    // public InputType?     inputUp;
    public Vector2        MousePos;
    public Vector2        MouseWorldPos;
    public float          MouseScroll;
    public bool           Consumed;
    
    public InputEvent(InputEventType type) {
        Type = type;
    }

    public void Consume() {
        Consumed = true;
    }
}

public class InputManager {
    // Constants
    private const int MouseButtonNull = -1;
    private static readonly int mouseButtonMax  = (int)MouseButton.Back;
    private static readonly int keyMax          = (int)KeyboardKey.KeyboardMenu;
    
    // Collections
    // private Dictionary<KeyboardKey, InputType[]> inputs = new() {
    //     // Default inputs
    //     {KeyboardKey.W, [InputType.CameraUp] },
    //     {KeyboardKey.A, [InputType.CameraLeft] },
    //     {KeyboardKey.S, [InputType.CameraDown] },
    //     {KeyboardKey.D, [InputType.CameraRight] },
    //     {KeyboardKey.Up, [InputType.CameraUp] },
    //     {KeyboardKey.Left, [InputType.CameraLeft] },
    //     {KeyboardKey.Down, [InputType.CameraDown] },
    //     {KeyboardKey.Right, [InputType.CameraRight] },
    // };

    // State
    private InputEvent currentEvent;

    public void ProcessInput() {
        // Key events
        for (int k = 0; k < keyMax; k++) {
            var key = (KeyboardKey)k;
            if (!Raylib.IsKeyPressed(key) && !Raylib.IsKeyReleased(key) && !Raylib.IsKeyDown(key))
                continue;

            // Raw Keys
            var evt = new InputEvent(InputEventType.Key);
            evt.KeyDown       = Raylib.IsKeyPressed(key) ? key : null;
            evt.KeyUp         = Raylib.IsKeyReleased(key) ? key : null;
            evt.MousePos      = Raylib.GetMousePosition();
            evt.MouseWorldPos = Find.Renderer.ScreenToWorldPos(evt.MousePos);

            FireInputEvent(evt);
            
            // Inputs
            // if (!inputs.ContainsKey(key)) continue;
            //
            // foreach (var input in inputs[key]) {
            //     evt = new InputEvent(InputEventType.Input);
            //     evt.inputDown       = Raylib.IsKeyPressed(key) ? input : null;
            //     evt.inputUp         = Raylib.IsKeyReleased(key) ? input : null;
            //     evt.mousePos        = Raylib.GetMousePosition();
            //     evt.mouseWorldPos   = Find.Renderer.ScreenToWorldPos(evt.mousePos);
            //     FireInputEvent(evt);
            // }
        }
        
        // Mouse events
        for (int mb = 0; mb < mouseButtonMax; mb++) {
            var mouseButton = (MouseButton)mb;
            if (!Raylib.IsMouseButtonPressed(mouseButton) && !Raylib.IsMouseButtonReleased(mouseButton) && !Raylib.IsMouseButtonDown(mouseButton))
                continue;

            var evt = new InputEvent(InputEventType.MouseButton);
            evt.MouseDown     = Raylib.IsMouseButtonPressed(mouseButton) ? mouseButton : null;
            evt.MouseUp       = Raylib.IsMouseButtonReleased(mouseButton) ? mouseButton : null;
            evt.MousePos      = Raylib.GetMousePosition();
            evt.MouseWorldPos = Find.Renderer.ScreenToWorldPos(evt.MousePos);

            FireInputEvent(evt);
        }

        if (Raylib.GetMouseWheelMove() != 0) {
            var evt = new InputEvent(InputEventType.MouseScroll);
            evt.MouseScroll   = Raylib.GetMouseWheelMove();
            evt.MousePos      = Raylib.GetMousePosition();
            evt.MouseWorldPos = Find.Renderer.ScreenToWorldPos(evt.MousePos);

            FireInputEvent(evt);
        }
    }

    public void FireInputEvent(InputEvent evt) {
        currentEvent = evt;
        
        Find.Game.OnInput(evt);
        // Messenger::fire(EventType::InputEvent);
    }

    // TODO: Change from InputType enum to something else
    // public void RegisterInput(InputType input, KeyboardKey key) {
    //     if (!inputs.ContainsKey(key))
    //         inputs[key] = Array.Empty<InputType>();
    //
    //     inputs[key] = inputs[key].Append(input).ToArray();
    // }

    public bool IsKeyHeld(KeyboardKey key) {
        return Raylib.IsKeyDown(key);
    }
    
    public bool IsMouseHeld(MouseButton button) {
        return Raylib.IsMouseButtonDown(button);
    }
    
    // public bool IsInputHeld(InputType input) {
    //     // Prob replace with reverse dictionary
    //     foreach (var (key, ins) in inputs) {
    //         if (ins.Contains(input))
    //             return Raylib.IsKeyDown(key);
    //     }
    //
    //     return false;
    // }
    
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