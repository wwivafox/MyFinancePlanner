using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class StatisticsPage : MonoBehaviour
{
    [Serializable]
    public class ExchangeRateResponse
    {
        public string result;
        public string base_code;
        public Rates rates;
    }

    [Serializable]
    public class Rates
    {
        public float BYN;
    }

    private const string API_URL = "https://open.er-api.com/v6/latest/{0}";

    private Dictionary<string, float> cachedRates = new Dictionary<string, float>();
    private bool ratesLoaded = false;

    [Header("UI — вкладки доход/расход")]
    public Button incomeTab;
    public Button expenseTab;

    [Header("UI — категории")]
    public Transform categoryContainer;
    public GameObject categoryButtonPrefab;

    [Header("UI — выбор типа статистики")]
    public Button typeSelectorButton;
    public TMP_Text typeLabel;

    [Header("UI — результат")]
    public TMP_Text resultText;

    [Header("Селектор периода")]
    public StatisticsPeriodSelector periodSelector;

    private bool isIncome = true;

    private List<Category> loadedCategories;
    private List<Account> loadedAccounts;
    private List<Transaction> loadedTransactions;

    private Category selectedCategory;
    private Button lastSelectedCategoryButton;

    private StatisticsType currentType = StatisticsType.All;
    private int currentYear = 0;
    private int currentMonth = 0;

    private int minYear = 2000;
    private int maxYear = 2100;

    private void Awake()
    {
        incomeTab.onClick.AddListener(() => SwitchType(true));
        expenseTab.onClick.AddListener(() => SwitchType(false));
        typeSelectorButton.onClick.AddListener(OpenPeriodSelector);

        periodSelector.OnPeriodSelected = OnPeriodSelected;
    }

    private IEnumerator Start()
    {
        while (DatabaseManager.Instance == null)
            yield return null;

        while (DatabaseManager.Instance.DB == null)
            yield return null;

        var conn = DatabaseManager.Instance.DB.GetConnection();

        loadedCategories = conn.Table<Category>().ToList();
        loadedAccounts = conn.Table<Account>().ToList();
        loadedTransactions = conn.Table<Transaction>().ToList();
        
        var now = DateTime.Now;
        minYear = 2000;
        maxYear = now.Year;

        currentYear = now.Year;
        currentMonth = now.Month;

        yield return StartCoroutine(LoadRates());

        SwitchType(true);
        UpdateTypeLabel();
        UpdateResult();
    }


    private IEnumerator LoadRates()
    {
        string[] currencies = { "USD", "EUR", "RUB" };

        foreach (string cur in currencies)
        {
            string url = string.Format(API_URL, cur);

            UnityWebRequest req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Ошибка API: " + req.error);
                cachedRates[cur] = 0f;
                continue;
            }

            var json = req.downloadHandler.text;
            ExchangeRateResponse data = JsonUtility.FromJson<ExchangeRateResponse>(json);

            if (data == null || data.rates == null || data.rates.BYN <= 0f)
            {
                Debug.LogError("Ошибка JSON или BYN=0");
                cachedRates[cur] = 0f;
                continue;
            }

            cachedRates[cur] = data.rates.BYN;
        }

        ratesLoaded = true;
    }

    private string MapCurrency(string c)
    {
        if (string.IsNullOrEmpty(c)) return "BYN";

        c = c.ToUpper();

        if (c.Contains("BYN") || c.Contains("БЕЛ")) return "BYN";
        if (c.Contains("USD") || c.Contains("ДОЛ")) return "USD";
        if (c.Contains("EUR") || c.Contains("ЕВРО")) return "EUR";
        if (c.Contains("RUB") || c.Contains("РУБ")) return "RUB";

        return "BYN";
    }

    private float ConvertToBYN(Account acc, float amount)
    {
        string currency = MapCurrency(acc.Currency);

        if (currency == "BYN")
            return amount;

        if (!ratesLoaded)
            return 0f;

        if (!cachedRates.ContainsKey(currency))
            return amount;

        float rate = cachedRates[currency];
        return amount * rate;
    }

    private void SwitchType(bool income)
    {
        isIncome = income;

        incomeTab.interactable = !income;
        expenseTab.interactable = income;

        incomeTab.GetComponentInChildren<TMP_Text>().fontSize = income ? 29 : 24;
        expenseTab.GetComponentInChildren<TMP_Text>().fontSize = income ? 24 : 29;

        LoadCategories();
    }

    private void LoadCategories()
    {
        Debug.Log("=== ГЕНЕРАЦИЯ КАТЕГОРИЙ ===");

        foreach (Transform child in categoryContainer)
            Destroy(child.gameObject);

        selectedCategory = null;
        lastSelectedCategoryButton = null;

        var list = loadedCategories.Where(c => c.IsIncome == isIncome).ToList();

        Debug.Log($"Категорий найдено: {list.Count}");

        foreach (var cat in list)
        {
            Debug.Log($"Создаю кнопку категории: {cat.Name}");

            GameObject btnObj = Instantiate(categoryButtonPrefab, categoryContainer);

            btnObj.GetComponentInChildren<TMP_Text>().text = cat.Name;

            var icon = btnObj.transform.Find("Icon").GetComponent<Image>();
            icon.sprite = Resources.Load<Sprite>("Sprites/Category/" + cat.IconName);

            Button btn = btnObj.GetComponent<Button>();

            Category localCat = cat;
            Button localBtn = btn;

            localBtn.onClick.AddListener(() =>
            {
                Debug.Log($"Нажата категория: {localCat.Name}");
                SelectCategory(localCat, localBtn);
            });

            CanvasGroup cg = btnObj.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = btnObj.AddComponent<CanvasGroup>();
                Debug.Log($"CanvasGroup добавлен на {cat.Name}");
            }
            cg.alpha = 1f;
        }

        UpdateResult();
    }

    private void SelectCategory(Category cat, Button btn)
    {
        Debug.Log($"SelectCategory вызван для: {cat.Name}");

        if (selectedCategory == cat)
        {
            Debug.Log("Категория снята");
            selectedCategory = null;
            lastSelectedCategoryButton = null;

            foreach (Transform child in categoryContainer)
                child.GetComponent<CanvasGroup>().alpha = 1f;

            UpdateResult();
            return;
        }

        selectedCategory = cat;
        lastSelectedCategoryButton = btn;

        Debug.Log($"Выбрана категория: {cat.Name}");

        foreach (Transform child in categoryContainer)
            child.GetComponent<CanvasGroup>().alpha = 0.5f;

        btn.GetComponent<CanvasGroup>().alpha = 1f;

        UpdateResult();
    }

    private void OpenPeriodSelector()
    {
        periodSelector.Open(currentType, currentYear, currentMonth, minYear, maxYear);
    }

    private void OnPeriodSelected(StatisticsType type, int year, int month)
    {
        currentType = type;
        currentYear = year;
        currentMonth = month;

        UpdateTypeLabel();
        UpdateResult();
    }

    private void UpdateTypeLabel()
    {
        if (currentType == StatisticsType.All)
        {
            typeLabel.text = "Вся статистика";
            return;
        }

        if (currentType == StatisticsType.Month)
        {
            string[] months =
            {
                "январь","февраль","март","апрель","май","июнь",
                "июль","август","сентябрь","октябрь","ноябрь","декабрь"
            };

            typeLabel.text = $"За месяц {months[currentMonth - 1]}";
            return;
        }

        if (currentType == StatisticsType.Year)
        {
            typeLabel.text = $"За год {currentYear}";
            return;
        }
    }

    private void UpdateResult()
    {
        if (selectedCategory == null)
        {
            resultText.text = "";
            return;
        }

        float totalBYN = 0f;

        foreach (var t in loadedTransactions)
        {
            if (t.CategoryId != selectedCategory.Id)
                continue;

            if (!DateTime.TryParseExact(t.Date, "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime d))
                continue;

            if (currentType == StatisticsType.Month &&
                (d.Year != currentYear || d.Month != currentMonth))
                continue;

            if (currentType == StatisticsType.Year &&
                d.Year != currentYear)
                continue;

            Account acc = loadedAccounts.First(a => a.Id == t.AccountId);

            float converted = ConvertToBYN(acc, t.Amount);
            totalBYN += converted;
        }

        string sign = isIncome ? "+" : "-";
        resultText.text = $"{sign} {totalBYN:0.00} BYN";
    }
}
