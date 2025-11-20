using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAspectRatio : MonoBehaviour
{
    // Default  16:9
    [SerializeField] private float aspectRatio = 1.7777777777f;

    public void Awake()
    {
        GetComponent<Camera>().aspect = aspectRatio;
    }
}
