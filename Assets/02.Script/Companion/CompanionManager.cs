using UnityEngine;

public class CompanionManager : MonoBehaviour
{
    public static CompanionManager instance;
    public CompanionStatus[] companionStatusArr;
    public static int EXPINTERVAL = 5;
    private void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
