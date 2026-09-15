Shader "SpatialVideo/StereoUnlit"
{
    // 依 _StereoMode 把 SBS/TnB 影片的左右半邊分別餵給對應眼睛。
    // 用 unity_StereoEyeIndex 而不是在 C# 端改 mainTextureScale/Offset，
    // 是因為 Quest 走 single-pass instanced，兩眼在同一個 draw call 裡，
    // 材質屬性沒辦法用兩份，只能在 shader 內用 stereo eye index 分流。
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        // 0 = Mono, 1 = SideBySide, 2 = TopAndBottom
        _StereoMode ("Stereo Mode", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _StereoMode;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 uv = i.uv;
                // unity_StereoEyeIndex: 0 = 左眼, 1 = 右眼
                uint eye = unity_StereoEyeIndex;

                if (_StereoMode > 1.5) // TopAndBottom
                {
                    uv.y = uv.y * 0.5 + (eye == 0 ? 0.5 : 0.0);
                }
                else if (_StereoMode > 0.5) // SideBySide
                {
                    uv.x = uv.x * 0.5 + (eye == 0 ? 0.0 : 0.5);
                }

                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
