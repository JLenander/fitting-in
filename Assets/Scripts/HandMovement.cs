using System;
using System.Collections;
using System.Collections.Generic;
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
    public StudioEventEmitter moveSfx;
    public StudioEventEmitter stopSfx;
    public StudioEventEmitter grappleSfx;
    
    private GameObject _currPlayer;

    [FormerlySerializedAs("lookSensitivity")] public float handPitchYawSensitivity = 0.4f;

    [FormerlySerializedAs("wristRotationSpeed")] [SerializeField] private float wristRollSpeed = 1.0f;

    // The transforms to control the hand/wrist roll/pitch/yaw (airplane degrees of freedom system).
    // Pitch and Yaw are separate from Roll as we want them to be independent of the hand/wrist roll orientation.
    [SerializeField] private Transform wristRoll;
    [SerializeField] private Transform wristPitchYaw;
    [SerializeField] private Transform wristBone;

    private bool _grappleShot;

    private Vector3 _wristRotation;

    public Animator oppositeHandAnimator; // animator of opposite hand
    public Animator handAnimator;
    
    // Interactable object information
    public InteractableObject currObj;    // currently interacting with hand
    // Ordered list of objects available to interact. First interactable object is priority for hand
    [SerializeField] private List<InteractableObject> candidateInteractables = new List<InteractableObject>();
    private Dictionary<InteractableObject, InteractableObjectData> _interactablesData = new Dictionary<InteractableObject, InteractableObjectData>();
    // A delay counter for auto-picking up stuff after dropping an item
    private float _pickupDelayCounter;
    private const float AfterDropPickupDelay = 0.5f;
    
    [SerializeField] private GameObject grappleArmSpline;
    private SplineController _grappleArmSplineController;
    
    public HeadConsole headConsole;

    public bool left;

    [SerializeField] private Transform grappleTarget;

    private bool _triggerWasPressed;

    private Vector3 _targetObjRest;
    private Vector3 _lastTargetPos;

    private Vector3 _shootPos;

    private void Start()
    {
        _grappleArmSplineController = grappleArmSpline.GetComponent<SplineController>();
        
        _ogPosition = transform.localPosition;
        _wristRotation = Vector3.zero;
        _disable = true;
        _grappleShot = false;
        _targetObjRest = grappleTarget.localPosition;
        _pickupDelayCounter = 0.0f;
    }

    private void OnDestroy()
    {
        moveSfx.Stop();
        stopSfx.Stop();
        grappleSfx.Stop();
    }

    private void Update()
    {
        if (_disable)
        {
            _grappleArmSplineController.SetRetracting();
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
                Debug.Log("Stopping interaction from frozen hand " + currObj);
                StopInteractingWithObject(currObj);
            }

            // so hand stays in position when frozen and walking
            grappleTarget.position = _lastTargetPos;
            _targetObjRest = grappleTarget.localPosition;
            return;
        }

        // hand rigid body movement
        Vector2 leftStickMove = _moveAction.ReadValue<Vector2>();
        Vector3 moveVector;
        if (!_grappleShot)
        {
            // Move vector for arm rig target
            moveVector = new Vector3(leftStickMove.x, leftStickMove.y, 0);
        }
        else
        {
            // Move vector for hand target (spline target)
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
            if (moveSfx != null && !moveSfx.IsPlaying() && !_grappleShot)
                moveSfx.Play();
        }

        // Movement stopped
        if (!movingNow && _isMoving)
        {
            _isMoving = false;

            if (moveSfx != null && moveSfx.IsPlaying())
                moveSfx.Stop();

            if (stopSfx != null && !_grappleShot)
                stopSfx.Play();
        }

        // If hand is empty and there is an object we can grab, pick up the object
        if (currObj == null)
        {
            // After we drop an item, add a small delay before allowing the hand to automatically pickup something else
            // (We will not pickup the same item after dropping it until the item leaves and re-enters the hand hitbox
            //  but this delay helps to make the process feel better)
            if (_pickupDelayCounter > 0.0f)
            {
                _pickupDelayCounter -= Time.deltaTime;
            }
            else
            {
                // Do not auto-grab when extending or retracting
                if (!_grappleArmSplineController.IsGrappling())
                {
                    InteractableObject obj = GetFirstInteractableObject();
                    if (obj != null)
                    {
                        InteractWithObject(obj);
                    }
                }
            }
        }
        else if (_interactAction.WasPressedThisFrame())
        {
            // If holding an object and interact is pressed, drop the object.
            if (currObj.canDrop)
            {
                _interactablesData[currObj].SetIsDropped();
                StopInteractingWithObject(currObj);
                _pickupDelayCounter = Mathf.Max(_pickupDelayCounter, AfterDropPickupDelay);
            }
        }

        bool triggerPressed = leftTrigger > 0.1f || rightTrigger > 0.1f;

        if (triggerPressed && !_triggerWasPressed && !_grappleDisabled)
        {
            if (!_grappleShot)
            {
                // EmergencyEvent.Instance.IncrementCount(true); // or pass correct value

                if (grappleSfx != null)
                    grappleSfx.Play();

                // get distance from head
                bool hit = headConsole.GrappleDistance(out var grappleTargetDist, out var grappleTargetPos);

                if (hit)
                {
                    _grappleArmSplineController.SetExtending(grappleTargetDist);
                    grappleTarget.position = grappleTargetPos;
                    _targetObjRest = grappleTarget.localPosition;
                }
                else
                {
                    // no target, aim towards the reticle with a default distance
                    _grappleArmSplineController.SetExtending(defaultGrappleDistance);
                    var defaultGrapplePos = headConsole.GetExternalCameraPosition() + (headConsole.GetExternalCameraDirection() * defaultGrappleDistance);
                    grappleTarget.position = defaultGrapplePos;
                    _targetObjRest = grappleTarget.localPosition;
                }


                // save shoot pos
                _shootPos = movement;

                movement = new Vector3(0, 0, 0); // change when we get direction from head
            }
            else
            {
                movement = _shootPos;
                _grappleArmSplineController.SetRetracting();
            }

            _grappleShot = !_grappleShot;
        }

        _triggerWasPressed = triggerPressed;

        // Calculate movement of the grapple target
        if (_grappleShot)
        {
            Vector3 grappleMvt;
            // control grapple target,
            if (!left)
            {
                grappleMvt = movement * speed + _targetObjRest;
            }
            else
            {
                Vector3 tmpMvt = movement;
                tmpMvt.x *= -1.0f;
                grappleMvt = tmpMvt * speed + _targetObjRest;
            }

            // totalMvt.x = Mathf.Clamp(totalMvt.x, -20f, 28f);
            // totalMvt.z = Mathf.Clamp(totalMvt.z, -21.8f, 23.5f);
            float currentY = grappleTarget.localPosition.y;

            grappleTarget.localPosition = new Vector3(grappleMvt.x, currentY, grappleMvt.z);

            Vector3 clampedMovement = (grappleTarget.localPosition - _targetObjRest) / speed;
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
        _interactAction = InputActionMapper.GetPlayerItemInteractAction(input);
        _disable = false;
    }

    public void TurnOff(GameObject playerUsing)
    {
        // failed method of making single hand stay on tray even if not at hand console: if (!_freeze)
        // somehow joins all player's all hand console button presses
        // so all interaction/move/rotate/launching will mess up 

        _disable = true;
        _grappleShot = false;
        
        // Stop movement sound and play stop sound if we were moving
        if (moveSfx != null && moveSfx.IsPlaying())
            moveSfx.Stop();
        if (_isMoving)
        {
            _isMoving = false;

            if (stopSfx != null)
                stopSfx.Play();
        }
    }

    public GameObject GetCurrPlayer()
    {
        return _currPlayer;
    }
    
    public void FreezeWristPosition(bool freeze)
    {
        if (freeze)
        {
            _lastTargetPos = grappleTarget.position;
        }
        _freeze = freeze;
    }

    // Make sure both hands not launched when both holding object
    // Also restrict xymovement
    public void AttachedCheckGrapple()
    {
        if (_grappleShot)
        {
            movement = _shootPos;
            _grappleArmSplineController.SetRetracting();
            _grappleShot = false;
        }
    }

    public void DisableGrapple(bool disable)
    {
        _grappleDisabled = disable;
    }
    
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Interactable object related methods
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public record InteractableObjectData
    {
        public InteractableObject InteractableObj { get; }
        // A HandInteractableObj cannot be picked up again once dropped (due to auto-grab)
        // until the object hitbox leaves and re-enters the hand hitbox (which would cause this record to be recreated
        // for the object
        private bool _isDropped;
        
        public InteractableObjectData(InteractableObject obj)
        {
            InteractableObj = obj;
            _isDropped = false;
        }

        /// <summary>
        /// Set that this interactable object has been dropped which disallows interaction for this instance.
        /// (This InteractableObjectData must be cleared from a queue and a new instance of interaction to occur, ex.
        ///  object exits and re-enters interaction hitbox of hand, for it to be interactable again).
        /// </summary>
        public void SetIsDropped()
        {
            _isDropped = true;
        }
        
        /// <returns>True if this interactable object is allowed to be interacted with</returns>
        public bool CanInteract()
        {
            // This mirrors implementation before refactor (canInteract is not checked 
            return !_isDropped && InteractableObj != null && InteractableObj.canPickup;
        }
    }

    /// <summary>
    /// Add an InteractableObject to the object(s) that this hand can interact with.
    /// </summary>
    public void AddInteractableObject(InteractableObject interactableObj)
    {
        if (_interactablesData.ContainsKey(interactableObj))
        {
            Debug.LogWarning("Duplicate interactableObject attempted to add to candidates " + interactableObj);
            return;
        }
        candidateInteractables.Add(interactableObj);
        InteractableObjectData interactableData = new InteractableObjectData(interactableObj);
        _interactablesData.Add(interactableObj, interactableData);
    }
    
    /// <summary>
    /// Remove an InteractableObject from the object(s) this hand can interact with.
    /// </summary>
    public void RemoveInteractableObject(InteractableObject interactableObj)
    {
        if (!_interactablesData.ContainsKey(interactableObj))
        {
            Debug.LogWarning("Game object "  + interactableObj + " was removed from hand interactables but not found");
        }
        if (!candidateInteractables.Contains(interactableObj))
        {
            Debug.LogWarning("Game object "  + interactableObj + " was removed from hand interactables but not found");
        }
        candidateInteractables.Remove(interactableObj);
        _interactablesData.Remove(interactableObj);
    }
    
    /// <summary>
    /// Return the first interactable object in the interactables list (ordered by list order) or null if none are interactable.
    /// </summary>
    /// <returns>The Interactable Object or Null if no object is interactable</returns>
    private InteractableObject GetFirstInteractableObject()
    {
        string handType = left ? "left" : "right";
        InteractableObject interactableObj = null;
        for (int i = 0; i < candidateInteractables.Count; i++)
        {
            InteractableObject obj = candidateInteractables[i];
            // hacks to ensure interaction not stuck
            if (obj == null)
            {
                candidateInteractables.RemoveAt(i);
                i--;
                Debug.LogWarning(handType + " hand pruned missing gameobject from interactable candidate list");
            }
            else if (obj.isInHand())
            {
                candidateInteractables.RemoveAt(i);
                i--;
                Debug.LogWarning(handType + " hand had object that was in a hand");
            }
            else if (!obj.gameObject.activeSelf)
            {
                candidateInteractables.RemoveAt(i);
                i--;
                Debug.LogWarning(handType + " hand had object that was inactive");
            }
            else if (obj.canInteract == false)
            {
                candidateInteractables.RemoveAt(i);
                i--;
                Debug.LogWarning(handType + " hand had object that had canInteract false");
            }
            else if (_interactablesData[obj].CanInteract())
            {
                interactableObj = obj;
                break;
            }
        }
        
        return interactableObj;
    }

    /// <summary>
    /// Force the hand to interact with an object (if the hand is empty). Normally the hand detects an interactable and
    /// auto-grabs it but this method can be used if the hand needs to be forced to interact (such as grabbing the foodbite spawned
    /// by the food plate).
    /// </summary>
    /// <returns>True if the force interaction was successful, false otherwise</returns>
    public void ForceInteractionWithObject(InteractableObject interactableObj)
    {
        if (currObj == null)
        {
            AddInteractableObject(interactableObj);
            InteractWithObject(interactableObj);
        }
        else
        {
            Debug.Log("Could not force hand to interact with " + interactableObj + " as hand is full");
        }
    }
    
    /// <summary>
    /// Start interacting with a particular interactableObject, in particular informing that object that its
    /// interacting with the hand.
    /// </summary>
    private void InteractWithObject(InteractableObject interactableObject)
    {
        string handType = left ? "left" : "right";
        if (currObj != null)
        {
            Debug.LogWarning("Tried to interact with: " + interactableObject + " but had " + currObj + " in " + handType + " hand");
            return;
        }
        Debug.Log(handType + " hand interacting with " + interactableObject);
        interactableObject.InteractWithHand(wristBone, this);
        currObj = interactableObject;
    }

    /// <summary>
    /// Stop interacting with a particular object, only if the object can be dropped.
    /// </summary>
    public void StopInteractingWithObject(InteractableObject interactableObject)
    {
        string handType = left ? "left" : "right";
        if (interactableObject.canDrop)
        {
            Debug.Log(handType + " hand stopping interaction with " + interactableObject);
            interactableObject.StopInteractWithHand(this);
            currObj = null;
        }
        else
        {
            Debug.Log(handType + " hand cannot stop interaction with " + interactableObject);
        }
    }
}
