using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AccountItem : MonoBehaviour
{
    private TransactionCreator creator;
    public Account Account { get; private set; }
    public Button button;


    [Header("UI")]
    public TMP_Text nameText;
    public TMP_Text balanceText;
    public Image background;

    public void Init(Account acc, TransactionCreator tc)
    {
        Account = acc;
        creator = tc;

        nameText.text = acc.Name;
        balanceText.text = $"{acc.StartAmount:0.00}\n{acc.Currency}";


        if (background != null && !string.IsNullOrEmpty(acc.ColorHex))
        {
            Color col;
            if (ColorUtility.TryParseHtmlString(acc.ColorHex, out col))
                background.color = col;
        }

        button.onClick.AddListener(OnClick);

    }

    public void OnClick()
    {
        creator.SelectAccount(Account);
    }
}
