using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TransactionItem : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text nameText;
    public TMP_Text amountText;
    public TMP_Text categoryText;
    public TMP_Text descriptionText;
    public Image categoryIcon;

    [Header("Buttons")]
    public Button editButton;
    public Button deleteButton;

    [Header("Confirm Panel")]
    public GameObject confirmPanel;
    public Button confirmYes;
    public Button confirmNo;

    private Transaction transaction;
    private CalendarPage calendar;
    private Repository repo;

    void Awake()
    {
        repo = new Repository();
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    public void Setup(Transaction t, CalendarPage page)
    {
        transaction = t;
        calendar = page;

        Account acc = repo.GetAccountById(t.AccountId);
        Category cat = repo.GetCategoryById(t.CategoryId);

        nameText.text = acc.Name;

        string sign = cat.IsIncome ? "+" : "-";
        amountText.text = $"{sign}{t.Amount:0.00} {acc.Currency}";

        categoryText.text = cat.Name;
        descriptionText.text = t.Description;

        categoryIcon.sprite = Resources.Load<Sprite>("Sprites/Category/" + cat.IconName);

        deleteButton.onClick.RemoveAllListeners();
        confirmYes.onClick.RemoveAllListeners();
        confirmNo.onClick.RemoveAllListeners();
        editButton.onClick.RemoveAllListeners();

        deleteButton.onClick.AddListener(OpenConfirm);
        confirmYes.onClick.AddListener(DeleteConfirmed);
        confirmNo.onClick.AddListener(CloseConfirm);
        editButton.onClick.AddListener(EditTransaction);
    }

    public void SetButtonsOnlyInteractable(bool value)
    {
        editButton.interactable = value;
        deleteButton.interactable = value;

        CanvasGroup cgEdit = editButton.GetComponent<CanvasGroup>();
        if (cgEdit == null) cgEdit = editButton.gameObject.AddComponent<CanvasGroup>();
        cgEdit.alpha = value ? 1f : 0.5f;

        CanvasGroup cgDelete = deleteButton.GetComponent<CanvasGroup>();
        if (cgDelete == null) cgDelete = deleteButton.gameObject.AddComponent<CanvasGroup>();
        cgDelete.alpha = value ? 1f : 0.5f;
    }

    private void OpenConfirm()
    {
        confirmPanel.SetActive(true);
        calendar.SetButtonsInteractable(false);
    }

    private void CloseConfirm()
    {
        confirmPanel.SetActive(false);
        calendar.SetButtonsInteractable(true);
    }

    private void DeleteConfirmed()
    {
        repo.DeleteTransaction(transaction.Id);
        Destroy(gameObject);
        calendar.SetButtonsInteractable(true);
    }

    private void EditTransaction()
    {
        EditTransactionData.EditingTransaction = transaction;

        SelectedDateMemory.MonthToOpen = DateTime.ParseExact(transaction.Date, "yyyy-MM-dd", null);

        UnityEngine.SceneManagement.SceneManager.LoadScene("Transaction");
    }

}
