using System;
using UnityEngine;

namespace Featherguard.Run
{
    public sealed class Wallet
    {
        public int Gold { get; private set; }
        public int Earned { get; private set; }
        public float GainMultiplier { get; set; } = 1f;

        public event Action Changed;

        public bool CanAfford(int amount) => Gold >= amount;

        public int Earn(int amount)
        {
            if (amount <= 0) return 0;
            amount = Mathf.RoundToInt(amount * GainMultiplier);
            Earned += amount;
            Gold += amount;
            Changed?.Invoke();
            return amount;
        }

        public void Deposit(int amount)
        {
            Gold = Mathf.Max(0, Gold + amount);
            Changed?.Invoke();
        }

        public bool TrySpend(int amount)
        {
            if (!CanAfford(amount)) return false;
            Gold -= amount;
            Changed?.Invoke();
            return true;
        }

        public void Lose(int amount) => Deposit(-Mathf.Abs(amount));
    }
}
