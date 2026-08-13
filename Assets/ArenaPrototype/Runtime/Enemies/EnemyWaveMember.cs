using System;
using UnityEngine;

namespace ArenaPrototype.Enemies
{
    public interface IWaveResettable
    {
        void ResetForWave(float enemyStatMultiplier);
    }

    public sealed class EnemyWaveMember : MonoBehaviour
    {
        [SerializeField, Min(1)] private int waveNumber = 1;

        public event Action<EnemyWaveMember> Defeated;

        public int WaveNumber => waveNumber;
        public bool IsDefeated { get; private set; }

        public void Configure(int newWaveNumber)
        {
            waveNumber = Mathf.Max(1, newWaveNumber);
        }

        public void PrepareForWave(float enemyStatMultiplier)
        {
            IsDefeated = false;
            gameObject.SetActive(true);

            MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IWaveResettable resettable)
                {
                    resettable.ResetForWave(enemyStatMultiplier);
                }
            }
        }

        public void MarkDefeated()
        {
            if (IsDefeated)
            {
                return;
            }

            IsDefeated = true;
            Defeated?.Invoke(this);
        }
    }
}
