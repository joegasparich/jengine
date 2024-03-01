using Raylib_cs;
using JEngine.util;

namespace JEngine.ui;

public enum UIEvent {
    None,
    Draw,
    Input
}

public class UIManager {
    // Constants
    private const string DefaultFontPath = "assets/fonts/Pixeltype.ttf";
    public const  int    DefaultFontSize = 10;
    
    // Resources
    public readonly Font DefaultFont = Raylib.LoadFontEx(DefaultFontPath, DefaultFontSize, null, 0);
    
    // State
    private List<Window>               windowStack       = new();
    private Dictionary<string, Window> openWindowMap     = new();
    private List<Window>               windowsToOpen     = new();
    private HashSet<string>            windowsToClose    = new();
    private HashSet<string>            immediateWindows  = new();
    public  UIEvent                    currentEvent      = UIEvent.Draw;
    public  Rectangle                  currentDrawBounds = new(0, 0, Find.Game.ScreenWidth, Find.Game.ScreenHeight); // TODO: Dynamic

    private MouseCursor cursor;
    private string      hoveredWindowId;
    private string      currentWindowId;
    private string?     currentFocusId = null;
    
    // Properties
    public float UIScale => Find.Game.playerConfig.uiScale;

    public void Init() {
        Debug.Log("Initializing UI");

        Raylib.SetTextureFilter(DefaultFont.Texture, TextureFilter.Point);
    }

    public void OnInput(InputEvent evt) {
        currentEvent = UIEvent.Input;
        
        // Lose focus
        if (evt.keyDown is KeyboardKey.Escape || evt.mouseDown is MouseButton.Left)
            currentFocusId = null;
        
        Find.Game.OnGUI();
        
        // Clear closed immediate windows
        for (var i = windowStack.Count - 1; i >= 0; i--) {
            if (windowStack[i].immediate && !immediateWindows.Contains(windowStack[i].id)) {
                openWindowMap.Remove(windowStack[i].id);
                windowStack.RemoveAt(i);
            }
        }
        immediateWindows.Clear();
        
        // Loop backwards so that top windows consume events first
        for (var i = windowStack.Count - 1; i >= 0; i--) {
            var window = windowStack[i];

            var mouseOver = JMath.PointInRect(window.absRect, evt.mousePos);
            if (hoveredWindowId.NullOrEmpty() && mouseOver && window.consumesHover)
                hoveredWindowId = window.id;
            
            currentDrawBounds = window.absRect;
            currentWindowId = window.id;
            window.OnInput(evt);
            
            // Consume event if it's a mouse button down on the window
            if (JMath.PointInRect(currentDrawBounds, evt.mousePos) && evt.mouseDown.HasValue) {
                evt.Consume();
                break;
            }
        }
        currentDrawBounds = new Rectangle(0, 0, Find.Game.ScreenWidth, Find.Game.ScreenHeight);
        currentEvent      = UIEvent.None;
    }

    public void PostInput(InputEvent evt) {
        if (!evt.consumed && evt.mouseDown == MouseButton.Right) {
            foreach (var window in windowStack) {
                if (window.dismissOnRightClick) {
                    CloseWindow(window.id);
                    evt.Consume();
                    break;
                }
            }
        }
    }

    public void PreRender() {
        Raylib.SetMouseCursor(cursor);
        SetCursor(MouseCursor.Default);
    }

    public void DrawUI() {
        currentEvent = UIEvent.Draw;
        Find.Game.OnGUI();

        // Clear closed immediate windows
        for (var i = windowStack.Count - 1; i >= 0; i--) {
            if (windowStack[i].immediate && !immediateWindows.Contains(windowStack[i].id)) {
                openWindowMap.Remove(windowStack[i].id);
                windowStack.RemoveAt(i);
            }
        }
        immediateWindows.Clear();
        
        // First loop through in reverse to find the hovered window
        for (var i = windowStack.Count - 1; i >= 0; i--) {
            var window = windowStack[i];
            if (JMath.PointInRect(window.absRect.Multiply(UIScale), Find.Input.GetMousePos()) && window.consumesHover) {
                hoveredWindowId = window.id;
                break;
            }
        }
        
        // Render windows
        foreach (var window in windowStack) {
            currentDrawBounds = window.absRect;
            currentWindowId   = window.id;
            window.DoWindowContents();
        }
        
        currentDrawBounds = new Rectangle(0, 0, Find.Game.ScreenWidth, Find.Game.ScreenHeight);
        currentEvent      = UIEvent.None;
    }

    public void PostRender() {
        windowStack.RemoveAll(window => windowsToClose.Contains(window.id));
        windowStack.AddRange(windowsToOpen);
        
        windowsToOpen.Clear();
        windowsToClose.Clear();
    }

    public void OnScreenResized() {
        foreach (var window in windowStack) {
            window.OnScreenResized(Find.Game.ScreenWidth, Find.Game.ScreenHeight);
        }
    }

    public string PushWindow(Window window) {
        windowsToOpen.Add(window);
        openWindowMap.Add(window.id, window);
        return window.id;
    }

    public void CloseWindow(string id) {
        if (!openWindowMap.ContainsKey(id)) 
            return;
        
        openWindowMap[id].OnClose();
        
        openWindowMap.Remove(id);
        windowsToClose.Add(id);
    }
    
    public void CloseAllWindows() {
        foreach (var window in windowStack) {
            window.OnClose();
        }
        windowStack.Clear();
        windowsToOpen.Clear();
        openWindowMap.Clear();
    }

    public void BringWindowToFront(string id) {
        var index = windowStack.FindIndex(window => window.id == id);
        windowStack.MoveItemAtIndexToBack(index);
    }
    
    public Window? GetWindow(string id) {
        return openWindowMap.TryGetValue(id, out var window) ? window : null;
    }
    
    public bool IsWindowOpen(string id) {
        if (id.NullOrEmpty()) 
            return false;
        
        return openWindowMap.ContainsKey(id);
    }

    public void DoImmediateWindow(string id, Rectangle rect, Action<Rectangle> onUI, bool draggable = false, bool dialog = true, bool consumesHover = true) {
        if (currentEvent == UIEvent.None) {
            Debug.Warn("Immediate windows must be called in OnGUI");
            return;
        }
        
        var found = openWindowMap.ContainsKey(id);

        if (!found) {
            Window window;
            if (dialog)
                window = new Dialog(id, rect, onUI);
            else
                window = new Window(id, rect, onUI);
            window.immediate     = true;
            window.consumesHover = consumesHover;
            PushWindow(window);
        } else {
            if (!draggable)
                openWindowMap[id].absRect = rect;
        }

        immediateWindows.Add(id);
    }

    public void SetCursor(MouseCursor c) {
        cursor = c;
    }
    
    public void SetFocus(string focusId) {
        currentFocusId = focusId;
    }
    
    public bool IsFocused(string focusId) {
        return currentFocusId == focusId;
    }

    public Rectangle GetAbsRect(Rectangle rect) {
        return new Rectangle(rect.X + currentDrawBounds.X, rect.Y + currentDrawBounds.Y, rect.Width, rect.Height).Multiply(UIScale);
    }

    public Rectangle ToAbsRect(Rectangle rect) {
        return new Rectangle(rect.X - currentDrawBounds.X, rect.Y - currentDrawBounds.Y, rect.Width, rect.Height);
    }

    public bool IsMouseOverRect(Rectangle rect) {
        if (currentWindowId != hoveredWindowId)
            return false;
        
        return JMath.PointInRect(GetAbsRect(rect), Find.Input.GetMousePos());
    }
}