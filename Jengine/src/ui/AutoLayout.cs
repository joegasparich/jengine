using Raylib_cs;
using JEngine.util;

namespace JEngine.ui; 

public class AutoLayout {
    public enum AlignMode {
        Left,
        Center,
        Right
    }

    // State
    private Rectangle rect;
    private AlignMode alignMode;
    private float     gap;
    public  float     curY;

    // Properties
    public float     RemainingHeight => rect.Height - curY;
    public Rectangle Rect            => rect;

    public AutoLayout(Rectangle rect, AlignMode alignMode = AlignMode.Left, float gap = GUI.GapTiny) {
        this.rect          = rect;
        curY               = rect.Y;
        this.alignMode     = alignMode;
        this.gap           = gap;
    }

    public void Header(FormattedString text) {
        using (new TextBlock(GUI.HeaderFontSize))
            GUI.Label(GetAlignedRect(rect.Width, GUI.HeaderFontSize), text);
        curY += GUI.HeaderFontSize + gap;
    }

    public void Label(FormattedString text, float? height = null) {
        height ??= GUI.fontSize;

        GUI.Label(GetAlignedRect(rect.Width, height.Value), text);
        curY += height.Value + gap;
    }

    public bool ButtonText(FormattedString text, Color? col = null, bool selected = false, float? width = null, float? height = null) {
        height ??= GUI.ButtonHeight;

        var res = GUI.ButtonText(GetAlignedRect(width ?? rect.Width, height.Value), text, col, selected);
        curY += height.Value + gap;
        
        return res;
    }

    public void TextInput(ref string text, string focusId, float? width = null, float? height = null) {
        height ??= GUI.ButtonHeight;

        GUI.TextInput(GetAlignedRect(width ?? rect.Width, height.Value), ref text, focusId);
        curY += height.Value + gap;
    }

    public void ProgressBar(float pct, Color? colour = null, Color? backgroundColour = null, float? width = null, float? height = null) {
        height ??= 10;

        GUI.ProgressBar(GetAlignedRect(width ?? rect.Width, height.Value), pct, colour, backgroundColour);
        curY += height.Value + gap;
    }

    public Rectangle GetRect(float height, float? width = null) {
        var r = GetAlignedRect(width ?? rect.Width, height);
        curY += height + gap;

        return r;
    }

    private Rectangle GetAlignedRect(float width, float height) {
        switch (alignMode) {
            default:
            case AlignMode.Left:
                return new Rectangle(rect.X, curY, width, height);
            case AlignMode.Center:
                return new Rectangle(rect.X + (rect.Width - width) / 2, curY, width, height);
            case AlignMode.Right:
                return new Rectangle(rect.X + rect.Width - width, curY, width, height);

        }
    }
}