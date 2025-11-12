// Start phone with FaceID and success when faces player camera - to home screen

using System.Collections;
using UnityEngine;

public class PhoneScreen : MonoBehaviour
{
    public Transform playerCamera;   // assign in Inspector, make angle relative to player view
    private readonly float _maxAngle = 20f;  // tolerance (CHANGEABLE)
    // Note 10f leaves a bit of room, 7f needs some adjusting but not too much
    private bool _faceIDDone;
    private PhoneUIController _phoneUI;

    // Start phone screen on FaceID
    private void Start()
    {
        _phoneUI = GetComponent<PhoneUIController>();
        _phoneUI.ShowFaceID();
    }

    // Check screen angle each frame until FaceID done - front faces player camera within _maxAngle
    private void Update()
    {
        if (_faceIDDone) return;

        // Phone's top face (screen) points along transform.up
        Vector3 toCamera = (playerCamera.position - transform.position).normalized;
        float screenAngle = Vector3.Angle(transform.up, toCamera);

        if (screenAngle < _maxAngle)
        {
            _faceIDDone = true;
            StartCoroutine(FaceAcceptedRoutine());
        }
    }

    IEnumerator FaceAcceptedRoutine()
    {
        _phoneUI.ShowFaceAccepted();

        yield return new WaitForSeconds(4);
        _phoneUI.ShowHome();
        Debug.Log("FaceID success! Phone screen facing camera.");
        Level0TaskManager.CompleteTaskUnlock();
        Level0TaskManager.StartTaskSwipe();
    }
}
