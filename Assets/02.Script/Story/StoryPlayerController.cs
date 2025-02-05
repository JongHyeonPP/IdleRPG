using System.Collections;
using UnityEngine;

public class StoryPlayerController : MonoBehaviour
{
    public Animator anim;
    public GameObject player;
     
    public IEnumerator TranslatePlayerCoroutine()
    {
        while (player.transform.position.x < -1.16f)
        {
            player.transform.Translate(Vector3.left * 5f * Time.deltaTime);
            yield return null; 
        }

        StartCoroutine(RotatePlayer());
    }
    private IEnumerator RotatePlayer()
    {
        for (int i = 0; i < 2; i++) 
        {
            player.transform.rotation = Quaternion.Euler(0, -1f, 0);
            yield return new WaitForSeconds(1f);

            player.transform.rotation = Quaternion.Euler(0, 180f, 0);
            yield return new WaitForSeconds(1f);
        }

    }
    public IEnumerator Run()
    {
        player.transform.rotation = Quaternion.Euler(0, -1f, 0);
        while (player.transform.position.x > -4.16f)
        {
            player.transform.Translate(Vector3.left * 5f * Time.deltaTime);
            yield return null;
        }
      
    }
}
