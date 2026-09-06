using System;

namespace FahrCheck
{
    public enum SignalAspect { Red, RedYellow, Green, Yellow }

    // Plain C# rules, separate from scene geometry and presentation.
    // The first course covers posted speed limits and red/red-yellow stop signals.
    public static class RuleLogic
    {
        public const float StartZ = 18f;
        public const float LimitStartZ = 65f;
        public const float LimitEndZ = 215f;
        public const float StopLineZ = 127f;
        public const float FinishZ = 330f;
        public const float RedSeconds = 9f;
        public const float RedYellowSeconds = 1f;
        public const float GreenSeconds = 12f;
        public const float YellowSeconds = 3f;
        public const float CycleSeconds = 25f;

        // Numerical epsilon only; not a representation of enforcement tolerances.
        public const float SpeedEpsilonKmh = 0.05f;

        public static int SpeedLimitAt(float leadingZ)
        {
            return leadingZ >= LimitStartZ && leadingZ < LimitEndZ ? 30 : 50;
        }

        public static bool IsSpeeding(float speedKmh, int limit)
        {
            return speedKmh > limit + SpeedEpsilonKmh;
        }

        public static SignalAspect AspectAt(float elapsedSeconds)
        {
            float t = Math.Max(0f, elapsedSeconds) % CycleSeconds;
            if (t < RedSeconds) return SignalAspect.Red;
            if (t < RedSeconds + RedYellowSeconds) return SignalAspect.RedYellow;
            if (t < RedSeconds + RedYellowSeconds + GreenSeconds) return SignalAspect.Green;
            return SignalAspect.Yellow;
        }

        public static bool RequiresStop(SignalAspect aspect)
        {
            return aspect == SignalAspect.Red || aspect == SignalAspect.RedYellow;
        }

        public static bool CrossedForward(float previous, float current, float boundary)
        {
            return previous < boundary && current >= boundary;
        }

        public static float CrossingFraction(float previous, float current, float boundary)
        {
            if (current <= previous) return 0f;
            return Math.Max(0f, Math.Min(1f, (boundary - previous) / (current - previous)));
        }

        public static bool ViolatesSignal(float previousZ, float currentZ,
            float previousTime, float currentTime)
        {
            if (!CrossedForward(previousZ, currentZ, StopLineZ)) return false;
            float fraction = CrossingFraction(previousZ, currentZ, StopLineZ);
            float timeAtCrossing = previousTime + (currentTime - previousTime) * fraction;
            return RequiresStop(AspectAt(timeAtCrossing));
        }
    }
}
