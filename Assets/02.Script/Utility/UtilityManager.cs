using UnityEngine;
using Random = UnityEngine.Random;

public static class UtilityManager
{
    private static float grayScaleAmount = 0.8f;

    public static bool CalculateProbability(float probability)
    {
        return Random.Range(0f, 1f) <= Mathf.Clamp(probability, 0f, 1f);
    }

    public static int AllocateProbability(params float[] probabilities)
    {
        float total = 0f;
        foreach (float prob in probabilities)
        {
            total += prob;
        }
        float randomValue = Random.Range(0f, total);
        float cumulative = 0f;

        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulative += probabilities[i];
            if (randomValue < cumulative)
            {
                return i;
            }
        }
        return 0;
    }
}
