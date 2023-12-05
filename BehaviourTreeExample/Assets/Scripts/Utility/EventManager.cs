using System.Collections.Generic;

public enum EventType
{
    GameOver = 0,
}

public static class EventManager
{
    public static Dictionary<EventType, System.Action> Events { get { return events; } }
    private static Dictionary<EventType, System.Action> events = new();

    public static void AddListener(EventType type, System.Action action)
    {
        if (!Events.ContainsKey(type))
        {
            Events.Add(type, action);
        }
        else
        {
            Events[type] += action;
        }
    }

    public static void RemoveListener(EventType type, System.Action action)
    {
        if (!Events.ContainsKey(type)) { return; }
        Events[type] -= action;
    }

    public static void InvokeEvent(EventType type)
    {
        if (!Events.ContainsKey(type)) { return; }
        Events[type]?.Invoke();
    }
}