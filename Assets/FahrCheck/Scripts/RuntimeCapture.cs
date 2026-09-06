using System;
using System.IO;
using System.Text;
using UnityEngine;
namespace FahrCheck
{
    public static class RuntimeCapture
    {
        public static void Save(string name)
        {
            string[] args=Environment.GetCommandLineArgs(); int option=Array.IndexOf(args,"-fahrcheckArtifacts");
            if(option<0 || option+1>=args.Length) return;
            Directory.CreateDirectory(args[option+1]);
            Camera camera=Camera.main; var target=new RenderTexture(960,600,24); var image=new Texture2D(960,600,TextureFormat.RGB24,false);
            var oldTarget=camera.targetTexture; var oldActive=RenderTexture.active;
            try
            {
                camera.targetTexture=target; camera.Render(); RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,960,600),0,0); image.Apply();
                var pixels=image.GetPixels32(); var data=new byte[960*600*3]; int at=0;
                for(int y=599;y>=0;y--) for(int x=0;x<960;x++) { var c=pixels[y*960+x]; data[at++]=c.r; data[at++]=c.g; data[at++]=c.b; }
                using(var stream=File.Create(Path.Combine(args[option+1],name+".ppm")))
                { byte[] header=Encoding.ASCII.GetBytes("P6\n960 600\n255\n"); stream.Write(header,0,header.Length); stream.Write(data,0,data.Length); }
            }
            finally { camera.targetTexture=oldTarget; RenderTexture.active=oldActive; UnityEngine.Object.Destroy(image); target.Release(); UnityEngine.Object.Destroy(target); }
        }
    }
}
