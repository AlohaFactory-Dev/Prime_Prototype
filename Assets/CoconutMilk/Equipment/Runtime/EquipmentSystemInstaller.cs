using Zenject;

namespace CoconutMilk.Equipments
{
    public class EquipmentSystemInstaller  : Installer<EquipmentSystemInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<EquipmentSystem>().AsSingle().NonLazy();
            Container.Bind<EquipmentDatabase>().AsSingle().NonLazy();
            Container.Bind<Equipment.Factory>().AsSingle().NonLazy();
            Container.Bind<EquipmentSlot.Factory>().AsSingle().NonLazy();
        }
    }
}