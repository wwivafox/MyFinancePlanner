using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.SceneManagement;

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

    private bool isIncome = true;
    private string selectedCategory = null;
    private Button lastSelectedCategoryButton;

    [Header("UI — Сумма")]
    public TMP_InputField amountInput;

    [Header("UI — Дата (из календаря)")]
    private DateTime selectedDate;

    [Header("UI — Описание")]
    public TMP_InputField descriptionInput;
    public TMP_Text descriptionCounter;

    [Header("UI — Кнопка создания")]
    public Button createButton;
    private CanvasGroup createButtonCanvas;

    // БД
    private Repository repo;
    private List<Category> loadedCategories;
    private List<Account> loadedAccounts;
    private Account selectedAccount;

    private static TransactionCreator instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        createButton.onClick.RemoveAllListeners();
        createButton.onClick.AddListener(CreateTransaction);
    }


    private System.Collections.IEnumerator Start()
    {
        while (DatabaseManager.Instance == null)
            yield return null;

        while (DatabaseManager.Instance.DB == null)
            yield return null;

        repo = new Repository();

        loadedCategories = repo.GetCategories();
        while (loadedCategories == null)
            yield return null;

        loadedAccounts = repo.GetAccounts();
        while (loadedAccounts == null)
            yield return null;

        GenerateAccountsUI();

        createButtonCanvas = createButton.GetComponent<CanvasGroup>();
        if (createButtonCanvas == null)
            createButtonCanvas = createButton.gameObject.AddComponent<CanvasGroup>();

        if (loadedAccounts.Count > 0)
            SelectAccount(loadedAccounts[0]);

        incomeTab.onClick.AddListener(() => SwitchType(true));
        expenseTab.onClick.AddListener(() => SwitchType(false));

        selectedDate = DateTime.Now.Date;

        amountInput.onValueChanged.AddListener(OnAmountChanged);
        amountInput.onEndEdit.AddListener(FormatAmount);

        descriptionInput.onValueChanged.AddListener((string v) => OnDescriptionChanged(v));

        UpdateCreateButtonState();
        SwitchType(true);
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

        foreach (Transform child in accountsContainer)
        {
            CanvasGroup cg = child.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = 0.5f;
        }

        foreach (Transform child in accountsContainer)
        {
            AccountItem ai = child.GetComponent<AccountItem>();
            if (ai != null && ai.Account == acc)
            {
                CanvasGroup cg = child.GetComponent<CanvasGroup>();
                if (cg != null)
                    cg.alpha = 1f;
            }
        }

        UpdateCreateButtonState();
    }

    // -----------------------------
    // КАТЕГОРИИ
    // -----------------------------
    private void SwitchType(bool income)
    {
        isIncome = income;

        incomeTab.interactable = !income;
        expenseTab.interactable = income;

        TMP_Text incomeText = incomeTab.GetComponentInChildren<TMP_Text>();
        TMP_Text expenseText = expenseTab.GetComponentInChildren<TMP_Text>();

        incomeText.fontSize = income ? 29 : 24;
        expenseText.fontSize = income ? 24 : 29;

        LoadCategories();
    }

    private void LoadCategories()
    {
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

            Image icon = btnObj.transform.Find("Icon").GetComponent<Image>();
            icon.sprite = Resources.Load<Sprite>("Sprites/Category/" + cat.IconName);

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => SelectCategory(cat.Name, btn));
        }

        UpdateCreateButtonState();
    }

    public void SelectCategory(string categoryName, Button btn)
    {
        if (selectedCategory == categoryName)
        {
            selectedCategory = null;
            lastSelectedCategoryButton = null;

            foreach (Transform child in categoryContainer)
            {
                CanvasGroup cg = child.GetComponent<CanvasGroup>();
                if (cg != null)
                    cg.alpha = 1f;
            }

            UpdateCreateButtonState();
            return;
        }

        selectedCategory = categoryName;

        foreach (Transform child in categoryContainer)
        {
            CanvasGroup cg = child.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = 0.5f;
        }

        CanvasGroup selectedCg = btn.GetComponent<CanvasGroup>();
        if (selectedCg != null)
            selectedCg.alpha = 1f;

        lastSelectedCategoryButton = btn;

        UpdateCreateButtonState();
    }

    // -----------------------------
    // ДАТА
    // -----------------------------
    public void SetDate(DateTime date)
    {
        selectedDate = date;
        UpdateCreateButtonState();
    }

    // -----------------------------
    // СУММА — ввод и форматирование
    // -----------------------------
    private void OnAmountChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            UpdateCreateButtonState();
            return;
        }

        string cleaned = "";
        bool dotFound = false;

        foreach (char c in value)
        {
            if (char.IsDigit(c))
                cleaned += c;
            else if ((c == '.' || c == ',') && !dotFound)
            {
                cleaned += '.';
                dotFound = true;
            }
        }

        if (cleaned != value)
        {
            int caret = amountInput.caretPosition;
            amountInput.text = cleaned;
            amountInput.caretPosition = Mathf.Min(caret, cleaned.Length);
        }

        UpdateCreateButtonState();
    }

    private void FormatAmount(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        // приводим к инвариантному виду: точка как разделитель
        string raw = value.Trim()
                          .Replace(" ", "")
                          .Replace("\u200B", "")
                          .Replace(",", ".");

        if (float.TryParse(raw,
                           NumberStyles.Any,
                           CultureInfo.InvariantCulture,
                           out float number))
        {
            // форматируем тоже через InvariantCulture
            amountInput.text = number.ToString("0.00", CultureInfo.InvariantCulture);
        }
        else
        {
            amountInput.text = "";
        }

        UpdateCreateButtonState();
    }

    // -----------------------------
    // ОПИСАНИЕ
    // -----------------------------
    public void OnDescriptionChanged(string _)
    {
        const int maxLen = 100;

        string real = descriptionInput.text;

        if (real.Length > maxLen)
        {
            int caret = descriptionInput.caretPosition;

            real = real.Substring(0, maxLen);
            descriptionInput.text = real;

            descriptionInput.caretPosition = Mathf.Min(caret, maxLen);
        }

        descriptionCounter.text = $"{real.Length}/{maxLen}";
    }

    // -----------------------------
    // СОЗДАНИЕ ТРАНЗАКЦИИ
    // -----------------------------
    public void CreateTransaction()
    {
        if (!ValidateInput())
            return;

        float amount;
        string raw = amountInput.text.Replace(" ", "").Replace("\u200B", "").Replace(",", ".");

        if (!float.TryParse(raw,
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out amount))
        {
            Debug.LogError("❌ Ошибка парсинга суммы в CreateTransaction()");
            return;
        }

        Category cat = loadedCategories.Find(c => c.Name == selectedCategory && c.IsIncome == isIncome);

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

        SceneManager.LoadScene("MainPage");
    }

    private bool ValidateInput()
    {
        if (amountInput == null)
            return false;

        string raw = amountInput.text ?? "";

        string clean = raw
            .Replace(" ", "")
            .Replace("\u200B", "")
            .Replace("\u2060", "")
            .Replace("\uFEFF", "")
            .Replace("\n", "")
            .Replace("\r", "")
            .Replace(",", ".")
            .Trim();

        if (string.IsNullOrEmpty(clean))
            return false;

        bool parsed = float.TryParse(clean,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out float amount);

        if (!parsed)
            return false;

        if (amount <= 0)
            return false;

        if (selectedCategory == null)
            return false;

        if (selectedAccount == null)
            return false;

        return true;
    }

    private void UpdateCreateButtonState()
    {
        if (createButtonCanvas == null)
            return;

        bool valid = ValidateInput();

        createButton.interactable = valid;
        createButtonCanvas.alpha = valid ? 1f : 0.5f;
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

    private string CleanAmount(string raw)
    {
        if (raw == null)
            return "";

        return raw
            .Replace(" ", "")
            .Replace("\u200B", "")
            .Replace("\u2060", "")
            .Replace("\uFEFF", "")
            .Replace("\n", "")
            .Replace("\r", "")
            .Replace(",", ".")
            .Trim();
    }

    
}
