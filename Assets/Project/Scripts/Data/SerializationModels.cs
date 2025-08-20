using System;
using System.Collections.Generic;

namespace FinancePlanner.Data
{
    /// <summary>
    /// Простая обёртка для сохранения/загрузки списка транзакций в JSON.
    /// </summary>
    [Serializable]
    public class TxListDTO
    {
        public long openingBalanceCents;
        public List<Tx> transactions = new();
    }
}
