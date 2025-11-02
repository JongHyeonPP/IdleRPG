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
        }
    }

    // 등록된 객체 해제
    public static void UnregisterMediator(T mediator)
    {
        if (mediators.Contains(mediator))
        {
            mediators.Remove(mediator);
        }
    }

    // 등록된 모든 객체 해제
    public static void UnregisterAllMediators()
    {
        mediators.Clear();
    }

    // 등록된 객체 목록 반환
    public static List<T> GetRegisteredObjects()
    {
        return new List<T>(mediators); // 복사본 반환
    }
}
