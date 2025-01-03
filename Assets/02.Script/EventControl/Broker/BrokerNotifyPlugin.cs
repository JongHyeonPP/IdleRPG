#if UNITY_EDITOR
using System;
using System.Collections.Generic;
public class BrokerNotifyPlugin
{
    private readonly Type brokerType;

    public string BrokerName { get; }

    public event Action OnChanged; // 변경 알림 이벤트

    public BrokerNotifyPlugin(Type brokerType, string brokerName)
    {
        this.brokerType = brokerType;
        BrokerName = brokerName;
    }

    public string[] GetAllDelegates()
    {
        var delegateNames = new List<string>();
        var fields = brokerType.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            if (typeof(Delegate).IsAssignableFrom(field.FieldType))
            {
                delegateNames.Add(field.Name);
            }
        }

        return delegateNames.ToArray();
    }

    public Delegate[] GetDelegateInvocationList(string delegateName)
    {
        var field = brokerType.GetField(delegateName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (field == null) return null;

        var del = field.GetValue(null) as Delegate;
        return del?.GetInvocationList();
    }
}
#endif