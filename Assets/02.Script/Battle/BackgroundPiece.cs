using UnityEngine;

public class BackgroundPiece : MonoBehaviour
{
    
    public void Move(float speed)
    {
        transform.Translate(Vector2.left*Time.deltaTime*speed);
        if (transform.position.x < -20f)
        {
            transform.localPosition += Vector3.right * 63.98f;
        }
    }
}
