Shader "Hidden/FahrCheck/DriveGrade"
{
 Properties { _MainTex("Scene",2D)="white" {} }
 SubShader { Cull Off ZWrite Off ZTest Always
 Pass { CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex; float4 _MainTex_TexelSize;
 fixed4 frag(v2f_img i):SV_Target
 {
     float3 c=tex2D(_MainTex,i.uv).rgb;
     float3 n=tex2D(_MainTex,i.uv+float2(0,_MainTex_TexelSize.y)).rgb;
     float3 s=tex2D(_MainTex,i.uv-float2(0,_MainTex_TexelSize.y)).rgb;
     float3 e=tex2D(_MainTex,i.uv+float2(_MainTex_TexelSize.x,0)).rgb;
     float3 w=tex2D(_MainTex,i.uv-float2(_MainTex_TexelSize.x,0)).rgb;
     float edge=length(n-s)+length(e-w); c=lerp(c,(n+s+e+w+c*4)/8,saturate(edge-.25)*.35);
     c=max(0,(c-.18)*1.055+.18); c*=float3(1.015,1.005,.985);
     float2 p=i.uv-.5; c*=1-dot(p,p)*.24;
     return float4(saturate(c),1);
 }
 ENDCG }
 }
}
