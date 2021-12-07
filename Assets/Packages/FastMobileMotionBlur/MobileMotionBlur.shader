Shader "SupGames/Mobile/MobileMotionBlur"
{
	Properties
	{
		[HideInInspector]_MainTex("Texture", 2D) = "white" {}
	}

	CGINCLUDE

#include "UnityCG.cginc"
#if defined(TEN) 
#define MULTIPLAYER 0.1h
#elif defined(EIGHT)
#define MULTIPLAYER 0.125h
#else
#define MULTIPLAYER 0.16666667h
#endif

	fixed _Distance;
	fixed4x4 _CurrentToPreviousViewProjectionMatrix;

	UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
	UNITY_DECLARE_SCREENSPACE_TEXTURE(_BlurTex);
	fixed4 _MainTex_ST;
	fixed4 _BlurTex_ST;

	struct appdata {
		fixed4 pos : POSITION;
		fixed2 uv : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};	

	struct v2f
	{
		fixed4 pos : POSITION;
		fixed2 uv : TEXCOORD0;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	struct v2fb
	{
		fixed4 pos : POSITION;
		fixed4 uv : TEXCOORD0;
		fixed4 uv1 : TEXCOORD1;
		fixed4 uv2 : TEXCOORD2;
#if defined(EIGHT)
		fixed4 uv3 : TEXCOORD3;
#endif
#if defined(TEN)
		fixed4 uv4 : TEXCOORD4;
#endif
		UNITY_VERTEX_OUTPUT_STEREO
	};

	v2f vert(appdata i)
	{
		v2f o;
		UNITY_SETUP_INSTANCE_ID(i);
		UNITY_INITIALIZE_OUTPUT(v2f, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.pos = UnityObjectToClipPos(i.pos);
		o.uv = i.uv;
		return o;
	}

	v2fb vertb(appdata i)
	{
		v2fb o;
		UNITY_SETUP_INSTANCE_ID(i);
		UNITY_INITIALIZE_OUTPUT(v2fb, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.pos = UnityObjectToClipPos(i.pos);
		fixed4 projPos = fixed4(i.uv * 2.0h - 1.0h, _Distance, 1.0h);
		fixed4 previous = mul(_CurrentToPreviousViewProjectionMatrix, projPos);
		previous /= previous.w;
		fixed2 vel = (previous.xy - projPos.xy)*MULTIPLAYER*0.5h;
		o.uv.xy = i.uv;
		o.uv.zw = vel;
		o.uv1.xy = vel * 2.0h;
		o.uv1.zw = vel * 3.0h;
		o.uv2.xy = vel * 4.0h;
		o.uv2.zw = vel * 5.0h;
#if defined(EIGHT)
		o.uv3.xy = vel * 6.0h;
		o.uv3.zw = vel * 7.0h;
#endif
#if defined(TEN)
		o.uv4.xy = vel * 8.0h;
		o.uv4.zw = vel * 9.0h;
#endif
		return o;
	}

	fixed4 fragb(v2fb i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		fixed2 uv = UnityStereoTransformScreenSpaceTex(i.uv);
		fixed4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv);
		fixed col1A = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv.zw).a;
		fixed col2A = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv1.xy).a;
		fixed col3A = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv1.zw).a;
		fixed col4A = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv2.xy).a;
		fixed col5A = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv2.zw).a;
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv.zw * col1A);
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv1.xy * col2A);
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv1.zw * col3A);
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv2.xy * col4A);
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv2.zw * col5A);

#if defined(EIGHT)
		fixed col6A = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv3.xy).a;
		fixed col7A = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv3.zw).a;
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv3.zw * col6A);
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv3.xy * col7A);
#endif
#if defined(TEN)
		fixed col8A = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv4.xy).a;
		fixed col9A = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv4.zw).a;
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv4.zw * col8A);
		col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + i.uv4.xy * col9A);
#endif
		return col * MULTIPLAYER;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		fixed4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, UnityStereoTransformScreenSpaceTex(i.uv));
		fixed4 blur = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_BlurTex, UnityStereoTransformScreenSpaceTex(i.uv));
		return lerp(col,blur,blur.a);
	}

	ENDCG

	SubShader
	{
		ZTest Always Cull Off ZWrite Off
		Fog{ Mode off }
		Pass //0
		{
			CGPROGRAM
			#pragma shader_feature EIGHT
			#pragma shader_feature TEN
			#pragma vertex vertb
			#pragma fragment fragb
			#pragma fragmentoption ARB_precision_hint_fastest 
			ENDCG
		}
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }
			Pass //1
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			ENDCG
		}
	}
}