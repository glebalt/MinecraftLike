Shader "Custom/NewBlitScriptableRenderPipelineShader"
{
    
    SubShader
    {
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl" // needed to sample scene depth
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl" // needed to sample scene normals
        
        ENDHLSL




        Tags { "RenderType"="Opaque" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            
            
            Name "NewBlitScriptableRenderPipelineShader"

            HLSLPROGRAM
            
            
            #pragma vertex Vert
            #pragma fragment Frag

            float4 Frag (Varyings input) : SV_Target
            {
                    float4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_CameraDepthTexture, input.texcoord).rgba;
                
                float scale =2;
                float halfScaleCeil = ceil(scale * 0.5);
                float halfScaleFloor = floor(scale * 0.5);
                
                float2 bottomleftuv = input.texcoord - float2(_BlitTexture_TexelSize.x,_BlitTexture_TexelSize.y) * halfScaleFloor;
                float2 topRightUv = input.texcoord + float2(_BlitTexture_TexelSize.x,_BlitTexture_TexelSize.y) * halfScaleCeil;
                
                float2 topLeftUv = input.texcoord + float2(- _BlitTexture_TexelSize.x * halfScaleCeil ,_BlitTexture_TexelSize.y *  halfScaleFloor);
                float2 bottomRightUv = input.texcoord + float2( _BlitTexture_TexelSize.x *  halfScaleFloor, - _BlitTexture_TexelSize.y * halfScaleCeil) ;
              
          
          
                float3 normal1 = SampleSceneNormals(bottomleftuv);
                float3 normal2 = SampleSceneNormals(topRightUv);
                float3 normal3 = SampleSceneNormals(bottomleftuv);
                float3 normal4 = SampleSceneNormals(bottomRightUv);
                float3 normalVS1 = TransformWorldToViewDir(normal1);
                float3 normalVS2 = TransformWorldToViewDir(normal2);
                float3 normalVS3 = TransformWorldToViewDir(normal3);
                float3 normalVS4 = TransformWorldToViewDir(normal4);
                float3 normalFiniteDifference0 = normalVS2 - normalVS1;
                float3 normalFiniteDifference1 = normalVS4 - normalVS3;
                
                float4 depth = SampleSceneDepth(input.texcoord);
                float4 depth1 =  SampleSceneDepth(bottomleftuv);
                float4 depth2 =  SampleSceneDepth(topRightUv);
                float4 depth3 =  SampleSceneDepth(bottomRightUv);
                float4 depth4 =  SampleSceneDepth(topLeftUv);
                float depthFiniteDifference0 = depth2 - depth1;
                float depthFiniteDifference1 = depth4 - depth3;
              
             
                float edgedepth = sqrt(pow(depthFiniteDifference0,2) + pow(depthFiniteDifference1,2)) * 1  ;
                edgedepth = edgedepth > 0.3 * depth ? 1 : 0;
       
              float edgeNormStr = sqrt(dot(normalFiniteDifference0,normalFiniteDifference0) + dot(normalFiniteDifference1,normalFiniteDifference1));
              //float depthTreshold =  edgedepth > 1? 1 : 0;
            edgeNormStr = edgeNormStr > 0.4 ? 1 : 0;
                float res = max(edgeNormStr,edgedepth);
                res *= 1000;
                res = smoothstep(0.5,0.7,res);
                float4 finalCol = color + (res);
                float4 finalCol2 = lerp(color,float4(0,20,.2,1),res);
                return finalCol2;
               
            }
            
            ENDHLSL
        }
    }
}
