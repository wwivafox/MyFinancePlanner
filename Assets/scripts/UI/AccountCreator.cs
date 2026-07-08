using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Globalization;

public class AccountCreator : MonoBehaviour
{
    [Header("Заголовок страницы")]
    public TMP_Text headerText;

    [Header("Название счёта")]
    public TMP_InputField nameInput;
    public TMP_Text nameCounter;

    [Header("Валюта")]
    public TMP_Text currencyText;
    public Button currencyButton;
    public GameObject currencyPanel;
    public Button closeCurrencyPanelButton;

    public TMP_Text currencyBYN;
    public TMP_Text currencyUSD;
    public TMP_Text currencyEUR;
    public TMP_Text currencyRUB;

    private string selectedCurrency = null;

    [Header("Начальная сумма")]
    public TMP_InputField amountInput;
    public TMP_Text amountLabel;

    [Header("Цвет счёта")]
    public Transform colorContainer;
    public GameObject colorCirclePrefab;

    private string selectedColorHex = null;
    private List<Image> colorCircles = new List<Image>();

    [Header("Кнопка сохранить")]
    public Button saveButton;
    private CanvasGroup saveButtonCanvas;

    private Repository repo;

    private readonly Color activeColor = new Color32(0x34, 0xC7, 0x59, 0xFF);
    private readonly Color inactiveColor = new Color32(0x5B, 0x5A, 0x5A, 0xFF);

    private readonly string[] availableColors = new string[]
    {
        "#DB1E21", "#4A1EDB", "#DBD21E", "#1EDB3A",
        "#DB1EC8", "#1EB2DB", "#EB8423", "#FF2D55",
        "#412F6E", "#2F7554", "#AF5A5C", "#6672AF"
    };

    private bool isEditMode = false;
    private Account editingAccount;

    void Start()
    {
        repo = new Repository();

        saveButtonCanvas = saveButton.GetComponent<CanvasGroup>();
        if (saveButtonCanvas == null)
            saveButtonCanvas = saveButton.gameObject.AddComponent<CanvasGroup>();

        currencyPanel.SetActive(false);

        nameInput.onValueChanged.AddListener(OnNameChanged);
        amountInput.onValueChanged.AddListener(OnAmountChanged);
        amountInput.onEndEdit.AddListener(FormatAmount);

        currencyButton.onClick.AddListener(OpenCurrencyPanel);
        closeCurrencyPanelButton.onClick.AddListener(CloseCurrencyPanel);

        currencyBYN.GetComponent<Button>().onClick.AddListener(() => SelectCurrency("BYN", currencyBYN));
        currencyUSD.GetComponent<Button>().onClick.AddListener(() => SelectCurrency("USD", currencyUSD));
        currencyEUR.GetComponent<Button>().onClick.AddListener(() => SelectCurrency("EUR", currencyEUR));
        currencyRUB.GetComponent<Button>().onClick.AddListener(() => SelectCurrency("RUB", currencyRUB));

        GenerateColorCircles();

        saveButton.onClick.AddListener(SaveAccount);

        if (EditAccountData.EditingAccount != null)
        {
            isEditMode = true;
            editingAccount = EditAccountData.EditingAccount;

            headerText.text = "Редактирование счёта";

            nameInput.text = editingAccount.Name;
            selectedCurrency = editingAccount.Currency;
            currencyText.text = selectedCurrency;

            amountInput.text = editingAccount.StartAmount.ToString("0.00");

            selectedColorHex = editingAccount.ColorHex;
            HighlightSelectedColor();

            amountLabel.text = "Текущая сумма";
        }
        else
        {
            headerText.text = "Новый счёт";
            amountLabel.text = "Начальная сумма";

            SelectCurrency("BYN", currencyBYN);
        }

        UpdateSaveButtonState();
    }


    private void FillEditMode()
    {
        nameInput.text = editingAccount.Name;
        nameCounter.text = $"{editingAccount.Name.Length}/20";

        selectedCurrency = editingAccount.Currency;
        currencyText.text = selectedCurrency;

        amountInput.text = editingAccount.StartAmount.ToString("0.00", CultureInfo.InvariantCulture);

        selectedColorHex = editingAccount.ColorHex;

        foreach (var img in colorCircles)
            img.canvasRenderer.SetAlpha(0.5f);

        foreach (var img in colorCircles)
        {
            if (ColorUtility.TryParseHtmlString(selectedColorHex, out var col))
            {
                if (img.color == col)
                {
                    img.canvasRenderer.SetAlpha(1f);
                    break;
                }
            }
        }

        saveButton.GetComponentInChildren<TMP_Text>().text = "Сохранить";
    }

    // НАЗВАНИЕ СЧЁТА
    private void OnNameChanged(string value)
    {
        if (value.Length > 20)
            nameInput.text = value.Substring(0, 20);

        nameCounter.text = $"{nameInput.text.Length}/20";
        UpdateSaveButtonState();
    }

    // ВАЛЮТА
    public void OpenCurrencyPanel() => currencyPanel.SetActive(true);
    private void CloseCurrencyPanel() => currencyPanel.SetActive(false);

    private void SelectCurrency(string currency, TMP_Text selectedText)
    {
        selectedCurrency = currency;
        currencyText.text = currency;

        currencyBYN.color = inactiveColor;
        currencyUSD.color = inactiveColor;
        currencyEUR.color = inactiveColor;
        currencyRUB.color = inactiveColor;

        selectedText.color = activeColor;

        CloseCurrencyPanel();
        UpdateSaveButtonState();
    }

    // СУММА
    private void OnAmountChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            UpdateSaveButtonState();
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
    }

    private void FormatAmount(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        string raw = value.Trim().Replace(" ", "").Replace("\u200B", "").Replace(",", ".");

        if (float.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out float number))
            amountInput.text = number.ToString("0.00", CultureInfo.InvariantCulture);
        else
            amountInput.text = "";

        UpdateSaveButtonState();
    }

    // ЦВЕТ
    private void GenerateColorCircles()
    {
        foreach (string hex in availableColors)
        {
            GameObject circle = Instantiate(colorCirclePrefab, colorContainer);

            Image img = circle.GetComponentInChildren<Image>();

            ColorUtility.TryParseHtmlString(hex, out var col);
            img.color = col;

            colorCircles.Add(img);

            Button btn = circle.GetComponent<Button>();
            btn.onClick.AddListener(() => SelectColor(hex, img));
        }
    }

    private void SelectColor(string hex, Image selectedImg)
    {
        if (selectedColorHex == hex)
        {
            selectedColorHex = null;
            foreach (var img in colorCircles)
                img.canvasRenderer.SetAlpha(1f);

            UpdateSaveButtonState();
            return;
        }

        selectedColorHex = hex;

        foreach (var img in colorCircles)
            img.canvasRenderer.SetAlpha(0.5f);

        selectedImg.canvasRenderer.SetAlpha(1f);

        UpdateSaveButtonState();
    }

    // СОХРАНЕНИЕ
    private void SaveAccount()
    {
        if (!ValidateInput())
            return;

        float amount = float.Parse(amountInput.text, CultureInfo.InvariantCulture);

        if (isEditMode)
        {
            editingAccount.Name = nameInput.text;
            editingAccount.Currency = selectedCurrency;
            editingAccount.StartAmount = amount;
            editingAccount.ColorHex = selectedColorHex;

            repo.UpdateAccount(editingAccount);

            EditAccountData.EditingAccount = null;
            SceneManager.LoadScene("MainPage");
            return;
        }

        Account acc = new Account
        {
            Name = nameInput.text,
            Currency = selectedCurrency,
            StartAmount = amount,
            ColorHex = selectedColorHex
        };

        repo.AddAccount(acc);

        SceneManager.LoadScene("MainPage");

        EditAccountData.EditingAccount = null;

    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(nameInput.text)) return false;
        if (selectedCurrency == null) return false;
        if (string.IsNullOrWhiteSpace(amountInput.text)) return false;
        if (selectedColorHex == null) return false;

        return true;
    }

    private void UpdateSaveButtonState()
    {
        bool valid = ValidateInput();
        saveButton.interactable = valid;
        saveButtonCanvas.alpha = valid ? 1f : 0.5f;
    }

    private void HighlightSelectedColor()
    {
        foreach (var img in colorCircles)
            img.canvasRenderer.SetAlpha(0.5f);

        foreach (var img in colorCircles)
        {
            if (ColorUtility.ToHtmlStringRGB(img.color) == selectedColorHex.Replace("#", ""))
            {
                img.canvasRenderer.SetAlpha(1f);
                break;
            }
        }
    }

    private void OnDisable()
    {
        EditAccountData.EditingAccount = null;
        isEditMode = false;
    }


}
