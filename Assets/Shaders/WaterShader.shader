Shader "Custom/WaterShader"
{
    Properties
    {
        _ShallowColor ("Shallow Color", Color) = (0.3, 0.5, 0.7, 0.8)
        _DeepColor ("Deep Color", Color) = (0.1, 0.2, 0.4, 0.9)
        _ColorTransitionDepth ("Color Transition Depth", Range(0, 10)) = 5
        _Transparency ("Transparency", Range(0, 1)) = 0.8
        _Reflectivity ("Reflectivity", Range(0, 1)) = 0.5
        _WaveSpeed ("Wave Speed", Range(0, 2)) = 1
        _WaveScale ("Wave Scale", Range(0, 2)) = 1
        _WaveHeight ("Wave Height", Range(0, 2)) = 0.5
        _RippleStrength ("Ripple Strength", Range(0, 1)) = 0.2
        _RippleSpeed ("Ripple Speed", Range(0, 5)) = 2
        _FlowDirection ("Flow Direction", Vector) = (1, 0, 0, 0)
        _FlowSpeed ("Flow Speed", Range(0, 2)) = 1
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _CubeMap ("Cube Map", CUBE) = "" {}
        _FoamTexture ("Foam Texture", 2D) = "white" {}
        _FoamAmount ("Foam Amount", Range(0, 2)) = 1
        _FoamSpeed ("Foam Speed", Range(0, 2)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        GrabPass { "_GrabTexture" }

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade vertex:vert
        #pragma target 3.0

        sampler2D _GrabTexture;
        sampler2D _NormalMap;
        sampler2D _FoamTexture;
        samplerCUBE _CubeMap;

        struct Input
        {
            float2 uv_NormalMap;
            float2 uv_FoamTexture;
            float4 screenPos;
            float3 worldRefl;
            float3 viewDir;
            float eyeDepth;
            INTERNAL_DATA
        };

        fixed4 _ShallowColor;
        fixed4 _DeepColor;
        float _ColorTransitionDepth;
        float _Transparency;
        float _Reflectivity;
        float _WaveSpeed;
        float _WaveScale;
        float _WaveHeight;
        float _RippleStrength;
        float _RippleSpeed;
        float4 _FlowDirection;
        float _FlowSpeed;
        float _FoamAmount;
        float _FoamSpeed;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            COMPUTE_EYEDEPTH(o.eyeDepth);

            // Apply wave animation
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            float time = _Time.y * _WaveSpeed;
            
            // Generate waves using multiple sine waves
            float wave1 = sin(time + worldPos.x * _WaveScale) * _WaveHeight;
            float wave2 = sin(time * 0.8 + worldPos.z * _WaveScale * 1.2) * _WaveHeight * 0.5;
            float wave3 = sin(time * 1.2 + (worldPos.x + worldPos.z) * _WaveScale * 0.8) * _WaveHeight * 0.3;
            
            v.vertex.y += wave1 + wave2 + wave3;

            // Calculate normal adjustment for waves
            float3 worldNormal = UnityObjectToWorldNormal(v.normal);
            float3 tangent = normalize(cross(worldNormal, float3(0, 0, 1)));
            float3 binormal = normalize(cross(worldNormal, tangent));
            
            float dwave1 = cos(time + worldPos.x * _WaveScale) * _WaveScale * _WaveHeight;
            float dwave2 = cos(time * 0.8 + worldPos.z * _WaveScale * 1.2) * _WaveScale * _WaveHeight * 0.5;
            float dwave3 = cos(time * 1.2 + (worldPos.x + worldPos.z) * _WaveScale * 0.8) * _WaveScale * _WaveHeight * 0.3;
            
            float3 waveNormal = normalize(float3(-dwave1 - dwave3, 1, -dwave2 - dwave3));
            v.normal = normalize(mul(unity_WorldToObject, float4(waveNormal, 0)).xyz);
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            // Calculate water depth
            float depth = IN.eyeDepth / _ColorTransitionDepth;
            
            // Sample normal maps with flow animation
            float2 flowOffset = _FlowDirection.xy * _Time.y * _FlowSpeed;
            float3 normal1 = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap + flowOffset));
            float3 normal2 = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap * 1.4 - flowOffset * 1.1));
            float3 finalNormal = normalize(normal1 + normal2);

            // Apply ripple effect
            float2 rippleOffset = sin(_Time.y * _RippleSpeed + IN.uv_NormalMap * 10) * _RippleStrength;
            finalNormal.xy += rippleOffset;
            o.Normal = finalNormal;

            // Calculate reflection
            float3 reflection = texCUBE(_CubeMap, WorldReflectionVector(IN, o.Normal)).rgb;
            
            // Calculate fresnel effect
            float fresnel = pow(1 - dot(normalize(IN.viewDir), o.Normal), 4);
            
            // Sample foam texture
            float2 foamUV = IN.uv_FoamTexture + flowOffset * _FoamSpeed;
            float4 foam = tex2D(_FoamTexture, foamUV) * _FoamAmount;
            
            // Calculate final color
            float4 waterColor = lerp(_ShallowColor, _DeepColor, saturate(depth));
            float4 finalColor = lerp(waterColor, float4(reflection, 1), fresnel * _Reflectivity);
            finalColor += foam * (1 - depth);

            // Apply distortion to grab texture
            float4 screenPos = IN.screenPos;
            screenPos.xy += finalNormal.xy * 0.1;
            float4 grabColor = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(screenPos));
            
            o.Albedo = lerp(grabColor.rgb, finalColor.rgb, _Transparency);
            o.Alpha = lerp(_ShallowColor.a, _DeepColor.a, saturate(depth));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
