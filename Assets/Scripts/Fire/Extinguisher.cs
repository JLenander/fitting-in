using UnityEngine;

/// <summary>
/// on active this shoots a ray out to find fires
/// </summary>
public class Extinguisher : MonoBehaviour
{
    public GameObject waterStream; // the graphic, maybe adjust z scaling based on distance
    public GameObject waterCylinder;
    public GameObject endSpray;
    public float reach = 10f;
    public LayerMask fireLayer;
    private bool active;
    private bool reset;

    void Start()
    {
        // disable at start
        waterStream.SetActive(false);
        active = false;
        reset = true;

        Vector3 scale = waterCylinder.transform.localScale;
        scale.z = 250;
        waterCylinder.transform.localScale = scale;
    }

    // Update is called once per frame
    void Update()
    {
        if (!active) return;

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Debug.DrawRay(ray.origin, ray.direction * reach, Color.cyan);

        if (Physics.Raycast(ray, out hit, reach, fireLayer))
        {
            Fire fire = hit.collider.GetComponent<Fire>();
            if (fire != null)
            {
                fire.ReduceFire();

                // shorten water stream
                float dist = hit.distance;
                Vector3 scale = waterCylinder.transform.localScale;
                scale.z = dist * 50; // model is shrunk for some reason
                waterCylinder.transform.localScale = scale;

                if (endSpray != null)
                {
                    endSpray.transform.position = hit.point + Vector3.down * 0.5f;
                    endSpray.SetActive(true);
                }

                reset = false;
            }
        }
        else
        {
            if (!reset)
            {
                reset = true;
                Vector3 scale = waterCylinder.transform.localScale;
                scale.z = 250;
                waterCylinder.transform.localScale = scale;
            }

            if (endSpray != null)
                endSpray.SetActive(false);
        }
    }

    public void ActivateExtinguisher(bool state)
    {
        active = state;
        waterStream.SetActive(state);
    }
}
