using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using FinancePlanner.Data;

namespace FinancePlanner.UI
{
    public class DashboardPresenter : MonoBehaviour
    {
        [SerializeField] private UIDocument ui;
        [SerializeField] private DataStore data;

        // KPI refs
        Label kpiBalance, kpiBalanceDelta;
        Label kpiIncome, kpiIncomeDelta;
        Label kpiExpense, kpiExpenseDelta;
        Label kpiSavings, kpiSavingsDelta;

        ScrollView content;
        CultureInfo ru = new CultureInfo("ru-RU");

        void OnEnable()
        {
            if (ui == null) ui = GetComponent<UIDocument>();
            content = ui.rootVisualElement.Q<ScrollView>("Content");

            // Когда контентный экран переключается — перехукиваемся и обновляем KPI
            if (content != null)
                content.RegisterCallback<GeometryChangedEvent>(_ => { ClearRefs(); TryHook(); Refresh(); });

            if (data != null) data.OnChanged += Refresh;

            TryHook();   // попробовать найти KPI на текущем экране
            Refresh();   // и обновить значения
        }

        void OnDisable()
        {
            if (data != null) data.OnChanged -= Refresh;
            if (content != null)
                content.UnregisterCallback<GeometryChangedEvent>(_ => { ClearRefs(); TryHook(); Refresh(); });
        }

        void ClearRefs()
        {
            kpiBalance = kpiBalanceDelta = null;
            kpiIncome = kpiIncomeDelta = null;
            kpiExpense = kpiExpenseDelta = null;
            kpiSavings = kpiSavingsDelta = null;
        }

        // Ищем KPI-лейблы внутри текущего контента (DashboardView)
        void TryHook()
        {
            if (content == null) return;
            var scope = content.contentContainer; // именно внутренняя область ScrollView

            // Уже нашли — выходим
            if (scope == null || kpiBalance != null) return;

            kpiBalance = scope.Q<Label>("KpiBalanceValue");
            kpiBalanceDelta = scope.Q<Label>("KpiBalanceDelta");
            kpiIncome = scope.Q<Label>("KpiIncomeValue");
            kpiIncomeDelta = scope.Q<Label>("KpiIncomeDelta");
            kpiExpense = scope.Q<Label>("KpiExpenseValue");
            kpiExpenseDelta = scope.Q<Label>("KpiExpenseDelta");
            kpiSavings = scope.Q<Label>("KpiSavingsValue");
            kpiSavingsDelta = scope.Q<Label>("KpiSavingsDelta");
        }

        string Fmt(long cents) => string.Format(ru, "{0:N0} ₽", cents / 100m);

        void Refresh()
        {
            if (data == null) return;

            // Если KPI ещё не нашли (например, только что переключили экран) — попробуем ещё раз
            if (kpiBalance == null) TryHook();
            if (kpiBalance == null) return; // не дашборд — просто выходим

            var (income, expense, savings, balanceNow) = data.GetMonthAgg(System.DateTime.Today);

            kpiIncome.text = Fmt(income);
            kpiExpense.text = Fmt(expense);
            kpiSavings.text = Fmt(savings);
            kpiBalance.text = Fmt(balanceNow);

            // Заглушки дельт — позже посчитаем по реальным данным
            kpiIncomeDelta.text = "▲ +3,1%";
            kpiExpenseDelta.text = "▼ −1,8%";
            kpiSavingsDelta.text = "▲ +12%";
            kpiBalanceDelta.text = "▲ +4,2%";
        }
    }
}
