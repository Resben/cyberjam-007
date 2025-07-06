Shader "Hidden/R3DWorks/URPDithering"{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float _Intensity;
        int _PixelSize;
        int _ColorRange;

        static const float4x4 ditherMatrix4x4 = {
             0.0f / 16.0f,  8.0f / 16.0f,  2.0f / 16.0f, 10.0f / 16.0f,
            12.0f / 16.0f,  4.0f / 16.0f, 14.0f / 16.0f,  6.0f / 16.0f,
             3.0f / 16.0f, 11.0f / 16.0f,  1.0f / 16.0f,  9.0f / 16.0f,
            15.0f / 16.0f,  7.0f / 16.0f, 13.0f / 16.0f,  5.0f / 16.0f
        };

        static const float ditherMatrix8x8[8][8] = {
            {  0.0/64.0, 32.0/64.0,  8.0/64.0, 40.0/64.0,  2.0/64.0, 34.0/64.0, 10.0/64.0, 42.0/64.0 },
            { 48.0/64.0, 16.0/64.0, 56.0/64.0, 24.0/64.0, 50.0/64.0, 18.0/64.0, 58.0/64.0, 26.0/64.0 },
            { 12.0/64.0, 44.0/64.0,  4.0/64.0, 36.0/64.0, 14.0/64.0, 46.0/64.0,  6.0/64.0, 38.0/64.0 },
            { 60.0/64.0, 28.0/64.0, 52.0/64.0, 20.0/64.0, 62.0/64.0, 30.0/64.0, 54.0/64.0, 22.0/64.0 },
            {  3.0/64.0, 35.0/64.0, 11.0/64.0, 43.0/64.0,  1.0/64.0, 33.0/64.0,  9.0/64.0, 41.0/64.0 },
            { 51.0/64.0, 19.0/64.0, 59.0/64.0, 27.0/64.0, 49.0/64.0, 17.0/64.0, 57.0/64.0, 25.0/64.0 },
            { 15.0/64.0, 47.0/64.0,  7.0/64.0, 39.0/64.0, 13.0/64.0, 45.0/64.0,  5.0/64.0, 37.0/64.0 },
            { 63.0/64.0, 31.0/64.0, 55.0/64.0, 23.0/64.0, 61.0/64.0, 29.0/64.0, 53.0/64.0, 21.0/64.0 }
        };
        
        float4 DitheringFrag(Varyings input) : SV_Target{
            float2 ps = float2(_PixelSize, _PixelSize);
            float2 halfTexel = _BlitTexture_TexelSize.xy / 2.0;
            float2 size = 1.0 / _BlitTexture_TexelSize.xy;
            float2 sizeDown = size / ps;
            float2 uv = input.texcoord;
            float2 pixelUV = uv / _BlitTexture_TexelSize.xy;
            uv = floor(uv * sizeDown) / sizeDown + halfTexel;
            int2 coord = int2(floor(pixelUV / ps)) % 8;
            float threshold = ditherMatrix8x8[coord.y][coord.x];
            float3 color_orig = SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, input.texcoord).rgb;
            float3 color_pixelated = SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, uv).rgb;
            float3 color = color_pixelated;
            float cm = float(_ColorRange);
            float3 c0 = floor(color_pixelated * cm) / cm;
            float3 c1 = ceil(color_pixelated * cm) / cm;

            color.r = frac(color.r*cm) < threshold ? c0.r : c1.r;
            color.g = frac(color.g*cm) < threshold ? c0.g : c1.g;
            color.b = frac(color.b*cm) < threshold ? c0.b : c1.b;
            return float4(lerp(color_orig, color, _Intensity), 1.0);
        }
    ENDHLSL

    SubShader{
        Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        Pass{
            Name "DitheringPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment DitheringFrag
            ENDHLSL
        }
    }
}
