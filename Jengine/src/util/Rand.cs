namespace JEngine.util; 

public class Rand {
    /// <summary>
    /// Generates a random int between min (inclusive) and max (exclusive)
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int Int(int min, int max) {
        var rand = new Random();
        var randomNum = rand.Next((max - min)) + min;
        return randomNum;
    }
    
    public static byte Byte() {
        var rand = new Random();
        var randomNum = (byte)rand.Next(0, 255);
        return randomNum;
    }

    public static float Float() {
        var rand = new Random();
        return (float)rand.NextDouble();
    }
    
    public static float Float(float min, float max) {
        var rand = new Random();
        var randomNum = (float)rand.NextDouble() * (max - min) + min;
        return randomNum;
    }

    public static bool Bool() {
        return Int(0, 2) == 1;
    }
    
    public static T EnumValue<T>() {
        var v = Enum.GetValues(typeof (T));
        return (T) v.GetValue(Int(0, v.Length));
    }

    public static bool Chance(float chance) {
        return Float(0, 1) < chance;
    }
}