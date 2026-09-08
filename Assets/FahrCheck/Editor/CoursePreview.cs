using System.IO;
using System.Text;
using UnityEngine;
namespace FahrCheck.Editor
{
    public static class CoursePreview
    {
        public static void Capture()
        {
            var camera = Object.FindObjectOfType<Camera>();
            Save(camera, "Logs/chase.ppm");
            var position = camera.transform.position; var rotation = camera.transform.rotation;
            camera.transform.position = new Vector3(85,120,110);
            camera.transform.LookAt(new Vector3(5,0,190));
            Save(camera, "Logs/course.ppm");
            camera.transform.position=new Vector3(10,1.65f,241);
            camera.transform.LookAt(new Vector3(6.5f,.95f,245));
            Save(camera,"Logs/academy-pedestrian.ppm");
            var car=Object.FindObjectOfType<CarController>();
            camera.transform.position=car.transform.TransformPoint(Object.FindObjectOfType<DrivingCamera>().cockpitOffset);
            camera.transform.LookAt(car.transform.position+car.transform.forward*12+Vector3.up*1.15f);
            Save(camera,"Logs/realism-cockpit.ppm");
            var highway=Object.FindObjectOfType<HighwayTraffic>();
            if(highway!=null){camera.transform.position=new Vector3(350,72,105);camera.transform.LookAt(new Vector3(300,0,310));Save(camera,"Logs/autobahn-course.ppm");camera.transform.position=new Vector3(312,3.4f,-9);camera.transform.LookAt(new Vector3(312,4.2f,250));Save(camera,"Logs/autobahn-mountains.ppm");}
            camera.transform.SetPositionAndRotation(position,rotation);
        }
        static void Save(Camera camera, string path)
        {
            var target = new RenderTexture(960,600,24);
            var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active;
            var pixels = new Texture2D(960,600,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0,0,960,600),0,0); pixels.Apply();
                var colors = pixels.GetPixels32(); var bytes = new byte[960*600*3]; int index=0;
                for(int y=599;y>=0;y--) for(int x=0;x<960;x++) { var c=colors[y*960+x]; bytes[index++]=c.r; bytes[index++]=c.g; bytes[index++]=c.b; }
                using(var stream = File.Create(Path.Combine(Application.dataPath,"../"+path)))
                { var header=Encoding.ASCII.GetBytes("P6\n960 600\n255\n"); stream.Write(header,0,header.Length); stream.Write(bytes,0,bytes.Length); }
            }
            finally { camera.targetTexture=oldTarget; RenderTexture.active=oldActive; Object.DestroyImmediate(pixels); target.Release(); Object.DestroyImmediate(target); }
        }
    }
}
