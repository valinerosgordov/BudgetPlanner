using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using FinancePlanner.Data;

namespace FinancePlanner.UI.Controllers
{
    public class NewTransactionController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private UIDocument ui;
        [SerializeField] private DataStore data;
        [SerializeField] private VisualTreeAsset newTxModal;   // NewTransactionModal.uxml

        // runtime
        private VisualElement _root, _host, _modal;
        private DropdownField _typeField, _currencyField;
        private TextField _amountField, _categoryField, _dateField, _noteField;
        private Button _btnCancel, _btnSave;

        // кнопки, открывающие модалку (чтобы корректно отписаться)
        private Button _openAddBtn, _openFabBtn;

        private void OnEnable()
        {
            _root = ui ? ui.rootVisualElement : null;
            if (_root == null) { Debug.LogError("[NewTx] UIDocument/root is null"); return; }

            _host = _root.Q<VisualElement>("ModalHost");
            if (_host == null) { Debug.LogError("[NewTx] ModalHost not found in RootLayout"); return; }

            _modal = newTxModal != null ? newTxModal.Instantiate() : null;
            if (_modal == null) { Debug.LogError("[NewTx] newTxModal is null"); return; }
            _host.Add(_modal);
            Hide();

            // поля
            _typeField = _modal.Q<DropdownField>("TypeField");
            _currencyField = _modal.Q<DropdownField>("CurrencyField");
            _amountField = _modal.Q<TextField>("AmountField");
            _categoryField = _modal.Q<TextField>("CategoryField");
            _dateField = _modal.Q<TextField>("DateField");
            _noteField = _modal.Q<TextField>("NoteField");

            _btnCancel = _modal.Q<Button>("CancelBtn");
            _btnSave = _modal.Q<Button>("SaveBtn");

            if (_btnCancel != null) _btnCancel.clicked += Hide;
            if (_btnSave != null) _btnSave.clicked += OnSave;

            // фон — закрывает модалку
            _host.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.target == _host) Hide();
            });

            // подписка на кнопки открытия — БЕЗ ?.clicked
            _openAddBtn = _root.Q<Button>("AddBtn");
            if (_openAddBtn != null) _openAddBtn.clicked += Show;

            _openFabBtn = _root.Q<Button>("FabAdd");
            if (_openFabBtn != null) _openFabBtn.clicked += Show;
        }

        private void OnDisable()
        {
            if (_btnCancel != null) _btnCancel.clicked -= Hide;
            if (_btnSave != null) _btnSave.clicked -= OnSave;

            if (_openAddBtn != null) _openAddBtn.clicked -= Show;
            if (_openFabBtn != null) _openFabBtn.clicked -= Show;
        }

        public void Show()
        {
            if (_host == null) return;
            _host.style.display = DisplayStyle.Flex;

            if (_typeField != null) _typeField.value = "Расход";
            if (_currencyField != null) _currencyField.value = "RUB";
            if (_amountField != null) _amountField.value = "";
            if (_categoryField != null) _categoryField.value = "";
            if (_dateField != null) _dateField.value = DateTime.Today.ToString("yyyy-MM-dd");
            if (_noteField != null) _noteField.value = "";
        }

        public void Hide()
        {
            if (_host == null) return;
            _host.style.display = DisplayStyle.None;
        }

        private void OnSave()
        {
            if (data == null) { Debug.LogError("[NewTx] DataStore is null"); return; }

            var type = (_typeField != null && _typeField.value == "Доход") ? TxType.Income : TxType.Expense;

            long amountCents = 0;
            if (_amountField != null && !string.IsNullOrWhiteSpace(_amountField.value))
            {
                if (decimal.TryParse(_amountField.value.Replace(" ", ""),
                        NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU"), out var dec) ||
                    decimal.TryParse(_amountField.value.Replace(" ", ""),
                        NumberStyles.Number, CultureInfo.InvariantCulture, out dec))
                {
                    amountCents = (long)Math.Round(dec * 100m);
                }
            }

            DateTime dt = DateTime.Today;
            if (_dateField != null && !string.IsNullOrWhiteSpace(_dateField.value))
                DateTime.TryParse(_dateField.value, out dt);

            var tx = new Tx
            {
                id = Guid.NewGuid().ToString("N"),
                type = type,
                amountCents = amountCents,
                isoDate = dt.ToString("yyyy-MM-dd")
            };

            data.AddTx(tx);
            Hide();
        }
    }
}
