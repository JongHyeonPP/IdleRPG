using System.Collections.Generic;

public static class MediatorManager<T> where T : class
{
    private static List<T> mediators = new();

    // ��ü ���
    public static void RegisterMediator(T mediator)
    {
        if (!mediators.Contains(mediator))
        {
            mediators.Add(mediator);
        }
    }

    // ��ϵ� ��ü ����
    public static void UnregisterMediator(T mediator)
    {
        if (mediators.Contains(mediator))
        {
            mediators.Remove(mediator);
        }
    }

    // ��ϵ� ��� ��ü ����
    public static void UnregisterAllMediators()
    {
        mediators.Clear();
    }

    // ��ϵ� ��ü ��� ��ȯ
    public static List<T> GetRegisteredObjects()
    {
        return new List<T>(mediators); // ���纻 ��ȯ
    }
}
