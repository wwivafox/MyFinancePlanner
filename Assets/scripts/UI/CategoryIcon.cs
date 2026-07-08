using UnityEngine;
using UnityEngine.UI;

public class CategoryIcon : MonoBehaviour
{
    public Image iconImage;         
    public Button button;
    [HideInInspector] public string iconName;

    private System.Action<string> onClick;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(string name, bool isSelected, System.Action<string> onClickCallback)
    {
        iconName = name;
        onClick = onClickCallback;

        iconImage.sprite = CategoryIconLoader.GetIcon(name);

        SetSelected(isSelected);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            onClick?.Invoke(iconName);
        });
    }

    public void SetSelected(bool selected)
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = selected ? 1f : 0.7f;
    }
}
