using UnityEngine;

namespace FahrCheck
{
    public sealed class TrafficSignal : MonoBehaviour
    {
        public CourseSession session;
        public Renderer redLamp;
        public Renderer yellowLamp;
        public Renderer greenLamp;
        public Material redOn;
        public Material yellowOn;
        public Material greenOn;
        public Material off;
        public SignalAspect Aspect => RuleLogic.AspectAt(session.ElapsedSeconds);

        void Update()
        {
            SignalAspect aspect = Aspect;
            redLamp.sharedMaterial = aspect == SignalAspect.Red || aspect == SignalAspect.RedYellow ? redOn : off;
            yellowLamp.sharedMaterial = aspect == SignalAspect.Yellow || aspect == SignalAspect.RedYellow ? yellowOn : off;
            greenLamp.sharedMaterial = aspect == SignalAspect.Green ? greenOn : off;
        }
    }
}
