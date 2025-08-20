using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using FinancePlanner.Data;

namespace FinancePlanner.UI
{
    public class NewTransactionController : MonoBehaviour
    {
        [SerializeField] private UIDocument ui;
        [SerializeField] private VisualTreeAsset newTxModal;
        [SerializeField] private DataStore data;

        public event Action<Tx> OnSaved;     // cобытие на сохранение (для Планера)

        VisualElement modalHost;

        void Awake()
        {
            if (ui == null) ui = GetComponent<UIDocument>();
            modalHost = ui.rootVisualElement.Q<VisualElement>("ModalHost");

            var addBtn = ui.rootVisualElement.Q<Button>("AddBtn");
            if (addBtn != null) addBtn.clicked += () => Open(); // глобальная кнопка "+"
        }

        // Открыть «как новую» (опционально с предустановленной датой)
        public void Open(DateTime? presetDate = null)
        {
            OpenInternal(null, presetDate);
        }

        // Открыть «редактирование» существующей транзакции
        public void OpenForEdit(Tx existing)
        {
            OpenInternal(existing, null);
        }

        void OpenInternal(Tx existing, DateTime? presetDate)
        {
            if (modalHost == null || newTxModal == null) return;

            modalHost.Clear();
            modalHost.Add(newTxModal.Instantiate());
            modalHost.AddToClassList("is-open");

            var type = modalHost.Q<DropdownField>("TypeField");
            var amount = modalHost.Q<TextField>("AmountField");
            var curr = modalHost.Q<DropdownField>("CurrencyField");
            var cat = modalHost.Q<TextField>("CategoryField");
            var date = modalHost.Q<TextField>("DateField");
            var note = modalHost.Q<TextField>("NoteField");
            var cancel = modalHost.Q<Button>("CancelBtn");
            var save = modalHost.Q<Button>("SaveBtn");

            // Заполняем дефолты / существующие значения
            if (existing != null)
            {
                type.value = existing.type == TxType.Income ? "Доход" : "Расход";
                amount.value = (existing.amountCents / 100m).ToString(CultureInfo.InvariantCulture);
                curr.value = existing.currency;
                cat.value = existing.category;
                date.value = existing.Date.ToString("yyyy-MM-dd");
                note.value = existing.note;
            }
            else
            {
                type.value = "Расход";
                date.value = (presetDate ?? DateTime.Today).ToString("yyyy-MM-dd");
                curr.value = "RUB";
            }

            cancel.clicked += Close;

            save.clicked += () =>
            {
                // Парсим сумму
                var raw = (amount.value ?? "").Trim().Replace(" ", "").Replace(',', '.');
                if (!decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                {
                    amount.value = "Ошибка";
                    return;
                }
                long cents = (long)Math.Round(dec * 100m);

                // Собираем модель
                var tx = existing ?? new Tx();
                tx.type = (type.value == "Доход") ? TxType.Income : TxType.Expense;
                tx.amountCents = cents;
                tx.currency = string.IsNullOrEmpty(curr.value) ? "RUB" : curr.value;
                tx.isoDate = DateTime.TryParse(date.value, out var dt) ? dt.ToString("yyyy-MM-dd") : DateTime.Today.ToString("yyyy-MM-dd");
                tx.category = cat.value ?? "";
                tx.note = note.value ?? "";

                if (existing == null) data?.AddTx(tx);
                else data?.UpdateTx(tx);

                OnSaved?.Invoke(tx);
                Close();
            };
        }

        public void Close()
        {
            if (modalHost == null) return;
            modalHost.Clear();
            modalHost.RemoveFromClassList("is-open");
        }
    }
}
