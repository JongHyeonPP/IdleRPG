using System.Collections.Generic;

/// <summary>
/// MediatorManager는 특정 타입의 객체들을 중앙에서 관리하는 정적 클래스.
/// 등록/해제/조회 기능을 제공하여, 
/// 개별 객체들이 서로 직접 참조하지 않고 Mediator를 통해 간접적으로 연결될 수 있도록 한다.
/// (예: Player 이동에 맞춰 Enemy, Background가 동작하도록 연결)
/// </summary>
/// <typeparam name="T">중재자(Mediator)로 관리할 인터페이스나 클래스 타입</typeparam>
public static class MediatorManager<T> where T : class
{
    // 등록된 객체들을 보관하는 리스트
    private static List<T> mediators = new();

    /// <summary>
    /// Mediator 객체 등록
    /// </summary>
    /// <param name="mediator">등록할 객체</param>
    public static void RegisterMediator(T mediator)
    {
        if (!mediators.Contains(mediator))
        {
            mediators.Add(mediator);
        }
    }

    /// <summary>
    /// Mediator 객체 해제
    /// </summary>
    /// <param name="mediator">해제할 객체</param>
    public static void UnregisterMediator(T mediator)
    {
        if (mediators.Contains(mediator))
        {
            mediators.Remove(mediator);
        }
    }

    /// <summary>
    /// 등록된 모든 Mediator 해제
    /// </summary>
    public static void UnregisterAllMediators()
    {
        mediators.Clear();
    }

    /// <summary>
    /// 현재 등록된 Mediator 객체 목록 반환
    /// </summary>
    /// <returns>등록된 Mediator들의 복사본 리스트</returns>
    public static IReadOnlyList<T> GetRegisteredObjects() => mediators;
}
