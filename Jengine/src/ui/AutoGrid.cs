using Raylib_cs;
using JEngine.util;

namespace JEngine.ui;

public enum GridDirection {
    Vertical,
    Horizontal
}

public class AutoGrid {
    // State
    private Rectangle     _rect;
    private GridDirection _direction;
    private float         _colWidth;
    private float         _rowHeight;
    private float         _gap;
    
    private float         _curX;
    private float         _curY;
    
    // Properties
    public float     CurX => _curX;
    public float     CurY => _curY;
    public Rectangle Rect => _rect;

    public AutoGrid(Rectangle rect, GridDirection direction, float colWidth, float rowHeight, float gap = Gui.GapTiny) {
        _direction     = direction;
        _rect          = rect;
        _colWidth      = colWidth;
        _rowHeight     = rowHeight;
        _gap           = gap;
        
        _curX               = rect.X;
        _curY               = rect.Y;
    }
    
    public void GetNext(out Rectangle rect) {
        rect = new Rectangle(_curX, _curY, _colWidth, _rowHeight);
        
        if (_direction == GridDirection.Vertical) {
            _curY += _rowHeight + _gap;
            if (_curY + _rowHeight > _rect.Y + _rect.Height) {
                _curY = _rect.Y;
                _curX += _colWidth + _gap;
            }
        } else {
            _curX += _colWidth + _gap;
            if (_curX + _colWidth > _rect.X + _rect.Width) {
                _curX = _rect.X;
                _curY += _rowHeight + _gap;
            }
        }
    }
}