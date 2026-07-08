using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddAmountPanel : MonoBehaviour
{
    public CanvasGroup panelGroup;

    public TMP_Text titleText;
    public TMP_Text sumLabel;
    public Image flowerImage;

    public Button plusButton;
    public Button minusButton;
    public TMP_InputField amountInput;
    public Button saveButton;
    public Button closeButton;

    public Button editButton;
    public Button deleteButton;
    public GameObject confirmDeletePanel;
    public Button confirmYesButton;
    public Button confirmNoButton;

    public GameObject flowers;

    private Goal currentGoal;
    private int sign = 0;
    private Action<Goal> onChanged;

    private void Awake()
    {
        Hide();

        plusButton.onClick.AddListener(() => SetSign(+1));
        minusButton.onClick.AddListener(() => SetSign(-1));
        saveButton.onClick.AddListener(OnSaveClicked);
        closeButton.onClick.AddListener(() =>
        {
            flowers.SetActive(true);
            Hide();
        });


        amountInput.onValueChanged.AddListener(_ => OnAmountTyping());
        amountInput.onEndEdit.AddListener(_ => OnAmountEndEdit());

        editButton.onClick.AddListener(OnEditClicked);
        deleteButton.onClick.AddListener(OnDeleteClicked);

        confirmYesButton.onClick.AddListener(OnConfirmDelete);

        confirmNoButton.onClick.AddListener(() =>
        {
            confirmDeletePanel.SetActive(false);

            panelGroup.interactable = true;
            panelGroup.blocksRaycasts = true;
        });
    }


    public void Open(Goal goal, Sprite flowerSprite, Action<Goal> onChanged)
    {
        
        this.currentGoal = goal;
        this.onChanged = onChanged;

        titleText.text = goal.Title;
        sumLabel.text = $"{goal.CurrentAmount:0.00}/{goal.TargetAmount:0.00}";
        flowerImage.sprite = flowerSprite;

        amountInput.text = "";
        sign = 0;

        UpdateSignButtons();
        UpdateSaveButton();

        Show();
    }

    private void Show()
    {
        panelGroup.alpha = 1f;
        panelGroup.interactable = true;
        panelGroup.blocksRaycasts = true;
    }

    private void Hide()
    {
        panelGroup.alpha = 0f;
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;
    }

    
    // ЗНАК
    private void SetSign(int s)
    {
        sign = s;
        UpdateSignButtons();
        UpdateSaveButton();
    }

    private void UpdateSignButtons()
    {
        SetAlpha(plusButton, sign == +1 ? 1f : 0.5f);
        SetAlpha(minusButton, sign == -1 ? 1f : 0.5f);
    }

    private void SetAlpha(Button btn, float a)
    {
        var cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = a;
    }

  
    // ВВОД (без форматирования)
    private void OnAmountTyping()
    {
        string clean = CleanAmount(amountInput.text);

        if (clean != amountInput.text)
        {
            int pos = amountInput.caretPosition;
            amountInput.text = clean;
            amountInput.caretPosition = Mathf.Clamp(pos - 1, 0, clean.Length);
        }

        UpdateSaveButton();
    }


    // ФОРМАТИРОВАНИЕ ПРИ ПОТЕРЕ ФОКУСА

    private void OnAmountEndEdit()
    {
        float.TryParse(CleanAmount(amountInput.text), NumberStyles.Any, CultureInfo.InvariantCulture, out float value);

        float maxAllowed = currentGoal.TargetAmount - currentGoal.CurrentAmount;
        if (value > maxAllowed)
            value = maxAllowed;

        amountInput.text = FormatMoney(value);

        UpdateSaveButton();
    }

 
    // ОЧИСТКА СТРОКИ
    private string CleanAmount(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return "";

        string clean = raw
            .Replace(" ", "")
            .Replace("\u200B", "")
            .Replace("\u2060", "")
            .Replace("\uFEFF", "")
            .Replace("\n", "")
            .Replace("\r", "")
            .Replace(",", ".")
            .Trim();

        string result = "";
        bool hasDot = false;

        foreach (char c in clean)
        {
            if (char.IsDigit(c))
                result += c;
            else if (c == '.' && !hasDot)
            {
                result += c;
                hasDot = true;
            }
        }

        return result;
    }

    // ФОРМАТИРОВАНИЕ

    private string FormatMoney(float value)
    {
        string s = value.ToString(CultureInfo.InvariantCulture);

        if (!s.Contains("."))
            return s + ".00";

        string[] parts = s.Split('.');
        string whole = parts[0];
        string frac = parts.Length > 1 ? parts[1] : "";

        if (frac.Length == 0)
            frac = "00";
        else if (frac.Length == 1)
            frac = frac + "0";
        else if (frac.Length > 2)
            frac = frac.Substring(0, 2);

        return whole + "." + frac;
    }

    // ВАЛИДАЦИЯ
   
    private bool Validate()
    {
        if (sign == 0)
            return false;

        float.TryParse(CleanAmount(amountInput.text), NumberStyles.Any, CultureInfo.InvariantCulture, out float amount);
        if (amount <= 0)
            return false;

        float maxAllowed = currentGoal.TargetAmount - currentGoal.CurrentAmount;
        if (amount > maxAllowed)
            return false;

        return true;
    }

    private void UpdateSaveButton()
    {
        bool valid = Validate();

        var cg = saveButton.GetComponent<CanvasGroup>();
        if (cg == null) cg = saveButton.gameObject.AddComponent<CanvasGroup>();

        cg.alpha = valid ? 1f : 0.5f;
        cg.interactable = valid;
        cg.blocksRaycasts = valid;
    }

  
    // СОХРАНЕНИЕ
   
    private void OnSaveClicked()
    {
        if (!Validate())
            return;

        float.TryParse(CleanAmount(amountInput.text), NumberStyles.Any, CultureInfo.InvariantCulture, out float amount);

        float maxAllowed = currentGoal.TargetAmount - currentGoal.CurrentAmount;
        amount = Mathf.Clamp(amount, 0f, maxAllowed);

        float newValue = currentGoal.CurrentAmount + sign * amount;
        newValue = Mathf.Clamp(newValue, 0f, currentGoal.TargetAmount);

        currentGoal.CurrentAmount = newValue;

        onChanged?.Invoke(currentGoal);
        flowers.SetActive(true);
        Hide();
    }


    private void OnEditClicked()
    {
        Hide(); 

        FindObjectOfType<NewGoalPanel>().Open(
            (newTitle, newTarget, newInitial) =>
            {
                currentGoal.Title = newTitle;
                currentGoal.TargetAmount = newTarget;

                onChanged?.Invoke(currentGoal);
            },
            currentGoal 
        );
    }

    private void OnDeleteClicked()
    {
        confirmDeletePanel.SetActive(true);

       
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;
        confirmYesButton.interactable = true;
        confirmNoButton.interactable = true;

    }

    private void OnConfirmDelete()
    {
        confirmDeletePanel.SetActive(false);

        flowers.SetActive(true);
        Hide();

        Repository repo = new Repository();
        repo.DeleteGoal(currentGoal.Id);

        onChanged?.Invoke(null);
    }

}
