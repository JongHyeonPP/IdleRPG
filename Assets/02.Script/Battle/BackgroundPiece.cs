using UnityEngine;

public class BackgroundPiece : MonoBehaviour
{
    
    public void Move()
    {
        transform.Translate(Vector2.left*Time.deltaTime*BattleManager.speed);
        if (transform.position.x < -20f)
        {
            transform.localPosition += Vector3.right * 64f;
        }
    }
}
