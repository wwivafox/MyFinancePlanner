using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapManeger : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("MainPage");
        Debug.Log("START");
    }


}
