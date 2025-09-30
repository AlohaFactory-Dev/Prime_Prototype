using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace FactorySystem
{
    public class FactoryManager : MonoBehaviour
    {
        public readonly ProjectileFactory ProjectileFactory = new();
        public readonly FloatingTextFactory FloatingTextFactory = new();

        public async Task Init(DiContainer container)
        {
            await Task.WhenAll(
                ProjectileFactory.Initialize(container, transform),
                FloatingTextFactory.Initialize(container, transform)
            );
        }
    }
}