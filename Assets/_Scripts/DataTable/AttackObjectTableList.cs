using System;
using System.Collections.Generic;
using _DataTable.Script;
using Aloha.Coconut;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

public class AttackObjectTable
{
    [CSVColumn] public string id;
    [CSVColumn] public bool onLookat;
    [CSVColumn] public AttackObjectSpeedType speedType;
    [CSVColumn] public float speedRandomMin;
    [CSVColumn] public float speedRandomMax;
    [CSVColumn] public float delayRandomMin;
    [CSVColumn] public float delayRandomMax;
    [CSVColumn] public float scale;
    [CSVColumn] public FireType fireType;
}

public class AttackObjectTableList : ITableList
{
    private List<AttackObjectTable> _attackObjectTables = new();
    private readonly Dictionary<string, AttackObjectTable> _cachedInfo = new();

    public async UniTask Init()
    {
        _attackObjectTables = await TableManager.GetAsync<AttackObjectTable>("AttackObject");
        _cachedInfo.Clear();
    }

    public AttackObjectTable GetAttackObjectTable(string id)
    {
        if (_cachedInfo.TryGetValue(id, out var objectInfo))
        {
            return objectInfo;
        }

        var info = _attackObjectTables.Find(a => a.id == id);
        if (info == null)
        {
            Debug.LogError($"AttackObjectInfo not found. id: {id}");
            return null;
        }

        _cachedInfo.Add(id, info);
        return info;
    }
}