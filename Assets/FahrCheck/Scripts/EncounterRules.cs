using System;
namespace FahrCheck
{
    // Small, pure evaluators; scenario distances/time margins are gameplay models, not statutory numbers.
    public static class EncounterRules
    {
        public const float PriorityEntryZ = 184f, CrossingEntryZ = 243f, TurnZ = 290f, FinishX = 55f;
        public static bool MustYieldRight(float npcX) { return npcX > -8f && npcX < 22f; }
        public static bool PedestrianHasPriority(float pedestrianX, bool intendsToCross) { return intendsToCross && pedestrianX > -6.5f; }
        public static bool ViolatesYield(float previous, float current, float entry, bool conflict)
        { return conflict && RuleLogic.CrossedForward(previous, current, entry); }
        public static bool MissingTurnIndicator(float heading, int indicator, float heldSeconds)
        { return heading >= 20f && heading < 150f && (indicator != 1 || heldSeconds < 1f); }
        public static bool MissingIndicator(float signedHeading, int direction, int indicator, float heldSeconds)
        { return MissingTurnIndicator(signedHeading * direction, indicator * direction, heldSeconds); }
        public static bool LeftTurnConflict(int direction, float otherZ)
        { return direction == -1 && otherZ > 280 && otherZ < 322; }
        public static bool InMission(float x, float z)
        { return z >= 5f && ((Math.Abs(x) <= 5.3f && z <= 300f) || (Math.Abs(x) <= 65 && z >= 283f && z <= 297f)); }
    }
}
