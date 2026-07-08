using System;
using System.Collections.Generic;
using UnityEngine;

public class CalendarPage : MonoBehaviour
{
    [Header("UI")]
    public Transform transactionsContainer;
    public GameObject transactionPrefab;

    [Header("Empty State")]
    public GameObject emptyPrefab;

    private Repository repo;

    void Awake()
    {
        repo = new Repository();
    }

    public void ClearTransactions()
    {
        foreach (Transform child in transactionsContainer)
            Destroy(child.gameObject);
    }

    public void ShowTransactionsForDate(DateTime? date)
    {
        ClearTransactions();

        if (date == null)
            return;

        string dateString = date.Value.ToString("yyyy-MM-dd");
        List<Transaction> list = repo.GetTransactionsByDate(dateString);

        if (list == null || list.Count == 0)
        {
            Instantiate(emptyPrefab, transactionsContainer);
            return;
        }

        foreach (var t in list)
        {
            GameObject item = Instantiate(transactionPrefab, transactionsContainer);
            TransactionItem ti = item.GetComponent<TransactionItem>();
            ti.Setup(t, this);
        }
    }

    public void SetButtonsInteractable(bool value)
    {
        foreach (Transform child in transactionsContainer)
        {
            var ti = child.GetComponent<TransactionItem>();
            if (ti != null)
                ti.SetButtonsOnlyInteractable(value);
        }
    }
}
