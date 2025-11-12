using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class DynamicRenderTexture : MonoBehaviour
{
    public RawImage targetImage; // Assign your RawImage in the inspector
    private Camera cam;
    private RenderTexture rt;
    private int lastW, lastH;

    void Start()
    {
        cam = GetComponent<Camera>();
        CreateRenderTexture();
    }

    void Update()
    {
        if (Screen.width != lastW || Screen.height != lastH)
        {
            CreateRenderTexture();
        }
    }

    void CreateRenderTexture()
    {
        // Release old texture
        if (rt != null)
        {
            rt.Release();
            Destroy(rt);
        }

        // Make new one matching current screen size
        rt = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
        rt.Create();

        // Assign to camera and RawImage
        cam.targetTexture = rt;
        if (targetImage != null)
            targetImage.texture = rt;

        // Remember size
        lastW = Screen.width;
        lastH = Screen.height;
    }
}