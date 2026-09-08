using UnityEngine;
namespace FahrCheck
{
    // Visual rolling follows actual actor travel, including stops and reverse.
    public sealed class TrafficWheelMotion : MonoBehaviour
    {
        public Transform[] wheels;
        public float[] radii;
        public float surfaceHeight;
        public float TravelledDistance { get; private set; }
        Vector3 previous;
        void OnEnable() { previous = transform.position; }
        void LateUpdate()
        {
            Vector3 delta = transform.position - previous;
            previous = transform.position;
            if (Time.deltaTime <= 0 || delta.sqrMagnitude > 25) return; // Ignore route wraps/retries.
            float distance = Vector3.Dot(delta, transform.forward);
            TravelledDistance += Mathf.Abs(distance);
            for (int i = 0; i < wheels.Length; i++)
                wheels[i].Rotate(Vector3.right, distance / Mathf.Max(.01f, radii[i]) * Mathf.Rad2Deg, Space.Self);
        }
    }
}
