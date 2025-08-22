using UnityEngine;

/// <summary>
/// 자원 생산 건물 - 4.5초마다 자원 생산
/// </summary>
public class ResourceBuilding : Building
{
    [Header("Resource Building Settings")]
    [SerializeField] private int resourceAmount = 100;
    [SerializeField] private string resourceType = "Gold";
    
    private int totalProduced = 0;

    protected override void Start()
    {
        buildingName = "Resource Building";
        actionInterval = 4.5f; // 4.5초마다 실행
        base.Start();
    }

    protected override void PerformAction()
    {
        ProduceResource();
    }

    private void ProduceResource()
    {
        totalProduced += resourceAmount;
        Debug.Log($"[{buildingName}] Produced {resourceAmount} {resourceType}! Total: {totalProduced}");
        
        // 여기서 실제 게임 로직 처리
        // 예: 플레이어 자원 증가, UI 업데이트 등
    }

    public int GetTotalProduced() => totalProduced;
}
