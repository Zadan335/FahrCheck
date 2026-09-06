using UnityEngine;
namespace FahrCheck
{
    [DefaultExecutionOrder(50)]
    public sealed class CourseEncounters : MonoBehaviour
    {
        public CourseSession session;
        public Transform priorityCar, pedestrian;
        float priorityStart = -1, pedestrianStart = -1;
        public float PriorityXAt(float time) => priorityStart < 0 ? 26 : Mathf.Max(-28, 26 - Mathf.Max(0, time - priorityStart) * 4);
        public float PedestrianXAt(float time) => pedestrianStart < 0 ? 6.5f : Mathf.Max(-7.5f, 6.5f - Mathf.Max(0, time - pedestrianStart - 2) * 1.3f);
        public bool PedestrianIntent => pedestrianStart >= 0;
        public string Guidance => !session.Mission.Turn ? session.Mission.Brief : session.car.FrontPosition.z < 155 ? "Ahead: signals, then an unsigned junction" : session.car.FrontPosition.z < 210 ? "Unsigned junction: yield to traffic from the right" : session.car.FrontPosition.z < 260 ? "Zebra crossing: allow the pedestrian to cross" : "Turn " + session.TurnName + " at the last junction. Indicate before turning.";
        void FixedUpdate()
        {
            if (!session.IsDriving) return;
            float z = session.car.FrontPosition.z, t = session.ElapsedSeconds;
            if (session.Mission.Priority && priorityStart < 0 && z >= 155) priorityStart = t;
            if (session.Mission.Crossing && pedestrianStart < 0 && z >= 210) pedestrianStart = t;
            priorityCar.GetComponent<Rigidbody>().MovePosition(new Vector3(PriorityXAt(t), .2f, 187));
            pedestrian.GetComponent<Rigidbody>().MovePosition(new Vector3(PedestrianXAt(t), 0, 245));
        }
    }
}
