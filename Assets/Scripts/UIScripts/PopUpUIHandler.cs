using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PopUpUIHandler : MonoBehaviour
{
    public static PopUpUIHandler Instance;
    public UIDocument uIDocument;
    public float blinkInterval = 0.5f;

    private VisualElement root;
    private VisualElement newTaskContainer;
    private VisualElement blinkContainer;
    private Coroutine blinkRoutine;

    void Start()
    {
        // Only allow one level manager
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        root = uIDocument.rootVisualElement;

        newTaskContainer = root.Query<VisualElement>("NewTaskContainer").First();
        blinkContainer = root.Query<VisualElement>("BlinkContainer").First();

        HideNewTaskPopUp();
        HideBlinkPopUp();
    }

    // show new task container popup
    public void ShowNewTaskPopUp()
    {
        newTaskContainer.style.display = DisplayStyle.Flex;
    }

    // hide new task popup
    public void HideNewTaskPopUp()
    {
        newTaskContainer.style.display = DisplayStyle.None;
    }

    public void ShowBlinkPopUp()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            blinkContainer.style.display = DisplayStyle.Flex;
            yield return new WaitForSeconds(blinkInterval);
            blinkContainer.style.display = DisplayStyle.None;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    public void HideBlinkPopUp()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        blinkContainer.style.display = DisplayStyle.None;
    }
}
