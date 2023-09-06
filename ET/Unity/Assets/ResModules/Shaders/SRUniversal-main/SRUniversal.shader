Shader "Custom/SRUniversal"
{
    Properties
    {
        [KeywordEnum (None, Face, Hair, UpperBody, LowerBody)] _Area("Material area", float) = 0
        [HideInInspector] _HeadForward("", Vector) = (0,0,1)
        [HideInInspector] _HeadRight("", Vector) = (1,0,0)

        [Header (Base Color)]
        [HideinInspector] _BaseMap ("", 2D) = "white" {}
        [NoScaleOffset] _FaceColorMap ("Face color map (Default white)", 2D) = "white" {}
        [HDR] _FaceColorMapColor("Face color map color (Default white)",Color) = (1,1,1)
        [NoScaleOffset] _HairColorMap ("Hair color map (Default white)", 2D) = "white" {}
        [HDR] _HairColorMapColor("Hair color map color (Default white)",Color) = (1,1,1)
        [NoScaleOffset] _UpperBodyColorMap ("Upper body color map (Default white)", 2D) = "white" {}
        [HDR] _UpperBodyColorMapColor("Upper body color map color (Default white)",Color) = (1,1,1)
        [NoScaleOffset] _LowerBodyColorMap ("Lower body color map (Default white)", 2D) = "white" {}
        [HDR] _LowerBodyColorMapColor("Lower body color map color (Default white)",Color) = (1,1,1)
        _FrontFaceTintColor("Front face tint color (Default white)",Color) = (1,1,1)
        _BackFaceTintColor("Back face tint color (Default white)",Color) = (1,1,1)
        _Alpha("Alpha (Default 1)", Range(0,1)) = 1
        _AlphaClip("Alpha clip (Default 0.333)", Range(0,1)) = 0.333

        [Header(Light Map)]
        [NoScaleOffset] _HairLightMap("Hair light map (Default black)",2D) = "black" {}
        [NoScaleOffset] _UpperBodyLightMap("Upper body map (Default black)",2D) = "black" {}
        [NoScaleOffset] _LowerBodyLightMap("Lower body map (Default black)",2D) = "black" {}

        [Header(Ramp Map)]
        [NoScaleOffset] _HairCoolRamp("Hair cool ramp (Default white)",2D) = "white" {}
        _HairCoolRampColor("Hair cool ramp color (Default white)",Color) = (1,1,1)
        [NoScaleOffset] _HairWarmRamp("Hair warm ramp (Default white)",2D) = "white" {}
        _HairWarmRampColor("Hair warm ramp color (Default white)",Color) = (1,1,1)
        [NoScaleOffset] _BodyCoolRamp("Body cool ramp (Default white)",2D) = "white" {}
        _BodyCoolRampColor("Body cool ramp color (Default white)",Color) = (1,1,1)
        [NoScaleOffset] _BodyWarmRamp("Body warm ramp (Default white)",2D) = "white" {}
        _BodyWarmRampColor("Body warm ramp color (Default white)",Color) = (1,1,1)
        [Toggle(_ISDAY_MANUAL_ON)] _isDayManualON("Use Lerp coolRamp & warmRamp Manual (Default NO)", float ) = 0
        _isDay("Lerp coolRamp & warmRamp value (Default 1)",Range(0,1)) = 0.5

        [Header(Indirect Lighting)]
        _IndirectLightFlattenNormal("Indirect light flatten normal (Default 0)",Range(0,1)) = 0
        _IndirectLightUsage("Indirect light usage (Default 0.5)",Range(0,1)) = 0.5
        _IndirectLightOcclusionUsage("Indirect light occlusion usage (Default 0.5)",Range(0,1)) = 0.5
        _IndirectLightMixBaseColor("Indirect light mix base color (Default 1)",Range(0,1)) = 1

        [Header(Main Lighting)]
        _MainLightColorUsage("Main light color usage (Default 1)",Range(0,1)) = 1
        _ShadowThresholdCenter("Shadow threshold center (Default 0)",Range(-1,1)) = 0
        _ShadowThresholdSoftness("Shadow threshold softness (Default 0.1)",Range(0,1)) = 0.1
        _ShadowRampOffset("Shadow ramp offset (Default 0.75)",Range(0,1)) = 0.75

        [Header(Face)]
        [NoScaleOffset] _FaceMap("Face map (Default black)",2D) = "black" {}
        _FaceShadowOffset("Face shadow offset (Default -0.01)",Range(-1,1)) = -0.01
        _FaceShadowTransitionSoftness("Face shadow transition softness (Default 0.05)", Range(0,1)) = 0.05

        [Header(Specular)]
        [Toggle(_SPECULAR_ON)] _EnableSpecular ("Enable Specular (Default YES)", float) = 1
        [Toggle(_METAL_SPECULAR_ON)] _EnableMetalSpecular ("Enable Metal Specular (Default YES)", float) = 1
        _SpecularExpon("Specular exponent (Default 50)",Range(0,100)) = 50
        _SpecularKsNonMrtal("Specular KS non-metal (Default 0.04)",Range(0,1)) = 0.04
        _SpecularKsMetal("Specular KS metal (Default 1)",Range(0,1)) = 1
        [Toggle(_SPECULAR_COLOR_CUSTOM)] _EnableCustomSpecularColor ("Enable Custom Specular Color (Default NO)", float) = 1
        _SpecularColor("Stockings dark color (Default white)",Color) = (1,1,1)
        _SpecularBrightness("Specular brightness (Default 1)",Range(0,10)) = 10

        [Header(Stockings)]
        [Toggle(_STOCKINGS_ON)] _UseStockings("Use Stockings (Default NO)",float) = 0
        _UpperBodyStockings("Upper body stockings (Default black)",2D) = "black" {}
        _LowerBodyStockings("Lower body stockings (Default black)",2D) = "black" {}
        _StockingsDarkColor("Stockings dark color (Default black)",Color) = (0,0,0)
        [HDR] _StockingsLightColor("Stockings light color (Default 1.8, 1.48299, 0.856821)",Color) = (1.8, 1.48299, 0.856821)
        _StockingsTransitionColor("Stockings transition color (Default 0.360381, 0.242986, 0.358131)",Color) = (0.360381, 0.242986, 0.358131)
        _StockingsTransitionThreshold("Stockings transition Threshold (Default 0.58)",Range(0,1)) = 0.58
        _StockingsTransitionPower("Stockings transition power (Default 1)",Range(0,50)) = 1
        _StockingsTransitionHardness("Stockings transition hardness (Default 0.4)",Range(0,1)) = 0.4
        _StockingsTextureUsage("Stockings texture usage (Default 0.1)",Range(0,1)) = 0.1

        [Header(Rim Lighting)]
        [Toggle(_RIM_LIGHTING_ON)] _UseRimLight("Use Rim light (Default YES)",float) = 1
        _RimLightWidth("Rim light width (Default 1)",Range(0, 10)) = 1
        _RimLightThreshold("Rin light threshold (Default 0.05)",Range(-1, 1)) = 0.05
        _RimLightFadeout("Rim light fadeout (Default 1)",Range(0.01, 1)) = 1
        [HDR] _RimLightTintColor("Rim light tint colar (Default white)",Color) = (1,1,1)
        _RimLightBrightness("Rim light brightness (Default 1)",Range(0, 10)) = 1
        _RimLightMixAlbedo("Rim light mix albedo (Default 0.9)",Range(0, 1)) = 0.9

        [Header(Bloom)]
        _BloomIntensity0("Intensity", Range(0, 2)) = 0.5

        [Header(Emission)]
        [Toggle(_EMISSION_ON)] _UseEmission("Use emission (Default NO)",float) = 0
        _EmissionMixBaseColor("Emission mix base color (Default 1)", Range(0,1)) = 1
        _EmissionTintColor("Emission tint color (Default white)", Color) = (1,1,1) 
        _EmissionIntensity("Emission intensity (Default 1)", Range(0,100)) = 1

        [Header(Outline)]
        [Toggle(_OUTLINE_ON)] _UseOutline("Use outline (Default YES)", float ) = 1
        [Toggle(_USE_RAMP_COLOR_ON)] _UseRampColor("Use Ramp Color (Default YES)", float ) = 1
        _OutlineColor("OutlineColor (Without Ramp Texture)", Color) = (0.5, 0.5, 0.5, 1)
        [Toggle(_OUTLINE_VERTEX_COLOR_SMOOTH_NORMAL)] _OutlineUseVertexColorSmoothNormal("Use vertex color smooth normal (Default NO)", float) = 0
        _OutlineWidth("Outline width (Default 1)", Range(0,10)) = 1
        _OutlineGamma("Outline gamma (Default 16)", Range(1,255)) = 16
        [Toggle(_FAKE_OUTLINE_ON)] _UseFakeOutline("Use face fake outline (Default YES)", float ) = 1

        [Header(Surface Options)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull (Default back)", Float) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendMode ("Cull (Default back)", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlendMode ("Cull (Default back)", Float) = 0
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp ("Cull (Default back)", Float) = 0
        [Enum(Off,0, On,1)] _ZWrite("ZWrite (Default On)",Float) = 1
        _StencilRef ("Stencil reference (Default 0)",Range(0,255)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil comparison (Default disabled)",Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPassOp("Stencil pass comparison (Default keep)",Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFailOp("Stencil fail comparison (Default keep)",Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFailOp("Stencil z fail comparison (Default keep)",Int) = 0

        [Header(Draw Overlay)]
        [Toggle(_DRAW_OVERLAY_ON)] _UseDrawOverlay("Use draw overlay (Default NO)",float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _ScrBlendModeOverlay("Overlay pass scr blend mode (Default One)",Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlendModeOverlay("Overlay pass dst blend mode (Default Zero)", Float) = 0
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOpOverlay("Overlay pass blend operation (Default Add)", Float) = 0
        _StencilRefOverlay ("Overlay pass stencil reference (Default 0)", Range(0,255)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilCompOverlay("Overlay pass stencil comparison (Default disabled)",Int) = 0

    }
    SubShader
    {
        LOD 100

        HLSLINCLUDE
        #pragma shader_feature_local _AREA_FACE
        #pragma shader_feature_local _AREA_HAIR
        #pragma shader_feature_local _AREA_UPPERBODY
        #pragma shader_feature_local _AREA_LOWERBODY
        #pragma shader_feature_local _ISDAY_MANUAL_ON
        #pragma shader_feature_local _SPECULAR_ON
        #pragma shader_feature_local _METAL_SPECULAR_ON
        #pragma shader_feature_local _SPECULAR_COLOR_CUSTOM
        #pragma shader_feature_local _STOCKINGS_ON
        #pragma shader_feature_local _RIM_LIGHTING_ON
        #pragma shader_feature_local _OUTLINE_ON
        #pragma shader_feature_local _FAKE_OUTLINE_ON
        #pragma shader_feature_local _USE_RAMP_COLOR_ON
        #pragma shader_feature_local _OUTLINE_VERTEX_COLOR_SMOOTH_NORMAL
        #pragma shader_feature_local _DRAW_OVERLAY_ON
        #pragma shader_feature_local _EMISSION_ON

        ENDHLSL

        Pass
        {
            Name "DrawCore"
            Tags
            {
                "RenderPipeline" = "UniversalPipeline"
                "RenderType" = "Opaque"
                "LightMode" = "HSRForward2"
            }
            Cull[_Cull]
            Stencil{
                Ref [_StencilRef]
                Comp [_StencilComp]
                Pass [_StencilPassOp]
                Fail [_StencilFailOp]
                ZFail [_StencilZFailOp]
            }
            Blend [_SrcBlendMode] [_DstBlendMode]
            BlendOp [_BlendOp]
            ZWrite [_ZWrite]

            HLSLPROGRAM
            #pragma multi_compile _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _SHADOWS_SOFT

            #pragma vertex SRUniversalVertex
            #pragma fragment SRUniversalFragment

            #pragma multi_compile_fog

            #include "SRUniversalInput.hlsl"
            #include "SRUniversalDrawCorePass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DrawOverlay"
            Tags
            {
                "RenderPipeline" = "UniversalPipeline"
                "RenderType" = "Opaque"
                "LightMode" = "HSRForward3"
            }
            Cull[_Cull]
            Stencil{
                Ref [_StencilRefOverlay]
                Comp [_StencilCompOverlay]
            }
            Blend [_ScrBlendModeOverlay] [_DstBlendModeOverlay]
            BlendOp [_BlendOpOverlay]
            ZWrite [_ZWrite]

            HLSLPROGRAM
            #pragma multi_compile _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _SHADOWS_SOFT

            #pragma vertex SRUniversalVertex
            #pragma fragment SRUniversalFragment

            #pragma multi_compile_fog

            #if _DRAW_OVERLAY_ON
                #include "SRUniversalInput.hlsl"
                #include "SRUniversalDrawCorePass.hlsl"
            #else
                struct Attributes {};
                struct Varyings
                {
                    float4 positionCS : SV_POSITION;
                };
                Varyings SRUniversalVertex(Attributes input)
                {
                    return (Varyings)0;
                }
                float4 SRUniversalFragment(Varyings input) : SV_TARGET
                {
                    return 0;
                }
            #endif

            ENDHLSL
        }
        
        Pass 
        {
            Name "DrawOutline"
            Tags 
            {
                "RenderPipeline" = "UniversalPipeline"
                "RenderType" = "Opaque"
                "LightMode" = "HSROutline"

            }

            Cull Front // Cull Front is a must for extra pass outline method
            ZWrite [_ZWrite]

            HLSLPROGRAM

            // Direct copy all keywords from "ForwardLit" pass
            // ---------------------------------------------------------------------------------------------
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            // ---------------------------------------------------------------------------------------------
            #pragma multi_compile_fog
            // ---------------------------------------------------------------------------------------------

            #pragma vertex SRUniversalVertex
            #pragma fragment SRUniversalFragment

            #if _OUTLINE_ON

                // all shader logic written inside this .hlsl, remember to write all #define BEFORE writing #include
                #include "SRUniversalInput.hlsl"
                #include "SRUniversalDrawOutline.hlsl"
            #else
                struct Attributes {};
                struct Varyings
                {
                    float4 positionCS : SV_POSITION;
                };
                Varyings SRUniversalVertex(Attributes input)
                {
                    return (Varyings)0;
                }
                float4 SRUniversalFragment(Varyings input) : SV_TARGET
                {
                    return 0;
                }
            #endif

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite [_ZWrite]
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            // Material keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            
            #pragma vertex ShadowPassVertex // 和後面的include 有關係
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite [_ZWrite]
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex DepthOnlyVertex // 和後面的include 有關係
            #pragma fragment DepthOnlyFragment

            // Material keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}

            ZWrite [_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex DepthNormalsVertex // 和後面的include 有關係
            #pragma fragment DepthNormalsFragment

            // Material keywords
            #pragma shader_feature_local _NORMALMAP 
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
            ENDHLSL
        }

    }
    FallBack "Diffuse"
}
