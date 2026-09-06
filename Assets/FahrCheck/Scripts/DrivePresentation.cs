using UnityEngine;
namespace FahrCheck
{
    [RequireComponent(typeof(Camera))]
    public sealed class DrivePresentation : MonoBehaviour
    {
        public Material grade;
        void OnRenderImage(RenderTexture source,RenderTexture destination)
        { if(grade!=null)Graphics.Blit(source,destination,grade);else Graphics.Blit(source,destination); }
    }
}
