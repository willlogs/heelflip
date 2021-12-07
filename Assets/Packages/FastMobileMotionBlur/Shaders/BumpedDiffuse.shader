Shader "SupGames/MotionBlur/BumpedDiffuse"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_BumpTex("Normal Map", 2D) = "bump" {}
	}
	SubShader
	{
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
			UNITY_DECLARE_SCREENSPACE_TEXTURE(_BumpTex);
			fixed4 _BumpTex_ST;
			fixed4 _MainTex_ST;
			fixed4 _Color;
			fixed4 _LightColor0;
			fixed4 _SpecColor;

			struct appdata
			{
				fixed4 vertex : POSITION;
				fixed2 uv : TEXCOORD0;
				fixed3 normal : NORMAL;
				fixed4 tangent : TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				fixed4 pos : SV_POSITION;
				fixed4 uv : TEXCOORD0;
				fixed4 normal : TEXCOORD1;
				fixed3 tangent : TEXCOORD2;
				LIGHTING_COORDS(3,4)
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				fixed3 viewDirection = _WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz;
				o.normal.xyz = normalize(mul(fixed4(v.normal, 0.0h), unity_WorldToObject).xyz);
				o.tangent.xyz = normalize(mul(unity_ObjectToWorld, fixed4(v.tangent.xyz, 0.0h)).xyz);
				fixed3 bitangent = cross(o.normal.xyz, o.tangent.xyz) * v.tangent.w * unity_WorldTransformParams.w;
				o.uv.zw = bitangent.xy;
				o.normal.w = bitangent.z;
				o.pos = UnityObjectToClipPos(v.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 encodedNormal = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_BumpTex, _BumpTex_ST.xy * i.uv.xy + _BumpTex_ST.zw);
				fixed3 bitangent = fixed3(i.uv.zw, i.normal.w);
				fixed3 localCoords = UnpackNormal(encodedNormal);
				return fixed4(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv.xy).rgb * (_LightColor0.rgb * max(0.0h, dot(normalize(mul(localCoords, fixed3x3(i.tangent, bitangent, i.normal.xyz))), _WorldSpaceLightPos0.xyz)) * LIGHT_ATTENUATION(i) + UNITY_LIGHTMODEL_AMBIENT.rgb), 0.0h)*_Color;
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