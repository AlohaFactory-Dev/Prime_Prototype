using CoconutMilk.Equipments;
using UnityEngine;

public class EquipmentStatViewer : MonoBehaviour
{
    private EquipmentStatCard[] _statCards;

    public void Init()
    {
        _statCards = GetComponentsInChildren<EquipmentStatCard>(true);
    }

    public void Refresh(Equipment equipment)
    {
        foreach (var card in _statCards)
        {
            card.Close();
        }

        if (equipment == null) return;

        var part = equipment.Type.Part;
        var stats = equipment.Type.EquipmentTypeTableData.StatBonus;
        foreach (var stat in stats)
        {
            foreach (var card in _statCards)
            {
                card.Show(part, stat);
            }
        }
    }
}