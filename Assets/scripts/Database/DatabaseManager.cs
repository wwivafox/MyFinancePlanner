using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }
    public Database DB { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("DBManager Awake() BEGIN");

            DB = new Database();
            DB.Init();

            Debug.Log("DBManager Awake() END");
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
