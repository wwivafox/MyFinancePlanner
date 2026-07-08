using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewGoalPanel : MonoBehaviour
{
    public CanvasGroup panelGroup;

    public TMP_InputField nameInput;
    public TMP_InputField targetInput;
    public TMP_InputField initialInput;
    public TMP_Text Title;

    public TMP_Text counterText;
    public Button saveButton;
    public Button closeButton;

    public GameObject flowers;

    private bool isEditMode = false;
    private Goal editingGoal;


    private Action<string, float, float> onSave;

    private void Awake()
    {
        Hide();

        nameInput.onValueChanged.AddListener(_ => OnNameChanged());
        targetInput.onValueChanged.AddListener(_ => OnTargetTyping());
        initialInput.onValueChanged.AddListener(_ => OnInitialTyping());

        targetInput.onEndEdit.AddListener(_ => OnTargetEndEdit());
        initialInput.onEndEdit.AddListener(_ => OnInitialEndEdit());

        saveButton.onClick.AddListener(OnSaveClicked);
        closeButton.onClick.AddListener(() =>
        {
            flowers.SetActive(true);
            Hide();
        });
    }

    public void Open(Action<string, float, float> onSave, Goal goalToEdit = null)
    {
        this.onSave = onSave;

        if (goalToEdit == null)
        {
           
            isEditMode = false;
            editingGoal = null;

            nameInput.text = "";
            targetInput.text = "";
            initialInput.text = "";

            nameInput.interactable = true;
            targetInput.interactable = true;
            initialInput.interactable = true;

            SetCanvasGroup(initialInput, 1f, true);

            counterText.text = "0/20";
        }
        else
        {
            
            isEditMode = true;
            editingGoal = goalToEdit;
            Title.text = "Редактирование цели";
            Title.fontSize = 30f;

            nameInput.text = goalToEdit.Title;
            targetInput.text = FormatMoney(goalToEdit.TargetAmount);
            initialInput.text = FormatMoney(goalToEdit.CurrentAmount);

            nameInput.interactable = true;
            targetInput.interactable = true;

           
            initialInput.interactable = false;
            SetCanvasGroup(initialInput, 0.5f, false);

            counterText.text = $"{nameInput.text.Length}/20";
        }

        UpdateSaveButton();
        Show();
    }

    private void SetCanvasGroup(TMP_InputField field, float alpha, bool interactable)
    {
        var cg = field.GetComponent<CanvasGroup>();
        if (cg == null) cg = field.gameObject.AddComponent<CanvasGroup>();

        cg.alpha = alpha;
        cg.interactable = interactable;
        cg.blocksRaycasts = interactable;
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

  
    // НАЗВАНИЕ
  
    private void OnNameChanged()
    {
        if (nameInput.text.Length > 20)
            nameInput.text = nameInput.text.Substring(0, 20);

        UpdateCounter();
        UpdateSaveButton();
    }

    private void UpdateCounter()
    {
        counterText.text = $"{nameInput.text.Length}/20";
    }
    // ВВОД ЦЕЛЕВОЙ СУММЫ

    private void OnTargetTyping()
    {
        string clean = CleanAmount(targetInput.text);

        if (clean != targetInput.text)
        {
            int pos = targetInput.caretPosition;
            targetInput.text = clean;
            targetInput.caretPosition = Mathf.Clamp(pos - 1, 0, clean.Length);
        }

        UpdateSaveButton();
    }

  
    private void OnTargetEndEdit()
    {
        float.TryParse(CleanAmount(targetInput.text), NumberStyles.Any, CultureInfo.InvariantCulture, out float value);

        targetInput.text = FormatMoney(value);

    
        LimitInitialToTarget();

        UpdateSaveButton();
    }

   
    // ВВОД НАЧАЛЬНОЙ СУММЫ 
    private void OnInitialTyping()
    {
        string clean = CleanAmount(initialInput.text);

        if (clean != initialInput.text)
        {
            int pos = initialInput.caretPosition;
            initialInput.text = clean;
            initialInput.caretPosition = Mathf.Clamp(pos - 1, 0, clean.Length);
        }
    }

   
    private void OnInitialEndEdit()
    {
        float.TryParse(CleanAmount(initialInput.text), NumberStyles.Any, CultureInfo.InvariantCulture, out float initial);
        float.TryParse(CleanAmount(targetInput.text), NumberStyles.Any, CultureInfo.InvariantCulture, out float target);

        if (initial > target)
            initial = target;

        initialInput.text = FormatMoney(initial);
    }

   
    // ОЧИСТКА СТРОКИ
  
    private string CleanAmount(string raw)
    {
        if (raw == null)
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

    private void LimitInitialToTarget()
    {
        float.TryParse(CleanAmount(targetInput.text), NumberStyles.Any, CultureInfo.InvariantCulture, out float target);
        float.TryParse(CleanAmount(initialInput.text), NumberStyles.Any, CultureInfo.InvariantCulture, out float initial);

        if (initial > target)
        {
            initial = target;
            initialInput.text = FormatMoney(initial);
        }
    }

   
    // ВАЛИДАЦИЯ
   
    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(nameInput.text))
            return false;

        float.TryParse(CleanAmount(targetInput.text), NumberStyles.Any, CultureInfo.InvariantCulture, out float target);
        if (target <= 0)
            return false;

        float.TryParse(CleanAmount(initialInput.text), NumberStyles.Any, CultureInfo.InvariantCulture, out float initial);
        if (initial < 0 || initial > target)
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

        float.TryParse(CleanAmount(targetInput.text), NumberStyles.Any, CultureInfo.InvariantCulture, out float target);
        float.TryParse(CleanAmount(initialInput.text), NumberStyles.Any, CultureInfo.InvariantCulture, out float initial);


        onSave?.Invoke(nameInput.text, target, initial);

        flowers.SetActive(true);
        Hide();
    }
}
