using UnityEngine;

public class BackgroundPiece : MonoBehaviour, IMoveByPlayer
{
    public Transform Transform => transform;

    private void Start()
    {
        MediatorManager<IMoveByPlayer>.RegisterMediator(this);
    }
    private void Update()
    {
        if (transform.position.x < -20f)
        {
            transform.localPosition += Vector3.right * 63.98f;
        }
    }
}
