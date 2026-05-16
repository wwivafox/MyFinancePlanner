using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class TransactionCreator : MonoBehaviour
{
    [Header("UI — Список счетов")]
    public Transform accountsContainer;
    public GameObject accountItemPrefab;

    [Header("UI — Категории")]
    public Button incomeTab;
    public Button expenseTab;
    public Transform categoryContainer;
    public GameObject categoryButtonPrefab;

    private bool isIncome = false;
    private string selectedCategory = null;
    private Button lastSelectedCategoryButton;

    [Header("UI — Сумма")]
    public TMP_InputField amountInput;

    [Header("UI — Дата (из календаря)")]
    private DateTime selectedDate;

    [Header("UI — Описание")]
    public TMP_InputField descriptionInput;

    [Header("UI — Кнопка создания")]
    public Button createButton;

    // БД
    private Repository repo;
    private List<Category> loadedCategories;
    private List<Account> loadedAccounts;
    private Account selectedAccount;

    private System.Collections.IEnumerator Start()
    {
        // Ждём появления DatabaseManager
        while (DatabaseManager.Instance == null)
            yield return null;

        // Ждём появления DB
        while (DatabaseManager.Instance.DB == null)
            yield return null;

        repo = new Repository();

        // Ждём, пока база реально отдаст данные
        loadedCategories = repo.GetCategories();
        while (loadedCategories == null)
            yield return null;

        loadedAccounts = repo.GetAccounts();
        while (loadedAccounts == null)
            yield return null;

        GenerateAccountsUI();

        if (loadedAccounts.Count > 0)
            SelectAccount(loadedAccounts[0]);

        incomeTab.onClick.AddListener(() => SwitchType(true));
        expenseTab.onClick.AddListener(() => SwitchType(false));
        createButton.onClick.AddListener(CreateTransaction);

        selectedDate = DateTime.Now.Date;

        SwitchType(false);
    }



    // -----------------------------
    // СЧЕТА
    // -----------------------------
    private void GenerateAccountsUI()
    {
        foreach (Transform child in accountsContainer)
            Destroy(child.gameObject);

        foreach (var acc in loadedAccounts)
        {
            GameObject item = Instantiate(accountItemPrefab, accountsContainer);
            AccountItem ai = item.GetComponent<AccountItem>();
            ai.Init(acc, this);
        }
    }

    public void SelectAccount(Account acc)
    {
        selectedAccount = acc;
    }

    // -----------------------------
    // КАТЕГОРИИ
    // -----------------------------
    private void SwitchType(bool income)
    {
        isIncome = income;

        incomeTab.interactable = !income;
        expenseTab.interactable = income;

        LoadCategories();
    }

    private void LoadCategories()
    {
        if (categoryContainer == null)
        {
            Debug.LogError("categoryContainer == NULL !!! Назначь контейнер категорий в инспекторе");
            return;
        }

        if (loadedCategories == null)
        {
            Debug.LogWarning("Категории ещё не загружены — LoadCategories() отменён");
            return;
        }

        foreach (Transform child in categoryContainer)
            Destroy(child.gameObject);

        selectedCategory = null;
        lastSelectedCategoryButton = null;

        var list = loadedCategories.FindAll(c => c.IsIncome == isIncome);

        foreach (var cat in list)
        {
            GameObject btnObj = Instantiate(categoryButtonPrefab, categoryContainer);
            TMP_Text txt = btnObj.GetComponentInChildren<TMP_Text>();
            txt.text = cat.Name;

            Button btn = btnObj.GetComponent<Button>();

            btn.onClick.AddListener(() =>
            {
                SelectCategory(cat.Name, btn);
            });
        }
    }


    public void SelectCategory(string categoryName, Button btn)
    {
        selectedCategory = categoryName;

        if (lastSelectedCategoryButton != null)
            lastSelectedCategoryButton.interactable = true;

        btn.interactable = false;
        lastSelectedCategoryButton = btn;
    }

    // -----------------------------
    // ДАТА
    // -----------------------------
    public void SetDate(DateTime date)
    {
        selectedDate = date;
    }

    // -----------------------------
    // СОЗДАНИЕ ТРАНЗАКЦИИ
    // -----------------------------
    private void CreateTransaction()
    {
        if (!ValidateInput())
            return;

        float amount = float.Parse(amountInput.text.Replace(",", "."));

        Category cat = loadedCategories.Find(c => c.Name == selectedCategory && c.IsIncome == isIncome);

        if (cat == null)
        {
            Debug.LogError("Категория не найдена в БД!");
            return;
        }

        Transaction t = new Transaction
        {
            AccountId = selectedAccount.Id,
            CategoryId = cat.Id,
            Amount = amount,
            Date = selectedDate.ToString("yyyy-MM-dd"),
            Description = descriptionInput.text
        };

        repo.AddTransaction(t);

        if (isIncome)
            selectedAccount.StartAmount += amount;
        else
            selectedAccount.StartAmount -= amount;

        repo.UpdateAccount(selectedAccount);

        Debug.Log("Транзакция успешно сохранена!");

        ClearForm();
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrEmpty(amountInput.text))
        {
            Debug.LogWarning("Введите сумму");
            return false;
        }

        if (selectedCategory == null)
        {
            Debug.LogWarning("Выберите категорию");
            return false;
        }

        if (selectedDate == default)
        {
            Debug.LogWarning("Выберите дату");
            return false;
        }

        if (selectedAccount == null)
        {
            Debug.LogWarning("Выберите счёт");
            return false;
        }

        return true;
    }

    private void ClearForm()
    {
        amountInput.text = "";
        descriptionInput.text = "";
        selectedCategory = null;

        if (lastSelectedCategoryButton != null)
            lastSelectedCategoryButton.interactable = true;

        lastSelectedCategoryButton = null;
    }
}
