using UnityEngine;

namespace FahrCheck
{
    public sealed class DrivingCamera : MonoBehaviour
    {
        public Transform target;
        public CourseSession session;
        public bool cockpit;
        Vector3 smoothVelocity;

        void Start() { Snap(); }
        void Update()
        {
            if (RuntimeVerification.Active) return;
            if (Input.GetKeyDown(KeyCode.C)) Toggle();
        }
        public void Toggle() { cockpit = !cockpit; Snap(); }
        void Snap()
        {
            if (target == null) return;
            transform.position = target.TransformPoint(cockpit ? new Vector3(-0.43f, 1.45f, 0.25f) : new Vector3(0f, 3.5f, -7.5f));
            smoothVelocity = Vector3.zero;
            transform.rotation = Quaternion.LookRotation(target.position + target.forward * 10f + Vector3.up * 1.1f - transform.position);
        }
        void LateUpdate()
        {
            if (target == null || session == null || !session.IsDriving) return;
            Vector3 offset = cockpit ? new Vector3(-0.43f, 1.45f, 0.25f) : new Vector3(0f, 3.5f, -7.5f);
            Vector3 position = target.TransformPoint(offset);
            transform.position = cockpit ? position : Vector3.SmoothDamp(transform.position, position, ref smoothVelocity, 0.12f);
            Vector3 lookPoint = target.position + target.forward * 12f + Vector3.up * 1.15f;
            Quaternion rotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
            transform.rotation = cockpit ? rotation : Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 9f);
        }
    }
}
