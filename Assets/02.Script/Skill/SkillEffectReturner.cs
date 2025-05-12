using UnityEngine;

public class SkillEffectReturner : MonoBehaviour
{
    public void ReturnToPool()
    {
        SkillEffectPoolManager.Instance.ReturnEffect(gameObject);
    }
}
