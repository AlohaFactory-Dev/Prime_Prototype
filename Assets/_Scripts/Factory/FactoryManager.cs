using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace FactorySystem
{
    public class FactoryManager : MonoBehaviour
    {
        public readonly AttackObjectFactory AttackObjectFactory = new();
        public readonly FloatingTextFactory FloatingTextFactory = new();

        public async Task Init(DiContainer container)
        {
            await Task.WhenAll(
                AttackObjectFactory.Initialize(container, transform),
                FloatingTextFactory.Initialize(container, transform)
            );
        }
    }
}