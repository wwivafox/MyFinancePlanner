using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatisticsPeriodSelector : MonoBehaviour
{
    [Header("Корневая панель селектора")]
    public GameObject panelRoot;

    [Header("Колонка типа статистики")]
    public TMP_Text typePrev;
    public TMP_Text typeCurrent;
    public TMP_Text typeNext;

    [Header("Колонка периода (месяц/год)")]
    public TMP_Text periodPrev;
    public TMP_Text periodCurrent;
    public TMP_Text periodNext;

    [Header("Области свайпа")]
    public RectTransform typeSwipeArea;
    public RectTransform periodSwipeArea;

    [Header("Кнопки")]
    public Button okButton;
    public Button cancelButton;

    public Action<StatisticsType, int, int> OnPeriodSelected;

    private StatisticsType currentType = StatisticsType.All;

    private int currentYear;
    private int currentMonth;
    private int minYear;
    private int maxYear;

    private DateTime now;

    
    private Vector2 swipeStartPos;
    private float swipeStartTime;
    private bool isSwiping = false;

    private void Awake()
    {
        if (okButton != null)
            okButton.onClick.AddListener(OnOk);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(Close);

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Open(StatisticsType initialType, int initialYear, int initialMonth, int minYear, int maxYear)
    {
        now = DateTime.Now.Date;

        this.minYear = minYear;
        this.maxYear = maxYear;

        currentType = initialType;

        currentYear = initialYear > 0 ? Mathf.Clamp(initialYear, minYear, maxYear) : now.Year;
        currentMonth = initialMonth > 0 ? Mathf.Clamp(initialMonth, 1, 12) : now.Month;

        panelRoot.SetActive(true);

        UpdateTypeTexts();
        UpdatePeriodTexts();
    }

    public void Close()
    {
        panelRoot.SetActive(false);
    }

    
    // Прокрутка типа статистики
    public void ScrollType(int direction)
    {
        int t = (int)currentType;
        int count = 3;

        t = (t + direction) % count;
        if (t < 0) t += count;

        currentType = (StatisticsType)t;

        if (currentType == StatisticsType.Month)
        {
            currentYear = now.Year;
            currentMonth = Mathf.Clamp(currentMonth, 1, now.Month);
        }
        else if (currentType == StatisticsType.Year)
        {
            currentYear = Mathf.Clamp(currentYear, minYear, maxYear);
        }

        UpdateTypeTexts();
        UpdatePeriodTexts();
    }

    
    // Прокрутка периода
    public void ScrollPeriod(int direction)
    {
        if (currentType == StatisticsType.All)
            return;

        if (currentType == StatisticsType.Month)
        {
            int minMonth = 1;
            int maxMonth = now.Month;

            currentMonth += direction;

            if (currentMonth > maxMonth) currentMonth = minMonth;
            if (currentMonth < minMonth) currentMonth = maxMonth;
        }
        else if (currentType == StatisticsType.Year)
        {
            currentYear += direction;

            if (currentYear > maxYear) currentYear = minYear;
            if (currentYear < minYear) currentYear = maxYear;
        }

        UpdatePeriodTexts();
    }

    private void OnOk()
    {
        int year = 0;
        int month = 0;

        if (currentType == StatisticsType.Month)
        {
            year = currentYear;
            month = currentMonth;
        }
        else if (currentType == StatisticsType.Year)
        {
            year = currentYear;
            month = 0;
        }

        OnPeriodSelected?.Invoke(currentType, year, month);
        Close();
    }

    private void UpdateTypeTexts()
    {
        string[] names = { "Вся статистика", "За месяц", "За год" };

        int idx = (int)currentType;
        int count = names.Length;

        int prev = (idx - 1 + count) % count;
        int next = (idx + 1) % count;

        typePrev.text = names[prev];
        typeCurrent.text = names[idx];
        typeNext.text = names[next];
    }

    private void UpdatePeriodTexts()
    {
        if (currentType == StatisticsType.All)
        {
            periodPrev.text = "";
            periodCurrent.text = "";
            periodNext.text = "";
            return;
        }

        if (currentType == StatisticsType.Month)
        {
            string[] months =
            {
                "Январь","Февраль","Март","Апрель","Май","Июнь",
                "Июль","Август","Сентябрь","Октябрь","Ноябрь","Декабрь"
            };

            int maxMonth = now.Month;
            int minMonth = 1;

            int cur = Mathf.Clamp(currentMonth, minMonth, maxMonth);

            int prev = cur - 1 < minMonth ? maxMonth : cur - 1;
            int next = cur + 1 > maxMonth ? minMonth : cur + 1;

            periodPrev.text = months[prev - 1];
            periodCurrent.text = months[cur - 1];
            periodNext.text = months[next - 1];
        }
        else if (currentType == StatisticsType.Year)
        {
            int cur = Mathf.Clamp(currentYear, minYear, maxYear);

            int prev = cur - 1 < minYear ? maxYear : cur - 1;
            int next = cur + 1 > maxYear ? minYear : cur + 1;

            periodPrev.text = prev.ToString();
            periodCurrent.text = cur.ToString();
            periodNext.text = next.ToString();
        }
    }

  
    // Свайпы
    private void HandleSwipe(Vector2 swipeDelta, Vector2 startPos, float swipeTime)
    {
        if (Mathf.Abs(swipeDelta.y) < 120f)
            return;

        bool inType = RectTransformUtility.RectangleContainsScreenPoint(typeSwipeArea, startPos);
        bool inPeriod = RectTransformUtility.RectangleContainsScreenPoint(periodSwipeArea, startPos);

        if (inType) inPeriod = false;
        if (!inType && !inPeriod) return;

        float speed = swipeDelta.magnitude / Mathf.Max(swipeTime, 0.05f);
        int steps = Mathf.Clamp(Mathf.FloorToInt(speed / 1200f), 1, 3);

        bool forward = swipeDelta.y > 0;

        for (int i = 0; i < steps; i++)
        {
            if (inType)
                ScrollType(forward ? +1 : -1);
            else
                ScrollPeriod(forward ? +1 : -1);
        }
    }

    private void Update()
    {
        if (!panelRoot.activeSelf)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            swipeStartPos = Input.mousePosition;
            swipeStartTime = Time.time;
            isSwiping = true;
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            Vector2 swipeEndPos = Input.mousePosition;
            float swipeTime = Time.time - swipeStartTime;

            HandleSwipe(swipeEndPos - swipeStartPos, swipeStartPos, swipeTime);

            isSwiping = false;
        }

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                swipeStartPos = t.position;
                swipeStartTime = Time.time;
                isSwiping = true;
            }
            else if (t.phase == TouchPhase.Ended && isSwiping)
            {
                float swipeTime = Time.time - swipeStartTime;
                HandleSwipe(t.position - swipeStartPos, swipeStartPos, swipeTime);
                isSwiping = false;
            }
        }
    }
}
