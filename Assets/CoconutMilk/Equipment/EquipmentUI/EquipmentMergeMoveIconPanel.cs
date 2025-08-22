using DG.Tweening;
using Aloha.CoconutMilk;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class EquipmentMergeMoveIconPanel : MonoBehaviour
{
    [SerializeField] private EquipmentMergeIngredientSlotContainer ingredientSlotContainer;
    [SerializeField] private Image image;
    [SerializeField] private float moveDuration = 0.25f;
    [Inject] private PropertyIconPool _propertyIconPool;

    public void Show()
    {
        image.raycastTarget = false;
    }

    public void MoveIcon(EquipmentIcon equipmentIcon)
    {
        if (image.raycastTarget) return;

        if (ingredientSlotContainer.leftSlot.IsEmpty)
        {
            MoveToSlot(
                equipmentIcon,
                ingredientSlotContainer.leftSlot.transform.position,
                () => ingredientSlotContainer.SetSlot(equipmentIcon)
            );
        }
        else if (ingredientSlotContainer.FirstSlotIsEmpty)
        {
            MoveToSlot(
                equipmentIcon,
                ingredientSlotContainer.FirstSlot.transform.position,
                () => ingredientSlotContainer.SetFirstSlot(equipmentIcon)
            );
        }
        else if (ingredientSlotContainer.SecondSlotIsEmpty)
        {
            MoveToSlot(
                equipmentIcon,
                ingredientSlotContainer.SecondSlot.transform.position,
                () => ingredientSlotContainer.SetSecondSlot(equipmentIcon)
            );
        }
    }

    private void MoveToSlot(EquipmentIcon equipmentIcon, Vector2 targetPosition, System.Action onArrived)
    {
        image.raycastTarget = true;
        var icon = _propertyIconPool.Get(equipmentIcon.Equipment, (RectTransform)image.transform);
        icon.SetPosition(equipmentIcon.transform.position);
        icon.transform.DOMove(targetPosition, moveDuration).OnComplete(() =>
        {
            onArrived?.Invoke();
            image.raycastTarget = false;
            equipmentIcon.OnCheck();
            icon.Remove();
            ingredientSlotContainer.CheckAllFilledSlots();
        });
    }
}