Shader "RGE/ImageWithAlpha"
{
    Properties
    {
        [HideInInspector] _MainTex("Texture", 2D) = "white" {}
        [HideInInspector] _UVCenter("_UVCenter", Vector) = (0,0,0,0)
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
            float _DoIt;
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
              if (col.a < .9f)  col.a = 0;
              return col;
            }
            ENDCG
        }
    }
}
