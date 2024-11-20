Shader "Custom/Terrain"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        const static int maxcolorcount = 8;

        float minheight;
        float maxheight;
        int basecolorcount;
        float3 basecolors[maxcolorcount];
        float basestartheights[maxcolorcount];

        struct Input
        {
            float3 worldPos;
        };

        float InverseLerp(float a, float b, float value)
        {
            return saturate((value-a)/(b-a));
        }


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        //UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        //UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float heightpercent = InverseLerp(minheight, maxheight, IN.worldPos.y);
            for(int i = 0; i < basecolorcount; i++)
            {
                float drawstrength = saturate(sign(heightpercent - basestartheights[i]));
                o.Albedo = o.Albedo * (1 - drawstrength) + basecolors[i] * drawstrength;
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}
