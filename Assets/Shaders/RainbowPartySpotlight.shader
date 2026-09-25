Shader "UI/RainbowPartySpotlight"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Bottom Corner Spotlight Settings)]
        _LightIntensity ("Light Intensity", Range(0, 5)) = 2.5
        _BeamWidth ("Beam Width", Range(0.05, 0.6)) = 0.25
        _BeamLength ("Beam Length", Range(0.5, 3.5)) = 2.0
        _SweepSpeed ("Sweep Speed", Range(0, 5)) = 1.2
        _CornerOffset ("Corner Offset (Padding)", Range(0.0, 0.4)) = 0.02

        [Header(Rainbow Color Settings)]
        _RainbowSpeed ("Rainbow Cycle Speed", Range(0, 5)) = 1.0
        _RainbowSpread ("Rainbow Angular Spread", Range(0, 5)) = 1.5
        _Saturation ("Rainbow Saturation", Range(0, 1)) = 0.95

        [Header(Volumetric Ray Settings)]
        _RayDensity ("Ray Fan Density", Range(2, 30)) = 12.0
        _RaySharpness ("Ray Sharpness", Range(1, 10)) = 2.5

        [Header(Party Strobe Rhythm)]
        _PulseSpeed ("Party Pulse Speed", Range(0, 10)) = 4.0
        _PulseAmount ("Party Pulse Amount", Range(0, 1)) = 0.25

        [Header(UI Stencil Masking)]
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha One
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _LightIntensity;
            float _BeamWidth;
            float _BeamLength;
            float _SweepSpeed;
            float _CornerOffset;

            float _RainbowSpeed;
            float _RainbowSpread;
            float _Saturation;

            float _RayDensity;
            float _RaySharpness;

            float _PulseSpeed;
            float _PulseAmount;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            // HSV to RGB conversion for smooth rainbow spectrum
            float3 HSVtoRGB(float3 hsv)
            {
                float3 c = abs(frac(hsv.x + float3(0.0, 2.0/3.0, 1.0/3.0)) * 6.0 - 3.0) - 1.0;
                c = saturate(c);
                return hsv.z * lerp(float3(1.0, 1.0, 1.0), c, hsv.y);
            }

            // Compute fan of volumetric rainbow light rays from a bottom corner
            float3 ComputeCornerRainbowRays(float2 uv, float2 startPoint, float isRightCorner, float time)
            {
                float2 toPixel = uv - startPoint;
                float dist = length(toPixel);
                if (dist < 0.001) return float3(0,0,0);

                float pixelAngle = atan2(toPixel.y, toPixel.x);
                
                // Mask for valid angle sector:
                // Bottom-Left (BL): 0° to ~90° (0 to PI/2 rad)
                // Bottom-Right (BR): ~90° to 180° (PI/2 to PI rad)
                float validAngleMask = (isRightCorner > 0.5) ? 
                    (smoothstep(0.4, 0.9, pixelAngle) * smoothstep(3.2, 2.7, pixelAngle)) :
                    (smoothstep(-0.2, 0.2, pixelAngle) * smoothstep(1.9, 1.4, pixelAngle));

                if (validAngleMask <= 0.001) return float3(0,0,0);

                // Dynamic sweeping motion offset
                float sweepWave = sin(time * _SweepSpeed + (isRightCorner > 0.5 ? 1.57 : 0.0)) * 0.25;
                float relativeAngle = pixelAngle + sweepWave;

                // 1. Primary Sweeping Beams (2 distinct wide spotlight beams per corner)
                float beamCenter1 = (isRightCorner > 0.5) ? (2.35 + sin(time * _SweepSpeed * 0.7) * 0.45) : (0.78 + sin(time * _SweepSpeed * 0.7) * 0.45);
                float beamCenter2 = (isRightCorner > 0.5) ? (1.95 - cos(time * _SweepSpeed * 0.9) * 0.35) : (1.18 - cos(time * _SweepSpeed * 0.9) * 0.35);

                float b1 = smoothstep(_BeamWidth * 2.5, 0.0, abs(pixelAngle - beamCenter1));
                float b2 = smoothstep(_BeamWidth * 2.5, 0.0, abs(pixelAngle - beamCenter2));
                float mainBeams = saturate(b1 * 1.2 + b2 * 0.9);

                // 2. Volumetric Ray Fan (Stage sunburst rays radiating outward)
                float rayPattern = sin(relativeAngle * _RayDensity + time * _SweepSpeed * 2.0) * 0.5 + 0.5;
                rayPattern = pow(rayPattern, _RaySharpness);

                // 3. Radial corner bloom & distance falloff
                float cornerGlow = 1.0 / (dist * 3.0 + 0.2);
                float distFalloff = (1.0 - smoothstep(0.1, _BeamLength, dist)) * smoothstep(0.0, 0.05, dist);

                // Combine light ray intensity
                float totalIntensity = (mainBeams * 0.8 + rayPattern * 0.6 + cornerGlow * 0.35) * validAngleMask * distFalloff;

                // 4. Dynamic Rainbow Hue
                // Hue cycles across angle (fan spread), distance (outward movement), and time
                float angleHue = (pixelAngle - (isRightCorner > 0.5 ? 1.57 : 0.0)) * _RainbowSpread * 0.2;
                float distHue = dist * 0.2;
                float timeHue = _Time.y * _RainbowSpeed * 0.2 + (isRightCorner > 0.5 ? 0.5 : 0.0);
                
                float hue = frac(angleHue + distHue + timeHue);
                float3 rainbowColor = HSVtoRGB(float3(hue, _Saturation, 1.0));

                return rainbowColor * totalIntensity;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 baseColor = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                float2 uv = IN.texcoord;
                float time = _Time.y * _SweepSpeed;

                // Corner origin coordinates for bottom corners
                float2 startBL = float2(0.0 + _CornerOffset, 0.0 + _CornerOffset); // Bottom-Left
                float2 startBR = float2(1.0 - _CornerOffset, 0.0 + _CornerOffset); // Bottom-Right

                // Calculate volumetric rainbow lights from bottom corners
                float3 lightBL = ComputeCornerRainbowRays(uv, startBL, 0.0, time);
                float3 lightBR = ComputeCornerRainbowRays(uv, startBR, 1.0, time);

                // Rhythm pulse factor
                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;

                // Combine bottom corners rainbow party lights
                float3 totalLight = (lightBL + lightBR) * _LightIntensity * pulse;

                // Compute light opacity so shader works seamlessly on transparent images
                float lightAlpha = saturate(max(totalLight.r, max(totalLight.g, totalLight.b)));
                float finalAlpha = saturate(baseColor.a + lightAlpha);

                // Blend base texture color with rainbow light
                float3 finalColor = baseColor.rgb * (baseColor.a > 0.001 ? 1.0 : 0.0) + totalLight;

                half4 color = half4(finalColor, finalAlpha);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                clip(color.a - 0.001);

                return color;
            }
            ENDCG
        }
    }
}
