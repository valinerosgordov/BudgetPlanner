using UnityEngine;
using UnityEngine.UIElements;

public class NavigationController : MonoBehaviour
{
    public UIDocument uiDocument;

    public VisualTreeAsset dashboardView;
    public VisualTreeAsset plannerView;
    public VisualTreeAsset budgetsView;
    public VisualTreeAsset paymentsView;

    ScrollView content;
    Button bDashboard, bPlanner, bBudgets, bPayments;

    void Awake()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        content = root.Q<ScrollView>("Content");
        bDashboard = root.Q<Button>("NavDashboard");
        bPlanner = root.Q<Button>("NavPlanner");
        bBudgets = root.Q<Button>("NavBudgets");
        bPayments = root.Q<Button>("NavPayments");

        if (bDashboard != null) bDashboard.clicked += () => Show(dashboardView, bDashboard);
        if (bPlanner != null) bPlanner.clicked += () => Show(plannerView, bPlanner);
        if (bBudgets != null) bBudgets.clicked += () => Show(budgetsView, bBudgets);
        if (bPayments != null) bPayments.clicked += () => Show(paymentsView, bPayments);

        // стартовый экран
        Show(dashboardView, bDashboard);
    }

    void SetActive(Button active)
    {
        foreach (var b in new[] { bDashboard, bPlanner, bBudgets, bPayments })
            if (b != null) b.RemoveFromClassList("is-active");
        if (active != null) active.AddToClassList("is-active");
    }

    void Show(VisualTreeAsset vta, Button owner)
    {
        if (content == null || vta == null) return;
        content.Clear();
        content.Add(vta.Instantiate());
        SetActive(owner);
    }
}
