    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class CategoryIconSelector : MonoBehaviour
    {
        public Transform content;
        public GameObject iconPrefab;

        public Button cancelButton;
        public Button saveButton;

        public ScrollRect mainScroll;

        private string selectedIcon;
        private System.Action<string> onSave;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Open(string currentIcon, System.Action<string> onSaveCallback)
        {
            onSave = onSaveCallback;

            if (string.IsNullOrEmpty(currentIcon))
                currentIcon = CategoryIconLoader.GetDefaultIcon().name;

            selectedIcon = currentIcon;

            if (mainScroll != null)
                mainScroll.enabled = false;

            foreach (Transform child in content)
                Destroy(child.gameObject);

            foreach (var iconName in CategoryIconLoader.GetAllIcons())
            {
                var item = Instantiate(iconPrefab, content);
                var icon = item.GetComponent<CategoryIcon>();

                bool isSelected = iconName == currentIcon;
                icon.Setup(iconName, isSelected, OnSelect);
            }

            gameObject.SetActive(true);

            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(() =>
            {
                onSave?.Invoke(selectedIcon);
                Close();
            });

            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(Close);
        }

        private void Close()
        {
            if (mainScroll != null)
                mainScroll.enabled = true;

            gameObject.SetActive(false);
        }

        private void OnSelect(string iconName)
        {
            selectedIcon = iconName;

            foreach (Transform child in content)
            {
                var icon = child.GetComponent<CategoryIcon>();
                if (icon != null)
                    icon.SetSelected(icon.iconName == iconName);
            }
        }
    }
