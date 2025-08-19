using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace FinancePlanner.Data
{
    [CreateAssetMenu(menuName = "Finance Planner/Data Store", fileName = "FinanceData")]
    public class DataStore : ScriptableObject
    {
        [SerializeField] private long openingBalanceCents = 100_000_00; // 100 000 ₽
        [SerializeField] private List<Tx> transactions = new();

        public IReadOnlyList<Tx> Transactions => transactions;
        public long OpeningBalanceCents => openingBalanceCents;

        public event Action OnChanged;

        public void AddTx(Tx tx)
        {
            if (string.IsNullOrEmpty(tx.id)) tx.id = Guid.NewGuid().ToString("N");
            transactions.Add(tx);
            OnChanged?.Invoke();
        }

        // агрегаты за месяц
        public (long income, long expense, long savings, long balanceNow) GetMonthAgg(DateTime today)
        {
            var monthStart = new DateTime(today.Year, today.Month, 1);
            long inc = 0, exp = 0;

            foreach (var t in transactions)
            {
                var dt = DateTime.Parse(t.isoDate);
                if (dt < monthStart) continue;
                if (t.type == TxType.Income) inc += t.amountCents;
                else exp += t.amountCents;
            }

            long savings = Math.Max(0, inc - exp);
            long balanceNow = OpeningBalanceCents + inc - exp;
            return (inc, exp, savings, balanceNow);
        }
    }
}
