using System.Collections;
using UnityEngine;

public class StoryPlayerController : MonoBehaviour
{
    public GameObject player;
     
    public IEnumerator TranslatePlayerCoroutine()
    {
        float duration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (player == null) yield break;
            player.transform.Translate(Vector3.left * 5f * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        for (int i = 0; i < 2; i++)
        {
            player.transform.rotation = Quaternion.Euler(0, -1f, 0);
            yield return new WaitForSeconds(1f);

            player.transform.rotation = Quaternion.Euler(0, 180f, 0);
            yield return new WaitForSeconds(1f);
        }

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
        float duration = 2f;
        float elapsedTime = 0f;

        player.transform.rotation = Quaternion.Euler(0, -1f, 0);

        while (elapsedTime < duration)
        {
            if (player == null) yield break;
            player.transform.Translate(Vector3.left * 2f * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
