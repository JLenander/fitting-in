// Controls phone UI, switch between Face ID, Home, Swipe (profiles to choose), and Match screens.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhoneUIController : MonoBehaviour
{
    public Image screenImage; // drag UI Image here
    public Image nextImage; // drag next UI Image
    // Drag and drop sprites here
    public Sprite faceIDSprite;
    public Sprite homeSprite;
    public List<Sprite> profiles;
    public Sprite match;

    public float flySpeed = 1000f;  // pixels per second
    public float tiltAngle = 30f;   // degrees

    public void ShowFaceID() => screenImage.sprite = faceIDSprite;
    public void ShowHome() => screenImage.sprite = homeSprite;

    public int countBeforeMatch = 6;

    [SerializeField] private DialogueScriptableObj openingDialogue;
    [SerializeField] private DialogueScriptableObj cheekyDialogue;

    [SerializeField] private DialogueScriptableObj matchDialogue;

    public SceneExitDoor sceneExitDoor;

    public Vector3 target;

    private int index = 0;
    private int count = 0;
    private int swipeDirection = 0; // -1 = left, 1 = right
    private RectTransform rt;
    private bool swiping = false;
    private bool locked = true;
    // private bool remarked = false;
    private TaskManager taskManager;
    private bool matchedText = false;

    private void Awake()
    {
        rt = screenImage.rectTransform;
    }

    // Toggle between two swipe screens, for now
    // CHANGE when more profiles added
    public bool ClickApp()
    {
        if (screenImage.sprite == homeSprite)
        {
            screenImage.sprite = profiles[index];
            index = (index + 1) % profiles.Count;
            nextImage.sprite = profiles[index];

            locked = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SwipeNope()
    {
        if (!locked)
        {
            // flick sceen image left
            StartSwipe(1);
            count++; // increase count
        }
    }
    public void SwipeLike()
    {
        if (!locked)
        {
            StartSwipe(-1);
            count++; // increase count
        }
    }

    private void StartSwipe(int direction)
    {
        swipeDirection = direction;

        // once hit min swipes no matter left or right
        // start count down before showing nova's profile
        // if (count > profiles.Count && !remarked)
        // {
        //     remarked = true;
        //     GlobalPlayerUIManager.Instance.LoadText(cheekyDialogue);
        // }

        if (count > countBeforeMatch && !matchedText)
        {
            matchedText = true;
            swiping = false;
            StartCoroutine(MatchRoutine());
        }
        else
        {
            swiping = true;
        }
    }

    private void Update()
    {
        if (!swiping) return;

        // Move horizontally based on direction
        rt.anchoredPosition += Vector2.right * swipeDirection * flySpeed * Time.deltaTime;

        // Tilt while flying (left/right)
        rt.localRotation = Quaternion.Euler(0, 0, 180 - tiltAngle * swipeDirection);

        // Off-screen check
        if (Mathf.Abs(rt.anchoredPosition.x) > Screen.width)
        {
            swiping = false;
            rt.anchoredPosition = Vector2.zero;
            rt.localRotation = Quaternion.Euler(0, 0, 180);

            // afterwards swap screen to next and next to next next
            screenImage.sprite = profiles[index];
            index = (index + 1) % profiles.Count; // loop around if past the list
            nextImage.sprite = profiles[index];
        }
    }

    IEnumerator MatchRoutine()
    {
        // yield return new WaitForSeconds(2);

        locked = true; // lock swiping
        screenImage.sprite = match;
        nextImage.sprite = match;

        yield return null;

        Level0TaskManager.CompleteTaskSwipe();

        // enable exit door
        sceneExitDoor.enabled = true;

        // move the door into scene
        while (Vector3.Distance(sceneExitDoor.transform.position, target) > 0.01f)
        {
            sceneExitDoor.transform.position = Vector3.MoveTowards(sceneExitDoor.transform.position, target, 10 * Time.deltaTime);
            yield return null;
        }

        // Snap to target just in case
        sceneExitDoor.transform.position = target;

        Level0TaskManager.StartTaskLeavePhone();

        // GlobalPlayerUIManager.Instance.LoadText(matchDialogue);
    }

    public void Unlock()
    {
        locked = false;
    }
}


