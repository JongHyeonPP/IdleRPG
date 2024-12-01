using UnityEngine;
using Background = EnumCollection.Background;
using Random = UnityEngine.Random;

//단순 계산인데 자주 쓰는 기능들 모아놓기
public class UtilityManager
{
    //probability의 확률로 true를 반환
    //ex) 0.2 => 20%의 확률로 true, 80%의 확률로 false 반환
    public static bool CalculateProbability(float probability)
    {
        return Random.Range(0f, 1f) <= Mathf.Clamp(probability, 0f, 1f);
    }
    //확률로 들어온 숫자들의 합을 기준으로 특정 인덱스를 반환한다
    //ex) 0.2, 0.8 => 20%의 확률로 0, 80%의 확률로 1을 반환
    //ex) 60, 40 => 60%의 확률로 0, 40%의 확률로 1을 반환
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