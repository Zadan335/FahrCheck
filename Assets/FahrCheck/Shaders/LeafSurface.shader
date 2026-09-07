Shader "FahrCheck/LeafSurface" {
 Properties { _Color("Tint",Color)=(1,1,1,1) }
 SubShader { Tags {"RenderType"="Opaque"} LOD 150 Cull Off
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 struct Input { float4 color:COLOR; };
 fixed4 _Color;
 void surf(Input IN,inout SurfaceOutputStandard o){o.Albedo=IN.color.rgb;o.Smoothness=.12;o.Metallic=0;}
 ENDCG
 }
 Fallback "Diffuse"
}
