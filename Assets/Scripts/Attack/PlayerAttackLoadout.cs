using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Attack
{
    public class PlayerAttackLoadout : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;

        private readonly List<AttackSlot> _slots = new();
        private bool _hasBuilt;

        public IReadOnlyList<AttackSlot> Slots => _slots;

        private void Awake()
        {
            BuildFromPlayerData();
        }

        private void OnEnable()
        {
            if (!_hasBuilt)
                return;

            if (_slots.Count == 0)
            {
                BuildFromPlayerData();
                return;
            }

            for (int i = 0; i < _slots.Count; i++)
            {
                if (!_slots[i].IsEquipped)
                    _slots[i].Equip(transform);
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < _slots.Count; i++)
                _slots[i].Unequip();
        }

        private void BuildFromPlayerData()
        {
            _hasBuilt = true;
            _slots.Clear();

            if (playerData == null)
                return;

            IReadOnlyList<AttackData> startingAttacks = playerData.StartingAttacks;
            for (int i = 0; i < startingAttacks.Count; i++)
            {
                AttackData attackData = startingAttacks[i];
                if (attackData == null)
                    continue;

                AttackSlot slot = new AttackSlot(attackData);
                slot.Equip(transform);
                _slots.Add(slot);
            }
        }

        public void Initialize(PlayerData sourcePlayerData)
        {
            playerData = sourcePlayerData;
            if (_hasBuilt)
            {
                for (int i = 0; i < _slots.Count; i++)
                    _slots[i].Unequip();
            }

            BuildFromPlayerData();
        }

        public void AddAttack(AttackData attackData)
        {
            if (attackData == null)
                return;

            AttackSlot slot = new AttackSlot(attackData);
            slot.Equip(transform);
            _slots.Add(slot);
        }

        public void RemoveAttack(AttackData attackData)
        {
            if (attackData == null)
                return;

            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                if (_slots[i].AttackData != attackData)
                    continue;

                _slots[i].Unequip();
                _slots.RemoveAt(i);
                break;
            }
        }

        /// <summary>Replace the whole loadout with the given attacks (used when restoring a save).</summary>
        public void RebuildFrom(IReadOnlyList<AttackData> attacks)
        {
            for (int i = 0; i < _slots.Count; i++)
                _slots[i].Unequip();
            _slots.Clear();

            _hasBuilt = true;

            if (attacks == null)
                return;

            for (int i = 0; i < attacks.Count; i++)
            {
                if (attacks[i] == null)
                    continue;

                AttackSlot slot = new AttackSlot(attacks[i]);
                slot.Equip(transform);
                _slots.Add(slot);
            }
        }
    }
}
