using System.Collections.Generic;
using HorrorRPG.Battle;
using HorrorRPG.Core;
using UnityEngine;

namespace HorrorRPG.Inventory
{
    /// <summary>Scene adapter for session-owned recent weapon order.</summary>
    public sealed class RecentWeaponsManager : MonoBehaviour, IGameContextReceiver
    {
        [SerializeField] private WeaponDatabase weaponDatabase;

        private RecentWeaponsRuntimeState recentWeapons;

        /// <summary>Connects this adapter to session-owned recent weapon state.</summary>
        public void Initialize(GameContext context)
        {
            recentWeapons = context?.Session.RecentWeapons ?? throw new System.ArgumentNullException(nameof(context));
        }

        /// <summary>Releases the session state reference.</summary>
        public void Deinitialize() => recentWeapons = null;

        /// <summary>Adds a weapon ID to the session-owned recent order.</summary>
        public void AddRecentWeapon(WeaponData weapon)
        {
            if (weapon != null && recentWeapons != null) recentWeapons.Add(GetWeaponId(weapon));
        }

        /// <summary>Resolves recent weapon IDs through the configured database.</summary>
        public List<WeaponData> GetRecentWeapons()
        {
            var weapons = new List<WeaponData>();
            if (weaponDatabase == null || recentWeapons == null) return weapons;
            foreach (string weaponId in recentWeapons.WeaponIds)
            {
                WeaponData weapon = weaponDatabase.GetWeaponByName(weaponId);
                if (weapon != null) weapons.Add(weapon);
            }
            return weapons;
        }

        private static string GetWeaponId(WeaponData weapon) => weapon.Id;
    }
}
