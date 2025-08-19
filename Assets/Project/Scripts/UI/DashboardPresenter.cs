using System;
using System.Collections.Generic;
using System.Globalization;

using UnityEngine;
using UnityEngine.UIElements;

public class DashboardPresenter : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string baseCurrency = "RUB"; // базовая валюта отчётов

    // Пример простой модели транзакций (замени на свой репозиторий)
    [Serializable]
    public class Tx
    {
        public DateTime At;
        public decimal Amount; // >0 доход, <0 расход
        public string Currency; // для MVP игнорируем конвертацию
    }

    // Временные данные пользователя (подменишь на БД)
    [SerializeField]
    private List<Tx> demoTransactions = new List<Tx>
    {
        new Tx{ At = DateTime.Today.AddDays(-2), Amount = 42000m, Currency = "RUB"},
        new Tx{ At = DateTime.Today.AddDays(-1), Amount = -18340m, Currency = "RUB"},
        new Tx{ At = DateTime.Today.AddDays(-1), Amount = -5200m, Currency = "RUB"},
        new Tx{ At = DateTime.Today, Amount = -3800m, Currency = "RUB"},
        new Tx{ At = DateTime.Today, Amount = 8900m, Currency = "RUB"}, // как перевод в сбережения
    };

    Label kpiBalance, kpiBalanceDelta;
    Label kpiIncome, kpiIncomeDelta;
    Label kpiExpense, kpiExpenseDelta;
    Label kpiSavings, kpiSavingsDelta;

    CultureInfo ru = new CultureInfo("ru-RU");

    void Awake()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        kpiBalance = root.Q<Label>("KpiBalanceValue");
        kpiBalanceDelta = root.Q<Label>("KpiBalanceDelta");
        kpiIncome = root.Q<Label>("KpiIncomeValue");
        kpiIncomeDelta = root.Q<Label>("KpiIncomeDelta");
        kpiExpense = root.Q<Label>("KpiExpenseValue");
        kpiExpenseDelta = root.Q<Label>("KpiExpenseDelta");
        kpiSavings = root.Q<Label>("KpiSavingsValue");
        kpiSavingsDelta = root.Q<Label>("KpiSavingsDelta");

        RefreshKpi();
    }

    void RefreshKpi()
    {
        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        decimal incomeMtd = 0, expenseMtd = 0, savingsMtd = 0;

        foreach (var t in demoTransactions)
        {
            if (t.At < monthStart) continue;
            if (t.Amount > 0) incomeMtd += t.Amount;
            if (t.Amount < 0) expenseMtd += -t.Amount;
        }

        // Допустим, всё, что помечено как «сбережения», мы бы знали по категории; тут просто пример:
        savingsMtd = 8900m;

        decimal startingBalance = 100000m; // возьми из сумм по счетам
        decimal balanceNow = startingBalance + incomeMtd - expenseMtd;

        // Форматирование
        string fmt(decimal v) => string.Format(ru, "{0:N0} ₽", v);

        kpiIncome.text = fmt(incomeMtd);
        kpiExpense.text = fmt(expenseMtd);
        kpiSavings.text = fmt(savingsMtd);
        kpiBalance.text = fmt(balanceNow);

        // Дельты для примера: сравнение с прошлым месяцем (заглушка)
        kpiIncomeDelta.text = "▲ +3,1%";
        kpiExpenseDelta.text = "▼ −1,8%";
        kpiSavingsDelta.text = "▲ +12%";
        kpiBalanceDelta.text = "▲ +4,2%";
    }
}
