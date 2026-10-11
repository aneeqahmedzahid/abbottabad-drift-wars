using UnityEngine;
using UnityEngine.InputSystem;

namespace ADW.Vehicle
{
    public class CarInput : MonoBehaviour
    {
        public bool AIControlled;
        public float Steer, Throttle, Brake;
        public bool Handbrake, Kick;
        public float steerSpeed = 5f;

        void Update()
        {
            if (AIControlled) return;
            var k = Keyboard.current;
            var g = Gamepad.current;
            float s = 0f, t = 0f, b = 0f;
            bool hb = false;
            if (k != null)
            {
                s += (k.dKey.isPressed ? 1f : 0f) - (k.aKey.isPressed ? 1f : 0f);
                t = k.wKey.isPressed ? 1f : 0f;
                b = k.sKey.isPressed ? 1f : 0f;
                hb |= k.spaceKey.isPressed;
                if (k.leftShiftKey.wasPressedThisFrame) Kick = true;
            }
            if (g != null)
            {
                s += g.leftStick.x.ReadValue();
                t = Mathf.Max(t, g.rightTrigger.ReadValue());
                b = Mathf.Max(b, g.leftTrigger.ReadValue());
                hb |= g.buttonEast.isPressed;
                if (g.rightShoulder.wasPressedThisFrame) Kick = true;
            }
            Steer = Mathf.MoveTowards(Steer, Mathf.Clamp(s, -1f, 1f), steerSpeed * Time.deltaTime);
            Throttle = t;
            Brake = b;
            Handbrake = hb;
        }
    }
}