using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class EditAccountData
{
    public static Account EditingAccount = null;
}


public class AccountItem2 : MonoBehaviour
{
    public Account Account { get; private set; }

    [Header("Основная кнопка счёта")]
    public Button button;

    [Header("Панель действий (Удалить / Редактировать)")]
    public GameObject actionPanel;

    [Header("UI")]
    public TMP_Text nameText;
    public TMP_Text balanceText;
    public Image background;

    public void Init(Account acc, TransactionCreator tc)
    {
        Account = acc;

        nameText.text = acc.Name;
        balanceText.text = $"{acc.StartAmount:0.00}\n{acc.Currency}";

        if (!string.IsNullOrEmpty(acc.ColorHex))
        {
            if (ColorUtility.TryParseHtmlString(acc.ColorHex, out var col))
                background.color = col;
        }

        button.onClick.AddListener(OnClick);

        actionPanel.SetActive(false);
    }

    public void OnClick()
    {
        if (AccountPanelState.AnyPanelOpen)
            return;

        actionPanel.SetActive(true);
        AccountPanelState.AnyPanelOpen = true;

        MainPage.Instance.openedActionPanel = actionPanel;
    }


    public void OnDeletePressed()
    {
        MainPage.Instance.OpenConfirmPanel(Account);
    }

    public void OnEditPressed()
    {
        EditAccountData.EditingAccount = Account;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Accounts");
    }

    public void CloseActionPanel()
    {
        actionPanel.SetActive(false);
        AccountPanelState.AnyPanelOpen = false;
    }
}
