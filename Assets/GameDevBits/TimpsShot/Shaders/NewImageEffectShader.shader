Shader "Custom/SilhouetteImageEffect"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,0)
    }
    SubShader
    {
        Tags { "RenderType"="transparent" "Queue"="Transparent" }
      
         ZWrite Off
         Cull Off
         Lighting Off
         ZTest Less
         Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            fixed4 _Color;

            v2f vert(appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the original screen pixel
                fixed4 col = tex2D(_MainTex, i.uv);

                // Preserve transparency and replace non-transparent pixels with the specified color
                if (col.a > 0 ) // If pixel is not fully transparent or opaque
                {
                    col.rgb = _Color.rgb;
                }
                return col;
            }
            ENDCG
        }
    }
}
