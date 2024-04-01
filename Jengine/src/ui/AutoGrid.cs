using Raylib_cs;
using JEngine.util;

namespace JEngine.ui;

public enum GridDirection {
    Vertical,
    Horizontal
}

public struct AutoGrid {
    // State
    private Rectangle     rect;
    private GridDirection direction;
    private float         colWidth;
    private float         rowHeight;
    private float         gap;
    
    private float         curX;
    private float         curY;
    
    // Properties
    public float     CurX => curX;
    public float     CurY => curY;
    public Rectangle Rect => rect;

    public AutoGrid(Rectangle rect, GridDirection direction, float colWidth, float rowHeight, float gap = GUI.GapTiny) {
        this.direction = direction;
        this.rect      = rect;
        this.colWidth  = colWidth;
        this.rowHeight = rowHeight;
        this.gap       = gap;
        
        curX               = rect.X;
        curY               = rect.Y;
    }
    
    public void GetNext(out Rectangle rect) {
        rect = new Rectangle(curX, curY, colWidth, rowHeight);
        
        if (direction == GridDirection.Vertical) {
            curY += rowHeight + gap;
            if (curY + rowHeight > this.rect.Y + this.rect.Height) {
                curY =  this.rect.Y;
                curX += colWidth + gap;
            }
        } else {
            curX += colWidth + gap;
            if (curX + colWidth > this.rect.X + this.rect.Width) {
                curX =  this.rect.X;
                curY += rowHeight + gap;
            }
        }
    }
}