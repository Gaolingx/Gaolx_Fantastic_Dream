// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Luoyinan/Scene/SceneStandard" 
{
	Properties
	{
		_Color ("Main Color", Color) = (1, 1, 1, 1)
		_MainTex("Main Texture RGB(Albedo) A(Gloss & Alpha)", 2D) = "white" {}
		_NormalTex("Normal Texture", 2D) = "bump" {}
		_GlossTex ("Gloss Texture", 2D) = "white" {}

		_HalfLambert("Half Lambert", Range (0.5, 1)) = 0.75

		_SpecularIntensity("Specular Intensity", Range (0, 2)) = 0
		_SpecularSharp("Specular Sharp",Float) = 32
		_SpecularLuminanceMask("Specular Luminance Mask", Range (0, 2)) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Background" 		
			"RenderType" = "Opaque" // 支持渲染到_CameraDepthNormalsTexture
		}

		Pass
		{
			Lighting Off
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			#pragma shader_feature _NORMAL_MAP
			#pragma shader_feature _LUMINANCE_MASK_ON

			#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
			#pragma multi_compile __ _FADING_ON
			#pragma multi_compile __ _POINT_LIGHT
			#pragma multi_compile __ _FANCY_STUFF
		
			struct appdata_lightmap 
			{
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
				half2 texcoord1 : TEXCOORD1;
#if _FANCY_STUFF
				half3 normal : NORMAL;
	#if _NORMAL_MAP
				half4 tangent : TANGENT;
	#endif
#endif 
			};

			// SM2.0的texture interpolator只有8个,要合理规划.
			struct v2f 
			{
				float4 pos : SV_POSITION;
				half2 uv0 : TEXCOORD0;
#ifndef LIGHTMAP_OFF 
				half2 uv1 : TEXCOORD1;
#endif
				UNITY_FOG_COORDS(2)
				float3 posWorld : TEXCOORD3;
#if _FANCY_STUFF
				half3 normalWorld : TEXCOORD4;
	#if _NORMAL_MAP
				half3 tangentWorld : TEXCOORD5;
				half3 binormalWorld : TEXCOORD6;
	#endif
#endif 
			};
		
			fixed4 _Color;
			sampler2D _MainTex;
			half4 _MainTex_ST;

#if _POINT_LIGHT
			float4 _GlobalPointLightPos;
			fixed4 _GlobalPointLightColor;
			fixed _GlobalPointLightRange;
#endif 

#ifndef LIGHTMAP_OFF
	#if _FADING_ON
			sampler2D _GlobalLightMap;
			fixed _GlobalFadingFactor;
	#endif
#endif

#if _FANCY_STUFF
			sampler2D _GlossTex;
			fixed _HalfLambert;

			fixed _SpecularIntensity;
			fixed _SpecularSharp;
			half4 _GlobalMainLightDir;
			fixed4 _GlobalMainLightColor;
			half4 _GlobalBackLightDir;
			fixed4 _GlobalBackLightColor;

	#if _LUMINANCE_MASK_ON
			fixed _SpecularLuminanceMask;
	#endif

	#if _NORMAL_MAP
			uniform sampler2D _NormalTex;
			half4 _NormalTex_ST;
	#endif
#endif 

			v2f vert(appdata_lightmap i)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(i.vertex);
				o.uv0 = TRANSFORM_TEX(i.texcoord, _MainTex);
#ifndef LIGHTMAP_OFF 
				o.uv1 = i.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#endif
				o.posWorld = mul(unity_ObjectToWorld, i.vertex).xyz;
#if _FANCY_STUFF
				o.normalWorld = UnityObjectToWorldNormal(i.normal);
	#if _NORMAL_MAP
				o.tangentWorld = UnityObjectToWorldDir(i.tangent);
				o.binormalWorld = cross(o.normalWorld, o.tangentWorld) * i.tangent.w;
	#endif
#endif 
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}
		
			fixed4 frag(v2f i) : COLOR
			{			
				fixed4 mainColor = tex2D(_MainTex, i.uv0);
				fixed alpha = mainColor.a; 
				fixed4 finalColor = mainColor * _Color;

				// lightmap
#ifndef LIGHTMAP_OFF 
				fixed3 lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv1));
	#if _FADING_ON
				fixed3 lm_fading = DecodeLightmap(UNITY_SAMPLE_TEX2D(_GlobalLightMap, i.uv1));
				lm = lerp(lm, lm_fading, _GlobalFadingFactor);			
	#endif
	#if _FANCY_STUFF && _LUMINANCE_MASK_ON
				half lumin = saturate(Luminance(lm) * _SpecularLuminanceMask);
	#endif
				finalColor.rgb *= lm;	
#endif				

#if _FANCY_STUFF			
				// gloss
				alpha *= tex2D(_GlossTex, i.uv0).r;

				// normalmap
	#if _NORMAL_MAP
				fixed3x3 tangentToWorld = fixed3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
				half3 normalMap = UnpackNormal(tex2D(_NormalTex, i.uv0));
				half3 fixedNormal = normalize(mul(normalMap, tangentToWorld));
	#else
				half3 fixedNormal = normalize(i.normalWorld);
	#endif

				// main light diffuse
	#if _NORMAL_MAP || LIGHTMAP_OFF 
				half nl = dot(fixedNormal, normalize(_GlobalMainLightDir.xyz));
				half diff = saturate(nl) * (1 - _HalfLambert) + _HalfLambert; 
				finalColor *= diff;
	#endif	
		
	#if _NORMAL_MAP 
				// main light specular
				half3 viewDir = normalize(_WorldSpaceCameraPos - i.posWorld);
				half3 h = normalize(normalize(_GlobalMainLightDir.xyz) + viewDir);
				half nh = saturate(dot(fixedNormal, h));
				nh = pow(nh, _SpecularSharp) * _SpecularIntensity;
		#if _LUMINANCE_MASK_ON && LIGHTMAP_ON
				finalColor.rgb += _GlobalMainLightColor.rgb * nh * alpha * _GlobalMainLightColor.a * lumin;
		#else
				finalColor.rgb += _GlobalMainLightColor.rgb * nh * alpha * _GlobalMainLightColor.a;
		#endif
		
				// back light specular
				h = normalize(normalize(_GlobalBackLightDir.xyz) + viewDir);
				nh = saturate(dot(fixedNormal, h));
				nh = pow(nh, _SpecularSharp) * _SpecularIntensity;
		#if _LUMINANCE_MASK_ON && LIGHTMAP_ON
				finalColor.rgb += _GlobalBackLightColor.rgb * nh * alpha * _GlobalBackLightColor.a * lumin;
		#else
				finalColor.rgb += _GlobalBackLightColor.rgb * nh * alpha * _GlobalBackLightColor.a;
		#endif

	#endif

	#if _POINT_LIGHT			
				half3 toLight = _GlobalPointLightPos.xyz - i.posWorld ;
				half ratio = saturate(length(toLight) / _GlobalPointLightRange);
				//half attenuation = 1 - ratio; // linear attenuation
				ratio *= ratio;
				half attenuation = 1.0 / (1.0 + 0.01 * ratio) * (1 - ratio); // quadratic attenuation
				if (attenuation > 0) // performance
				{
					// point light diffuse
					toLight = normalize(toLight);
					half intensity = 8;
					half nl2 = max(0, dot(fixedNormal, toLight));
					finalColor.rgb += mainColor.rgb * _GlobalPointLightColor.rgb * nl2 * attenuation * intensity;

					// point light specular
		#if _NORMAL_MAP 
					h = normalize(toLight + viewDir);
					nh = saturate(dot(fixedNormal, h));
					nh = pow(nh, _SpecularSharp) * _SpecularIntensity;
					intensity *= _GlobalPointLightColor.a;
					finalColor.rgb += _GlobalPointLightColor.rgb * nh * alpha * attenuation * intensity;
		#endif
				}	
	#endif

#endif 
				UNITY_APPLY_FOG(i.fogCoord, finalColor);

				// 没有高光贴图,alpha默认为0,便于处理Bloom的Alpha Gloss
#if _NORMAL_MAP
				finalColor.a = alpha;
#else
				finalColor.a = 0;
#endif
				return  finalColor;
			}
			ENDCG
		}

		// 没用Unity自带的阴影,只是用来来渲染_CameraDepthsTexture.
		Pass
		{
			Tags { "LightMode" = "ShadowCaster" }

			Fog { Mode Off }
			ZWrite On 
			Offset 1, 1

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			struct v2f
			{
				V2F_SHADOW_CASTER;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER(o)
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				SHADOW_CASTER_FRAGMENT(i)
			}

			ENDCG
		}
	}

	Fallback off
	CustomEditor "SceneStandard_ShaderGUI"
}
