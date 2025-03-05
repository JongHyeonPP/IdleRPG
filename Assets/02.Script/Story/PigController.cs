
using System.Collections;
using UnityEngine;

public class PigController : MonoBehaviour
{
    private bool _isRun = false;
    public GameObject pig;
    private float bigPigTargetX = 2f;  
    private float pigTargetX = -8.00f;
   
    public IEnumerator TranslateBigPigs()
    {

        if (gameObject.name != "BigPig_Pink") yield break;

        float duration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (pig == null) yield break;
            pig.transform.Translate(Vector3.left * 4f * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    public IEnumerator TranslatePigs()
    {
        if (gameObject.name != "Pig_Pink") yield break;

        float duration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (pig == null) yield break;
            pig.transform.Translate(Vector3.left * 2f * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
