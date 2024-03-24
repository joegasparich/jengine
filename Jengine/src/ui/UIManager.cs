using JEngine.defs;
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
    private const string DefaultFontPath = "assets/textures/fonts/Joepx.png";
    private const string VisibilityFontPath = "assets/textures/fonts/Joepx_vis.png";
    
    // Resources
    public Font DefaultFont;
    public Font VisibilityFont;
    
    // State
    private List<Window>               windowStack       = new();
    private Dictionary<string, Window> openWindowMap     = new();
    private List<Window>               windowsToOpen     = new();
    private HashSet<string>            windowsToClose    = new();
    private HashSet<string>            immediateWindows  = new();
    public  UIEvent                    CurrentEvent      = UIEvent.Draw;
    public  Rectangle                  CurrentDrawBounds = new(0, 0, Find.Game.ScreenWidth, Find.Game.ScreenHeight); // TODO: Dynamic

    private MouseCursor cursor;
    private string      hoveredWindowId;
    private string      currentWindowId;
    private string?     currentFocusId = null;
    
    // Properties
    public float UiScale => Find.Game.PlayerConfig.UiScale;

    public void Init() {
        Debug.Log("Initializing UI");

        Raylib.SetTextureFilter(DefaultFont.Texture, TextureFilter.Point);
        
        DefaultFont    = Raylib.LoadFont(DefaultFontPath);
        VisibilityFont = Raylib.LoadFont(VisibilityFontPath);
        GUI.Font       = DefaultFont;
    }

    public void OnInput(InputEvent evt) {
        CurrentEvent = UIEvent.Input;
        
        // Lose focus
        if (evt.KeyDown is KeyboardKey.Escape || evt.MouseDown is MouseButton.Left)
            currentFocusId = null;
        
        Find.Game.OnGUI();
        
        // Clear closed immediate windows
        for (var i = windowStack.Count - 1; i >= 0; i--) {
            if (windowStack[i].Immediate && !immediateWindows.Contains(windowStack[i].Id)) {
                openWindowMap.Remove(windowStack[i].Id);
                windowStack.RemoveAt(i);
            }
        }
        immediateWindows.Clear();
        
        // Loop backwards so that top windows consume events first
        for (var i = windowStack.Count - 1; i >= 0; i--) {
            var window = windowStack[i];

            var mouseOver = JMath.PointInRect(window.AbsRect, evt.MousePos);
            if (hoveredWindowId.NullOrEmpty() && mouseOver && window.ConsumesHover)
                hoveredWindowId = window.Id;
            
            CurrentDrawBounds = window.AbsRect;
            currentWindowId = window.Id;
            window.OnInput(evt);
            
            // Consume event if it's a mouse button down on the window
            if (JMath.PointInRect(CurrentDrawBounds, evt.MousePos) && evt.MouseDown.HasValue) {
                evt.Consume();
                break;
            }
        }
        CurrentDrawBounds = new Rectangle(0, 0, Find.Game.ScreenWidth, Find.Game.ScreenHeight);
        CurrentEvent      = UIEvent.None;
    }

    public void PostInput(InputEvent evt) {
        if (!evt.Consumed && evt.MouseDown == MouseButton.Right) {
            foreach (var window in windowStack) {
                if (window.DismissOnRightClick) {
                    CloseWindow(window.Id);
                    evt.Consume();
                    break;
                }
            }
        }
    }

    public void PreDraw() {
        Raylib.SetMouseCursor(cursor);
        SetCursor(MouseCursor.Default);
    }

    public void DrawUI() {
        CurrentEvent = UIEvent.Draw;
        Find.Game.OnGUI();

        // Clear closed immediate windows
        for (var i = windowStack.Count - 1; i >= 0; i--) {
            if (windowStack[i].Immediate && !immediateWindows.Contains(windowStack[i].Id)) {
                openWindowMap.Remove(windowStack[i].Id);
                windowStack.RemoveAt(i);
            }
        }
        immediateWindows.Clear();
        
        // First loop through in reverse to find the hovered window
        for (var i = windowStack.Count - 1; i >= 0; i--) {
            var window = windowStack[i];
            if (JMath.PointInRect(window.AbsRect.Multiply(UiScale), Find.Input.GetMousePos()) && window.ConsumesHover) {
                hoveredWindowId = window.Id;
                break;
            }
        }
        
        // Render windows
        foreach (var window in windowStack) {
            CurrentDrawBounds = window.AbsRect;
            currentWindowId   = window.Id;
            window.DoWindowContents();
        }
        
        CurrentDrawBounds = new Rectangle(0, 0, Find.Game.ScreenWidth, Find.Game.ScreenHeight);
        CurrentEvent      = UIEvent.None;
    }

    public void PostDraw() {
        windowStack.RemoveAll(window => windowsToClose.Contains(window.Id));
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
        openWindowMap.Add(window.Id, window);
        return window.Id;
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
        var index = windowStack.FindIndex(window => window.Id == id);
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

    public void DoImmediateWindow(string id, Rectangle rect, Action<Rectangle> onUi, bool draggable = false, bool dialog = true, bool consumesHover = true) {
        if (CurrentEvent == UIEvent.None) {
            Debug.Warn("Immediate windows must be called in OnGUI");
            return;
        }
        
        var found = openWindowMap.ContainsKey(id);

        if (!found) {
            Window window;
            if (dialog)
                window = new Panel(id, rect, onUi);
            else
                window = new Window(id, rect, onUi);
            window.Immediate     = true;
            window.ConsumesHover = consumesHover;
            PushWindow(window);
        } else {
            if (!draggable)
                openWindowMap[id].AbsRect = rect;
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
        return new Rectangle(rect.X + CurrentDrawBounds.X, rect.Y + CurrentDrawBounds.Y, rect.Width, rect.Height).Multiply(UiScale);
    }

    public Rectangle ToAbsRect(Rectangle rect) {
        return new Rectangle(rect.X - CurrentDrawBounds.X, rect.Y - CurrentDrawBounds.Y, rect.Width, rect.Height);
    }

    public bool IsMouseOverRect(Rectangle rect) {
        if (currentWindowId != hoveredWindowId)
            return false;
        
        return JMath.PointInRect(GetAbsRect(rect), Find.Input.GetMousePos());
    }
}