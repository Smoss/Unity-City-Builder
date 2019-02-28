// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/FertilityShader"
{
    Properties
    {
		_PeakColor("PeakColor", Color) = (0, 0, 0, 1)
		_BaseColor("BaseColor", Color) = (0, 0, 0, 1)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
				fixed4 color : COLOR;
            };

            struct v2f
            {
                fixed4 color : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

			sampler2D _VertTex;
			fixed4 _PeakColor;
			fixed4 _BaseColor;

            v2f vert (appdata v)
            {
                v2f o;
				float4 vert = v.vertex;
				//vert.y = -vert.y;
				o.color = v.color;
				//vert.y = 0;            
				//float4 origin = mul(unity_ObjectToWorld, float4(1, 1, 1, 1));
				//o.pos = mul(unity_ObjectToWorld, vert);
				//o.pos.y = origin.y;
				o.pos = UnityObjectToClipPos(vert);
                return o;
            }

			sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                // just invert the colors
                return i.color;
            }
            ENDCG
        }
    }
}
