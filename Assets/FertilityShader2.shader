Shader "Custom/FertilityShader2"
{
    Properties
    {
		_PeakColor("Peak Color", Color) = (0, 0, 0, 1)
		_BaseColor("Base Color", Color) = (0, 0, 0, 1)
		_Water("Water Color", Color) = (0,0,0,1)
		_WaterHeight("Water Height", Float) = 0.0
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
			float2 uv_MainTex;
			fixed4 color;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		float4 _PeakColor;
		float4 _BaseColor;
		float4 _Water;
		float _WaterHeight;
		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			if (v.vertex.y >= _WaterHeight)
			{
				o.color = lerp(_BaseColor, _PeakColor, v.color.a);
			}
			else
			{
				o.color = _Water;
			}
		}
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
			
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = IN.color.rgb * c;
            // Metallic and smoothness come from slider variables
            o.Alpha = 1;
        }
        ENDCG
    }
}
