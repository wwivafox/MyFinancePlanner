using UnityEngine;
using UnityEngine.SceneManagement;

public class NavigationButtons : MonoBehaviour
{
    public string sceneName;

    public void OnClick()
    {

        SceneManager.LoadScene(sceneName);
    }
}
