using UnityEngine;
namespace FahrCheck
{
    public static class TrafficSafety
    {
        public static bool ObstacleAhead(float laneX,float carZ,int direction,float obstacleX,float obstacleZ,float gap)
        {
            float forwardGap=(obstacleZ-carZ)*direction;
            return Mathf.Abs(obstacleX-laneX)<2f&&forwardGap>0&&forwardGap<gap;
        }
    }
}
