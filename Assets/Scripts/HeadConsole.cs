using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeadConsole : Interactable
{
    private ISplitscreenUIHandler _splitscreenUIHandler;

    [SerializeField] private Camera exteriorCamera; //reference for robot head camera
    public float reach = 1000f;
    public float turnOffAfter = 3f;

    private bool _canInteract = true;

    private bool _leftJammed, _rightJammed;
    // private bool _leftShot, _rightShot;
    private InputAction _leftTriggerAction, _rightTriggerAction;
    private GameObject _currPlayer;

    public AudioSource hookSource;
    public AudioSource interactSource;
    public AudioSource denySource;

    private OverlayUIHandler uIHandler;
    private int layerMask;
    
    void Start()
    {
        DisableOutline();
        _splitscreenUIHandler = FindAnyObjectByType<SplitscreenUIHandler>();
        // for grapple arm
        _leftJammed = _rightJammed = false;
        // _leftShot = _rightShot = false;
        uIHandler = HeadUIHandler.Instance;

        layerMask = LayerMask.GetMask("GrappleStop");
    }

    // for grapple arm, check trigger input to shoot or retract
    // void Update()
    // {
    //     if (_currPlayer != null)
    //     {
    //         // Left arm
    //         if (_leftTriggerAction != null && _leftTriggerAction.ReadValue<float>() > 0.1f)
    //         {
    //             if (!_leftJammed)
    //             {
    //                 if (!_leftShot)
    //                 {
    //                     _leftShot = true;
    //                     EmergencyEvent.Instance.IncrementCount(true); // or pass correct value

    //                     if (hookSource != null)
    //                         hookSource.Play();
    //                 }
    //                 leftGrappleArmSpline.GetComponent<SplineController>().SetExtending(_leftTriggerAction.ReadValue<float>());
    //             }
    //             else
    //             {
    //                 if (denySource != null)
    //                     denySource.Play();
    //             }

    //         }
    //         else
    //         {
    //             if (_leftShot)
    //             {
    //                 _leftShot = false;
    //             }
    //             leftGrappleArmSpline.GetComponent<SplineController>().SetRetracting();
    //         }

    //         // Right arm
    //         if (_rightTriggerAction != null && _rightTriggerAction.ReadValue<float>() > 0.1f && !_rightJammed)
    //         {
    //             if (!_rightJammed)
    //             {
    //                 if (!_rightShot)
    //                 {
    //                     _rightShot = true;
    //                     EmergencyEvent.Instance.IncrementCount(false);

    //                     if (hookSource != null)
    //                         hookSource.Play();
    //                 }
    //                 rightGrappleArmSpline.GetComponent<SplineController>().SetExtending(_rightTriggerAction.ReadValue<float>());
    //             }
    //             else
    //             {
    //                 if (denySource != null)
    //                     denySource.Play();
    //             }

    //         }
    //         else
    //         {
    //             if (_rightShot)
    //                 _rightShot = false;
    //             rightGrappleArmSpline.GetComponent<SplineController>().SetRetracting();
    //         }
    //     }
    // }

    void Update()
    {
        Ray ray = new Ray(exteriorCamera.transform.position, exteriorCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, reach, layerMask))
        {
            if (hit.collider.CompareTag("GrappleStop"))
            {
                _splitscreenUIHandler.ReticleHit();
            }
        }
        else
        {
            _splitscreenUIHandler.ReticleNeutral();
        }
    }

    public override void Interact(GameObject player)
    {
        if (!_canInteract)
        {
            if (denySource != null)
                denySource.Play();
            return;
        }
        
        _splitscreenUIHandler.ShowOutsideCamera();

        player.GetComponent<Player>().TurnOff();
        player.GetComponent<Player>().switchToHead(exteriorCamera);
        _canInteract = false;

        if (interactSource != null)
            interactSource.Play();

        // for grapple arm
        _currPlayer = player;
        var input = _currPlayer.GetComponent<PlayerInput>();
        _leftTriggerAction = input.actions.FindAction("LeftTrigger");
        _rightTriggerAction = input.actions.FindAction("RightTrigger");
        uIHandler.ShowContainer(player);
    }

    public override void Return(GameObject player)
    {
        HideOutsideCamera();

        player.GetComponent<Player>().TurnOn();
        player.GetComponent<Player>().switchOffHead();

        _canInteract = true;

        // for grapple arm
        _currPlayer = null;
        _leftTriggerAction = null;
        _rightTriggerAction = null;
        uIHandler.HideContainer(player);
    }

    private void HideOutsideCamera()
    {
        _splitscreenUIHandler.HideOutsideCamera(turnOffAfter);
    }
    
    public override bool CanInteract()
    {
        return _canInteract;
    }

    public void DisableInteract()
    {
        _canInteract = false;
        hoverMessage = "[CONTROL DISABLED]";
        msgColour = new Color(1, 0, 0, 1);
        outlineColour = new Color(1, 0, 0, 1);
    }

    public void EnableInteract()
    {
        _canInteract = true;
        hoverMessage = "Control Head";
        msgColour = new Color(1, 1, 1, 1);
        outlineColour = new Color(1, 1, 1, 1);
    }

    // for grapple arm - to be called by HandConsole when arm is broken or fixed
    public void JamArm(bool left, bool state)
    {
        if (left)
        {
            _leftJammed = state;
            if (state)
            {
                TaskManager.StartTaskFixLeftArm();
            }
            else
            {
                TaskManager.CompleteTaskFixLeftArm();
            }
        }

        else
        {
            _rightJammed = state;
            if (state)
            {
                TaskManager.StartTaskFixRightArm();
            }
            else
            {
                TaskManager.CompleteTaskFixRightArm();
            }
        }

    }

    /// <summary>
    /// calculates the distance based on first POI struck
    /// </summary>
    /// <returns></returns>
    public bool GrappleDistance(out float dist, out Vector3 direction)
    {
        dist = 0;
        direction = Vector3.zero;

        Ray ray = new Ray(exteriorCamera.transform.position, exteriorCamera.transform.forward);
        RaycastHit hit;

        // Debug.DrawRay(exteriorCamera.transform.position, exteriorCamera.transform.forward * reach, Color.green, 10f);

        if (Physics.Raycast(ray, out hit, reach, layerMask))
        {
            if (hit.collider.CompareTag("GrappleStop"))
            {
                Vector3 targetPos = hit.collider.bounds.center;

                // end up above the centre
                float yOffset = 10.0f;
                targetPos.y += yOffset;


                direction = targetPos;
                dist = hit.distance;
                return true;
            }
        }

        return false;
    }
}
