Shader "Unlit/DepthDumpShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_DepthTex("DepthTexture", 2D) = "white" {}
		_DepthMin("DepthMin", Float) = 1
		_DepthMax("DepthMax", Float) = 2
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

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

			struct f
			{
				fixed4 colour : SV_Target;
				float depth : DEPTH;
			};

            sampler2D _MainTex;
			sampler2D _DepthTex;
            float4 _MainTex_ST;
			float _DepthMin;
			float _DepthMax;

			/*
			Despite Unity documentation's LIES, here are the values Unity supplies in _ZBufferParams
			
			_ZBufferParams.x = far/near
			_ZBufferParams.y = 1
			_ZBufferParams.z = 1/near
			_ZBufferParams.w = 1/far

			LinearEyeDepth:
			return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
			*/

			inline float OldLinearEyeDepth(float z)
			{
				return 1.0 / (1/_DepthMin * z + 1/_DepthMax);
			}

			inline float LinearToDepth(float linearZ)
			{
				return (1 / linearZ - _ZBufferParams.w) / _ZBufferParams.z;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            f frag (v2f i)
            {
				f output;
                // sample the textures
                fixed4 col = tex2D(_MainTex, i.uv);
				float bufferDepth = tex2D(_DepthTex, i.uv);

				// Clip transparent and high-depth pixels
				clip(col.a - 0.1);
				clip(bufferDepth - 0.0001);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

				float depth = OldLinearEyeDepth(bufferDepth) + 9;
				output.depth = LinearToDepth(depth);
				output.colour = col;

				//output.colour = (depth) / 50;

                return output;
            }

			ENDCG
				/*struct v2f {
					float4 pos : SV_POSITION;
					float2 depth : TEXCOORD0;
				};

				v2f vert(appdata_base v) {
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					UNITY_TRANSFER_DEPTH(o.depth);
					return o;
				}

				half4 frag(v2f i) : SV_Target {
					UNITY_OUTPUT_DEPTH(i.depth);
				}*/
        }
    }
}
