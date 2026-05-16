using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AccountItem : MonoBehaviour
{
    private Account account;
    private TransactionCreator creator;

    [Header("UI")]
    public TMP_Text nameText;
    public TMP_Text balanceText;
    public Image colorImage; // если у тебя есть цвет счёта

    public void Init(Account acc, TransactionCreator tc)
    {
        account = acc;
        creator = tc;

        nameText.text = acc.Name;
        balanceText.text = acc.StartAmount.ToString("0.00");

        if (colorImage != null && !string.IsNullOrEmpty(acc.ColorHex))
        {
            Color col;
            if (ColorUtility.TryParseHtmlString(acc.ColorHex, out col))
                colorImage.color = col;
        }
    }

    public void OnClick()
    {
        creator.SelectAccount(account);
    }
}
