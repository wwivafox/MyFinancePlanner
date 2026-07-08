using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GreenhousePage : MonoBehaviour
{
    [Header("Слоты")]
    public GoalSlotView[] slots; 

    [Header("Кнопки и страницы")]
    public Button addButton;
    public Button prevPageButton;
    public Button nextPageButton;
    public TMP_Text pageLabel;

    [Header("Панели")]
    public GameObject flowers;
    public NewGoalPanel newGoalPanel;
    public AddAmountPanel addAmountPanel;
    public GameObject confirmDeletePanel;

    private Repository repo;
    private List<Goal> allGoals = new List<Goal>();
    private List<FlowerType> flowerTypes = new List<FlowerType>();

    private int currentPage = 1;
    private int totalPages = 1;

    private System.Random rng = new System.Random();

    private void Start()
    {
        repo = new Repository();
        flowerTypes = repo.GetFlowerTypes();

        addButton.onClick.AddListener(OnAddGoalClicked);
        prevPageButton.onClick.AddListener(OnPrevPage);
        nextPageButton.onClick.AddListener(OnNextPage);
        confirmDeletePanel.SetActive(false);

        LoadGoals();
        UpdatePagination();
        RefreshPage();
    }

    private void LoadGoals()
    {
        allGoals = repo.GetGoals();

        if (allGoals.Count == 0)
        {
            currentPage = 1;
            totalPages = 1;
            return;
        }

        int maxPage = allGoals.Max(g => g.PageIndex);
        if (maxPage <= 0) maxPage = 1;

        totalPages = maxPage;
        currentPage = Mathf.Clamp(currentPage, 1, totalPages);
    }

    private void UpdatePagination()
    {
        bool multi = totalPages > 1;

        SetButtonState(prevPageButton, multi && currentPage > 1);
        SetButtonState(nextPageButton, multi && currentPage < totalPages);

        if (pageLabel != null)
        {
            pageLabel.alpha = multi ? 1f : 0f;
            pageLabel.text = multi ? $"{currentPage}/{totalPages}" : "";
        }
    }

    private void SetButtonState(Button btn, bool enabled)
    {
        var cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();

        cg.alpha = enabled ? 1f : 0f;
        cg.interactable = enabled;
        cg.blocksRaycasts = enabled;
    }

    private void RefreshPage()
    {
        for (int i = 0; i < slots.Length; i++)
            ShowSlotEmpty(slots[i]);

        var pageGoals = allGoals
            .Where(g => g.PageIndex == currentPage)
            .OrderBy(g => g.SlotIndex)
            .ToList();

        foreach (var goal in pageGoals)
        {
            if (goal.SlotIndex >= 0 && goal.SlotIndex < 9)
                ShowSlotGoal(slots[goal.SlotIndex], goal);
        }
    }

    private void ShowSlotEmpty(GoalSlotView slot)
    {
        slot.flowerGroup.alpha = 0f;
        slot.flowerGroup.interactable = false;
        slot.flowerGroup.blocksRaycasts = false;

        slot.boardGroup.alpha = 0f;
        slot.boardGroup.interactable = false;
        slot.boardGroup.blocksRaycasts = false;

        slot.boardLabel.text = "";
        slot.flowerImage.sprite = null;

        slot.clickAreaButton.onClick.RemoveAllListeners();
    }

    private void ShowSlotGoal(GoalSlotView slot, Goal goal)
    {
        slot.boardGroup.alpha = 1f;
        slot.boardGroup.interactable = true;
        slot.boardGroup.blocksRaycasts = true;

        slot.boardLabel.text =
            $"{goal.Title}\n{goal.CurrentAmount:0.00}/{goal.TargetAmount:0.00}";

        slot.flowerGroup.alpha = 1f;
        slot.flowerGroup.interactable = true;
        slot.flowerGroup.blocksRaycasts = true;

        var flowerType = flowerTypes.FirstOrDefault(f => f.Id == goal.FlowerTypeId);
        if (flowerType != null)
        {
            string spriteName = GetFlowerStageSpriteName(flowerType, goal);
            slot.flowerImage.sprite =
                Resources.Load<Sprite>("Sprites/Flowers/" + spriteName);
        }

        slot.clickAreaButton.onClick.RemoveAllListeners();

        bool goalCompleted = goal.CurrentAmount >= goal.TargetAmount;

        if (goalCompleted)
        {
        
            slot.clickAreaButton.interactable = false;

            var cg = slot.clickAreaButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = slot.clickAreaButton.gameObject.AddComponent<CanvasGroup>();

           
            cg.blocksRaycasts = false; 
        }
        else
        {
           
            slot.clickAreaButton.interactable = true;

            var cg = slot.clickAreaButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = slot.clickAreaButton.gameObject.AddComponent<CanvasGroup>();

          
            cg.blocksRaycasts = true;

            slot.clickAreaButton.onClick.AddListener(() =>
            {
                OpenAddAmountPanel(goal);
            });
        }

    }

    private string GetFlowerStageSpriteName(FlowerType flowerType, Goal goal)
    {
        float part = goal.TargetAmount / 5f;
        float v = Mathf.Clamp(goal.CurrentAmount, 0f, goal.TargetAmount);

        if (v >= goal.TargetAmount) return flowerType.Stage6;
        if (v >= part * 4) return flowerType.Stage5;
        if (v >= part * 3) return flowerType.Stage4;
        if (v >= part * 2) return flowerType.Stage3;
        if (v >= part * 1) return flowerType.Stage2;
        return flowerType.Stage1;
    }

    private void OnPrevPage()
    {
        if (currentPage <= 1) return;
        currentPage--;
        UpdatePagination();
        RefreshPage();
    }

    private void OnNextPage()
    {
        if (currentPage >= totalPages) return;
        currentPage++;
        UpdatePagination();
        RefreshPage();
    }

    public void OnAddGoalClicked()
    {
        flowers.SetActive(false);
        Debug.Log("ADD BUTTON CLICKED");

        int slotIndex = FindFreeSlot(out int pageIndex);

        newGoalPanel.Open((title, target, initial) =>
        {
            Debug.Log($"SAVE NEW GOAL: {title}, {target}, {initial}");

            var flowerType = PickRandomFlower(pageIndex);

            var goal = new Goal
            {
                Title = title,
                TargetAmount = target,
                CurrentAmount = initial,
                FlowerTypeId = flowerType.Id,
                PageIndex = pageIndex,
                SlotIndex = slotIndex,
                CreatedDate = DateTime.Now.ToString("yyyy-MM-dd")
            };

            repo.AddGoal(goal);
            LoadGoals();
            currentPage = pageIndex;
            UpdatePagination();
            RefreshPage();
        });
    }


    private int FindFreeSlot(out int pageIndex)
    {
        if (allGoals.Count == 0)
        {
            pageIndex = 1;
            totalPages = 1;
            return 0;
        }

        int maxPage = allGoals.Max(g => g.PageIndex);

        for (int p = 1; p <= maxPage; p++)
        {
            var pageGoals = allGoals.Where(g => g.PageIndex == p).ToList();
            for (int s = 0; s < 9; s++)
                if (!pageGoals.Any(g => g.SlotIndex == s))
                {
                    pageIndex = p;
                    return s;
                }
        }

        pageIndex = maxPage + 1;
        totalPages = pageIndex;
        return 0;
    }

    private FlowerType PickRandomFlower(int pageIndex)
    {
        var used = allGoals
            .Where(g => g.PageIndex == pageIndex)
            .Select(g => g.FlowerTypeId)
            .Distinct()
            .ToList();

        var candidates = flowerTypes.Where(f => !used.Contains(f.Id)).ToList();
        if (candidates.Count == 0) candidates = flowerTypes;

        return candidates[rng.Next(candidates.Count)];
    }

    private void OpenAddAmountPanel(Goal goal)
    {
        flowers.SetActive(false);
        var flowerType = flowerTypes.FirstOrDefault(f => f.Id == goal.FlowerTypeId);
        string spriteName = GetFlowerStageSpriteName(flowerType, goal);
        Sprite sprite = Resources.Load<Sprite>("Sprites/Flowers/" + spriteName);

        addAmountPanel.Open(goal, sprite, updated =>
        {
            repo.UpdateGoal(updated);
            LoadGoals();
            RefreshPage();
        });
    }
}
