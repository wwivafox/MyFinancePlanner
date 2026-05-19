using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System;
using System.Globalization;


public class MainPage : MonoBehaviour
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
    
    public TMP_Text totalBalanceText;

    private Repository repo;
    private List<Account> accounts;


    private const string API_URL = "https://open.er-api.com/v6/latest/{0}";

    public Transform accountsContainer;
    public GameObject accountViewPrefab;
    public Transform addButtonTransform; // кнопка "Добавить"

    public Transform transactionsContainer;
    public GameObject transactionPrefab;



    void Start()
    {
        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        while (DatabaseManager.Instance == null)
            yield return null;

        while (DatabaseManager.Instance.DB == null)
            yield return null;

        yield return StartCoroutine(LoadTotalBalance());
        yield return StartCoroutine(LoadAccountsList());
        yield return StartCoroutine(LoadLastTransactions());
    }




    private IEnumerator LoadTotalBalance()
    {
        while (DatabaseManager.Instance == null)
            yield return null;

        while (DatabaseManager.Instance.DB == null)
            yield return null;

        repo = new Repository();
        accounts = repo.GetAccounts();

        float totalBYN = 0f;

        foreach (var acc in accounts)
        {
            float amount = acc.StartAmount;
            string currency = MapCurrency(acc.Currency);

            if (currency == "BYN")
            {
                totalBYN += amount;
            }
            else
            {
                float converted = 0f;
                yield return StartCoroutine(ConvertToBYN(amount, currency, r => converted = r));
                totalBYN += converted;
            }
        }

        totalBalanceText.text = $"{totalBYN:0.00} BYN";
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

    private IEnumerator ConvertToBYN(float amount, string currency, Action<float> callback)
    {
        string url = string.Format(API_URL, currency);
        Debug.Log("Запрос курса: " + url);

        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Ошибка API: " + req.error);
            callback(0f);
            yield break;
        }

        var json = req.downloadHandler.text;
        Debug.Log("Ответ API: " + json);

        ExchangeRateResponse data = JsonUtility.FromJson<ExchangeRateResponse>(json);

        if (data == null || data.rates == null)
        {
            Debug.LogError("JSON не распознан");
            callback(0f);
            yield break;
        }

        // Проверяем, что BYN есть и не ноль
        if (data.rates.BYN <= 0f)
        {
            Debug.LogError("В ответе нет курса BYN или он равен 0");
            callback(0f);
            yield break;
        }

        float rate = data.rates.BYN;
        callback(amount * rate);
    }

    private IEnumerator LoadAccountsList()
    {
        // очищаем старые элементы, но НЕ кнопку "Добавить"
        foreach (Transform child in accountsContainer)
        {
            if (child != addButtonTransform)
                Destroy(child.gameObject);
        }

        foreach (var acc in accounts)
        {
            GameObject item = Instantiate(accountViewPrefab, accountsContainer);

            AccountItem view = item.GetComponent<AccountItem>();
            view.Init(acc, null); // второй параметр — TransactionCreator, но он тут не нужен
        }

        // кнопка "Добавить" всегда последняя
        addButtonTransform.SetAsLastSibling();

        yield break;
    }

    private IEnumerator LoadLastTransactions()
    {
        var repo = new Repository();
        List<Transaction> list = repo.GetTransactions();

        if (list == null || list.Count == 0)
            yield break;

        // сортируем по дате (самые свежие сверху)
        list.Sort((a, b) =>
        {
            DateTime da = DateTime.ParseExact(a.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime db = DateTime.ParseExact(b.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return db.CompareTo(da); // свежие → вверх
        });

        // берём только 3
        var last3 = list.GetRange(0, Mathf.Min(3, list.Count));

        // очищаем контейнер
        foreach (Transform child in transactionsContainer)
            Destroy(child.gameObject);

        foreach (var t in last3)
        {
            GameObject item = Instantiate(transactionPrefab, transactionsContainer);

            // UI элементы по твоей структуре
            TMP_Text nameText = item.transform.Find("Название счёта").GetComponentInChildren<TMP_Text>();
            TMP_Text amountText = item.transform.Find("Сумма").GetComponentInChildren<TMP_Text>();
            TMP_Text dateText = item.transform.Find("дата").GetComponent<TMP_Text>();
            TMP_Text weekdayText = item.transform.Find("день недели").GetComponent<TMP_Text>();
            TMP_Text categoryText = item.transform.Find("категория/категория").GetComponent<TMP_Text>();
            UnityEngine.UI.Image categoryIcon = item.transform.Find("категория/Изображение категории").GetComponent<UnityEngine.UI.Image>();

            // данные
            Account acc = repo.GetAccountById(t.AccountId);
            Category cat = repo.GetCategoryById(t.CategoryId);

            // заполнение
            nameText.text = acc.Name;
            string sign = t.Amount >= 0 ? "+" : "-";
            float absAmount = Mathf.Abs(t.Amount);

            amountText.text = $"{sign}{absAmount:0.00} {acc.Currency}";

            dateText.text = t.Date;

            // день недели
            weekdayText.text = DateTime.ParseExact(t.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("dddd", CultureInfo.GetCultureInfo("ru-RU"));

            categoryText.text = cat.Name;

            // иконка категории
            categoryIcon.sprite = Resources.Load<Sprite>("Sprites/Category/" + cat.IconName);
        }

        yield break;
    }




}
