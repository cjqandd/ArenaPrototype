using System.Collections.Generic;
using UnityEngine;

namespace ArenaPrototype.Weapons
{
    public sealed class WeaponDropRegistry : MonoBehaviour
    {
        private readonly List<WeaponInstance> activeDrops = new List<WeaponInstance>();

        public void Register(WeaponInstance weapon)
        {
            if (weapon != null && !activeDrops.Contains(weapon))
            {
                activeDrops.Add(weapon);
            }
        }

        public void CleanupUnequippedDrops()
        {
            for (int i = activeDrops.Count - 1; i >= 0; i--)
            {
                WeaponInstance weapon = activeDrops[i];
                if (weapon == null)
                {
                    activeDrops.RemoveAt(i);
                }
                else if (weapon.State != WeaponState.Equipped)
                {
                    activeDrops.RemoveAt(i);
                    Destroy(weapon.gameObject);
                }
            }
        }
    }
}
