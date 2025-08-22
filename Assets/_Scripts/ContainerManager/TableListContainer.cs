// TableListContainer 클래스 작성

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using _DataTable.Script;
using Cysharp.Threading.Tasks;

public static class TableListContainer
{
    private static readonly List<ITableList> TableLists = new List<ITableList>();

    static TableListContainer()
    {
        // 현재 어셈블리에서 ITableList를 구현한 모든 클래스 검색
        var tableListTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(ITableList).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in tableListTypes)
        {
            // 각 타입의 인스턴스를 생성하여 리스트에 추가
            if (Activator.CreateInstance(type) is ITableList instance)
            {
                TableLists.Add(instance);
            }
        }
    }

    public static async UniTask InitAllTables()
    {
        await UniTask.WhenAll(TableLists.Select(tableList => tableList.Init()));
    }

    public static T Get<T>() where T : ITableList
    {
        foreach (var tableList in TableLists)
        {
            if (tableList is T)
            {
                return (T)tableList;
            }
        }

        return default;
    }
}