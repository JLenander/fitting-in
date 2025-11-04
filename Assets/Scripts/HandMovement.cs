using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class HandMovement : MonoBehaviour
{
    public float speed = 5f;

    // Default distance to grapple from the body  if the aiming doesn't target a grapple stop
    public float defaultGrappleDistance = 50f;

    private InputAction _moveAction;
    private InputAction _leftTriggerAction;
    private InputAction _rightTriggerAction;
    private InputAction _leftBumperAction;
    private InputAction _rightBumperAction;
    private InputAction _lookAction;
    private InputAction _interactAction;

    public Vector3 movement = Vector3.zero;

    public float baseZ = 4.23f;
    private Vector3 _ogPosition;
    
    // Disable the arm entirely, keeping the arm retracting
    private bool _disable;
    // Freeze the position of the hand but still allow interacting and stopping interacting
    private bool _freeze;
    // Disable the grapple shooting out
    private bool _grappleDisabled;

    private bool _isMoving;
    public AudioSource moveSource;
    public AudioSource stopSource;

    private GameObject _currPlayer;

    [FormerlySerializedAs("lookSensitivity")] public float handPitchYawSensitivity = 0.4f;

    [FormerlySerializedAs("wristRotationSpeed")] [SerializeField] private float wristRollSpeed = 1.0f;

    // The transforms to control the hand/wrist roll/pitch/yaw (airplane degrees of freedom system).
    // Pitch and Yaw are separate from Roll as we want them to be independent of the hand/wrist roll orientation.
    [SerializeField] private Transform wristRoll;
    [SerializeField] private Transform wristPitchYaw;
    [SerializeField] private Transform wristBone;

    private bool grappleShot;

    private Vector3 _wristRotation;

    public Animator oppositeHandAnimator; // animator of opposite hand
    public Animator handAnimator;
    public InteractableObject currObj;    // currently interacting with hand
    private GameObject _toInteractObj;  // check which object is it colliding with
    private bool _canInteract;  // can interact status

    [SerializeField] private GameObject grappleArmSpline;

    public AudioSource hookSource;
    public StudioEventEmitter grappleSfx;

    public HeadConsole headConsole;

    public bool left;

    [SerializeField] private Transform grappleTarget;

    private bool triggerWasPressed = false;

    private Vector3 targetObjRest;
    private Vector3 lastTargetPos;

    private Vector3 shootPos;

    private void Start()
    {
        _ogPosition = transform.localPosition;
        _wristRotation = Vector3.zero;
        _disable = true;
        grappleShot = false;
        targetObjRest = grappleTarget.localPosition;
    }

    private void Update()
    {
        if (_disable)
        {
            grappleArmSpline.GetComponent<SplineController>().SetRetracting();
            // keep arm retracting without player input
            return;
        }
        
        if (_freeze)
        {
            // code below somehow allows any hand console player's any button press to stop interaction
            // if (currObj != null && _currPlayer != null)
            // {
            //     // Only stop if this hand's player pressed interact
            //     var playerInput = _currPlayer.GetComponent<PlayerInput>();
            //     var playerInteract = InputActionMapper.GetPlayerItemInteractAction(playerInput);
            //
            //     if (playerInteract.WasPressedThisFrame())
            //     {
            //         Debug.Log("interaction " + _toInteractObj + _canInteract);
            //         StopInteractingWithObject(currObj);
            //     }
            // }
            
            // still allow stopping interaction with frozen hand
            if (_interactAction.WasPressedThisFrame() && currObj != null && _currPlayer!= null)
            {
                Debug.Log("interaction " + _toInteractObj + _canInteract);
                StopInteractingWithObject(currObj);
            }

            // so hand stays in position when frozen and walking
            grappleTarget.position = lastTargetPos;
            targetObjRest = grappleTarget.localPosition;
            return;
        }

        // hand rigid body movement
        Vector2 leftStickMove = _moveAction.ReadValue<Vector2>();
        Vector3 moveVector;
        if (!grappleShot)
        {
            // Move vector for hand target
            moveVector = new Vector3(leftStickMove.x, leftStickMove.y, 0);
        }
        else
        {
            // Move vector for target zone
            moveVector = new Vector3(leftStickMove.x, 0, leftStickMove.y);
        }
        
        // wrist rotation. For less confusing controls, give each axis a 45 degree angle of exclusivity
        // (only roll or only pitch depending on which direction the stick is most moved in)
        Vector2 rightStickMove = _lookAction.ReadValue<Vector2>() * Time.deltaTime;
        if (Mathf.Abs(rightStickMove.x) > Mathf.Abs(rightStickMove.y))
        {
            // roll
            _wristRotation.z += rightStickMove.x * wristRollSpeed;
        }
        else
        {
            // pitch
            _wristRotation.y += rightStickMove.y * handPitchYawSensitivity;
        }
        ClampWristRotate();
        
        float leftTrigger = _leftTriggerAction.ReadValue<float>();
        float rightTrigger = _rightTriggerAction.ReadValue<float>();
        // Vector3 triggerMovement = new Vector3(0, 0, leftTrigger - rightTrigger);

        movement += moveVector * Time.deltaTime;

        // changed from movement.magnitude to this addition because movement is now += instead of =
        bool movingNow = moveVector.magnitude > 0.5f;

        // Movement started
        if (movingNow && !_isMoving)
        {
            _isMoving = true;

            // != expensive but confirmed the right approach
            if (moveSource != null && !moveSource.isPlaying && !grappleShot)
                moveSource.Play();
        }

        // Movement stopped
        if (!movingNow && _isMoving)
        {
            _isMoving = false;

            if (moveSource != null && moveSource.isPlaying)
                moveSource.Stop();

            if (stopSource != null && !grappleShot)
                stopSource.Play();
        }

        // check if hand is empty and is there an object to interact with
        if (_interactAction.WasPressedThisFrame() && _toInteractObj != null && _canInteract && currObj == null)
        {
            if (_toInteractObj.TryGetComponent(out InteractableObject interactable))
            {
                if (interactable.canPickup)
                {
                    InteractWithObject(interactable);
                    _canInteract = false;
                }
            }
        }

        // check if hand is not empty
        else if (_interactAction.WasPressedThisFrame() && currObj != null)
        {
            Debug.Log("interaction " + _toInteractObj + _canInteract);
            StopInteractingWithObject(currObj);
        }

        bool triggerPressed = leftTrigger > 0.1f || rightTrigger > 0.1f;

        if (triggerPressed && !triggerWasPressed && !_grappleDisabled)
        {
            if (!grappleShot)
            {
                // EmergencyEvent.Instance.IncrementCount(true); // or pass correct value

                if (grappleSfx != null)
                    grappleSfx.Play();

                // get distance from head
                bool hit = headConsole.GrappleDistance(out var grappleTargetDist, out var grappleTargetPos);

                if (hit)
                {
                    grappleArmSpline.GetComponent<SplineController>().SetExtending(grappleTargetDist);
                    grappleTarget.position = grappleTargetPos;
                    targetObjRest = grappleTarget.localPosition;
                }
                else
                {
                    // no target, aim towards the reticle with a default distance
                    grappleArmSpline.GetComponent<SplineController>().SetExtending(defaultGrappleDistance);
                    var defaultGrapplePos = headConsole.GetExternalCameraPosition() + (headConsole.GetExternalCameraDirection() * defaultGrappleDistance);
                    grappleTarget.position = defaultGrapplePos;
                    targetObjRest = grappleTarget.localPosition;
                }


                // save shoot pos
                shootPos = movement;

                movement = new Vector3(0, 0, 0); // change when we get direction from head
            }
            else
            {
                movement = shootPos;
                grappleArmSpline.GetComponent<SplineController>().SetRetracting();
            }

            grappleShot = !grappleShot;
        }

        triggerWasPressed = triggerPressed;

        // Calculate movement of the grapple target
        if (grappleShot)
        {
            Vector3 grappleMvt;
            // control grapple target,
            if (!left)
            {
                grappleMvt = movement * speed + targetObjRest;
            }
            else
            {
                Vector3 tmpMvt = movement;
                tmpMvt.x *= -1.0f;
                grappleMvt = tmpMvt * speed + targetObjRest;
            }

            // totalMvt.x = Mathf.Clamp(totalMvt.x, -20f, 28f);
            // totalMvt.z = Mathf.Clamp(totalMvt.z, -21.8f, 23.5f);
            float currentY = grappleTarget.localPosition.y;

            grappleTarget.localPosition = new Vector3(grappleMvt.x, currentY, grappleMvt.z);

            Vector3 clampedMovement = (grappleTarget.localPosition - targetObjRest) / speed;
            if (left)
                clampedMovement.x *= -1.0f;

            movement = clampedMovement;
        }
        else
        {
            Vector3 handMvt;
            if (left)
            {
                handMvt = movement * speed + _ogPosition;
            }
            else
            {
                Vector3 tmpMvt = movement;
                tmpMvt.x *= -1.0f;
                handMvt = tmpMvt * speed + _ogPosition;
            }

            handMvt.x = Mathf.Clamp(handMvt.x, 97f, 117f);
            handMvt.y = Mathf.Clamp(handMvt.y, -21.8f, -4.5f);
            float currentZ = transform.localPosition.z;

            transform.localPosition = new Vector3(handMvt.x, handMvt.y, currentZ);

            Vector3 clampedMovement = (transform.localPosition - _ogPosition) / speed;
            if (!left)
                clampedMovement.x *= -1.0f;

            movement = clampedMovement;
        }
        
        // Rotation
        // pitch on parent object so the direction is independent of the wrist roll orientation.
        if (left)
        {
            // left hand pitch & yaw
            wristPitchYaw.localRotation = Quaternion.Euler(_wristRotation.y, 0, 0);
            // left hand roll
            wristRoll.localRotation = Quaternion.Euler(0, _wristRotation.z, 0);
        }
        else
        {
            // right hand pitch & yaw
            wristPitchYaw.localRotation = Quaternion.Euler(_wristRotation.y, 0, 0);
            // right hand roll
            wristRoll.localRotation = Quaternion.Euler(0, _wristRotation.z * -1.0f, 0);
        }
    }

    /// <summary>
    /// Clamp the wrist rotate to the appropriate values to prevent excessive wrist rotation
    /// </summary>
    private void ClampWristRotate()
    {
        _wristRotation.x = Mathf.Clamp(_wristRotation.x, -110f, 110f);
        _wristRotation.y = Mathf.Clamp(_wristRotation.y, -110f, 110f);
    }

    public Vector3 GetWristRotation()
    {
        return _wristRotation;
    }

    public void SetWristRotation(Vector3 wristRotation)
    {
        _wristRotation = wristRotation;
        ClampWristRotate();
    }

    public void MoveTargetZ(float z)
    {
        // Vector3 triggerMovement = new Vector3(0, 0, leftTrigger - rightTrigger);
        Vector3 pos = transform.localPosition;
        pos.z = z;
        transform.localPosition = pos;
    }

    public void RevertTargetZ()
    {
        // Vector3 triggerMovement = new Vector3(0, 0, leftTrigger - rightTrigger);
        Vector3 pos = transform.localPosition;
        pos.z = baseZ;
        transform.localPosition = pos;
    }

    // using TurnOn to initialize when player starts using the hand, not in Start() when object instantiate
    public void TurnOn(GameObject playerUsing)
    {
        _currPlayer = playerUsing;
        var input = _currPlayer.GetComponent<PlayerInput>();
        _moveAction = InputActionMapper.GetPlayerMoveAction(input);
        _lookAction = InputActionMapper.GetPlayerLookAction(input);
        _leftTriggerAction = InputActionMapper.GetPlayerLeftTriggerAction(input);
        _rightTriggerAction = InputActionMapper.GetPlayerRightTriggerAction(input);
        _leftBumperAction = InputActionMapper.GetPlayerLeftBumperAction(input);
        _rightBumperAction = InputActionMapper.GetPlayerRightBumperAction(input);
        _interactAction = InputActionMapper.GetPlayerItemInteractAction(input);
        _disable = false;
    }

    public void TurnOff(GameObject playerUsing)
    {
        // failed method of making single hand stay on tray even if not at hand console: if (!_freeze)
        // somehow joins all player's all hand console button presses
        // so all interaction/move/rotate/launching will mess up 

        _disable = true;
        grappleShot = false;
        
        // Stop movement sound and play stop sound if we were moving
        if (moveSource != null && moveSource.isPlaying)
            moveSource.Stop();
        if (_isMoving)
        {
            _isMoving = false;

            if (stopSource != null)
                stopSource.Play();
        }
    }

    public void SetCurrentInteractableObject(GameObject handUsing, bool canInteract)
    {
        _toInteractObj = handUsing;
        _canInteract = canInteract;
    }

    public void InteractWithObject(InteractableObject interactableObject)
    {
        Debug.Log("Interacting with " + interactableObject);
        interactableObject.InteractWithHand(wristBone, this);
    }

    public void StopInteractingWithObject(InteractableObject interactableObject)
    {
        Debug.Log("Stopping interaction with " + interactableObject);
        interactableObject.StopInteractWithHand(this);
        currObj = null;
    }

    public void SetTargetCurrentObject(InteractableObject obj)
    {
        currObj = obj;
    }

    public GameObject GetCurrPlayer()
    {
        return _currPlayer;
    }
    
    public void FreezeWristPosition(bool freeze)
    {
        if (freeze)
        {
            lastTargetPos = grappleTarget.position;
        }
        _freeze = freeze;
    }

    // Make sure both hands not launched when both holding object
    // Also restrict xymovement
    public void attachedCheckGrapple()
    {
        if (grappleShot)
        {
            movement = shootPos;
            grappleArmSpline.GetComponent<SplineController>().SetRetracting();
            grappleShot = false;
        }
    }

    public void disableGrapple(bool disable)
    {
        _grappleDisabled = disable;
    }
}
