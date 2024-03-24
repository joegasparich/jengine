namespace JEngine.util; 

public class IntRange {
    public int Min;
    public int Max;

    public IntRange(int min, int max) {
        Min = min;
        Max = max;
    }

    public bool Includes(int value) {
        return value >= Min && value <= Max;
    }

    public int Random() {
        return Rand.Int(Min, Max);
    }
    
}