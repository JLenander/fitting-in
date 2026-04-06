using System;
using UnityEngine;
using FMODUnity;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// on active this shoots a ray out to find fires
/// </summary>
public class Extinguisher : MonoBehaviour
{
    // Other render is only rendered to other player's camera.
    public GameObject otherPlayerWaterStream;
    public GameObject otherPlayerWaterCylinder;
    public GameObject otherPlayerEndSpray;
    // Exclusive render is only rendered to this player's camera.
    [FormerlySerializedAs("waterStream")] public GameObject exclusiveRenderWaterStream; // the graphic, maybe adjust z scaling based on distance
    [FormerlySerializedAs("waterCylinder")] public GameObject exclusiveRenderWaterCylinder;
    [FormerlySerializedAs("endSpray")] public GameObject exclusiveRenderEndSpray;
    public float reach = 10f;
    public LayerMask fireLayer;
    private bool active;
    public StudioEventEmitter waterSfx;

    void Start()
    {
        // disable at start
        ActivateExtinguisher(false);
        
        UpdateWaterStream();
        SceneManager.activeSceneChanged += OnSceneChange;
    }

    // Update is called once per frame
    void Update()
    {
        if (!active) return;
        
        Ray fireRay = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(fireRay, out hit, reach, fireLayer))
        {
            Fire fire = hit.collider.GetComponent<Fire>();
            if (fire != null)
            {
                fire.ReduceFire();
            }
        }
        
        UpdateWaterStream();
    }

    private void OnSceneChange(Scene current, Scene next)
    {
        ActivateExtinguisher(false);
    }

    public void ActivateExtinguisher(bool state)
    {
        active = state;
        exclusiveRenderWaterStream.SetActive(state);
        otherPlayerWaterStream.SetActive(state);
        if (state && !waterSfx.IsPlaying()) 
        {
            Debug.Log($"Water sound start");
            waterSfx.Play();
        }
        else if (!state && waterSfx.IsPlaying())
        {
            Debug.Log($"Water sound stop");
            waterSfx.Stop();
        }
    }

    /// <summary>
    /// Update the water streams (exclusive render and other player render) visually
    /// based on raycasts (one for each render type)
    /// </summary>
    private void UpdateWaterStream()
    {
       UpdateWaterStreamHelper(exclusiveRenderWaterStream, exclusiveRenderWaterCylinder, exclusiveRenderEndSpray);
       UpdateWaterStreamHelper(otherPlayerWaterStream, otherPlayerWaterCylinder, otherPlayerEndSpray);
    }

    private void UpdateWaterStreamHelper(GameObject waterStream, GameObject waterCylinder, GameObject endSpray)
    {
        var waterStreamContainerTransform = waterStream.transform.parent;
        
        // Rendering for the water stream
        Ray waterRay = new Ray(waterStreamContainerTransform.position, waterStreamContainerTransform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(waterRay, out hit, reach))
        {
            // shorten water stream
            float dist = hit.distance;
            Vector3 scale = waterCylinder.transform.localScale;
            scale.z = dist * 50; // model is shrunk for some reason

            if (endSpray != null)
            {
                endSpray.transform.position = hit.point + Vector3.down * 0.5f;
                endSpray.SetActive(true);
            }
        }
        else
        {
            Vector3 scale = waterCylinder.transform.localScale;
            scale.z = 250;
            waterCylinder.transform.localScale = scale;

            if (endSpray != null)
                endSpray.SetActive(false);
        }
    }
}
