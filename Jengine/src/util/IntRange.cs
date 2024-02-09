namespace JEngine.util; 

public class IntRange {
    public int min;
    public int max;

    public IntRange(int min, int max) {
        this.min = min;
        this.max = max;
    }

    public bool Includes(int value) {
        return value >= min && value <= max;
    }

    public int Random() {
        return Rand.Int(min, max);
    }
    
}