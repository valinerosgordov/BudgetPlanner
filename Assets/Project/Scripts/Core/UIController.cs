using UnityEngine;
using UnityEngine.UIElements;

public class NavigationController : MonoBehaviour
{
    public UIDocument uiDocument;

    // Ссылки на экраны (подключи в инспекторе)
    public VisualTreeAsset dashboardUxml;
    public VisualTreeAsset plannerUxml;
    public VisualTreeAsset budgetsUxml;
    public VisualTreeAsset paymentsUxml;

    VisualElement _content;

    void Awake()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        _content = root.Q<VisualElement>("Content");

        // Кнопки сайдбара по именам из Sidebar.uxml
        root.Q<Button>("NavDashboard")?.RegisterCallback<ClickEvent>(_ => Show(dashboardUxml));
        root.Q<Button>("NavPlanner")?.RegisterCallback<ClickEvent>(_ => Show(plannerUxml));
        root.Q<Button>("NavBudgets")?.RegisterCallback<ClickEvent>(_ => Show(budgetsUxml));
        root.Q<Button>("NavPayments")?.RegisterCallback<ClickEvent>(_ => Show(paymentsUxml));

        // Показать стартовый экран
        Show(dashboardUxml);
    }

    void Show(VisualTreeAsset screen)
    {
        if (_content == null || screen == null) return;
        _content.Clear();
        var inst = screen.Instantiate();
        _content.Add(inst);
    }
}

