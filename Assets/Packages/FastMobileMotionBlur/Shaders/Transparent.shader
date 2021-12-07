Shader "SupGames/MotionBlur/Transparent" {
   Properties {
	  _Color("Color", Color) = (1,1,1,1)
      _MainTex ("RGBA Texture Image", 2D) = "white" {} 
      _Cutoff ("Alpha Cutoff", Float) = 0.5
   }
   SubShader {
	  Tags { "RenderType"="Opaque" }
	  LOD 100
      Pass {    
         Cull Off
         CGPROGRAM
 
         #pragma vertex vert  
         #pragma fragment frag 
         #pragma fragmentoption ARB_precision_hint_fastest
 		 #include "UnityCG.cginc"
         UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
		 fixed4 _MainTex_ST;
         fixed _Cutoff;
		 fixed4 _Color;

         struct appdata 
		 {
            fixed4 vertex : POSITION;
            fixed2 texcoord : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
         };
         struct v2f 
		 {
            fixed4 pos : SV_POSITION;
            fixed2 uv : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
         };
 
         v2f vert(appdata i) 
         {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            o.uv.xy = i.texcoord.xy;
            o.pos = UnityObjectToClipPos(i.vertex);
            return o;
         }

         fixed4 frag(v2f i) : COLOR
         {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            fixed4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv.xy);
            if (col.a < _Cutoff)
            {
               discard; 
            }
            col.a=0;
            return col * _Color;
         }
 
         ENDCG
      }
   }
}