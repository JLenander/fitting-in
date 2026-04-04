using System;
using System.Linq;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Controls the spline segment to be anchored at a starting position and move towards a target object.
/// The spline segment has 3 points (start, middle, end) where the start is fixed to an anchor.
/// The end of the spline segment targets the target object or the start anchor depending on setting.
/// If the end of the spline segment is within a threshold distance of the start anchor (the rest position) then
/// the segment snaps to the rest position.
///
/// Should be attached to a GameObject with a SplineContainer component
/// </summary>
public class SplineController : MonoBehaviour
{
    // If true, the spline is extending regardless of the state of <extending>.
    public bool OverrideExtend;
    // True if the spline segment is extending, false if it is retracting
    [SerializeField] private bool extending;
    // The anchor for the starting position of the spline segment
    [SerializeField] private Transform startObject;
     // The transform of the object to be placed on the end of this spline segment
    [SerializeField] [CanBeNull] private Transform endObject;
    [SerializeField] private Vector3 endPositionOffset;
    // [SerializeField] private Vector3 endInitialRotation;
    // The anchor for the target end position of the spline (on extend).
    [SerializeField] private Transform targetObject;
    
    [SerializeField] private float extendSpeed;
    [SerializeField] private float interpolateFactor;
    
    // The rest position (local space relative to the knots).
    [SerializeField] private Vector3 restLocalPosition = new Vector3(0.0f, 0.0f, 0.0f);
    [SerializeField] private float restPositionSnapThreshold = 0.1f;

    // The threshold for reporting if the spline is fully extended or retracted
    [SerializeField] private float FullExtensionThreshold = 2f;
    
    // SplineContainer is the unity component for the actual Spline.
    private SplineContainer _splineContainer;
    private BezierKnot _startKnot;
    private BezierKnot _midKnot;
    private BezierKnot _endKnot;
    
    // Private vars for smooth motion.
    private Vector3 _currEndPointForce;
    private Vector3 _currMidPointForce;

    private Vector3 targetObjRest;
    
    private void Start()
    {
        _splineContainer = GetComponent<SplineContainer>();
        var knots = _splineContainer.Spline.Knots.ToArray();
        _startKnot = knots[0];
        _midKnot = knots[1];
        _endKnot = knots[2];
        
        _currEndPointForce = Vector3.zero;
        _currMidPointForce = Vector3.zero;
        targetObjRest = targetObject.localPosition;
    }

    private void Update()
    {
        UpdateStartEndObjectTransforms();
        UpdateSplineSegment();
    }
    
    public void SetExtending(float value)
    {
        targetObject.localPosition = targetObjRest + new Vector3(0f, value * 15f, 0f);
        extending = true;
    }

    public void SetRetracting()
    {
        extending = false;
    }

    /// <summary>
    /// The spline is fully extended (within some tolerance) if it is near the target length
    /// </summary>
    /// <returns>True if the spline is fully extended, false otherwise</returns>
    public bool IsFullyExtended()
    {
        if (!extending) return false;
        var localTargetPos = GetLocalTargetPosition();
        var endpointPos = GetKnotLocalPosition(_endKnot);
        return (localTargetPos - endpointPos).magnitude < FullExtensionThreshold;
    }

    /// <summary>
    /// The spline is fully retracted (within some tolerance) if it is near the rest length
    /// </summary>
    /// <returns>True if the spline is fully extended, false otherwise</returns>
    public bool IsFullyRetracted()
    {
        if (extending) return false;
        var localTargetPos = GetLocalTargetPosition();
        var endpointPos = GetKnotLocalPosition(_endKnot);
        return (localTargetPos - endpointPos).magnitude < FullExtensionThreshold;
    }

    /// <returns>True if the grapple arm is extending or retracting</returns>
    public bool IsGrappling()
    {
        return !IsFullyExtended() && !IsFullyRetracted();
    }

    /// <summary>
    /// Get the target's position in local space relative to the Spline where (0,0,0) is the position of hte first knot
    /// </summary>
    /// <returns>The Vector3 local position of the spline target</returns>
    private Vector3 GetLocalTargetPosition()
    {
        return OverrideExtend || extending ? _splineContainer.transform.InverseTransformPoint(targetObject.position) : restLocalPosition;
    }

    private void UpdateSplineSegment()
    {
        var localTargetPos = GetLocalTargetPosition();
        var endpointPos = GetKnotLocalPosition(_endKnot);
        
        // Update the End Knot
        // Direction Vector in local space from the current spline segment endpoint to the target's postion in local space
        var directionVector = localTargetPos - endpointPos;
        // If close to the rest position then just snap to rest position.
        if (!OverrideExtend && !extending && directionVector.magnitude <= restPositionSnapThreshold)
        {
            _endKnot.Rotation = quaternion.EulerXYZ(0,0,0);
            _endKnot.Position = restLocalPosition;
        }
        else
        {
            // Add a portion of this direction vector to the current forces
            _currEndPointForce = Vector3.LerpUnclamped(_currEndPointForce, directionVector, 1f) * (extendSpeed * Time.deltaTime);
            _endKnot += _currEndPointForce;
        }
        UpdateKnot(KnotIndex.End, _endKnot);
        
        // Update the Midpoint knot
        var midpointPos = GetKnotLocalPosition(_midKnot);
        var endpointRelativeMidpointPos = new Vector3(endpointPos.x / 2, endpointPos.y / 2, endpointPos.z / 2);
        var midpointDirectionVector = endpointRelativeMidpointPos - midpointPos;
        // Lag behind if extending (or extended) for a bendy look
        float midpointLagBehindFactor = 1.0f;
        if (OverrideExtend || extending)
        {
            midpointLagBehindFactor = 0.7f;
        }
        _currMidPointForce = Vector3.LerpUnclamped(_currMidPointForce, midpointDirectionVector, midpointLagBehindFactor) * (extendSpeed * Time.deltaTime);
        // If we're extending lag the middle knot for a bendy look
        _midKnot += _currMidPointForce;
        UpdateKnot(KnotIndex.Mid, _midKnot);
    }

    // Update our position relative to the start object
    // and update the end object relative to the end of our spline segment.
    private void UpdateStartEndObjectTransforms()
    {
        _splineContainer.transform.position = startObject.position;
        if (endObject != null)
        {
            endObject.position = GetKnotWorldPosition(_endKnot) + endPositionOffset;
        }
    }

    private Vector3 GetKnotLocalPosition(BezierKnot knot)
    {
        var knotPos = knot.Position;
        return new Vector3(knotPos.x, knotPos.y, knotPos.z);
    }
    
    private Vector3 GetKnotWorldPosition(BezierKnot knot)
    {
        return _splineContainer.transform.TransformPoint(GetKnotLocalPosition(knot));
    }

    private void UpdateKnot(KnotIndex index, BezierKnot newKnot)
    {
        _splineContainer.Spline.SetKnot((int) index, newKnot);
    }

    private enum KnotIndex
    {
        Start,
        Mid,
        End
    }
}
