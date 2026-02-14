using UnityEngine;
using System;

namespace Farming
{
    /// <summary>
    /// Tracks the player's money. Earns money every time a progress square fills up.
    /// </summary>
    public class PlayerEconomy : MonoBehaviour
    {
        [Header("Economy Settings")]
        [SerializeField] private int startingMoney = 0;
    [SerializeField] private int moneyPerSquare = 10; // Money earned per completed progress square

    private int currentMoney;

    /// <summary>Fires whenever money changes (newAmount)</summary>
    public static event Action<int> OnMoneyChanged;

    /// <summary>Fires when money is earned (amountEarned)</summary>
    public static event Action<int> OnMoneyEarned;

    public int CurrentMoney => currentMoney;
    public int MoneyPerSquare => moneyPerSquare;

    void Awake()
    {
        currentMoney = startingMoney;
    }

    void Start()
    {
        OnMoneyChanged?.Invoke(currentMoney);
    }

    public void EarnMoney(int amount)
    {
        currentMoney += amount;
        Debug.Log($"[Economy] Earned ${amount}! Total: ${currentMoney}");
        OnMoneyEarned?.Invoke(amount);
        OnMoneyChanged?.Invoke(currentMoney);
    }

    /// <summary>
    /// Try to spend money. Returns true if successful.
    /// </summary>
    public bool TrySpend(int amount)
        {
            if (currentMoney >= amount)
            {
                currentMoney -= amount;
                Debug.Log($"[Economy] Spent ${amount}. Remaining: ${currentMoney}");
                OnMoneyChanged?.Invoke(currentMoney);
                return true;
            }

            Debug.Log($"[Economy] Not enough money! Need ${amount}, have ${currentMoney}");
            return false;
        }

        public void AddMoney(int amount)
        {
            EarnMoney(amount);
        }
    }
}
