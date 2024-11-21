using System.Collections.Generic;
public static class MediatorManager<T> where T : class
{
    private static List<T> mediators = new();

    // 객체 등록
    public static void RegisterMediator(T mediator)
    {
        if (!mediators.Contains(mediator))
        {
            mediators.Add(mediator);
            UnityEngine.Debug.Log($"{mediator.GetType().Name}가 등록되었습니다.");
        }
    }

    // 등록된 객체 목록 반환
    public static List<T> GetRegisteredObjects()
    {
        return new List<T>(mediators);
    }
}
