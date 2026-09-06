using UnityEngine;
namespace FahrCheck
{
    public sealed class MissionGate : MonoBehaviour
    {
        public int direction=1;
        public void Configure(MissionDefinition mission,int selectedTurn)
        {
            gameObject.SetActive(mission.Turn ? direction==selectedTurn : direction==1);
            transform.SetPositionAndRotation(mission.Turn ? new Vector3(direction*55,0,290) : new Vector3(0,0,mission.Finish), Quaternion.Euler(0,mission.Turn?direction*90:0,0));
        }
    }
}
