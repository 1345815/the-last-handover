Shader "Hidden/LastHandoverVhs"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            Cull Off ZWrite Off ZTest Always
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform float _NoiseAmount;
            uniform float _ScanlineAmount;
            uniform float _ChromaAmount;
            uniform float _VignetteAmount;
            uniform float _TrackingJitter;

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float t = _Time.y;

                // 偶发磁迹横向撕裂
                float bandGate = step(0.9965, hash(float2(floor(t * 2.0), floor(uv.y * 36.0))));
                uv.x += bandGate * _TrackingJitter * (hash(float2(t, uv.y)) - 0.5) * 2.0;

                // 整体轻微漂移
                uv.x += (hash(float2(floor(t * 9.0), 1.0)) - 0.5) * 0.0022;

                // RGB 色差，向画面边缘增强
                float2 dir = (uv - 0.5) * _ChromaAmount;
                fixed r = tex2D(_MainTex, uv + dir).r;
                fixed g = tex2D(_MainTex, uv).g;
                fixed b = tex2D(_MainTex, uv - dir).b;
                fixed4 col = fixed4(r, g, b, 1);

                // 扫描线
                float scan = 0.5 + 0.5 * sin(uv.y * _ScreenParams.y * 3.14159);
                col.rgb *= 1.0 - _ScanlineAmount * scan;

                // 胶片颗粒噪点
                float n = hash(uv * _ScreenParams.xy + frac(t) * 173.0);
                col.rgb += (n - 0.5) * _NoiseAmount;

                // 偶发亮度闪动
                col.rgb *= 1.0 - step(0.997, hash(float2(floor(t * 13.0), 7.0))) * 0.35;

                // 暗角
                float2 d = abs(uv - 0.5);
                float vig = 1.0 - _VignetteAmount * smoothstep(0.28, 0.78, max(d.x, d.y));
                col.rgb *= vig;

                return col;
            }
            ENDCG
        }
    }
    Fallback Off
}
