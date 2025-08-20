using System;

namespace FinancePlanner.Data
{
    public enum TxType { Income, Expense }

    [Serializable]
    public class Tx
    {
        public string id;                 // GUID (строкой)
        public string isoDate;            // "yyyy-MM-dd"
        public TxType type;               // Доход / Расход
        public long amountCents;        // сумма в копейках

        public string currency = "RUB";
        public string category;
        public string comment;
    }
}
