using System;
using System.Collections.Generic;
using System.Globalization;

using UnityEngine;

namespace FinancePlanner.Data
{
    [CreateAssetMenu(menuName = "Finance Planner/Data Store", fileName = "FinanceData")]
    public class DataStore : ScriptableObject
    {
        [Header("Initial State")]
        [SerializeField] private long openingBalanceCents = 100_000_00;
        [SerializeField] private List<Tx> transactions = new();

        public IReadOnlyList<Tx> Transactions => transactions;
        public long OpeningBalanceCents => openingBalanceCents;

        public event Action OnChanged;

        public Tx AddTx(Tx tx)
        {
            if (tx == null) return null;
            if (string.IsNullOrWhiteSpace(tx.id))
                tx.id = Guid.NewGuid().ToString("N");
            if (!TryEnsureIso(tx.isoDate, out _))
                tx.isoDate = DateTime.Today.ToString("yyyy-MM-dd");

            transactions.Add(tx);
            OnChanged?.Invoke();
            return tx;
        }

        public Tx AddTx(TxType type, long amountCents, DateTime date,
                        string category = null, string comment = null, string currency = "RUB")
        {
            return AddTx(new Tx
            {
                type = type,
                amountCents = amountCents,
                isoDate = date.ToString("yyyy-MM-dd"),
                category = category,
                comment = comment,
                currency = currency
            });
        }

        public bool UpdateTx(Tx tx)
        {
            if (tx == null || string.IsNullOrWhiteSpace(tx.id)) return false;
            var i = transactions.FindIndex(t => t.id == tx.id);
            if (i < 0) return false;

            if (!TryEnsureIso(tx.isoDate, out _))
                tx.isoDate = DateTime.Today.ToString("yyyy-MM-dd");

            transactions[i] = tx;
            OnChanged?.Invoke();
            return true;
        }

        public bool UpdateTx(string id, Action<Tx> mutator)
        {
            if (string.IsNullOrWhiteSpace(id) || mutator == null) return false;
            var i = transactions.FindIndex(t => t.id == id);
            if (i < 0) return false;

            var copy = transactions[i];
            mutator(copy);

            if (!TryEnsureIso(copy.isoDate, out _))
                copy.isoDate = DateTime.Today.ToString("yyyy-MM-dd");

            transactions[i] = copy;
            OnChanged?.Invoke();
            return true;
        }

        public bool RemoveTx(string id)
        {
            var i = transactions.FindIndex(t => t.id == id);
            if (i < 0) return false;
            transactions.RemoveAt(i);
            OnChanged?.Invoke();
            return true;
        }

        public void ClearAll()
        {
            transactions.Clear();
            OnChanged?.Invoke();
        }

        public (long income, long expense, long savings, long balanceNow) GetMonthAgg(DateTime today)
        {
            var monthStart = new DateTime(today.Year, today.Month, 1);
            long inc = 0, exp = 0;

            foreach (var t in transactions)
            {
                if (!TryEnsureIso(t.isoDate, out var d)) continue;
                if (d < monthStart || d > today) continue;

                if (t.type == TxType.Income) inc += t.amountCents;
                else exp += t.amountCents;
            }

            long savings = Math.Max(0, inc - exp);
            long balanceNow = OpeningBalanceCents + inc - exp;
            return (inc, exp, savings, balanceNow);
        }

        public long GetBalanceAt(DateTime dateInclusive)
        {
            long inc = 0, exp = 0;
            foreach (var t in transactions)
            {
                if (!TryEnsureIso(t.isoDate, out var d)) continue;
                if (d > dateInclusive) continue;

                if (t.type == TxType.Income) inc += t.amountCents;
                else exp += t.amountCents;
            }
            return OpeningBalanceCents + inc - exp;
        }

        public List<Tx> GetMonthTxs(DateTime anyDateInMonth)
        {
            var start = new DateTime(anyDateInMonth.Year, anyDateInMonth.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            var list = new List<Tx>();
            foreach (var t in transactions)
            {
                if (!TryEnsureIso(t.isoDate, out var d)) continue;
                if (d >= start && d <= end) list.Add(t);
            }
            return list;
        }

        private static bool TryEnsureIso(string s, out DateTime date)
        {
            if (!string.IsNullOrWhiteSpace(s))
            {
                if (DateTime.TryParseExact(s, "yyyy-MM-dd",
                                           System.Globalization.CultureInfo.InvariantCulture,
                                           System.Globalization.DateTimeStyles.None, out date))
                    return true;

                if (DateTime.TryParse(s, out date))
                    return true;
            }
            date = default;
            return false;
        }
    }
}
