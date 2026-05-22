using UnityEngine;
using UnityEngine.InputSystem;

public class GoKartInputTester : MonoBehaviour
{
    private GoKartController kart;

    void Start()
    {
        kart = GetComponent<GoKartController>();
    }

    void Update()
    {
        if (Gamepad.current == null)
            return;

        Vector2 move = Gamepad.current.leftStick.ReadValue();
        Vector2 steer = Gamepad.current.rightStick.ReadValue();

        float steering = steer.x;   // left/right
        float throttle = move.y;   // forward/back

        kart.ChangeDirection(steering);
        kart.ChangeSpeed(throttle);
    }
}
