Shader "Unlit/ImpostorRenderShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

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

            struct v2g
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

			struct g2f {
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
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

            fixed4 frag (g2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }

			[maxvertexcount(6)]
			void geom(point v2g input[1], inout TriangleStream<g2f> tristream) {
				g2f output;

				// Generate a plane
				output.uv = float2(0, 0);
				output.vertex = input[0].vertex + float3(5, 0, 5);
				triStream.Append(output);

				output.uv = float2(1, 1);
				output.vertex = input[0].vertex + float3(-5, 0, -5);
				triStream.Append(output);

				output.uv = float2(1, 0);
				output.vertex = input[0].vertex + float3(-5, 0, 5);
				triStream.Append(output);

				output.uv = float2(0, 0);
				output.vertex = input[0].vertex + float3(5, 0, 5);
				triStream.Append(output);

				output.uv = float2(0, 1);
				output.vertex = input[0].vertex + float3(5, 0, -5);
				triStream.Append(output);

				output.uv = float2(1, 0);
				output.vertex = input[0].vertex + float3(-5, 0, 5);
				triStream.Append(output);
			}
            ENDCG
        }
    }
}
