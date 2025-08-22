using UnityEngine;

namespace Aloha.Particle
{
    public enum ParticleType { Basic, Floating, Scale }

    public abstract class RecycleParticle : RecycleObject
    {
        public abstract ParticleType particleType { get; }


        public abstract void Play();

        public void Init(Vector2 position)
        {
            transform.position = position;
        }
    }
}