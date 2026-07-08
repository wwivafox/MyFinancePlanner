using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System;
using System.Globalization;
using UnityEngine.EventSystems;


public static class AccountPanelState
{
    public static bool AnyPanelOpen = false;
}

public class MainPage : MonoBehaviour
{
    public static MainPage Instance;

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

    [HideInInspector]
    public GameObject openedActionPanel = null;


    public TMP_Text totalBalanceText;

    private Repository repo;
    private List<Account> accounts;

    private const string API_URL = "https://open.er-api.com/v6/latest/{0}";

    [Header("Счета")]
    public Transform accountsContainer;
    public GameObject accountViewPrefab;
    public Transform addButtonTransform;

    public Canvas mainCanvas;



    [Header("Последние транзакции")]
    public Transform transactionsContainer;
    public GameObject transactionPrefab;

    [Header("Панель подтверждения удаления")]
    public GameObject confirmDeletePanel;

    void Start()
    {
        Instance = this;

        if (confirmDeletePanel != null)
            confirmDeletePanel.SetActive(false);

        StartCoroutine(Init());
    }

    public void OpenConfirmPanel(Account acc)
    {
        var panel = confirmDeletePanel.GetComponent<ConfirmDeletePanel>();
        panel.SetAccount(acc);

        confirmDeletePanel.SetActive(true);
        AccountPanelState.AnyPanelOpen = true;
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

        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            callback(0f);
            yield break;
        }

        var json = req.downloadHandler.text;
        ExchangeRateResponse data = JsonUtility.FromJson<ExchangeRateResponse>(json);

        if (data == null || data.rates == null || data.rates.BYN <= 0f)
        {
            callback(0f);
            yield break;
        }

        callback(amount * data.rates.BYN);
    }

    private IEnumerator LoadAccountsList()
    {
        foreach (Transform child in accountsContainer)
        {
            if (child != addButtonTransform)
                Destroy(child.gameObject);
        }

        foreach (var acc in accounts)
        {
            GameObject item = Instantiate(accountViewPrefab, accountsContainer);

            AccountItem2 view = item.GetComponent<AccountItem2>();
            view.Init(acc, null);
        }

        addButtonTransform.SetAsLastSibling();

        yield break;
    }

    private IEnumerator LoadLastTransactions()
    {
        var repo = new Repository();
        List<Transaction> list = repo.GetTransactions();

        if (list == null || list.Count == 0)
            yield break;

        list.Sort((a, b) =>
        {
            DateTime da = DateTime.ParseExact(a.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime db = DateTime.ParseExact(b.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return db.CompareTo(da);
        });

        var last3 = list.GetRange(0, Mathf.Min(3, list.Count));

        foreach (Transform child in transactionsContainer)
            Destroy(child.gameObject);

        foreach (var t in last3)
        {
            GameObject item = Instantiate(transactionPrefab, transactionsContainer);

            TMP_Text nameText = item.transform.Find("Название счёта").GetComponentInChildren<TMP_Text>();
            TMP_Text amountText = item.transform.Find("Сумма").GetComponentInChildren<TMP_Text>();
            TMP_Text dateText = item.transform.Find("дата").GetComponent<TMP_Text>();
            TMP_Text weekdayText = item.transform.Find("день недели").GetComponent<TMP_Text>();
            TMP_Text categoryText = item.transform.Find("категория/категория").GetComponent<TMP_Text>();
            UnityEngine.UI.Image categoryIcon = item.transform.Find("категория/Изображение категории").GetComponent<UnityEngine.UI.Image>();

            Account acc = repo.GetAccountById(t.AccountId);
            Category cat = repo.GetCategoryById(t.CategoryId);

            nameText.text = acc.Name;
            string sign = cat.IsIncome ? "+" : "-";
            amountText.text = $"{sign}{t.Amount:0.00} {acc.Currency}";
            dateText.text = t.Date;

            weekdayText.text = DateTime.ParseExact(t.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                .ToString("dddd", CultureInfo.GetCultureInfo("ru-RU"));

            categoryText.text = cat.Name;
            categoryIcon.sprite = Resources.Load<Sprite>("Sprites/Category/" + cat.IconName);
        }

        yield break;
    }

    private void OnDisable()
    {
        AccountPanelState.AnyPanelOpen = false;
    }

    void Update()
    {
        if (!AccountPanelState.AnyPanelOpen)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (ClickedInsideActionPanel())
                return;

            if (confirmDeletePanel.activeSelf && ClickedInside(confirmDeletePanel))
                return;

            CloseActionPanel();
        }
    }

    private bool ClickedInside(GameObject panel)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var r in results)
        {
            if (r.gameObject == panel || r.gameObject.transform.IsChildOf(panel.transform))
                return true;
        }

        return false;
    }



    public void CloseActionPanel()
    {
        if (openedActionPanel != null)
        {
            openedActionPanel.SetActive(false);
            openedActionPanel = null;
        }

        AccountPanelState.AnyPanelOpen = false;
    }


    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }


    private bool IsPointerOverUIObject()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }

    private bool ClickedInsideActionPanel()
    {
        if (openedActionPanel == null)
            return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var r in results)
        {
            if (r.gameObject == openedActionPanel || r.gameObject.transform.IsChildOf(openedActionPanel.transform))
                return true;
        }

        return false;
    }

}
