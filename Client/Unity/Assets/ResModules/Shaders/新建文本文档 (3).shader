Shader "URP_normal_01"
{
    Properties
    {
        _BaseColor("Base Color",color) = (1,1,1,1)
        [MainTexture]_BaseMap("BaseMap", 2D) = "white" {}
        
        //法线相关------------
        [Normal]_NormalMap("NormalMap",2D)="bump"{}
    }
 
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
 
        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
 
 
            struct Attributes
            {
                float4        : POSITION;
                float2 uv               : TEXCOORD0;
                //法线相关------------
                half3 normalOS          : NORMAL;
                half4 tangentOS         : TANGENT;
            };
 
            struct Varyings
            {
                float4 positionCS       : SV_POSITION;
                float2 uv               : TEXCOORD0;
                //法线相关------------
                half4 normalWS          : TEXCOORD1;
                half4 tangentWS         : TEXCOORD2;
                half4 bitangentWS       : TEXCOORD3;
            };
 
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            float4 _BaseMap_ST;
            CBUFFER_END
            TEXTURE2D (_BaseMap);SAMPLER(sampler_BaseMap);
            //法线相关------------
            TEXTURE2D (_NormalMap);SAMPLER(sampler_NormalMap);
            
 
            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
 
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);

                //法线相关------------
                o.normalWS.xyz=TransformObjectToWorldNormal(v.normalOS);
                o.tangentWS.xyz=TransformObjectToWorldDir(v.tangentOS.xyz);
                half sign=v.tangentOS.w*GetOddNegativeScale();
                o.bitangentWS.xyz=cross(o.normalWS,o.tangentWS)*sign;

                return o;
            }
 
            half4 frag(Varyings i) : SV_Target
            {
                half4 c;
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                c = baseMap * _BaseColor;

                //法线相关------------
                half3 normalMap=UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,i.uv));
                half3 normalWS=mul(normalMap,half3x3(i.tangentWS.xyz,i.bitangentWS.xyz,i.normalWS.xyz));

                Light mainLight=GetMainLight();
                half3 L=mainLight.direction;
                half NdotL=saturate(dot(normalWS,L));
                c*=NdotL;
                return c;
            }
            ENDHLSL
        }
    }
}

