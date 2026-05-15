using System.Collections;
using UnityEngine;

public class CarnivalLevel2Manager : MonoBehaviour
{
    public static CarnivalLevel2Manager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(WaitForTaskManager());
        Level2TaskManager.StartTaskLevel2Intro();
    }
    
    IEnumerator WaitForTaskManager()
    {
        yield return new WaitUntil(() => Level2TaskManager.Instance != null);
    }
}
