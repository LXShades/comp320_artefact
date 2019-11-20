Shader "Unlit/ImpostorRenderShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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

			struct f2t
			{
				fixed4 main : COLOR0;
				fixed4 impostor : COLOR1;
			};

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            f2t frag (v2f i)
            {
				f2t output;

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

				clip(col.a - 0.1f);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

				output.main = col;
				output.impostor = col;

                return output;
            }
            ENDCG
        }
    }
}
