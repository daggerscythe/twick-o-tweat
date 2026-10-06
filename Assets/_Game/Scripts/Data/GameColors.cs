using UnityEngine;

// 4 colors shared by bags and kids
public enum BagColor
{
    LightBlue,
    Yellow,
    Red,
    Green
}

// static so it can't be put on a GameObject
public static class GameColors
{
    // turn enum into a Unity color for material and UI
    public static Color ToColor(BagColor c)
    {
        switch(c)
        {
            case BagColor.LightBlue: return new Color(0.45f, 0.80f, 1.00f);
            case BagColor.Yellow: return new Color(1.00f, 0.88f, 0.20f);
            case BagColor.Red: return new Color(0.90f, 0.20f, 0.20f);
            case BagColor.Green: return new Color(0.30f, 0.85f, 0.35f);
            default: return Color.white;
        }
    }

    // human readable name
    public static string ToName(BagColor c)
    {
        return c == BagColor.LightBlue ? "Light Blue" : c.ToString();
    }

    // pick 1 of 4 colors randomly
    public static BagColor RandomColor()
    {
        return (BagColor)Random.Range(0, 4);
    }
}