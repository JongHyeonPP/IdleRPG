using GooglePlayGames.BasicApi;
using System.Collections;
using UnityEngine;

public class PigController : MonoBehaviour
{
    private bool _isRun = false;
    public GameObject pig;
    public void PigRun(bool Run)
    {
        _isRun = Run;
    }
    private void Update()
    {
        if (gameObject.name == "BigPig_Pink"&&pig.transform.position.x > 0.95f&&_isRun)
        {
            pig.transform.Translate(Vector3.left * 5f * Time.deltaTime);
        }
       
    }
    public IEnumerator TranslatePigs()
    {

        while (pig.transform.position.x > -2.95f&&gameObject.name=="Pig_Pink")
        {
            pig.transform.Translate(Vector3.left * 5f * Time.deltaTime);
            yield return null; 
        }
    }
}
