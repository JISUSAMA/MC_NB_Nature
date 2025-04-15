Shader "Custom/SBS_Right"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _OnscreenPercent("OnscreenPercent", Vector) = (0,0,1,1)
        _EnableOnscreenPercent("EnableOnscreenPercent", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _OnscreenPercent;
            float _EnableOnscreenPercent;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float4 onscreenPercentClipPos = float4(0, 0, 0, 1);
                o.uv = v.uv;
                float offsetX = _OnscreenPercent.x;
                float offsetY = _OnscreenPercent.y;
                float width = _OnscreenPercent.z;
                float height = _OnscreenPercent.w;

#if defined(UNITY_REVERSED_Z)
                onscreenPercentClipPos.x = (v.uv.x * width + offsetX) * 2 - 1;
                onscreenPercentClipPos.y = -(v.uv.y * height + offsetY) * 2 + 1;
                onscreenPercentClipPos.z = 1;
#else
                onscreenPercentClipPos.x = (v.uv.x * width + offsetX) * 2 - 1;
                onscreenPercentClipPos.y = -((1 - v.uv.y) * height - offsetY + 1 - height) * 2 + 1;
                onscreenPercentClipPos.z = -1;
#endif
                o.vertex = lerp(o.vertex, onscreenPercentClipPos, _EnableOnscreenPercent);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                i.uv.x = i.uv.x * 0.5 + 0.5; // Modify UV to sample right half
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}