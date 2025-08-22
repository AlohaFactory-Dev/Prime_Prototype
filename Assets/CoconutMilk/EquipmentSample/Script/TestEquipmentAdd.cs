using Aloha.Coconut;

namespace Aloha.CoconutMilk.EquipmentSample
{
    public class TestEquipmentAdd
    {
        public TestEquipmentAdd()
        {
            var propertyManager = LobbyConainer.Get<PropertyManager>();

            for (int i = 0; i < 10; i++)
            {
                propertyManager.Obtain(new Property(PropertyTypeAlias.Weapon_Normal_01_Common, 10), PlayerAction.TEST);
                propertyManager.Obtain(new Property(PropertyTypeAlias.Ring_Normal_01_Common, 10), PlayerAction.TEST);
            }

            propertyManager.Obtain(new Property(PropertyTypeAlias.EquipmentScroll, 10000), PlayerAction.TEST);
            propertyManager.Obtain(new Property(PropertyTypeAlias.EquipmentStone, 10000), PlayerAction.TEST);
        }
    }
}