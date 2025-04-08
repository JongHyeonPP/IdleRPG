using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract  class AbstractStoryObjectController : MonoBehaviour
{
    [Header("주인공 오브젝트")]
    public GameObject protagonist;  
    protected Animator protagonistAnimator; 

    [Header("기타 스토리 오브젝트")]
    public List<GameObject> additionalObjects = new List<GameObject>(); 

    protected Dictionary<string, GameObject> objectDictionary = new Dictionary<string, GameObject>(); 

    protected virtual void Awake()
    {
        InitializeObjects();
    }

    private void InitializeObjects()
    {
        if (protagonist != null)
        {
            protagonistAnimator = protagonist.GetComponent<Animator>();
            objectDictionary["UnitRoot"] = protagonist;
        }

        foreach (var obj in additionalObjects)
        {
            if (obj != null && !objectDictionary.ContainsKey(obj.name))
            {
                objectDictionary[obj.name] = obj;
            }
        }
    }

    public GameObject GetTargetObject(string name)
    {
        objectDictionary.TryGetValue(name, out GameObject obj);
        return obj;
    }

    protected IEnumerator MoveObject(GameObject target, Vector3 direction, float speed, float duration)
    {
        if (target == null) yield break;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            target.transform.Translate(direction * speed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    protected IEnumerator RotateObject(GameObject target, float angle, float delay)
    {
        if (target == null) yield break;
        yield return new WaitForSeconds(delay);
        target.transform.rotation = Quaternion.Euler(0, angle, 0);
    }

    protected void SetAnimatorState(GameObject target, string parameter, float value)
    {
        if (target == null) return;
        Animator animator = target.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetFloat(parameter, value);
        }
    }

    public abstract IEnumerator Run(GameObject target);
    public abstract IEnumerator RunAway(GameObject target);
}
