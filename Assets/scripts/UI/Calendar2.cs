using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Globalization; // ★ нужно для русского месяца

public class Calendar2 : MonoBehaviour
{
    [Header("UI References")]
    public Transform grid;
    public GameObject dayCellPrefab;
    public TMP_Text monthLabel;

    public GameObject monthPickerPanel;

    public RectTransform monthSwipeArea;
    public RectTransform yearSwipeArea;

    public TMP_Text prevYearText;
    public TMP_Text currentYearText;
    public TMP_Text nextYearText;

    public TMP_Text prevItem;
    public TMP_Text currentItem;
    public TMP_Text nextItem;

    private DateTime currentDate;
    private DateTime today;

    private int selectedMonth;
    private int selectedYear;

    private Vector2 swipeStartPos;
    private float swipeStartTime;
    private float swipeEndTime;
    private bool isSwiping = false;

    public ScrollRect mainScroll;

    public CalendarPage calendarPage;


    // Выбор даты
    private DateTime? selectedDay = null;
    private DayCell lastSelectedCell = null;


    void Start()
    {
        today = DateTime.Now.Date;
        currentDate = today;

        GenerateCalendar(currentDate.Year, currentDate.Month);
        monthPickerPanel.SetActive(false);
    }

    // ------------------------------
    // Основной календарь
    // ------------------------------

    public void GenerateCalendar(int year, int month)
    {
        // ★ Новый формат заголовка: Месяц + перенос строки + Год
        string monthName = new DateTime(year, month, 1)
            .ToString("MMMM", CultureInfo.GetCultureInfo("ru-RU"));

        monthName = char.ToUpper(monthName[0]) + monthName.Substring(1);

        monthLabel.text = monthName + "\n" + year;

        // Удаляем старые ячейки (кроме заголовков дней недели)
        for (int i = grid.childCount - 1; i >= 7; i--)
            Destroy(grid.GetChild(i).gameObject);

        DateTime firstDay = new DateTime(year, month, 1);
        int daysInMonth = DateTime.DaysInMonth(year, month);

        int startIndex = ((int)firstDay.DayOfWeek + 6) % 7;

        for (int i = 0; i < startIndex; i++)
        {
            var empty = Instantiate(dayCellPrefab, grid);
            empty.GetComponentInChildren<TMP_Text>().text = "";
            empty.GetComponent<Button>().interactable = false;
        }

        for (int day = 1; day <= daysInMonth; day++)
        {
            var cell = Instantiate(dayCellPrefab, grid);
            DayCell dc = cell.GetComponent<DayCell>();

            dc.date = new DateTime(year, month, day);
            dc.text.text = day.ToString();
            dc.background.alpha = 0f;

            Button btn = cell.GetComponent<Button>();
            CanvasGroup cg = cell.GetComponent<CanvasGroup>();
            if (cg == null) cg = cell.AddComponent<CanvasGroup>();

            // Базовый цвет
            dc.text.color = new Color32(0x5B, 0x5A, 0x5A, 0xFF);

            // Сегодня — зелёный
            if (dc.date == today)
                dc.text.color = new Color32(0x34, 0xC7, 0x59, 0xFF);

            // Будущие даты — недоступны
            if (dc.date > today)
            {
                btn.interactable = false;
                cg.alpha = 0.5f;
            }
            else
            {
                btn.interactable = true;
                cg.alpha = 1f;

                btn.onClick.AddListener(() =>
                {
                    OnDateSelected(dc);
                });
            }
        }

        selectedDay = null;
        lastSelectedCell = null;
    }

    // ------------------------------
    // Выбор даты
    // ------------------------------

    private void OnDateSelected(DayCell dc)
    {
        if (selectedDay == dc.date)
        {
            selectedDay = null;
            dc.background.alpha = 0f;

            if (dc.date != today)
                dc.text.color = new Color32(0x5B, 0x5A, 0x5A, 0xFF);

            lastSelectedCell = null;
            return;
        }

        if (lastSelectedCell != null)
        {
            lastSelectedCell.background.alpha = 0f;

            if (lastSelectedCell.date != today)
                lastSelectedCell.text.color = new Color32(0x5B, 0x5A, 0x5A, 0xFF);
        }

        selectedDay = dc.date;
        dc.background.alpha = 1f;
        dc.text.color = new Color32(0x34, 0xC7, 0x59, 0xFF);

        lastSelectedCell = dc;

        // ⭐ ВЫЗЫВАЕМ CalendarPage
        calendarPage.ShowTransactionsForDate(dc.date);
    }


    public DateTime? GetSelectedDate()
    {
        return selectedDay;
    }

    // ------------------------------
    // Month Picker
    // ------------------------------

    public void OpenMonthPicker()
    {
        selectedMonth = currentDate.Month;
        selectedYear = currentDate.Year;

        UpdateDateCarousel();
        UpdateYearCarousel();

        mainScroll.enabled = false;
        monthPickerPanel.SetActive(true);
    }

    private void UpdateDateCarousel()
    {
        if (selectedYear > today.Year)
            selectedYear = today.Year;

        if (selectedYear == today.Year && selectedMonth > today.Month)
            selectedMonth = today.Month;

        DateTime curr = new DateTime(selectedYear, selectedMonth, 1);
        DateTime prev = curr.AddMonths(-1);
        DateTime next = curr.AddMonths(1);

        prevItem.text = prev.ToString("MMM", CultureInfo.GetCultureInfo("ru-RU"));
        currentItem.text = curr.ToString("MMM", CultureInfo.GetCultureInfo("ru-RU"));

        if (selectedYear == today.Year && selectedMonth == today.Month)
            nextItem.text = "";
        else
            nextItem.text = next.ToString("MMM", CultureInfo.GetCultureInfo("ru-RU"));
    }

    public void DateCarouselNext()
    {
        if (selectedYear == today.Year && selectedMonth >= today.Month)
            return;

        selectedMonth++;
        if (selectedMonth > 12)
        {
            selectedMonth = 1;
            selectedYear++;
        }

        currentDate = new DateTime(selectedYear, selectedMonth, 1);

        UpdateDateCarousel();
        UpdateYearCarousel();
    }

    public void DateCarouselPrev()
    {
        selectedMonth--;
        if (selectedMonth < 1)
        {
            selectedMonth = 12;
            selectedYear--;
        }

        currentDate = new DateTime(selectedYear, selectedMonth, 1);

        UpdateDateCarousel();
        UpdateYearCarousel();
    }

    public void ApplySelection()
    {
        DateTime chosen = new DateTime(selectedYear, selectedMonth, 1);

        if (chosen > today)
            return;

        currentDate = chosen;
        GenerateCalendar(selectedYear, selectedMonth);

        mainScroll.enabled = true;
        monthPickerPanel.SetActive(false);
    }

    public void CancelSelection()
    {
        mainScroll.enabled = true;
        monthPickerPanel.SetActive(false);
    }

    // ------------------------------
    // Year Picker
    // ------------------------------

    public void UpdateYearCarousel()
    {
        if (selectedYear > today.Year)
            selectedYear = today.Year;

        prevYearText.text = (selectedYear - 1).ToString();
        currentYearText.text = selectedYear.ToString();

        if (selectedYear < today.Year)
            nextYearText.text = (selectedYear + 1).ToString();
        else
            nextYearText.text = "";
    }

    public void ScrollDown()
    {
        if (selectedYear >= today.Year)
            return;

        selectedYear++;
        currentDate = new DateTime(selectedYear, selectedMonth, 1);

        UpdateYearCarousel();
        UpdateDateCarousel();
    }

    public void ScrollUp()
    {
        selectedYear--;
        currentDate = new DateTime(selectedYear, selectedMonth, 1);

        UpdateYearCarousel();
        UpdateDateCarousel();
    }

    // ------------------------------
    // Свайпы
    // ------------------------------

    private void HandleSwipe(Vector2 swipeDelta, Vector2 startPos, float swipeTime)
    {
        if (Mathf.Abs(swipeDelta.y) < 120f)
            return;

        bool inMonth = RectTransformUtility.RectangleContainsScreenPoint(monthSwipeArea, startPos);
        bool inYear = RectTransformUtility.RectangleContainsScreenPoint(yearSwipeArea, startPos);

        if (inMonth)
            inYear = false;

        if (!inMonth && !inYear)
            return;

        mainScroll.enabled = false;

        float speed = swipeDelta.magnitude / Mathf.Max(swipeTime, 0.05f);
        int steps = Mathf.Clamp(Mathf.FloorToInt(speed / 1200f), 1, 3);

        bool forward = swipeDelta.y > 0;

        if (inMonth)
        {
            for (int i = 0; i < steps; i++)
            {
                if (forward)
                    DateCarouselNext();
                else
                    DateCarouselPrev();
            }
        }
        else if (inYear)
        {
            for (int i = 0; i < steps; i++)
            {
                if (forward)
                    ScrollDown();
                else
                    ScrollUp();
            }
        }
    }

    void Update()
    {
        if (!monthPickerPanel.activeSelf)
            return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                swipeStartPos = touch.position;
                swipeStartTime = Time.time;
                isSwiping = true;
            }
            else if (touch.phase == TouchPhase.Ended && isSwiping)
            {
                swipeEndTime = Time.time;
                Vector2 swipeEndPos = touch.position;

                HandleSwipe(swipeEndPos - swipeStartPos, swipeStartPos, swipeEndTime - swipeStartTime);

                isSwiping = false;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            swipeStartPos = Input.mousePosition;
            swipeStartTime = Time.time;
            isSwiping = true;
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            swipeEndTime = Time.time;
            Vector2 swipeEndPos = Input.mousePosition;

            HandleSwipe(swipeEndPos - swipeStartPos, swipeStartPos, swipeEndTime - swipeStartTime);

            isSwiping = false;
        }
    }
}
