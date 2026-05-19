using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Globalization;

public class CalendarPage : MonoBehaviour
{
    [Header("UI")]
    public Transform transactionsContainer;
    public GameObject transactionPrefab;

    [Header("Empty State")]               // ★ добавляем
    public GameObject emptyPrefab;        // ★ префаб "пусто"

    private Repository repo;

    void Awake()
    {
        repo = new Repository();
    }

    public void ShowTransactionsForDate(DateTime date)
    {
        // очищаем старые элементы
        foreach (Transform child in transactionsContainer)
            Destroy(child.gameObject);

        string dateString = date.ToString("yyyy-MM-dd");

        // получаем транзакции за день
        List<Transaction> list = repo.GetTransactionsByDate(dateString);

        // ★ если нет транзакций — показываем пустой префаб
        if (list == null || list.Count == 0)
        {
            Instantiate(emptyPrefab, transactionsContainer);
            return;
        }

        // иначе — выводим транзакции
        foreach (var t in list)
        {
            GameObject item = Instantiate(transactionPrefab, transactionsContainer);

            TMP_Text nameText = item.transform.Find("Название счёта").GetComponentInChildren<TMP_Text>();
            TMP_Text amountText = item.transform.Find("Сумма").GetComponentInChildren<TMP_Text>();
            TMP_Text categoryText = item.transform.Find("категория/категория").GetComponent<TMP_Text>();
            TMP_Text descriptionText = item.transform.Find("Описание").GetComponent<TMP_Text>();
            Image categoryIcon = item.transform.Find("категория/Изображение категории").GetComponent<Image>();

            Account acc = repo.GetAccountById(t.AccountId);
            Category cat = repo.GetCategoryById(t.CategoryId);

            nameText.text = acc.Name;

            // ⭐ ДОБАВЛЯЕМ ЗНАК
            string sign = cat.IsIncome ? "+" : "-";
            amountText.text = $"{sign}{t.Amount:0.00} {acc.Currency}";

            categoryText.text = cat.Name;
            descriptionText.text = t.Description;

            categoryIcon.sprite = Resources.Load<Sprite>("Sprites/Category/" + cat.IconName);
        }

    }
}
