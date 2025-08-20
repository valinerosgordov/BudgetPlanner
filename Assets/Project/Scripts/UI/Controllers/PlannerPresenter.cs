using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using FinancePlanner.Data;

namespace FinancePlanner.UI
{
    public class PlannerPresenter : MonoBehaviour
    {
        [SerializeField] private UIDocument ui;
        [SerializeField] private DataStore data;
        [SerializeField] private VisualTreeAsset plannerRow;   // PlannerRow.uxml
        [SerializeField] private NewTransactionController newTx;

        ScrollView content;
        ListView list;
        Label monthTitle, totalIncome, totalExpense, totalNet;
        Button btnPrev, btnNext, btnAdd;

        DateTime currentMonth;
        List<Tx> monthItems = new();
        CultureInfo ru = new CultureInfo("ru-RU");

        void Awake()
        {
            if (ui == null) ui = GetComponent<UIDocument>();
            if (newTx == null) newTx = GetComponent<NewTransactionController>();

            var root = ui.rootVisualElement;
            content = root.Q<ScrollView>("Content");

            // Перехватим подключение экрана Планера (когда пользователь его открыл)
            if (content != null)
                content.RegisterCallback<GeometryChangedEvent>(_ => TryHookScreen());
        }

        void OnEnable()
        {
            if (data != null) data.OnChanged += Refresh;
            currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }

        void OnDisable()
        {
            if (data != null) data.OnChanged -= Refresh;
        }

        void TryHookScreen()
        {
            var scope = content?.contentContainer;
            if (scope == null) return;

            // Ищем элементы Планера — если их нет, значит открыт другой экран
            list = scope.Q<ListView>("PlannerList");
            if (list == null) return; // не наш экран

            monthTitle = scope.Q<Label>("MonthTitle");
            totalIncome = scope.Q<Label>("TotalIncome");
            totalExpense = scope.Q<Label>("TotalExpense");
            totalNet = scope.Q<Label>("TotalNet");
            btnPrev = scope.Q<Button>("PrevMonth");
            btnNext = scope.Q<Button>("NextMonth");
            btnAdd = scope.Q<Button>("AddTxBtn");

            if (btnPrev != null) btnPrev.clicked += () => { currentMonth = currentMonth.AddMonths(-1); Refresh(); };
            if (btnNext != null) btnNext.clicked += () => { currentMonth = currentMonth.AddMonths(+1); Refresh(); };
            if (btnAdd != null) btnAdd.clicked += () => { newTx?.Open(currentMonth); }; // открыть модалку с датой месяца

            SetupListView();
            Refresh();
        }

        void SetupListView()
        {
            if (list == null) return;

            list.makeItem = () => plannerRow.Instantiate();
            list.bindItem = (ve, i) =>
            {
                var tx = monthItems[i];
                ve.Q<Label>("Date").text = tx.Date.ToString("dd.MM.yyyy");
                ve.Q<Label>("Type").text = tx.type == TxType.Income ? "Доход" : "Расход";
                ve.Q<Label>("Category").text = string.IsNullOrEmpty(tx.category) ? "—" : tx.category;
                ve.Q<Label>("Amount").text = string.Format(ru, "{0:N0} ₽", tx.amountCents / 100m);

                var edit = ve.Q<Button>("EditBtn");
                var del = ve.Q<Button>("DeleteBtn");

                edit.clicked += () => { newTx?.OpenForEdit(tx); };
                del.clicked += () => { data?.RemoveTx(tx.id); };
            };

            list.itemsSource = monthItems;
            list.fixedItemHeight = 36; // ровные строки
        }

        void Refresh()
        {
            // если лист не найден — мы не на экране Планера
            if (list == null) return;

            monthItems.Clear();
            var start = currentMonth;
            var end = start.AddMonths(1);

            long inc = 0, exp = 0;
            foreach (var t in data.Transactions)
            {
                var dt = DateTime.Parse(t.isoDate);
                if (dt >= start && dt < end)
                {
                    monthItems.Add(t);
                    if (t.type == TxType.Income) inc += t.amountCents;
                    else exp += t.amountCents;
                }
            }

            monthItems.Sort((a, b) => DateTime.Parse(a.isoDate).CompareTo(DateTime.Parse(b.isoDate)));
            list.RefreshItems();

            if (monthTitle != null) monthTitle.text = start.ToString("MMMM yyyy", ru);
            if (totalIncome != null) totalIncome.text = $"Доход: {inc / 100m:N0} ₽";
            if (totalExpense != null) totalExpense.text = $"Расход: {exp / 100m:N0} ₽";
            if (totalNet != null) totalNet.text = $"Итог: {(inc - exp) / 100m:N0} ₽";
        }
    }
}
