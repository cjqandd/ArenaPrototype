using System.Collections.Generic;
using UnityEngine;

namespace ArenaPrototype.Enemies
{
    public sealed class EnemyAttackCoordinator : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maximumConcurrentAttackers = 2;
        [SerializeField, Min(0f)] private float minimumClaimInterval = 0.18f;

        private readonly HashSet<GameObject> claimants = new HashSet<GameObject>();
        private float nextClaimTime;

        public int ActiveClaimCount => claimants.Count;

        public bool TryClaim(GameObject attacker)
        {
            RemoveDestroyedClaimants();
            if (attacker == null)
            {
                return false;
            }

            if (claimants.Contains(attacker))
            {
                return true;
            }

            if (claimants.Count >= maximumConcurrentAttackers || Time.time < nextClaimTime)
            {
                return false;
            }

            claimants.Add(attacker);
            nextClaimTime = Time.time + minimumClaimInterval;
            return true;
        }

        public void Release(GameObject attacker)
        {
            if (attacker != null)
            {
                claimants.Remove(attacker);
            }
        }

        public void ResetClaims()
        {
            claimants.Clear();
            nextClaimTime = 0f;
        }

        private void RemoveDestroyedClaimants()
        {
            claimants.RemoveWhere(claimant => claimant == null);
        }
    }
}
