namespace JEngine.util; 

public struct FloatRange {
    public float min;
    public float max;

    public FloatRange(float min, float max) {
        this.min = min;
        this.max = max;
    }

    public bool Includes(float value) {
        return value >= min && value <= max;
    }

    public float Random() {
        return Rand.Float() * (max - min) + min;
    }
}