public static class ViewDependenceNetworkShader {
    public const string Template = @"Shader ""MobileNeRF/ViewDependenceNetworkShader/OBJECT_NAME"" {
    Properties {
        tDiffuse0x (""Diffuse Texture 0"", 2D) = ""white"" {}
        tDiffuse1x (""Diffuse Texture 1"", 2D) = ""white"" {}
    }

    CGINCLUDE
    #include ""UnityCG.cginc""

    struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
        float3 rayDirection : TEXCOORD1;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    v2f vert(appdata v) {
        v2f o;

        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_OUTPUT(v2f, o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        o.rayDirection = -WorldSpaceViewDir(v.vertex);
        AXIS_SWIZZLE

        return o;
    }

    sampler2D tDiffuse0x;
    sampler2D tDiffuse1x;

    half3 evaluateNetwork(fixed4 f0, fixed4 f1, fixed4 viewdir) {
        float4x4 intermediate_one = { BIAS_LIST_ZERO };
        intermediate_one += f0.r * float4x4(__W0_0__)
            + f0.g * float4x4(__W0_1__)
            + f0.b * float4x4(__W0_2__)
            + f0.a * float4x4(__W0_3__)
            + f1.r * float4x4(__W0_4__)
            + f1.g * float4x4(__W0_5__)
            + f1.b * float4x4(__W0_6__)
            + f1.a * float4x4(__W0_7__)
            + viewdir.r * float4x4(__W0_8__)
            + viewdir.g * float4x4(__W0_9__)
            + viewdir.b * float4x4(__W0_10__);
        intermediate_one[0] = max(intermediate_one[0], 0.0);
        intermediate_one[1] = max(intermediate_one[1], 0.0);
        intermediate_one[2] = max(intermediate_one[2], 0.0);
        intermediate_one[3] = max(intermediate_one[3], 0.0);
        float4x4 intermediate_two = float4x4(
            BIAS_LIST_ONE
        );
        intermediate_two += intermediate_one[0][0] * float4x4(__W1_0__)
            + intermediate_one[0][1] * float4x4(__W1_1__)
            + intermediate_one[0][2] * float4x4(__W1_2__)
            + intermediate_one[0][3] * float4x4(__W1_3__)
            + intermediate_one[1][0] * float4x4(__W1_4__)
            + intermediate_one[1][1] * float4x4(__W1_5__)
            + intermediate_one[1][2] * float4x4(__W1_6__)
            + intermediate_one[1][3] * float4x4(__W1_7__)
            + intermediate_one[2][0] * float4x4(__W1_8__)
            + intermediate_one[2][1] * float4x4(__W1_9__)
            + intermediate_one[2][2] * float4x4(__W1_10__)
            + intermediate_one[2][3] * float4x4(__W1_11__)
            + intermediate_one[3][0] * float4x4(__W1_12__)
            + intermediate_one[3][1] * float4x4(__W1_13__)
            + intermediate_one[3][2] * float4x4(__W1_14__)
            + intermediate_one[3][3] * float4x4(__W1_15__);
        intermediate_two[0] = max(intermediate_two[0], 0.0);
        intermediate_two[1] = max(intermediate_two[1], 0.0);
        intermediate_two[2] = max(intermediate_two[2], 0.0);
        intermediate_two[3] = max(intermediate_two[3], 0.0);
        float3 result = float3(
            BIAS_LIST_TWO
        );
        result += intermediate_two[0][0] * float3(__W2_0__)
                + intermediate_two[0][1] * float3(__W2_1__)
                + intermediate_two[0][2] * float3(__W2_2__)
                + intermediate_two[0][3] * float3(__W2_3__)
                + intermediate_two[1][0] * float3(__W2_4__)
                + intermediate_two[1][1] * float3(__W2_5__)
                + intermediate_two[1][2] * float3(__W2_6__)
                + intermediate_two[1][3] * float3(__W2_7__)
                + intermediate_two[2][0] * float3(__W2_8__)
                + intermediate_two[2][1] * float3(__W2_9__)
                + intermediate_two[2][2] * float3(__W2_10__)
                + intermediate_two[2][3] * float3(__W2_11__)
                + intermediate_two[3][0] * float3(__W2_12__)
                + intermediate_two[3][1] * float3(__W2_13__)
                + intermediate_two[3][2] * float3(__W2_14__)
                + intermediate_two[3][3] * float3(__W2_15__);
		result = 1.0 / (1.0 + exp(-result));
        return result*viewdir.a+(1.0-viewdir.a);
    }
    ENDCG

    SubShader {
        Cull Off
        ZTest LEqual

        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag(v2f i) : SV_Target {
                fixed4 diffuse0 = tex2D( tDiffuse0x, i.uv );
                if (diffuse0.r == 0.0) discard;
                fixed4 diffuse1 = tex2D( tDiffuse1x, i.uv );
                fixed4 rayDir = fixed4(normalize(i.rayDirection), 1.0);

                // normalize range to [-1, 1]
                diffuse0.a = diffuse0.a * 2.0 - 1.0;
                diffuse1.a = diffuse1.a * 2.0 - 1.0;

                fixed4 fragColor;
                fragColor.rgb = evaluateNetwork(diffuse0,diffuse1,rayDir);
                fragColor.a = 1.0;

                #if(!UNITY_COLORSPACE_GAMMA)
                    fragColor.rgb = GammaToLinearSpace(fragColor.rgb);
                #endif

                return fragColor;
            }
            ENDCG
        }

        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Tags {""LightMode"" = ""ShadowCaster""}

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment fragShadowCaster
            #pragma multi_compile_shadowcaster

            fixed4 fragShadowCaster(v2f i) : SV_Target{
                fixed4 diffuse0 = tex2D(tDiffuse0x, i.uv);
                if (diffuse0.r == 0.0) discard;
                return 0;
            }
            ENDCG
        }
    }
}";
}
