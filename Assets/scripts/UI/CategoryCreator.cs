using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CategoryCreator : MonoBehaviour
{
    public static CategoryCreator Instance;

    [Header("UI")]
    public Button expensesButton;
    public Button incomeButton;

    public TMP_Text expensesText;
    public TMP_Text incomeText;

    public Transform content;
    public GameObject categoryPrefab;
    public GameObject addButtonPrefab;

    [Header("Icon Selector")]
    public CategoryIconSelector iconSelector;

    private bool showingIncome = true; // доходы первыми
    private List<Category> categories;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        expensesButton.onClick.AddListener(() => SwitchMode(false));
        incomeButton.onClick.AddListener(() => SwitchMode(true));

        SwitchMode(true); // сразу показываем доходы
    }

    private void SwitchMode(bool income)
    {
        showingIncome = income;

        incomeText.fontSize = income ? 29 : 24;
        expensesText.fontSize = income ? 24 : 29;

        LoadCategories();
    }

    private void LoadCategories()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        categories = DatabaseManager.Instance.DB
            .GetConnection()
            .Query<Category>("SELECT * FROM Category WHERE IsIncome = ?", showingIncome ? 1 : 0);

        foreach (var cat in categories)
        {
            var item = Instantiate(categoryPrefab, content);
            var ci = item.GetComponent<CategoryItem>();

            ci.iconSelector = iconSelector;
            ci.Setup(cat);
        }

        var addButton = Instantiate(addButtonPrefab, content);
        addButton.GetComponent<Button>().onClick.AddListener(CreateNewCategory);
    }

    private void CreateNewCategory()
    {
        var newCat = new Category
        {
            Name = "",
            IconName = "Еда",
            IsIncome = showingIncome
        };

        var item = Instantiate(categoryPrefab, content);
        var ci = item.GetComponent<CategoryItem>();

        ci.iconSelector = iconSelector;
        ci.SetupNew(newCat);
    }

    public void SetEditingMode(CategoryItem activeItem)
    {
        foreach (Transform child in content)
        {
            var item = child.GetComponent<CategoryItem>();
            if (item == null) continue;

            if (item == activeItem)
                item.SetInteractable(true);
            else
                item.SetInteractable(false);
        }
    }

    public void ResetEditingMode()
    {
        foreach (Transform child in content)
        {
            var item = child.GetComponent<CategoryItem>();
            if (item == null) continue;

            item.SetInteractable(true);
        }
    }
}
