Shader "Custom/Shaderz"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
         [MainTexture] _VolumeMap("Volume Map", 3D) = "" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 positionOS : TEXCOORD1;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE3D(_VolumeMap);
            SAMPLER(sampler_VolumeMap);
            SamplerState clamp_sampler;
            
            
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END
            
            
            float sdfSphere(float3 p,float r)
            {
                return length(p) -r;
            }
            
            float raymarch(float3 rayOrigin,float3 rayDir,float3 cameraOS)
            {
                float t = 0;
                float d = 0;
                float density = 0;
                float result = 0;
                normalize(rayDir);
                
                for (int i = 0; i < 100; ++i)
                {
                    
                   rayOrigin += rayDir * 0.01 ;
                    
                    float sphereDist = distance(rayOrigin,float3(0,0,0));
                   float sampledDensity = SAMPLE_TEXTURE3D(_VolumeMap,sampler_VolumeMap,rayOrigin + float3(.5,.5,.5) ).r;
                  
                       density +=sampledDensity;
                       
                   
                    
                }
                density *= 0.02;
                return density;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionOS = (IN.positionOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 cameraOS = mul(unity_WorldToObject,float4(GetCameraPositionWS(),1.0));
                float3 dir = IN.positionOS - cameraOS;
              float sex = raymarch(IN.positionOS,dir,cameraOS);
                
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                  float ses = sdfSphere(cameraOS,0.5);
           
                half4 col = half4(1,1,1,sex);
             
              
                return col;
            }
            ENDHLSL
        }
    }
}
