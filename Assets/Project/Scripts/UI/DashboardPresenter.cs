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

        Label kpiBalance, kpiBalanceDelta;
        Label kpiIncome, kpiIncomeDelta;
        Label kpiExpense, kpiExpenseDelta;
        Label kpiSavings, kpiSavingsDelta;

        CultureInfo ru = new CultureInfo("ru-RU");

        void OnEnable()
        {
            if (ui == null) ui = GetComponent<UIDocument>();
            Hook();
            if (data != null) data.OnChanged += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            if (data != null) data.OnChanged -= Refresh;
        }

        void Hook()
        {
            var root = ui.rootVisualElement;
            kpiBalance = root.Q<Label>("KpiBalanceValue");
            kpiBalanceDelta = root.Q<Label>("KpiBalanceDelta");
            kpiIncome = root.Q<Label>("KpiIncomeValue");
            kpiIncomeDelta = root.Q<Label>("KpiIncomeDelta");
            kpiExpense = root.Q<Label>("KpiExpenseValue");
            kpiExpenseDelta = root.Q<Label>("KpiExpenseDelta");
            kpiSavings = root.Q<Label>("KpiSavingsValue");
            kpiSavingsDelta = root.Q<Label>("KpiSavingsDelta");
        }

        string FmtLong(long cents) => string.Format(ru, "{0:N0} ₽", cents / 100m);

        void Refresh()
        {
            if (data == null) return;
            var agg = data.GetMonthAgg(System.DateTime.Today);
            kpiIncome.text = FmtLong(agg.income);
            kpiExpense.text = FmtLong(agg.expense);
            kpiSavings.text = FmtLong(agg.savings);
            kpiBalance.text = FmtLong(agg.balanceNow);

            // временные заглушки дельт
            kpiIncomeDelta.text = "▲ +3,1%";
            kpiExpenseDelta.text = "▼ −1,8%";
            kpiSavingsDelta.text = "▲ +12%";
            kpiBalanceDelta.text = "▲ +4,2%";
        }
    }
}
