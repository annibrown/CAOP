using UnityEngine;

public static class UnitConverter
{
    public static float InchesToMeters(float inches)
    {
        return inches * 0.0254f;
    }

    public static float MetersToInches(float meters)
    {
        return meters / 0.0254f;
    }
}

