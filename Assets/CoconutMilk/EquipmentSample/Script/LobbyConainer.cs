using Zenject;

namespace Aloha.CoconutMilk.EquipmentSample
{
    public class LobbyConainer
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
}
