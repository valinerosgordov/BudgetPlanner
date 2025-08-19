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

        VisualElement modalHost;

        void Awake()
        {
            if (ui == null) ui = GetComponent<UIDocument>();
            var root = ui.rootVisualElement;

            modalHost = root.Q<VisualElement>("ModalHost");

            var addBtn = root.Q<Button>("AddBtn");
            if (addBtn != null) addBtn.clicked += Open;

            // автозаполнения по умолчанию
            var search = root.Q<TextField>("GlobalSearch");
            if (search != null) search.value = "";
        }

        public void Open()
        {
            if (modalHost == null || newTxModal == null) return;

            modalHost.Clear();
            modalHost.Add(newTxModal.Instantiate());
            modalHost.AddToClassList("is-open");

            var m = modalHost.Q<VisualElement>(className: "modal");
            var type = modalHost.Q<DropdownField>("TypeField");
            var amount = modalHost.Q<TextField>("AmountField");
            var curr = modalHost.Q<DropdownField>("CurrencyField");
            var cat = modalHost.Q<TextField>("CategoryField");
            var date = modalHost.Q<TextField>("DateField");
            var note = modalHost.Q<TextField>("NoteField");
            var cancel = modalHost.Q<Button>("CancelBtn");
            var save = modalHost.Q<Button>("SaveBtn");

            date.value = DateTime.Today.ToString("yyyy-MM-dd");

            cancel.clicked += Close;
            save.clicked += () =>
            {
                if (data == null) { Close(); return; }

                // парсим сумму
                var raw = (amount.value ?? "").Trim().Replace(" ", "");
                if (string.IsNullOrEmpty(raw)) { amount.value = "0"; raw = "0"; }
                // поддержим и запятую, и точку
                raw = raw.Replace(',', '.');
                if (!decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                {
                    amount.value = "Ошибка";
                    return;
                }
                long cents = (long)Math.Round(dec * 100m);

                var tx = new Tx
                {
                    type = (type.value == "Доход") ? TxType.Income : TxType.Expense,
                    amountCents = cents,
                    currency = string.IsNullOrEmpty(curr.value) ? "RUB" : curr.value,
                    isoDate = DateTime.TryParse(date.value, out var dt) ? dt.ToString("yyyy-MM-dd") : DateTime.Today.ToString("yyyy-MM-dd"),
                    category = cat.value ?? "",
                    note = note.value ?? ""
                };

                data.AddTx(tx);
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
