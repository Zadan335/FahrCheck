using UnityEngine;

namespace FahrCheck
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CarController : MonoBehaviour
    {
        public WheelCollider[] wheels;
        public Transform[] wheelVisuals;
        public Renderer[] leftIndicators;
        public Renderer[] rightIndicators;
        public Renderer[] brakeLamps;
        public Material lampOff;
        public Material indicatorOn;
        public Material brakeOn;
        public CourseSession session;
        public float driveTorque = 260f;
        public float brakeTorque = 1800f;
        public float maxSteer = 28f;

        public Rigidbody Body { get; private set; }
        public int Indicator { get; private set; }
        public float SpeedKmh => Body == null ? 0f :
            Vector3.ProjectOnPlane(Body.velocity, Vector3.up).magnitude * 3.6f;
        public float SignedSpeed => Body == null ? 0f : Vector3.Dot(Body.velocity, transform.forward);
        public Vector3 FrontPosition => transform.position + transform.forward * 2.12f;
        public bool Braking { get; private set; }

        public struct Controls { public float throttle, steer; public bool brake, reverse, handbrake; }
        public Controls? AutomatedInput { get; set; }
        public void SetIndicator(int value) { Indicator = Mathf.Clamp(value, -1, 1); }
        float steering;

        void Awake()
        {
            Body = GetComponent<Rigidbody>();
            Body.mass = 1250f;
            Body.centerOfMass = new Vector3(0f, 0.1f, 0f);
            Body.interpolation = RigidbodyInterpolation.Interpolate;
            Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Body.drag = 0.035f;
            Body.angularDrag = 0.8f;
            if (wheels != null && wheels.Length > 0)
                wheels[0].ConfigureVehicleSubsteps(5f, 12, 15);
        }

        void Update()
        {
            if (RuntimeVerification.Active) return;
            if (session == null || !session.IsDriving) return;
            if (Input.GetKeyDown(KeyCode.Q)) Indicator = Indicator == -1 ? 0 : -1;
            if (Input.GetKeyDown(KeyCode.E)) Indicator = Indicator == 1 ? 0 : 1;
        }

        void FixedUpdate()
        {
            if (wheels == null || wheels.Length != 4) return;
            bool active = session != null && session.IsDriving;
            float throttle = active && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) ? 1f : 0f;
            bool brakeKey = active && (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow));
            bool reverseKey = active && Input.GetKey(KeyCode.X);
            bool handbrake = active && Input.GetKey(KeyCode.Space);
            float targetSteer = 0f;
            if (active && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))) targetSteer -= 1f;
            if (active && (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))) targetSteer += 1f;
            if (AutomatedInput.HasValue)
            {
                var input = AutomatedInput.Value;
                throttle = active ? input.throttle : 0;
                targetSteer = active ? input.steer : 0;
                brakeKey = active && input.brake;
                reverseKey = active && input.reverse;
                handbrake = active && input.handbrake;
            }
            steering = Mathf.MoveTowards(steering, targetSteer, Time.fixedDeltaTime * 2.2f);
            float angle = steering * Mathf.Lerp(maxSteer, 10f, Mathf.Clamp01(SpeedKmh / 65f));
            float torque = throttle * driveTorque;
            Braking = brakeKey || handbrake || !active;
            if (reverseKey && throttle == 0f)
            {
                if (SignedSpeed > 0.4f) Braking = true;
                else torque = SpeedKmh < 12f ? -driveTorque * 0.55f : 0f;
            }
            if (throttle > 0f && SignedSpeed < -0.4f) Braking = true;
            if (SpeedKmh > 85f && torque > 0f) torque = 0f;

            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].steerAngle = i < 2 ? angle : 0f;
                wheels[i].motorTorque = Braking ? 0f : torque;
                wheels[i].brakeTorque = Braking ? brakeTorque : (Mathf.Abs(torque) < 0.1f ? 35f : 0f);
            }
            // Aerodynamic load increases grip gradually without locking steering.
            Body.AddForce(-transform.up * Body.velocity.sqrMagnitude * 0.7f);
        }

        void OnCollisionEnter(Collision hit)
        {
            if(session==null || !session.IsDriving) return;
            var actor=hit.collider.GetComponentInParent<CollisionParticipant>();
            bool horizontal=false; foreach(var contact in hit.contacts) if(Mathf.Abs(contact.normal.y)<.65f) horizontal=true;
            if(actor!=null || (horizontal && hit.relativeVelocity.magnitude>.7f)) session.CollisionFailure(actor!=null ? (actor.pedestrian?"a pedestrian":"another vehicle") : hit.collider.name);
        }
        void LateUpdate()
        {
            if (wheels != null && wheelVisuals != null)
                for (int i = 0; i < Mathf.Min(wheels.Length, wheelVisuals.Length); i++)
                {
                    wheels[i].GetWorldPose(out Vector3 position, out Quaternion rotation);
                    wheelVisuals[i].SetPositionAndRotation(position, rotation);
                }
            bool blink = Mathf.Repeat(Time.time, 0.8f) < 0.4f;
            SetLamps(leftIndicators, Indicator == -1 && blink ? indicatorOn : lampOff);
            SetLamps(rightIndicators, Indicator == 1 && blink ? indicatorOn : lampOff);
            SetLamps(brakeLamps, Braking ? brakeOn : lampOff);
        }

        static void SetLamps(Renderer[] lamps, Material material)
        {
            if (lamps == null || material == null) return;
            foreach (Renderer lamp in lamps) if (lamp != null) lamp.sharedMaterial = material;
        }
    }
}
