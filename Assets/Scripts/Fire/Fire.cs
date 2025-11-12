using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// controls the fire
/// </summary>
public class Fire : MonoBehaviour
{
    [FormerlySerializedAs("maxFireCount")] public float fireTimeToPutOut; // fire counter to decrement
    public FireArea fireArea; // the fire area controller
    [SerializeField] private GameObject fireParticle;
    [SerializeField] private float remainingFireTime; // curr fire counter (how long player needs to spray before this is put out)
    private Collider detectCollider; // used for player to call ReduceFire

    void Start()
    {
        remainingFireTime = 0.0f;

        // set up the collider for putting out
        detectCollider = GetComponent<Collider>();
        detectCollider.enabled = false;

        fireParticle.SetActive(false);
    }

    public void StartFire()
    {
        // fill up the counter
        remainingFireTime = fireTimeToPutOut;

        // enable the collider for putting out
        detectCollider.enabled = true;

        // enable particle
        fireParticle.SetActive(true);

        fireParticle.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// called by player when putting fire out
    /// </summary>
    public void ReduceFire()
    {
        remainingFireTime -= Time.deltaTime;

        remainingFireTime = Mathf.Max(remainingFireTime, 0);

        float scale = remainingFireTime / fireTimeToPutOut;
        scale = Mathf.Clamp(scale, 0.1f, 1f);
        fireParticle.transform.localScale = Vector3.one * scale;

        if (remainingFireTime <= 0)
        {
            // notify fire area
            if (fireArea != null)
                fireArea.DisableFire();

            // disable the collider
            detectCollider.enabled = false;

            // disable particle
            fireParticle.SetActive(false);
        }
    }
}
