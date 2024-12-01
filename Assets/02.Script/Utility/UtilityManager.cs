using UnityEngine;
using Background = EnumCollection.Background;
using Random = UnityEngine.Random;

//�ܼ� ����ε� ���� ���� ��ɵ� ��Ƴ���
public class UtilityManager
{
    //probability�� Ȯ���� true�� ��ȯ
    //ex) 0.2 => 20%�� Ȯ���� true, 80%�� Ȯ���� false ��ȯ
    public static bool CalculateProbability(float probability)
    {
        return Random.Range(0f, 1f) <= Mathf.Clamp(probability, 0f, 1f);
    }
    //Ȯ���� ���� ���ڵ��� ���� �������� Ư�� �ε����� ��ȯ�Ѵ�
    //ex) 0.2, 0.8 => 20%�� Ȯ���� 0, 80%�� Ȯ���� 1�� ��ȯ
    //ex) 60, 40 => 60%�� Ȯ���� 0, 40%�� Ȯ���� 1�� ��ȯ
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