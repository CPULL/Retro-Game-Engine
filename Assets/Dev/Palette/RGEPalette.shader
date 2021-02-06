Shader "RGE/Palette"
{
    Properties
    {
        [HideInInspector] _MainTex("Texture", 2D) = "white" {}
        [HideInInspector] _UVCenter("_UVCenter", Vector) = (0,0,0,0)

        [MaterialToggle] _DoIt("DoIt", Float) = 0

        _Color01("01", Color) = (1, 1, 1, 1)
        _Color02("02", Color) = (1, 1, 1, 1)
        _Color03("03", Color) = (1, 1, 1, 1)
        _Color04("04", Color) = (1, 1, 1, 1)
        _Color05("05", Color) = (1, 1, 1, 1)
        _Color06("06", Color) = (1, 1, 1, 1)
        _Color07("07", Color) = (1, 1, 1, 1)
        _Color08("08", Color) = (1, 1, 1, 1)
        _Color09("09", Color) = (1, 1, 1, 1)
        _Color0A("0A", Color) = (1, 1, 1, 1)
        _Color0B("0B", Color) = (1, 1, 1, 1)
        _Color0C("0C", Color) = (1, 1, 1, 1)
        _Color0D("0D", Color) = (1, 1, 1, 1)
        _Color0E("0E", Color) = (1, 1, 1, 1)
        _Color0F("0F", Color) = (1, 1, 1, 1)

        _Color10("10", Color) = (1, 1, 1, 1)
        _Color11("11", Color) = (1, 1, 1, 1)
        _Color12("12", Color) = (1, 1, 1, 1)
        _Color13("13", Color) = (1, 1, 1, 1)
        _Color14("14", Color) = (1, 1, 1, 1)
        _Color15("15", Color) = (1, 1, 1, 1)
        _Color16("16", Color) = (1, 1, 1, 1)
        _Color17("17", Color) = (1, 1, 1, 1)
        _Color18("18", Color) = (1, 1, 1, 1)
        _Color19("19", Color) = (1, 1, 1, 1)
        _Color1A("1A", Color) = (1, 1, 1, 1)
        _Color1B("1B", Color) = (1, 1, 1, 1)
        _Color1C("1C", Color) = (1, 1, 1, 1)
        _Color1D("1D", Color) = (1, 1, 1, 1)
        _Color1E("1E", Color) = (1, 1, 1, 1)
        _Color1F("1F", Color) = (1, 1, 1, 1)

        _Color20("20", Color) = (1, 1, 1, 1)
        _Color21("21", Color) = (1, 1, 1, 1)
        _Color22("22", Color) = (1, 1, 1, 1)
        _Color23("23", Color) = (1, 1, 1, 1)
        _Color24("24", Color) = (1, 1, 1, 1)
        _Color25("25", Color) = (1, 1, 1, 1)
        _Color26("26", Color) = (1, 1, 1, 1)
        _Color27("27", Color) = (1, 1, 1, 1)
        _Color28("28", Color) = (1, 1, 1, 1)
        _Color29("29", Color) = (1, 1, 1, 1)
        _Color2A("2A", Color) = (1, 1, 1, 1)
        _Color2B("2B", Color) = (1, 1, 1, 1)
        _Color2C("2C", Color) = (1, 1, 1, 1)
        _Color2D("2D", Color) = (1, 1, 1, 1)
        _Color2E("2E", Color) = (1, 1, 1, 1)
        _Color2F("2F", Color) = (1, 1, 1, 1)

        _Color30("30", Color) = (1, 1, 1, 1)
        _Color31("31", Color) = (1, 1, 1, 1)
        _Color32("32", Color) = (1, 1, 1, 1)
        _Color33("33", Color) = (1, 1, 1, 1)
        _Color34("34", Color) = (1, 1, 1, 1)
        _Color35("35", Color) = (1, 1, 1, 1)
        _Color36("36", Color) = (1, 1, 1, 1)
        _Color37("37", Color) = (1, 1, 1, 1)
        _Color38("38", Color) = (1, 1, 1, 1)
        _Color39("39", Color) = (1, 1, 1, 1)
        _Color3A("3A", Color) = (1, 1, 1, 1)
        _Color3B("3B", Color) = (1, 1, 1, 1)
        _Color3C("3C", Color) = (1, 1, 1, 1)
        _Color3D("3D", Color) = (1, 1, 1, 1)
        _Color3E("3E", Color) = (1, 1, 1, 1)
        _Color3F("3F", Color) = (1, 1, 1, 1)

        _Color40("40", Color) = (1, 1, 1, 1)
        _Color41("41", Color) = (1, 1, 1, 1)
        _Color42("42", Color) = (1, 1, 1, 1)
        _Color43("43", Color) = (1, 1, 1, 1)
        _Color44("44", Color) = (1, 1, 1, 1)
        _Color45("45", Color) = (1, 1, 1, 1)
        _Color46("46", Color) = (1, 1, 1, 1)
        _Color47("47", Color) = (1, 1, 1, 1)
        _Color48("48", Color) = (1, 1, 1, 1)
        _Color49("49", Color) = (1, 1, 1, 1)
        _Color4A("4A", Color) = (1, 1, 1, 1)
        _Color4B("4B", Color) = (1, 1, 1, 1)
        _Color4C("4C", Color) = (1, 1, 1, 1)
        _Color4D("4D", Color) = (1, 1, 1, 1)
        _Color4E("4E", Color) = (1, 1, 1, 1)
        _Color4F("4F", Color) = (1, 1, 1, 1)

        _Color50("50", Color) = (1, 1, 1, 1)
        _Color51("51", Color) = (1, 1, 1, 1)
        _Color52("52", Color) = (1, 1, 1, 1)
        _Color53("53", Color) = (1, 1, 1, 1)
        _Color54("54", Color) = (1, 1, 1, 1)
        _Color55("55", Color) = (1, 1, 1, 1)
        _Color56("56", Color) = (1, 1, 1, 1)
        _Color57("57", Color) = (1, 1, 1, 1)
        _Color58("58", Color) = (1, 1, 1, 1)
        _Color59("59", Color) = (1, 1, 1, 1)
        _Color5A("5A", Color) = (1, 1, 1, 1)
        _Color5B("5B", Color) = (1, 1, 1, 1)
        _Color5C("5C", Color) = (1, 1, 1, 1)
        _Color5D("5D", Color) = (1, 1, 1, 1)
        _Color5E("5E", Color) = (1, 1, 1, 1)
        _Color5F("5F", Color) = (1, 1, 1, 1)

        _Color60("60", Color) = (1, 1, 1, 1)
          _Color61("61", Color) = (1, 1, 1, 1)
          _Color62("62", Color) = (1, 1, 1, 1)
          _Color63("63", Color) = (1, 1, 1, 1)
          _Color64("64", Color) = (1, 1, 1, 1)
          _Color65("65", Color) = (1, 1, 1, 1)
          _Color66("66", Color) = (1, 1, 1, 1)
          _Color67("67", Color) = (1, 1, 1, 1)
          _Color68("68", Color) = (1, 1, 1, 1)
          _Color69("69", Color) = (1, 1, 1, 1)
          _Color6A("6A", Color) = (1, 1, 1, 1)
          _Color6B("6B", Color) = (1, 1, 1, 1)
          _Color6C("6C", Color) = (1, 1, 1, 1)
          _Color6D("6D", Color) = (1, 1, 1, 1)
          _Color6E("6E", Color) = (1, 1, 1, 1)
          _Color6F("6F", Color) = (1, 1, 1, 1)

        _Color70("70", Color) = (1, 1, 1, 1)
          _Color71("71", Color) = (1, 1, 1, 1)
          _Color72("72", Color) = (1, 1, 1, 1)
          _Color73("73", Color) = (1, 1, 1, 1)
          _Color74("74", Color) = (1, 1, 1, 1)
          _Color75("75", Color) = (1, 1, 1, 1)
          _Color76("76", Color) = (1, 1, 1, 1)
          _Color77("77", Color) = (1, 1, 1, 1)
          _Color78("78", Color) = (1, 1, 1, 1)
          _Color79("79", Color) = (1, 1, 1, 1)
          _Color7A("7A", Color) = (1, 1, 1, 1)
          _Color7B("7B", Color) = (1, 1, 1, 1)
          _Color7C("7C", Color) = (1, 1, 1, 1)
          _Color7D("7D", Color) = (1, 1, 1, 1)
          _Color7E("7E", Color) = (1, 1, 1, 1)
          _Color7F("7F", Color) = (1, 1, 1, 1)

        _Color80("80", Color) = (1, 1, 1, 1)
          _Color81("81", Color) = (1, 1, 1, 1)
          _Color82("82", Color) = (1, 1, 1, 1)
          _Color83("83", Color) = (1, 1, 1, 1)
          _Color84("84", Color) = (1, 1, 1, 1)
          _Color85("85", Color) = (1, 1, 1, 1)
          _Color86("86", Color) = (1, 1, 1, 1)
          _Color87("87", Color) = (1, 1, 1, 1)
          _Color88("88", Color) = (1, 1, 1, 1)
          _Color89("89", Color) = (1, 1, 1, 1)
          _Color8A("8A", Color) = (1, 1, 1, 1)
          _Color8B("8B", Color) = (1, 1, 1, 1)
          _Color8C("8C", Color) = (1, 1, 1, 1)
          _Color8D("8D", Color) = (1, 1, 1, 1)
          _Color8E("8E", Color) = (1, 1, 1, 1)
          _Color8F("8F", Color) = (1, 1, 1, 1)

        _Color90("90", Color) = (1, 1, 1, 1)
          _Color91("91", Color) = (1, 1, 1, 1)
          _Color92("92", Color) = (1, 1, 1, 1)
          _Color93("93", Color) = (1, 1, 1, 1)
          _Color94("94", Color) = (1, 1, 1, 1)
          _Color95("95", Color) = (1, 1, 1, 1)
          _Color96("96", Color) = (1, 1, 1, 1)
          _Color97("97", Color) = (1, 1, 1, 1)
          _Color98("98", Color) = (1, 1, 1, 1)
          _Color99("99", Color) = (1, 1, 1, 1)
          _Color9A("9A", Color) = (1, 1, 1, 1)
          _Color9B("9B", Color) = (1, 1, 1, 1)
          _Color9C("9C", Color) = (1, 1, 1, 1)
          _Color9D("9D", Color) = (1, 1, 1, 1)
          _Color9E("9E", Color) = (1, 1, 1, 1)
          _Color9F("9F", Color) = (1, 1, 1, 1)

        _ColorA0("A0", Color) = (1, 1, 1, 1)
          _ColorA1("A1", Color) = (1, 1, 1, 1)
          _ColorA2("A2", Color) = (1, 1, 1, 1)
          _ColorA3("A3", Color) = (1, 1, 1, 1)
          _ColorA4("A4", Color) = (1, 1, 1, 1)
          _ColorA5("A5", Color) = (1, 1, 1, 1)
          _ColorA6("A6", Color) = (1, 1, 1, 1)
          _ColorA7("A7", Color) = (1, 1, 1, 1)
          _ColorA8("A8", Color) = (1, 1, 1, 1)
          _ColorA9("A9", Color) = (1, 1, 1, 1)
          _ColorAA("AA", Color) = (1, 1, 1, 1)
          _ColorAB("AB", Color) = (1, 1, 1, 1)
          _ColorAC("AC", Color) = (1, 1, 1, 1)
          _ColorAD("AD", Color) = (1, 1, 1, 1)
          _ColorAE("AE", Color) = (1, 1, 1, 1)
          _ColorAF("AF", Color) = (1, 1, 1, 1)

          _ColorB0("B0", Color) = (1, 1, 1, 1)
          _ColorB1("B1", Color) = (1, 1, 1, 1)
          _ColorB2("B2", Color) = (1, 1, 1, 1)
          _ColorB3("B3", Color) = (1, 1, 1, 1)
          _ColorB4("B4", Color) = (1, 1, 1, 1)
          _ColorB5("B5", Color) = (1, 1, 1, 1)
          _ColorB6("B6", Color) = (1, 1, 1, 1)
          _ColorB7("B7", Color) = (1, 1, 1, 1)
          _ColorB8("B8", Color) = (1, 1, 1, 1)
          _ColorB9("B9", Color) = (1, 1, 1, 1)
          _ColorBA("BA", Color) = (1, 1, 1, 1)
          _ColorBB("BB", Color) = (1, 1, 1, 1)
          _ColorBC("BC", Color) = (1, 1, 1, 1)
          _ColorBD("BD", Color) = (1, 1, 1, 1)
          _ColorBE("BE", Color) = (1, 1, 1, 1)
          _ColorBF("BF", Color) = (1, 1, 1, 1)

        _ColorC0("C0", Color) = (1, 1, 1, 1)
          _ColorC1("C1", Color) = (1, 1, 1, 1)
          _ColorC2("C2", Color) = (1, 1, 1, 1)
          _ColorC3("C3", Color) = (1, 1, 1, 1)
          _ColorC4("C4", Color) = (1, 1, 1, 1)
          _ColorC5("C5", Color) = (1, 1, 1, 1)
          _ColorC6("C6", Color) = (1, 1, 1, 1)
          _ColorC7("C7", Color) = (1, 1, 1, 1)
          _ColorC8("C8", Color) = (1, 1, 1, 1)
          _ColorC9("C9", Color) = (1, 1, 1, 1)
          _ColorCA("CA", Color) = (1, 1, 1, 1)
          _ColorCB("CB", Color) = (1, 1, 1, 1)
          _ColorCC("CC", Color) = (1, 1, 1, 1)
          _ColorCD("CD", Color) = (1, 1, 1, 1)
          _ColorCE("CE", Color) = (1, 1, 1, 1)
          _ColorCF("CF", Color) = (1, 1, 1, 1)

          _ColorD0("D0", Color) = (1, 1, 1, 1)
          _ColorD1("D1", Color) = (1, 1, 1, 1)
          _ColorD2("D2", Color) = (1, 1, 1, 1)
          _ColorD3("D3", Color) = (1, 1, 1, 1)
          _ColorD4("D4", Color) = (1, 1, 1, 1)
          _ColorD5("D5", Color) = (1, 1, 1, 1)
          _ColorD6("D6", Color) = (1, 1, 1, 1)
          _ColorD7("D7", Color) = (1, 1, 1, 1)
          _ColorD8("D8", Color) = (1, 1, 1, 1)
          _ColorD9("D9", Color) = (1, 1, 1, 1)
          _ColorDA("DA", Color) = (1, 1, 1, 1)
          _ColorDB("DB", Color) = (1, 1, 1, 1)
          _ColorDC("DC", Color) = (1, 1, 1, 1)
          _ColorDD("DD", Color) = (1, 1, 1, 1)
          _ColorDE("DE", Color) = (1, 1, 1, 1)
          _ColorDF("DF", Color) = (1, 1, 1, 1)

          _ColorE0("E0", Color) = (1, 1, 1, 1)
          _ColorE1("E1", Color) = (1, 1, 1, 1)
          _ColorE2("E2", Color) = (1, 1, 1, 1)
          _ColorE3("E3", Color) = (1, 1, 1, 1)
          _ColorE4("E4", Color) = (1, 1, 1, 1)
          _ColorE5("E5", Color) = (1, 1, 1, 1)
          _ColorE6("E6", Color) = (1, 1, 1, 1)
          _ColorE7("E7", Color) = (1, 1, 1, 1)
          _ColorE8("E8", Color) = (1, 1, 1, 1)
          _ColorE9("E9", Color) = (1, 1, 1, 1)
          _ColorEA("EA", Color) = (1, 1, 1, 1)
          _ColorEB("EB", Color) = (1, 1, 1, 1)
          _ColorEC("EC", Color) = (1, 1, 1, 1)
          _ColorED("ED", Color) = (1, 1, 1, 1)
          _ColorEE("EE", Color) = (1, 1, 1, 1)
          _ColorEF("EF", Color) = (1, 1, 1, 1)

          _ColorF0("F0", Color) = (1, 1, 1, 1)
          _ColorF1("F1", Color) = (1, 1, 1, 1)
          _ColorF2("F2", Color) = (1, 1, 1, 1)
          _ColorF3("F3", Color) = (1, 1, 1, 1)
          _ColorF4("F4", Color) = (1, 1, 1, 1)
          _ColorF5("F5", Color) = (1, 1, 1, 1)
          _ColorF6("F6", Color) = (1, 1, 1, 1)
          _ColorF7("F7", Color) = (1, 1, 1, 1)
          _ColorF8("F8", Color) = (1, 1, 1, 1)
          _ColorF9("F9", Color) = (1, 1, 1, 1)
          _ColorFA("FA", Color) = (1, 1, 1, 1)
          _ColorFB("FB", Color) = (1, 1, 1, 1)
          _ColorFC("FC", Color) = (1, 1, 1, 1)
          _ColorFD("FD", Color) = (1, 1, 1, 1)
          _ColorFE("FE", Color) = (1, 1, 1, 1)
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

    fixed4 _Color01, _Color02, _Color03, _Color04, _Color05, _Color06, _Color07, _Color08, _Color09, _Color0A, _Color0B, _Color0C, _Color0D, _Color0E, _Color0F;
    fixed4 _Color10, _Color11, _Color12, _Color13, _Color14, _Color15, _Color16, _Color17, _Color18, _Color19, _Color1A, _Color1B, _Color1C, _Color1D, _Color1E, _Color1F;
    fixed4 _Color20, _Color21, _Color22, _Color23, _Color24, _Color25, _Color26, _Color27, _Color28, _Color29, _Color2A, _Color2B, _Color2C, _Color2D, _Color2E, _Color2F;
    fixed4 _Color30, _Color31, _Color32, _Color33, _Color34, _Color35, _Color36, _Color37, _Color38, _Color39, _Color3A, _Color3B, _Color3C, _Color3D, _Color3E, _Color3F;

    fixed4 _Color40, _Color41, _Color42, _Color43, _Color44, _Color45, _Color46, _Color47, _Color48, _Color49, _Color4A, _Color4B, _Color4C, _Color4D, _Color4E, _Color4F;
    fixed4 _Color50, _Color51, _Color52, _Color53, _Color54, _Color55, _Color56, _Color57, _Color58, _Color59, _Color5A, _Color5B, _Color5C, _Color5D, _Color5E, _Color5F;
    fixed4 _Color60, _Color61, _Color62, _Color63, _Color64, _Color65, _Color66, _Color67, _Color68, _Color69, _Color6A, _Color6B, _Color6C, _Color6D, _Color6E, _Color6F;
    fixed4 _Color70, _Color71, _Color72, _Color73, _Color74, _Color75, _Color76, _Color77, _Color78, _Color79, _Color7A, _Color7B, _Color7C, _Color7D, _Color7E, _Color7F;
    fixed4 _Color80, _Color81, _Color82, _Color83, _Color84, _Color85, _Color86, _Color87, _Color88, _Color89, _Color8A, _Color8B, _Color8C, _Color8D, _Color8E, _Color8F;
    fixed4 _Color90, _Color91, _Color92, _Color93, _Color94, _Color95, _Color96, _Color97, _Color98, _Color99, _Color9A, _Color9B, _Color9C, _Color9D, _Color9E, _Color9F;
    fixed4 _ColorA0, _ColorA1, _ColorA2, _ColorA3, _ColorA4, _ColorA5, _ColorA6, _ColorA7, _ColorA8, _ColorA9, _ColorAA, _ColorAB, _ColorAC, _ColorAD, _ColorAE, _ColorAF;
    fixed4 _ColorB0, _ColorB1, _ColorB2, _ColorB3, _ColorB4, _ColorB5, _ColorB6, _ColorB7, _ColorB8, _ColorB9, _ColorBA, _ColorBB, _ColorBC, _ColorBD, _ColorBE, _ColorBF;

    fixed4 _ColorC0, _ColorC1, _ColorC2, _ColorC3, _ColorC4, _ColorC5, _ColorC6, _ColorC7, _ColorC8, _ColorC9, _ColorCA, _ColorCB, _ColorCC, _ColorCD, _ColorCE, _ColorCF;
    fixed4 _ColorD0, _ColorD1, _ColorD2, _ColorD3, _ColorD4, _ColorD5, _ColorD6, _ColorD7, _ColorD8, _ColorD9, _ColorDA, _ColorDB, _ColorDC, _ColorDD, _ColorDE, _ColorDF;
    fixed4 _ColorE0, _ColorE1, _ColorE2, _ColorE3, _ColorE4, _ColorE5, _ColorE6, _ColorE7, _ColorE8, _ColorE9, _ColorEA, _ColorEB, _ColorEC, _ColorED, _ColorEE, _ColorEF;
    fixed4 _ColorF0, _ColorF1, _ColorF2, _ColorF3, _ColorF4, _ColorF5, _ColorF6, _ColorF7, _ColorF8, _ColorF9, _ColorFA, _ColorFB, _ColorFC, _ColorFD, _ColorFE, _ColorFF;

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
              if (col.a == 0 || _DoIt == 0) return col;

              int h = col.r * 16;
              int l = col.g * 16;

              if (h == 0 && l == 0) return __Black;
              if (h > 15 && l > 15) return __Transp;

              if (h < 1) {
                if (l < 2) return _Color01;
                if (l < 3) return _Color02;
                if (l < 4) return _Color03;
                if (l < 5) return _Color04;
                if (l < 6) return _Color05;
                if (l < 7) return _Color06;
                if (l < 8) return _Color07;
                if (l < 9) return _Color08;
                if (l < 10) return _Color09;
                if (l < 11) return _Color0A;
                if (l < 12) return _Color0B;
                if (l < 13) return _Color0C;
                if (l < 14) return _Color0D;
                if (l < 15) return _Color0E;
                if (l < 16) return _Color0F;
              }
              if (h < 2) {
                if (l < 2) return _Color11;
                if (l < 3) return _Color12;
                if (l < 4) return _Color13;
                if (l < 5) return _Color14;
                if (l < 6) return _Color15;
                if (l < 7) return _Color16;
                if (l < 8) return _Color17;
                if (l < 9) return _Color18;
                if (l < 10) return _Color19;
                if (l < 11) return _Color1A;
                if (l < 12) return _Color1B;
                if (l < 13) return _Color1C;
                if (l < 14) return _Color1D;
                if (l < 15) return _Color1E;
                if (l < 16) return _Color1F;
              }
              if (h < 3) {
                if (l < 2) return _Color21;
                if (l < 3) return _Color22;
                if (l < 4) return _Color23;
                if (l < 5) return _Color24;
                if (l < 6) return _Color25;
                if (l < 7) return _Color26;
                if (l < 8) return _Color27;
                if (l < 9) return _Color28;
                if (l < 10) return _Color29;
                if (l < 11) return _Color2A;
                if (l < 12) return _Color2B;
                if (l < 13) return _Color2C;
                if (l < 14) return _Color2D;
                if (l < 15) return _Color2E;
                if (l < 16) return _Color2F;
              }
              if (h < 4) {
                if (l < 2) return _Color31;
                if (l < 3) return _Color32;
                if (l < 4) return _Color33;
                if (l < 5) return _Color34;
                if (l < 6) return _Color35;
                if (l < 7) return _Color36;
                if (l < 8) return _Color37;
                if (l < 9) return _Color38;
                if (l < 10) return _Color39;
                if (l < 11) return _Color3A;
                if (l < 12) return _Color3B;
                if (l < 13) return _Color3C;
                if (l < 14) return _Color3D;
                if (l < 15) return _Color3E;
                if (l < 16) return _Color3F;
              }
              if (h < 5) {
                if (l < 2) return _Color41;
                if (l < 3) return _Color42;
                if (l < 4) return _Color43;
                if (l < 5) return _Color44;
                if (l < 6) return _Color45;
                if (l < 7) return _Color46;
                if (l < 8) return _Color47;
                if (l < 9) return _Color48;
                if (l < 10) return _Color49;
                if (l < 11) return _Color4A;
                if (l < 12) return _Color4B;
                if (l < 13) return _Color4C;
                if (l < 14) return _Color4D;
                if (l < 15) return _Color4E;
                if (l < 16) return _Color4F;
              }
              if (h < 6) {
                if (l < 2) return _Color51;
                if (l < 3) return _Color52;
                if (l < 4) return _Color53;
                if (l < 5) return _Color54;
                if (l < 6) return _Color55;
                if (l < 7) return _Color56;
                if (l < 8) return _Color57;
                if (l < 9) return _Color58;
                if (l < 10) return _Color59;
                if (l < 11) return _Color5A;
                if (l < 12) return _Color5B;
                if (l < 13) return _Color5C;
                if (l < 14) return _Color5D;
                if (l < 15) return _Color5E;
                if (l < 16) return _Color5F;
              }
              if (h < 7) {
                if (l < 2) return _Color61;
                if (l < 3) return _Color62;
                if (l < 4) return _Color63;
                if (l < 5) return _Color64;
                if (l < 6) return _Color65;
                if (l < 7) return _Color66;
                if (l < 8) return _Color67;
                if (l < 9) return _Color68;
                if (l < 10) return _Color69;
                if (l < 11) return _Color6A;
                if (l < 12) return _Color6B;
                if (l < 13) return _Color6C;
                if (l < 14) return _Color6D;
                if (l < 15) return _Color6E;
                if (l < 16) return _Color6F;
              }
              if (h < 8) {
                if (l < 2) return _Color71;
                if (l < 3) return _Color72;
                if (l < 4) return _Color73;
                if (l < 5) return _Color74;
                if (l < 6) return _Color75;
                if (l < 7) return _Color76;
                if (l < 8) return _Color77;
                if (l < 9) return _Color78;
                if (l < 10) return _Color79;
                if (l < 11) return _Color7A;
                if (l < 12) return _Color7B;
                if (l < 13) return _Color7C;
                if (l < 14) return _Color7D;
                if (l < 15) return _Color7E;
                if (l < 16) return _Color7F;
              }
              if (h < 9) {
                if (l < 2) return _Color81;
                if (l < 3) return _Color82;
                if (l < 4) return _Color83;
                if (l < 5) return _Color84;
                if (l < 6) return _Color85;
                if (l < 7) return _Color86;
                if (l < 8) return _Color87;
                if (l < 9) return _Color88;
                if (l < 10) return _Color89;
                if (l < 11) return _Color8A;
                if (l < 12) return _Color8B;
                if (l < 13) return _Color8C;
                if (l < 14) return _Color8D;
                if (l < 15) return _Color8E;
                if (l < 16) return _Color8F;
              }
              if (h < 10) {
                if (l < 2) return _Color91;
                if (l < 3) return _Color92;
                if (l < 4) return _Color93;
                if (l < 5) return _Color94;
                if (l < 6) return _Color95;
                if (l < 7) return _Color96;
                if (l < 8) return _Color97;
                if (l < 9) return _Color98;
                if (l < 10) return _Color99;
                if (l < 11) return _Color9A;
                if (l < 12) return _Color9B;
                if (l < 13) return _Color9C;
                if (l < 14) return _Color9D;
                if (l < 15) return _Color9E;
                if (l < 16) return _Color9F;
              }
              if (h < 11) {
                if (l < 2) return _ColorA1;
                if (l < 3) return _ColorA2;
                if (l < 4) return _ColorA3;
                if (l < 5) return _ColorA4;
                if (l < 6) return _ColorA5;
                if (l < 7) return _ColorA6;
                if (l < 8) return _ColorA7;
                if (l < 9) return _ColorA8;
                if (l < 10) return _ColorA9;
                if (l < 11) return _ColorAA;
                if (l < 12) return _ColorAB;
                if (l < 13) return _ColorAC;
                if (l < 14) return _ColorAD;
                if (l < 15) return _ColorAE;
                if (l < 16) return _ColorAF;
              }
              if (h < 12) {
                if (l < 2) return _ColorB1;
                if (l < 3) return _ColorB2;
                if (l < 4) return _ColorB3;
                if (l < 5) return _ColorB4;
                if (l < 6) return _ColorB5;
                if (l < 7) return _ColorB6;
                if (l < 8) return _ColorB7;
                if (l < 9) return _ColorB8;
                if (l < 10) return _ColorB9;
                if (l < 11) return _ColorBA;
                if (l < 12) return _ColorBB;
                if (l < 13) return _ColorBC;
                if (l < 14) return _ColorBD;
                if (l < 15) return _ColorBE;
                if (l < 16) return _ColorBF;
              }
              if (h < 13) {
                if (l < 2) return _ColorC1;
                if (l < 3) return _ColorC2;
                if (l < 4) return _ColorC3;
                if (l < 5) return _ColorC4;
                if (l < 6) return _ColorC5;
                if (l < 7) return _ColorC6;
                if (l < 8) return _ColorC7;
                if (l < 9) return _ColorC8;
                if (l < 10) return _ColorC9;
                if (l < 11) return _ColorCA;
                if (l < 12) return _ColorCB;
                if (l < 13) return _ColorCC;
                if (l < 14) return _ColorCD;
                if (l < 15) return _ColorCE;
                if (l < 16) return _ColorCF;
              }
              if (h < 14) {
                if (l < 2) return _ColorD1;
                if (l < 3) return _ColorD2;
                if (l < 4) return _ColorD3;
                if (l < 5) return _ColorD4;
                if (l < 6) return _ColorD5;
                if (l < 7) return _ColorD6;
                if (l < 8) return _ColorD7;
                if (l < 9) return _ColorD8;
                if (l < 10) return _ColorD9;
                if (l < 11) return _ColorDA;
                if (l < 12) return _ColorDB;
                if (l < 13) return _ColorDC;
                if (l < 14) return _ColorDD;
                if (l < 15) return _ColorDE;
                if (l < 16) return _ColorDF;
              }
              if (h < 15) {
                if (l < 2) return _ColorE1;
                if (l < 3) return _ColorE2;
                if (l < 4) return _ColorE3;
                if (l < 5) return _ColorE4;
                if (l < 6) return _ColorE5;
                if (l < 7) return _ColorE6;
                if (l < 8) return _ColorE7;
                if (l < 9) return _ColorE8;
                if (l < 10) return _ColorE9;
                if (l < 11) return _ColorEA;
                if (l < 12) return _ColorEB;
                if (l < 13) return _ColorEC;
                if (l < 14) return _ColorED;
                if (l < 15) return _ColorEE;
                if (l < 16) return _ColorEF;
              }

              if (l < 2) return _ColorF1;
              if (l < 3) return _ColorF2;
              if (l < 4) return _ColorF3;
              if (l < 5) return _ColorF4;
              if (l < 6) return _ColorF5;
              if (l < 7) return _ColorF6;
              if (l < 8) return _ColorF7;
              if (l < 9) return _ColorF8;
              if (l < 10) return _ColorF9;
              if (l < 11) return _ColorFA;
              if (l < 12) return _ColorFB;
              if (l < 13) return _ColorFC;
              if (l < 14) return _ColorFD;
              if (l < 15) return _ColorFE;
              return _ColorFF;
            }


            ENDCG
        }
    }
}
