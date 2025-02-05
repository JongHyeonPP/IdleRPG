
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
        
        while (gameObject.name == "BigPig_Pink" && pig.transform.position.x > bigPigTargetX)
        {
            pig.transform.Translate(Vector3.left * 5f * Time.deltaTime);
            yield return null;
        }
    }
    public IEnumerator TranslatePigs()
    {

        while (pig.transform.position.x > pigTargetX && gameObject.name=="Pig_Pink")
        {
            pig.transform.Translate(Vector3.left * 5f * Time.deltaTime);
            yield return null; 
        }
    }
}
