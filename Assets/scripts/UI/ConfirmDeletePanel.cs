using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfirmDeletePanel : MonoBehaviour
{
    private Account accountToDelete;

    public void SetAccount(Account acc)
    {
        accountToDelete = acc;
    }

    public void OnCancel()
    {
        gameObject.SetActive(false);
        AccountPanelState.AnyPanelOpen = false;
    }

    public void OnConfirmDelete()
    {
        if (accountToDelete == null)
            return;

        Repository repo = new Repository();
        repo.DeleteAccount(accountToDelete.Id);

        AccountPanelState.AnyPanelOpen = false;

        SceneManager.LoadScene("MainPage");
    }
}
