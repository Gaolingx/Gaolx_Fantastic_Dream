Shader "URP/NormalMapInWorldSpace"
{
    Properties
    {
        _Color ("Color Tint", Color) = (1, 1, 1, 1)
        _MainTex ("Main Tex", 2D) = "white" { }
        _BumpMap ("Normal Map", 2D) = "bump" { }
        _BumpScale ("Bump Scale", Float) = 1.0
        _Specular ("Specular", Color) = (1, 1, 1, 1)
        _Gloss ("Gloss", Range(8.0, 256)) = 20
    }
    SubShader
    {
        //      指定渲染通道使用URP渲染管线
        Tags { "RenderPipeline" = "UniversalRenderPipeline" }
        Pass
        {
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            // 引用URP函数库
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
            
            // 声明纹理
            TEXTURE2D(_MainTex);
            // 声明采样器
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            
            // 为了确保UnityShader可以兼容SRP批处理
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            // 纹理缩放平移系数
            float4 _MainTex_ST;
            float4 _BumpMap_ST;
            float _BumpScale;
            half4 _Specular;
            float _Gloss;
            CBUFFER_END
            
            struct a2v
            {
                float4 vertex: POSITION;
                float3 normal: NORMAL;
                float4 tangent: TANGENT;
                float4 texcoord: TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos: SV_POSITION;
                float4 uv: TEXCOORD0;
                float4 TtoW0: TEXCOORD1;
                float4 TtoW1: TEXCOORD2;
                float4 TtoW2: TEXCOORD3;
            };
            
            v2f vert(a2v v)
            {
                v2f o;
                // 初始化变量
                ZERO_INITIALIZE(v2f, o);
                
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                
                // 使用纹理的属性_ST对顶点纹理和法线纹理坐标进行变换
                // o.uv.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                // o.uv.zw = v.texcoord.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;
                o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.uv.zw = TRANSFORM_TEX(v.texcoord, _BumpMap);
                // 或者直接调用内置函数
                // o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                
                // 计算世界坐标下的顶点，法线，切线，副法线
                float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
                half3 worldNormal = TransformObjectToWorldNormal(v.normal);
                half3 worldTangent = TransformObjectToWorldDir(v.tangent.xyz);
                half3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;
                
                // 计算从切线空间到世界空间的方向变换矩阵
                // 按列摆放得到从切线转世界空间的变换矩阵
                o.TtoW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                o.TtoW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                o.TtoW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                
                return o;
            }
            
            half4 frag(v2f i): SV_Target
            {
                // 得到世界空间中的位置
                float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
                // 计算世界空间中的光照和视角方向
                half3 lightDir = normalize(TransformObjectToWorldDir(_MainLightPosition.xyz));
                half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);
                
                // 采样得到切线空间的法线纹理
                half3 bump = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.uv.zw));
                bump.xy *= _BumpScale;
                bump.z = sqrt(1.0 - saturate(dot(bump.xy, bump.xy)));
                // 将切线空间转换为世界空间
                bump = normalize(half3(dot(i.TtoW0.xyz, bump), dot(i.TtoW1.xyz, bump), dot(i.TtoW2.xyz, bump)));
                
                // 材质的漫反射系数，使用纹理来采样漫反射的颜色
                half3 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy).rgb * _Color.rgb;
                
                // 环境光
                half3 ambient = _GlossyEnvironmentColor * albedo;
                
                // 漫反射光=入射光线强度*材质的漫反射系数*取值为正数(表面法线方向 · 光源方向)
                half3 diffuse = _MainLightColor.rgb * albedo * max(0, dot(bump, lightDir));
                
                // 半角方向
                half3 halfDir = normalize(lightDir + viewDir);
                // BlinnPhong高光反射
                half3 specular = _MainLightColor.rgb * _Specular.rgb * pow(max(0, dot(bump, halfDir)), _Gloss);
                
                return half4(ambient + diffuse + specular, 1.0);
            }
            
            ENDHLSL
            
        }
    }
    FallBack "Packages/com.unity.render-pipelines.universal/FallbackError"
}