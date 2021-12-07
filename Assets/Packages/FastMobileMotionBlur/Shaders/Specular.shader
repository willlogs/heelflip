Shader "SupGames/MotionBlur/Specular" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_SpecColor("Specular Color", Color) = (1,1,1,1)
		_Glossiness("Glossiness", Range(0.01,100)) = 0.03
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 150
		Pass 
		{
			Tags { "LightMode" = "ForwardBase" }
			CGPROGRAM

			#pragma vertex vert  
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc" 
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			#include "AutoLight.cginc"
			UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
			fixed _Glossiness;
			fixed4 _MainTex_ST;
			fixed4 _Color;
			fixed4 _LightColor0;
			fixed4 _SpecColor;

			struct appdata 
			{
				fixed4 vertex : POSITION;
				fixed3 normal : NORMAL;
				fixed2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct v2f {
				fixed4 pos : SV_POSITION;
				fixed4 uv : TEXCOORD0;
				fixed4 diff : TEXCOORD1;
				LIGHTING_COORDS(2, 3)
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				fixed3 normalDirection = normalize(mul(fixed4(v.normal, 0.0h), unity_WorldToObject).xyz);
				fixed3 diff = UNITY_LIGHTMODEL_AMBIENT.rgb + _LightColor0.rgb * max(0.0h, dot(normalDirection, _WorldSpaceLightPos0.xyz));
				o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv.zw = diff.xy;
				o.diff.x = diff.z;
				o.diff.yzw = _LightColor0.rgb * _SpecColor.rgb * pow(saturate(dot(normalDirection, normalize(_WorldSpaceLightPos0.xyz + _WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz))), _Glossiness);
				o.pos = UnityObjectToClipPos(v.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv.xy);
				fixed3 diff = fixed3(i.uv.zw, i.diff.x);
				return fixed4(i.diff.yzw * col.a + diff * col.rgb * LIGHT_ATTENUATION(i), 0.0h)*_Color;
			}
			ENDCG
		}
		Pass
		{
			Tags {"LightMode" = "ShadowCaster"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}