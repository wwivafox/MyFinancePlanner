using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CategoryItem : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public TMP_InputField nameField;

    public Button editButton;
    public Button deleteButton;

    public Sprite editSprite;
    public Sprite saveSprite;

    [Header("Icon Selector")]
    public CategoryIconSelector iconSelector;

    private Category category;
    private Button iconButton;

    private void Awake()
    {
        iconButton = icon.GetComponent<Button>();

        var cg = GetComponent<CanvasGroup>();
        if (cg != null)
            Debug.Log("[CategoryItem] Awake: CanvasGroup.ALPHA = " + cg.alpha + " на объекте " + name);
        else
            Debug.Log("[CategoryItem] Awake: CanvasGroup НЕТ на объекте " + name);
    }

    public void SetAlpha(float value)
{
    var cg = GetComponent<CanvasGroup>();
    if (cg == null)
    {
        Debug.Log("[CategoryItem] SetAlpha(" + value + ") НО CanvasGroup НЕТ на " + name);
        return;
    }

    Debug.Log("[CategoryItem] SetAlpha(" + value + ") на " + name + " (БЫЛО " + cg.alpha + ")");
    cg.alpha = value;
}

    public void SetInteractable(bool value)
    {
        nameField.interactable = value;
        editButton.interactable = value;
        deleteButton.interactable = value;
        iconButton.interactable = value;
    }

    public void Setup(Category cat)
    {
        category = cat;

        nameField.text = cat.Name;
        nameField.interactable = false;

        icon.sprite = CategoryIconLoader.GetIcon(cat.IconName);
        iconButton.interactable = false;

        editButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        iconButton.onClick.RemoveAllListeners();

        editButton.image.sprite = editSprite;
        editButton.onClick.AddListener(OnEdit);
        deleteButton.onClick.AddListener(OnDelete);
    }

    public void SetupNew(Category cat)
    {
        category = cat;

        nameField.text = "";
        nameField.interactable = true;

        icon.sprite = CategoryIconLoader.GetDefaultIcon();
        iconButton.interactable = true;

        editButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        iconButton.onClick.RemoveAllListeners();

        editButton.image.sprite = saveSprite;
        editButton.onClick.AddListener(OnSaveNew);

        deleteButton.onClick.AddListener(OnCancelNew);

        iconButton.onClick.AddListener(OpenIconSelector);
    }

    private void OnEdit()
    {
        CategoryCreator.Instance.SetEditingMode(this);

        nameField.interactable = true;
        iconButton.interactable = true;

        editButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        iconButton.onClick.RemoveAllListeners();

        editButton.image.sprite = saveSprite;
        editButton.onClick.AddListener(OnSaveEdit);

        deleteButton.onClick.AddListener(OnCancelEdit);

        iconButton.onClick.AddListener(OpenIconSelector);
    }

    private void OnSaveEdit()
    {
        category.Name = nameField.text;
        DatabaseManager.Instance.DB.GetConnection().Update(category);

        nameField.interactable = false;
        iconButton.interactable = false;

        editButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        iconButton.onClick.RemoveAllListeners();

        editButton.image.sprite = editSprite;
        editButton.onClick.AddListener(OnEdit);
        deleteButton.onClick.AddListener(OnDelete);

        CategoryCreator.Instance.ResetEditingMode();
    }

    private void OnDelete()
    {
        DatabaseManager.Instance.DB.GetConnection().Delete(category);
        Destroy(gameObject);
    }

    private void OnSaveNew()
    {
        if (string.IsNullOrWhiteSpace(nameField.text))
            return;

        category.Name = nameField.text;
        DatabaseManager.Instance.DB.GetConnection().Insert(category);

        Setup(category);
    }

    private void OnCancelNew()
    {
        Destroy(gameObject);
    }

    private void OnCancelEdit()
    {
        nameField.text = category.Name;

        nameField.interactable = false;
        iconButton.interactable = false;

        editButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        iconButton.onClick.RemoveAllListeners();

        editButton.image.sprite = editSprite;
        editButton.onClick.AddListener(OnEdit);
        deleteButton.onClick.AddListener(OnDelete);

        CategoryCreator.Instance.ResetEditingMode();
    }

    void OpenIconSelector()
    {
        iconSelector.Open(category.IconName, (newIcon) =>
        {
            category.IconName = newIcon;
            icon.sprite = CategoryIconLoader.GetIcon(newIcon);
        });
    }
}
