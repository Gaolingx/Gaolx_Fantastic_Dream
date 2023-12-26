/*
 * StarRailNPRShader - Fan-made shaders for Unity URP attempting to replicate
 * the shading of Honkai: Star Rail.
 * https://github.com/stalomeow/StarRailNPRShader
 *
 * Copyright (C) 2023 Stalo <stalowork@163.com>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

Shader "Honkai Star Rail/Character/Hair"
{
    Properties
    {
        [KeywordEnum(Game, MMD)] _Model("Model Type", Float) = 0
        _ModelScale("Model Scale", Float) = 1

        [HeaderFoldout(Shader Options)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 0                    // 默认 Off
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendAlpha("Src Blend (A)", Float) = 0 // 默认 Zero
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlendAlpha("Dst Blend (A)", Float) = 0 // 默认 Zero
        [Space(5)]
        [Toggle] _AlphaTest("Alpha Test", Float) = 0
        [If(_ALPHATEST_ON)] [Indent] _AlphaTestThreshold("Threshold", Range(0, 1)) = 0.5

        [HeaderFoldout(Maps)]
        [SingleLineTextureNoScaleOffset(_Color)] _MainTex("Albedo", 2D) = "white" {}
        [HideInInspector] _Color("Color", Color) = (1, 1, 1, 1)
        [SingleLineTextureNoScaleOffset] _LightMap("Light Map", 2D) = "white" {}
        [TextureScaleOffset] _Maps_ST("Maps Scale Offset", Vector) = (1, 1, 0, 0)
        [Header(Overrides)] [Space(5)]
        [If(_MODEL_GAME)] _BackColor("Back Face Color", Color) = (1, 1, 1, 1)
        [If(_MODEL_GAME)] [Toggle] _BackFaceUV2("Back Face Use UV2", Float) = 0

        [HeaderFoldout(Diffuse)]
        [RampTexture] _RampMapCool("Ramp Map (Cool)", 2D) = "white" {}
        [RampTexture] _RampMapWarm("Ramp Map (Warm)", 2D) = "white" {}
        _RampCoolWarmLerpFactor("Cool / Warm", Range(0, 1)) = 1

        [HeaderFoldout(Specular)]
        _SpecularColor0("Color", Color) = (1,1,1,1)
        _SpecularShininess0("Shininess", Range(0.1, 500)) = 10
        _SpecularIntensity0("Intensity", Range(0, 100)) = 1
        _SpecularEdgeSoftness0("Edge Softness", Range(0, 1)) = 0.1

        [HeaderFoldout(Emission, Use Albedo.a as emission map)]
        _EmissionColor("Color", Color) = (1, 1, 1, 1)
        _EmissionThreshold("Threshold", Range(0, 1)) = 1
        _EmissionIntensity("Intensity", Float) = 0

        [HeaderFoldout(Bloom)]
        _BloomIntensity0("Intensity", Range(0, 2)) = 0.5

        [HeaderFoldout(Rim Light)]
        _RimIntensity("Intensity (Front)", Range(0, 1)) = 1
        _RimIntensityBackFace("Intensity (Back)", Range(0, 1)) = 0
        _RimThresholdMin("Threshold Min", Float) = 0.6
        _RimThresholdMax("Threshold Max", Float) = 0.9
        _RimEdgeSoftness("Edge Softness", Float) = 0.05
        _RimWidth0("Width", Range(0, 1)) = 0.5
        _RimColor0("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _RimDark0("Darken Value", Range(0, 1)) = 0.5

        [HeaderFoldout(Outline)]
        [KeywordEnum(Tangent, Normal)] _OutlineNormal("Normal Source", Float) = 0
        _OutlineWidth("Width", Range(0,4)) = 1
        _OutlineZOffset("Z Offset", Float) = 0
        _OutlineColor0("Color", Color) = (0, 0, 0, 1)

        [HeaderFoldout(Eye Hair Blend)]
        _HairBlendAlpha("Hair Alpha", Range(0, 1)) = 0.6

        [HeaderFoldout(Dither)]
        _DitherAlpha("Alpha", Range(0, 1)) = 1

        // Head Bone
        [HideInInspector] _MMDHeadBoneForward("MMD Head Bone Forward", Vector) = (0, 0, 1, 0)
        [HideInInspector] _MMDHeadBoneUp("MMD Head Bone Up", Vector) = (0, 1, 0, 0)
        [HideInInspector] _MMDHeadBoneRight("MMD Head Bone Right", Vector) = (1, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "UniversalMaterialType" = "Lit"
            "Queue" = "Geometry+20"  // 必须在脸和眼睛之后绘制
        }

        Pass
        {
            Name "HairOpaque"

            Tags
            {
                "LightMode" = "HSRForward2"
            }

            // 没有遮住眼睛的部分
            Stencil
            {
                Ref 7
                ReadMask 1   // 眼睛位
                WriteMask 4  // 头发位
                Comp NotEqual
                Pass Replace // 写入头发位
                Fail Keep
            }

            Cull [_Cull]
            ZWrite On

            Blend 0 One Zero, [_SrcBlendAlpha] [_DstBlendAlpha]
            Blend 1 One Zero

            ColorMask RGBA 0
            ColorMask R 1

            HLSLPROGRAM

            #pragma vertex HairVertex
            #pragma fragment HairOpaqueFragment

            #pragma shader_feature_local _MODEL_GAME _MODEL_MMD
            #pragma shader_feature_local_fragment _ _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _BACKFACEUV2_ON

            #include "CharHairCore.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "HairFakeTransparent"

            Tags
            {
                "LightMode" = "HSRForward3"
            }

            // 遮住眼睛的部分
            Stencil
            {
                Ref 7
                ReadMask 1   // 眼睛位
                WriteMask 4  // 头发位
                Comp Equal
                Pass Replace // 写入头发位
                Fail Keep
            }

            // 这个 pass 画的是刘海，Back Face 一般情况下看不见
            // 把 Back Face 剔除掉，避免 alpha 混合时和 Front Face 叠加导致颜色错误
            Cull Back // [_Cull]
            ZWrite On

            Blend 0 SrcAlpha OneMinusSrcAlpha, [_SrcBlendAlpha] [_DstBlendAlpha]
            Blend 1 One Zero

            ColorMask RGBA 0
            ColorMask R 1

            HLSLPROGRAM

            #pragma vertex HairVertex
            #pragma fragment HairFakeTransparentFragment

            #pragma shader_feature_local _MODEL_GAME _MODEL_MMD
            #pragma shader_feature_local_fragment _ _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _BACKFACEUV2_ON

            #include "CharHairCore.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "HairOutline"

            Tags
            {
                "LightMode" = "HSROutline"
            }

            Cull Front
            ZTest LEqual
            ZWrite On

            ColorMask RGB 0
            ColorMask 0 1

            HLSLPROGRAM

            #pragma vertex HairOutlineVertex
            #pragma fragment HairOutlineFragment

            #pragma shader_feature_local _MODEL_GAME _MODEL_MMD
            #pragma shader_feature_local_fragment _ _ALPHATEST_ON

            #pragma shader_feature_local_vertex _OUTLINENORMAL_TANGENT _OUTLINENORMAL_NORMAL

            #include "CharHairCore.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "HairShadow"

            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            Cull [_Cull]
            ZWrite On
            ZTest LEqual

            ColorMask 0 0
            ColorMask 0 1

            HLSLPROGRAM

            #pragma target 2.0

            #pragma vertex HairShadowVertex
            #pragma fragment HairShadowFragment

            #pragma shader_feature_local _MODEL_GAME _MODEL_MMD
            #pragma shader_feature_local_fragment _ _ALPHATEST_ON

            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "CharHairCore.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "HairDepthOnly"

            Tags
            {
                "LightMode" = "DepthOnly"
            }

            Cull [_Cull]
            ZWrite On
            ColorMask 0

            HLSLPROGRAM

            #pragma vertex HairDepthOnlyVertex
            #pragma fragment HairDepthOnlyFragment

            #pragma shader_feature_local _MODEL_GAME _MODEL_MMD
            #pragma shader_feature_local_fragment _ _ALPHATEST_ON

            #include "CharHairCore.hlsl"

            ENDHLSL
        }
    }

    CustomEditor "StaloSRPShaderGUI"
    Fallback Off
}
