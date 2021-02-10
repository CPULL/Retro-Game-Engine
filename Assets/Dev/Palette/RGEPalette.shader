Shader "RGE/Palette"
{
    Properties
    {
        [HideInInspector] _MainTex("Texture", 2D) = "white" {}
        [HideInInspector] _UVCenter("_UVCenter", Vector) = (0,0,0,0)
        [MaterialToggle] _UsePalette("_UsePalette", Float) = 0
        _Luma("_Luma", Range(-1, 1)) = 0
        _Contrast("_Contrast", Range(-1, 1)) = 0
    }


    SubShader
    {
        Tags{
          "RenderType" = "Transparent"
          "Queue" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite off
        Cull off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON

            #include "UnityCG.cginc"


            fixed4 _Colors[256];
            float _Contrast, _Luma;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _UsePalette;
            static const fixed4 __Black = fixed4(0, 0, 0, 1);
            static const fixed4 __Transp = fixed4(0, 0, 0, 0);

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
              float2 uv = IN.uv;
              fixed4 col = tex2D(_MainTex, uv);
              if (col.a == 0 || _UsePalette == 0) return col;

              uint h = ((uint)(col.r * 256) - 4) / 8;
              uint l = ((uint)(col.g * 256) - 4) / 8;
              if (h > 15 && l > 15) return __Transp;
              col = _Colors[h * 16 + l];
              if (h == 0 && l == 0) col = __Black;

              if (_Luma == 0 && _Contrast == 0) return col;
              if (_Luma < -1) _Luma = -1;
              if (_Luma > 1) _Luma = 1;
              if (_Contrast < -1) _Contrast = -1;
              if (_Contrast > 1) _Contrast = 1;

              col.rgb = ((col.rgb - 0.5f) * (_Contrast + 1)) + 0.5f;
              col.rgb += _Luma;

              return col;
            }
            ENDCG
        }
    }
}
