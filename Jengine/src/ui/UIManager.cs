using Raylib_cs;
using JEngine.util;

namespace JEngine.ui;

public enum UIEvent {
    None,
    Draw,
    Input
}

public class UiManager {
    // Constants
    private const string DefaultFontPath = "assets/fonts/Pixeltype.ttf";
    public const  int    DefaultFontSize = 10;
    
    // Resources
    public readonly Font DefaultFont = Raylib.LoadFontEx(DefaultFontPath, DefaultFontSize, null, 0);
    
    // State
    private List<Window>               _windowStack       = new();
    private Dictionary<string, Window> _openWindowMap     = new();
    private List<Window>               _windowsToOpen     = new();
    private HashSet<string>            _windowsToClose    = new();
    private HashSet<string>            _immediateWindows  = new();
    public  UIEvent                    CurrentEvent      = UIEvent.Draw;
    public  Rectangle                  CurrentDrawBounds = new(0, 0, Find.Game.ScreenWidth, Find.Game.ScreenHeight); // TODO: Dynamic

    private MouseCursor _cursor;
    private string      _hoveredWindowId;
    private string      _currentWindowId;
    private string?     _currentFocusId = null;
    
    // Properties
    public float UiScale => Find.Game.PlayerConfig.UiScale;

    public void Init() {
        Debug.Log("Initializing UI");

        Raylib.SetTextureFilter(DefaultFont.Texture, TextureFilter.Point);
    }

    public void OnInput(InputEvent evt) {
        CurrentEvent = UIEvent.Input;
        
        // Lose focus
        if (evt.KeyDown is KeyboardKey.Escape || evt.MouseDown is MouseButton.Left)
            _currentFocusId = null;
        
        Find.Game.OnGUI();
        
        // Clear closed immediate windows
        for (var i = _windowStack.Count - 1; i >= 0; i--) {
            if (_windowStack[i].Immediate && !_immediateWindows.Contains(_windowStack[i].Id)) {
                _openWindowMap.Remove(_windowStack[i].Id);
                _windowStack.RemoveAt(i);
            }
        }
        _immediateWindows.Clear();
        
        // Loop backwards so that top windows consume events first
        for (var i = _windowStack.Count - 1; i >= 0; i--) {
            var window = _windowStack[i];

            var mouseOver = JMath.PointInRect(window.AbsRect, evt.MousePos);
            if (_hoveredWindowId.NullOrEmpty() && mouseOver && window.ConsumesHover)
                _hoveredWindowId = window.Id;
            
            CurrentDrawBounds = window.AbsRect;
            _currentWindowId = window.Id;
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
            foreach (var window in _windowStack) {
                if (window.DismissOnRightClick) {
                    CloseWindow(window.Id);
                    evt.Consume();
                    break;
                }
            }
        }
    }

    public void PreRender() {
        Raylib.SetMouseCursor(_cursor);
        SetCursor(MouseCursor.Default);
    }

    public void DrawUi() {
        CurrentEvent = UIEvent.Draw;
        Find.Game.OnGUI();

        // Clear closed immediate windows
        for (var i = _windowStack.Count - 1; i >= 0; i--) {
            if (_windowStack[i].Immediate && !_immediateWindows.Contains(_windowStack[i].Id)) {
                _openWindowMap.Remove(_windowStack[i].Id);
                _windowStack.RemoveAt(i);
            }
        }
        _immediateWindows.Clear();
        
        // First loop through in reverse to find the hovered window
        for (var i = _windowStack.Count - 1; i >= 0; i--) {
            var window = _windowStack[i];
            if (JMath.PointInRect(window.AbsRect.Multiply(UiScale), Find.Input.GetMousePos()) && window.ConsumesHover) {
                _hoveredWindowId = window.Id;
                break;
            }
        }
        
        // Render windows
        foreach (var window in _windowStack) {
            CurrentDrawBounds = window.AbsRect;
            _currentWindowId   = window.Id;
            window.DoWindowContents();
        }
        
        CurrentDrawBounds = new Rectangle(0, 0, Find.Game.ScreenWidth, Find.Game.ScreenHeight);
        CurrentEvent      = UIEvent.None;
    }

    public void PostRender() {
        _windowStack.RemoveAll(window => _windowsToClose.Contains(window.Id));
        _windowStack.AddRange(_windowsToOpen);
        
        _windowsToOpen.Clear();
        _windowsToClose.Clear();
    }

    public void OnScreenResized() {
        foreach (var window in _windowStack) {
            window.OnScreenResized(Find.Game.ScreenWidth, Find.Game.ScreenHeight);
        }
    }

    public string PushWindow(Window window) {
        _windowsToOpen.Add(window);
        _openWindowMap.Add(window.Id, window);
        return window.Id;
    }

    public void CloseWindow(string id) {
        if (!_openWindowMap.ContainsKey(id)) 
            return;
        
        _openWindowMap[id].OnClose();
        
        _openWindowMap.Remove(id);
        _windowsToClose.Add(id);
    }
    
    public void CloseAllWindows() {
        foreach (var window in _windowStack) {
            window.OnClose();
        }
        _windowStack.Clear();
        _windowsToOpen.Clear();
        _openWindowMap.Clear();
    }

    public void BringWindowToFront(string id) {
        var index = _windowStack.FindIndex(window => window.Id == id);
        _windowStack.MoveItemAtIndexToBack(index);
    }
    
    public Window? GetWindow(string id) {
        return _openWindowMap.TryGetValue(id, out var window) ? window : null;
    }
    
    public bool IsWindowOpen(string id) {
        if (id.NullOrEmpty()) 
            return false;
        
        return _openWindowMap.ContainsKey(id);
    }

    public void DoImmediateWindow(string id, Rectangle rect, Action<Rectangle> onUi, bool draggable = false, bool dialog = true, bool consumesHover = true) {
        if (CurrentEvent == UIEvent.None) {
            Debug.Warn("Immediate windows must be called in OnGUI");
            return;
        }
        
        var found = _openWindowMap.ContainsKey(id);

        if (!found) {
            Window window;
            if (dialog)
                window = new Dialog(id, rect, onUi);
            else
                window = new Window(id, rect, onUi);
            window.Immediate     = true;
            window.ConsumesHover = consumesHover;
            PushWindow(window);
        } else {
            if (!draggable)
                _openWindowMap[id].AbsRect = rect;
        }

        _immediateWindows.Add(id);
    }

    public void SetCursor(MouseCursor c) {
        _cursor = c;
    }
    
    public void SetFocus(string focusId) {
        _currentFocusId = focusId;
    }
    
    public bool IsFocused(string focusId) {
        return _currentFocusId == focusId;
    }

    public Rectangle GetAbsRect(Rectangle rect) {
        return new Rectangle(rect.X + CurrentDrawBounds.X, rect.Y + CurrentDrawBounds.Y, rect.Width, rect.Height).Multiply(UiScale);
    }

    public Rectangle ToAbsRect(Rectangle rect) {
        return new Rectangle(rect.X - CurrentDrawBounds.X, rect.Y - CurrentDrawBounds.Y, rect.Width, rect.Height);
    }

    public bool IsMouseOverRect(Rectangle rect) {
        if (_currentWindowId != _hoveredWindowId)
            return false;
        
        return JMath.PointInRect(GetAbsRect(rect), Find.Input.GetMousePos());
    }
}