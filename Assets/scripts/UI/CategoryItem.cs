using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CategoryItem : MonoBehaviour
{
    public Image icon;
    public TMP_InputField nameField;
    public Button editButton;
    public Button deleteButton;
    public Button iconButton;

    public Sprite editSprite;
    public Sprite saveSprite;

    [HideInInspector] public CategoryIconSelector iconSelector;

    private Category category;
    private bool isNew = false;

    public void Setup(Category cat)
    {
        category = cat;
        isNew = false;

        nameField.text = cat.Name;
        nameField.interactable = false;

        icon.sprite = CategoryIconLoader.GetIcon(cat.IconName);
        iconButton.interactable = false;

        editButton.image.sprite = editSprite;
        editButton.onClick.RemoveAllListeners();
        editButton.onClick.AddListener(OnEdit);

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(OnDelete);

        CategoryCreator.Instance.ResetEditingMode();
    }

    public void SetupNew(Category cat)
    {
        category = cat;
        isNew = true;

        nameField.text = "";
        nameField.interactable = true;

        icon.sprite = CategoryIconLoader.GetDefaultIcon();
        iconButton.interactable = true;

        editButton.image.sprite = saveSprite;
        editButton.onClick.RemoveAllListeners();
        editButton.onClick.AddListener(OnSaveNew);

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(OnCancelNew);

        CategoryCreator.Instance.SetEditingMode(this);
    }

    private void OnEdit()
    {
        nameField.interactable = true;
        iconButton.interactable = true;

        editButton.image.sprite = saveSprite;
        editButton.onClick.RemoveAllListeners();
        editButton.onClick.AddListener(OnSaveEdit);

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(OnCancelEdit);

        CategoryCreator.Instance.SetEditingMode(this);
    }

    private void OnSaveEdit()
    {
        category.Name = nameField.text;
        DatabaseManager.Instance.DB.GetConnection().Update(category);

        CategoryCreator.Instance.ResetEditingMode();
        Setup(category);
    }

    private void OnDelete()
    {
        DatabaseManager.Instance.DB.GetConnection().Delete(category);
        Destroy(gameObject);
        CategoryCreator.Instance.ResetEditingMode();
    }

    private void OnSaveNew()
    {
        if (string.IsNullOrWhiteSpace(nameField.text))
            return;

        category.Name = nameField.text;
        DatabaseManager.Instance.DB.GetConnection().Insert(category);

        CategoryCreator.Instance.ResetEditingMode();
        Setup(category);
    }

    private void OnCancelNew()
    {
        CategoryCreator.Instance.ResetEditingMode();
        Destroy(gameObject);
    }

    private void OnCancelEdit()
    {
        nameField.text = category.Name;
        CategoryCreator.Instance.ResetEditingMode();
        Setup(category);
    }

    private void Start()
    {
        iconButton.onClick.AddListener(() =>
        {
            if (!iconButton.interactable) return;

            iconSelector.Open(category.IconName, (newIcon) =>
            {
                category.IconName = newIcon;
                icon.sprite = CategoryIconLoader.GetIcon(newIcon);

                if (!isNew)
                    DatabaseManager.Instance.DB.GetConnection().Update(category);
            });
        });
    }
}
