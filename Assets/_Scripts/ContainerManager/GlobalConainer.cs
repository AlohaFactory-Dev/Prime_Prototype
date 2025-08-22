using UnityEngine;
using Zenject;

public static class GlobalConainer 
{
    public static T Get<T>()
    {
        return _container.Resolve<T>();
    }

    public static T GetWithId<T>(object id)
    {
        return _container.ResolveId<T>(id);
    }

    private static DiContainer _container;

    public static void Initialize(DiContainer container)
    {
        _container = container;
    }
}