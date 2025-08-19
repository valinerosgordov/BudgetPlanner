using System;

using UnityEngine;

namespace FinancePlanner.Data
{
    public enum TxType { Income, Expense }

    [Serializable]
    public class Tx
    {
        public string id;
        public TxType type;
        public long amountCents;     // без плавающей точки
        public string currency = "RUB";
        public string isoDate;       // DateTime в ISO, чтобы сериализовалось
        public string category;
        public string note;

        public DateTime Date => DateTime.Parse(isoDate);
        public decimal Amount => amountCents / 100m;
    }
}
