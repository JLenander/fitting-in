using UnityEngine;

/// <summary>
/// controls the fire
/// </summary>
public class Fire : MonoBehaviour
{
    public int maxFireCount = 500; // fire counter to decrement
    public FireArea fireArea; // the fire area controller
    [SerializeField] private GameObject fireParticle;
    private int currFireCount; // curr fire counter
    private Collider detectCollider; // used for player to call ReduceFire

    void Start()
    {
        currFireCount = 0;

        // set up the collider for putting out
        detectCollider = GetComponent<Collider>();
        detectCollider.enabled = false;

        fireParticle.SetActive(false);
    }

    public void StartFire()
    {
        // fill up the counter
        currFireCount = maxFireCount;

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
        currFireCount--;

        currFireCount = Mathf.Max(currFireCount, 0);

        float scale = (float)currFireCount / maxFireCount;
        scale = Mathf.Clamp(scale, 0.1f, 1f);
        fireParticle.transform.localScale = Vector3.one * scale;

        if (currFireCount <= 0)
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
