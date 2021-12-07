Shader "SupGames/MotionBlur/Unlit"
{
	Properties
	{
		_Color("Specular Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Tags {"LightMode" = "ForwardBase"}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			
			#include "UnityCG.cginc"
			UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
			fixed4 _MainTex_ST;
			fixed4 _Color;


			struct appdata
			{
				fixed4 vertex : POSITION;
				fixed2 uv: TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct v2f
			{
				fixed4 vertex : SV_POSITION;
				fixed2 uv: TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert (appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.uv.xy=TRANSFORM_TEX(i.uv,_MainTex);
				o.vertex = UnityObjectToClipPos(i.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				return fixed4(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv.xy).rgb, 0.0h)*_Color;
			}
			ENDCG
		}
	}
}
