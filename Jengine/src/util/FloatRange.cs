namespace JEngine.util; 

public struct FloatRange {
    public float Min;
    public float Max;

    public FloatRange(float min, float max) {
        this.Min = min;
        this.Max = max;
    }

    public bool Includes(float value) {
        return value >= Min && value <= Max;
    }

    public float Random() {
        return Rand.Float() * (Max - Min) + Min;
    }

    public float Lerp(float t) {
        return Min + (Max - Min) * t;
    }
}