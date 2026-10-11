using UnityEngine;

namespace ADW.Vehicle
{
    [RequireComponent(typeof(Rigidbody), typeof(CarInput))]
    public class CarController : MonoBehaviour
    {
        public WheelCollider fl, fr, rl, rr;
        public Transform flMesh, frMesh, rlMesh, rrMesh;

        public float motorTorque = 700f, brakeTorque = 3000f, handbrakeTorque = 5000f;
        public float coastBrake = 80f, topSpeedKmh = 170f;
        public float maxSteerLow = 32f, maxSteerHigh = 12f;
        [Range(0f, 1f)] public float assist = 0.5f;

        public float gripStiffness = 2f, handbrakeStiffness = 0.8f, kickStiffness = 1f;
        public float driftStiffnessMax = 1.5f, driftStiffnessMin = 1.1f;
        public float kickImpulse = 900f;
        public float antiRoll = 6000f;
        public float downforce = 2f;
        public Vector3 centerOfMass = new Vector3(0f, -0.45f, 0.1f);

        public float SpeedKmh { get; private set; }
        public float ForwardSpeed { get; private set; }
        public float SlipAngle { get; private set; }

        Rigidbody rb;
        CarInput inp;
        float rearStiffness, kickTimer, kickCooldown;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            inp = GetComponent<CarInput>();
            rb.centerOfMass = centerOfMass;
            rearStiffness = gripStiffness;
            SetStiffness(fl, fr, gripStiffness);
        }

        void FixedUpdate()
        {
            Vector3 v = rb.linearVelocity;
            SpeedKmh = v.magnitude * 3.6f;
            ForwardSpeed = Vector3.Dot(v, transform.forward);
            Vector3 flat = new Vector3(v.x, 0f, v.z);
            SlipAngle = (flat.magnitude > 3f && ForwardSpeed > 0f)
                ? Vector3.SignedAngle(transform.forward, flat, Vector3.up) : 0f;

            Steer();
            Drive();
            Grip();
            AntiRoll(fl, fr);
            AntiRoll(rl, rr);
            rb.AddForce(-transform.up * downforce * v.sqrMagnitude);
        }

        void Steer()
        {
            float limit = Mathf.Lerp(maxSteerLow, maxSteerHigh, Mathf.InverseLerp(30f, 150f, SpeedKmh));
            float counter = 0f;
            if (assist > 0f && ForwardSpeed > 5f && Mathf.Abs(SlipAngle) > 5f)
                counter = Mathf.Clamp(SlipAngle / 30f, -1f, 1f) * assist;
            float s = Mathf.Clamp(inp.Steer + counter, -1f, 1f);
            fl.steerAngle = fr.steerAngle = s * limit;
        }

        void Drive()
        {
            float t = inp.Throttle, b = inp.Brake;
            float motor = 0f, brake = 0f;
            if (b > 0.1f && ForwardSpeed > 1f) brake = b * brakeTorque;
            else if (b > 0.1f) motor = -b * motorTorque * 0.5f;
            else if (t > 0.05f) motor = SpeedKmh < topSpeedKmh ? t * motorTorque : 0f;
            else brake = coastBrake;

            rl.motorTorque = rr.motorTorque = motor;
            fl.brakeTorque = fr.brakeTorque = brake;
            float rear = brake + (inp.Handbrake ? handbrakeTorque : 0f);
            rl.brakeTorque = rr.brakeTorque = rear;
        }

        void Grip()
        {
            float dt = Time.fixedDeltaTime;
            if (kickCooldown > 0f) kickCooldown -= dt;
            if (inp.Kick && kickCooldown <= 0f && inp.Throttle > 0.6f && SpeedKmh > 40f)
            {
                kickTimer = 0.4f;
                kickCooldown = 0.8f;
                float dir = Mathf.Abs(inp.Steer) < 0.1f ? 1f : Mathf.Sign(inp.Steer);
                rb.AddTorque(transform.up * dir * kickImpulse, ForceMode.Impulse);
            }
            inp.Kick = false;

            float target = gripStiffness;
            if (kickTimer > 0f) { kickTimer -= dt; target = kickStiffness; }
            if (inp.Handbrake) target = handbrakeStiffness;
            else if (kickTimer <= 0f && Mathf.Abs(SlipAngle) > 15f && inp.Throttle > 0.2f)
                target = Mathf.Lerp(driftStiffnessMax, driftStiffnessMin,
                    Mathf.InverseLerp(15f, 45f, Mathf.Abs(SlipAngle)));

            float rate = target < rearStiffness ? 20f : 3f;
            rearStiffness = Mathf.MoveTowards(rearStiffness, target, rate * dt);
            SetStiffness(rl, rr, rearStiffness);
        }

        static void SetStiffness(WheelCollider a, WheelCollider b, float s)
        {
            var f = a.sidewaysFriction; f.stiffness = s; a.sidewaysFriction = f;
            f = b.sidewaysFriction; f.stiffness = s; b.sidewaysFriction = f;
        }

        void AntiRoll(WheelCollider l, WheelCollider r)
        {
            float tl = 1f, tr = 1f;
            bool gl = l.GetGroundHit(out WheelHit hl);
            bool gr = r.GetGroundHit(out WheelHit hr);
            if (gl) tl = (-l.transform.InverseTransformPoint(hl.point).y - l.radius) / l.suspensionDistance;
            if (gr) tr = (-r.transform.InverseTransformPoint(hr.point).y - r.radius) / r.suspensionDistance;
            float force = (tl - tr) * antiRoll;
            if (gl) rb.AddForceAtPosition(l.transform.up * -force, l.transform.position);
            if (gr) rb.AddForceAtPosition(r.transform.up * force, r.transform.position);
        }

        void LateUpdate()
        {
            Sync(fl, flMesh); Sync(fr, frMesh); Sync(rl, rlMesh); Sync(rr, rrMesh);
        }

        static void Sync(WheelCollider c, Transform m)
        {
            if (m == null) return;
            c.GetWorldPose(out Vector3 p, out Quaternion q);
            m.SetPositionAndRotation(p, q);
        }
    }
}