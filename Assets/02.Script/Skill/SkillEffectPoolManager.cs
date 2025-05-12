using System.Collections.Generic;
using UnityEngine;

public class SkillEffectPoolManager : MonoBehaviour
{
    public static SkillEffectPoolManager Instance { get; private set; }

    private Dictionary<SkillData, Queue<GameObject>> pools = new Dictionary<SkillData, Queue<GameObject>>();
    private Dictionary<GameObject, SkillData> effectToSkillDataMap = new Dictionary<GameObject, SkillData>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public GameObject SpawnEffect(SkillData skillData, Vector3 position)
    {
        if (skillData == null || skillData.visualEffectPrefab == null)
            return null;

        GameObject effect = null;

        if (pools.TryGetValue(skillData, out var queue) && queue.Count > 0)
        {
            effect = queue.Dequeue();
        }
        else
        {
            effect = Instantiate(skillData.visualEffectPrefab);
            if (effect.GetComponent<SkillEffectReturner>() == null)
                effect.AddComponent<SkillEffectReturner>();
        }

        effect.transform.position = position;
        effect.SetActive(true);

        effectToSkillDataMap[effect] = skillData;

        return effect;
    }

    public void ReturnEffect(GameObject effect)
    {
        if (effect == null)
            return;

        if (effectToSkillDataMap.TryGetValue(effect, out var skillData))
        {
            effect.SetActive(false);

            if (!pools.ContainsKey(skillData))
            {
                pools[skillData] = new Queue<GameObject>();
            }

            pools[skillData].Enqueue(effect);
            effectToSkillDataMap.Remove(effect);
        }
        else
        {
            Destroy(effect);
        }
    }
   
}
