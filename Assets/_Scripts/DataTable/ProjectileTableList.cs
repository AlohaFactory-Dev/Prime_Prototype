using System;
using System.Collections.Generic;
using _DataTable.Script;
using Aloha.Coconut;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

public class ProjectileTable
{
    [CSVColumn] public string id;
    [CSVColumn] public bool onLookat;
    [CSVColumn] public ProjectileSpeedType speedType;
    [CSVColumn] public float speedRandomMin;
    [CSVColumn] public float speedRandomMax;
    [CSVColumn] public float delayRandomMin;
    [CSVColumn] public float delayRandomMax;
    [CSVColumn] public float scale;
    [CSVColumn] public FireType fireType;
}

public class ProjectileTableList : ITableList
{
    private List<ProjectileTable> _projectileTables = new();
    private readonly Dictionary<string, ProjectileTable> _cachedInfo = new();

    public async UniTask Init()
    {
        _projectileTables = await TableManager.GetAsync<ProjectileTable>("Projectile");
        _cachedInfo.Clear();
    }

    public ProjectileTable GetProjectileTable(string id)
    {
        if (_cachedInfo.TryGetValue(id, out var objectInfo))
        {
            return objectInfo;
        }

        var info = _projectileTables.Find(a => a.id == id);
        if (info == null)
        {
            Debug.LogError($"ProjectileTable not found. id: {id}");
            return null;
        }

        _cachedInfo.Add(id, info);
        return info;
    }
}