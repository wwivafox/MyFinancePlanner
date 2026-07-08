using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CategoryCreator : MonoBehaviour
{
    public static CategoryCreator Instance;

    public Button expensesButton;
    public Button incomeButton;

    public TMP_Text expensesText;
    public TMP_Text incomeText;

    public Transform content;
    public GameObject categoryPrefab;
    public GameObject addButtonPrefab;

    public CategoryIconSelector iconSelector;

    private bool showingIncome = true;
    private List<Category> categories;

    private GameObject addButtonInstance;
    private CategoryItem activeEditingItem = null;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        expensesButton.onClick.AddListener(() => SwitchMode(false));
        incomeButton.onClick.AddListener(() => SwitchMode(true));
        SwitchMode(true);
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

        addButtonInstance = Instantiate(addButtonPrefab, content);
        var btn = addButtonInstance.GetComponentInChildren<Button>(true);
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(CreateNewCategory);
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
        int addIndex = addButtonInstance.transform.GetSiblingIndex();
        item.transform.SetSiblingIndex(addIndex);

        var ci = item.GetComponent<CategoryItem>();
        ci.iconSelector = iconSelector;
        ci.SetupNew(newCat);
    }

    public void SetEditingMode(CategoryItem item)
    {
        activeEditingItem = item;

        foreach (Transform child in content)
        {
            var ci = child.GetComponent<CategoryItem>();
            if (ci == null) continue;

            var cg = child.GetComponent<CanvasGroup>() ?? child.gameObject.AddComponent<CanvasGroup>();

            if (ci == item)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            else
            {
                cg.alpha = 0.5f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
        }

        if (addButtonInstance != null)
        {
            var cg = addButtonInstance.GetComponent<CanvasGroup>() ?? addButtonInstance.AddComponent<CanvasGroup>();
            cg.alpha = 0.5f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }

    public void ResetEditingMode()
    {
        activeEditingItem = null;

        foreach (Transform child in content)
        {
            var cg = child.GetComponent<CanvasGroup>() ?? child.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        if (addButtonInstance != null)
        {
            var cg = addButtonInstance.GetComponent<CanvasGroup>() ?? addButtonInstance.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
    }
}
