using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using FinancePlanner.Data; // DataStore, Tx, TxType

namespace FinancePlanner.UI.Controllers
{
    public class PlannerPresenter : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private UIDocument ui;
        [SerializeField] private DataStore data;
        [SerializeField] private VisualTreeAsset plannerRow; // шаблон строки (PlannerRow.uxml)

        private VisualElement _root;
        private ListView _list;
        private Label _monthTitle, _ttlInc, _ttlExp, _ttlNet;
        private Button _prevBtn, _nextBtn, _addBtn;

        private IVisualElementScheduledItem _poll;
        private DateTime _currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        private readonly List<Tx> _items = new();

        private void OnEnable()
        {
            _root = ui ? ui.rootVisualElement : null;
            if (_root == null)
            {
                Debug.LogError("[PlannerPresenter] UIDocument/root is null");
                return;
            }

            // Ждём, пока экран Planner реально окажется в ScrollView "Content"
            _poll = _root.schedule.Execute(TryBind).Every(200);

            if (data != null) data.OnChanged += Rebuild;
        }

        private void OnDisable()
        {
            if (_poll != null) _poll.Pause();
            if (data != null) data.OnChanged -= Rebuild;
        }

        private void TryBind()
        {
            if (_list != null) { _poll.Pause(); return; }

            var container = _root.Q("Content"); // ScrollView
            if (container == null) return;

            _list = container.Q<ListView>("PlannerList");
            _monthTitle = container.Q<Label>("MonthTitle");
            _ttlInc = container.Q<Label>("TotalIncome");
            _ttlExp = container.Q<Label>("TotalExpense");
            _ttlNet = container.Q<Label>("TotalNet");
            _prevBtn = container.Q<Button>("PrevMonth");
            _nextBtn = container.Q<Button>("NextMonth");
            _addBtn = container.Q<Button>("AddTxBtn");

            if (_list == null) return; // экран ещё не подставился

            if (_prevBtn != null) _prevBtn.clicked += () => { _currentMonth = _currentMonth.AddMonths(-1); Rebuild(); };
            if (_nextBtn != null) _nextBtn.clicked += () => { _currentMonth = _currentMonth.AddMonths(+1); Rebuild(); };
            // if (_addBtn != null) _addBtn.clicked += () => FindObjectOfType<NewTransactionController>()?.Show();

            // ListView
            _list.makeItem = () => plannerRow != null ? plannerRow.Instantiate() : new Label("Row?");
            _list.bindItem = (ve, i) =>
            {
                var tx = _items[i];

                var lblDate = ve.Q<Label>("ColDate");
                var lblType = ve.Q<Label>("ColType");
                var lblText = ve.Q<Label>("ColText");
                var lblAmount = ve.Q<Label>("ColAmount");

                if (lblDate != null) lblDate.text = tx.isoDate;
                if (lblType != null) lblType.text = tx.type == TxType.Income ? "Доход" : "Расход";
                if (lblText != null) lblText.text = string.Empty; // нет поля note — оставляем пусто/категорию по желанию
                if (lblAmount != null) lblAmount.text = FormatAmount(tx.amountCents);
            };
            _list.itemsSource = _items;
            _list.selectionType = SelectionType.None;

            Rebuild();
            _poll.Pause();
        }

        private void Rebuild()
        {
            if (_list == null || data == null) return;

            // Заголовок месяца
            if (_monthTitle != null)
                _monthTitle.text = CultureInfo.GetCultureInfo("ru-RU")
                    .TextInfo.ToTitleCase(_currentMonth.ToString("MMMM yyyy", new CultureInfo("ru-RU")));

            // Фильтруем транзакции текущего месяца
            _items.Clear();
            foreach (var tx in data.Transactions)
            {
                if (!DateTime.TryParse(tx.isoDate, out var dt)) continue;
                if (dt.Year == _currentMonth.Year && dt.Month == _currentMonth.Month)
                    _items.Add(tx);
            }
            _list.Rebuild();

            // Итоги
            long inc = 0, exp = 0;
            foreach (var t in _items)
                if (t.type == TxType.Income) inc += t.amountCents; else exp += t.amountCents;

            var net = inc - exp;

            if (_ttlInc != null) _ttlInc.text = $"Доход: {FormatAmount(inc)}";
            if (_ttlExp != null) _ttlExp.text = $"Расход: {FormatAmount(exp)}";
            if (_ttlNet != null) _ttlNet.text = $"Итог: {FormatAmount(net)}";
        }

        private static string FormatAmount(long cents)
        {
            var rub = cents / 100m;
            return string.Format(CultureInfo.GetCultureInfo("ru-RU"), "{0:N0} ₽", rub);
        }
    }
}
