using System.Collections.Generic;
using UnityEngine;

// stores everything about one delivery 
public struct DeliveryResult
{
    public int Correct; // correct candies
    public int Wrong; // wrong types + extras beyond what was asked
    public int Missing; // requested but not delivered
    public bool Perfect;
    public bool ColorMatch; // bag color == kid color

    public int CandyPoints;
    public int SpeedBonus;
    public int PerfectBonus;
    public int ColorBonus;
    public int Total;
}


public class ScoreCalculator
{
    public const int PointsPerCorrect = 50;
    public const int PenaltyPerWrong = 25;
    public const int MaxSpeedBonus = 100;
    public const int PerfectBonus = 50;
    public const int ColorMatchBonus = 100;

    public static DeliveryResult Calculate(Order order, List<CandyData> delivered, BagColor bagColor, BagColor kidColor, float timeLeft, float totalTime)
    {
        DeliveryResult r = new DeliveryResult();

        // 1. count whats in the bag for each candy
        Dictionary<CandyData, int> counts = new Dictionary<CandyData, int>();
        foreach (CandyData c in delivered)
        {
            counts.TryGetValue(c, out int n);
            counts[c] = n + 1;
        }

        // 2. compare delivered type with order
        foreach (KeyValuePair<CandyData, int> pair in counts)
        {
            order.Items.TryGetValue(pair.Key, out int wanted); // 0 if the kid didnt want it
            r.Correct += Mathf.Min(pair.Value, wanted);
            r.Wrong += Mathf.Max(0, pair.Value - wanted);
        }
        r.Missing = order.TotalCount - r.Correct;

        // 3. tally points
        r.CandyPoints = r.Correct * PointsPerCorrect - r.Wrong * PenaltyPerWrong;
        r.Perfect = r.Missing == 0 && r.Wrong == 0;
        r.ColorMatch = bagColor == kidColor;

        // bonus only applies to a fully correct order
        if (r.Perfect)
        {
            r.SpeedBonus = Mathf.RoundToInt(MaxSpeedBonus * Mathf.Clamp01(timeLeft / totalTime));
            r.PerfectBonus = PerfectBonus;
            if (r.ColorMatch) r.ColorBonus = ColorMatchBonus;
        }

        // never negative for a single delivery
        r.Total = Mathf.Max(0, r.CandyPoints + r.SpeedBonus + r.PerfectBonus + r.ColorBonus);
        return r;
    }
}
