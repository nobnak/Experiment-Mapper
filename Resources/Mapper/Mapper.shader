Shader "Hidden/Mapper" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_BlendTex ("Blend", 2D) = "white" {}
		_Depth ("Depth", Range(-1, 1)) = 0.5

		_Wireframe_Color("Wire Color", Color) = (1,1,1,1)
		_Wireframe_Gain("Wire Gain", Float) = 1
		_Wireframe_Repeat("Wire Repeat", Range(1, 10)) = 1

		_Feature("Feature Flags", int) = -1
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always

		CGINCLUDE
			#pragma multi_compile ___ OUTPUT_VIN
			#include "UnityCG.cginc"
			#include "Assets/Packages/Gist/CGIncludes/Wireframe.cginc"
			
            struct appdata_if {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
			struct v2f {
				float4 uv : TEXCOORD0;
				float4 bary : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};
            struct v2f_if {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			sampler2D _BlendTex;
			float _Depth;
			int _Feature;
			float4 _Wireframe_Color;
			int _Wireframe_Repeat;

			StructuredBuffer<float3> voutputs;
			StructuredBuffer<float2> vinputs;
			StructuredBuffer<int> indices;
			StructuredBuffer<float4> barys;

			v2f vert (uint vid : SV_VertexID) {
				int vindex = indices[vid];
				float3 vout = voutputs[vindex];
				float2 vin = vinputs[vindex];
				float4 bary = barys[vindex];

				float4 uv = 0.5 * (float4(vin.xy, vout.xy) + 1.0);
				if (_MainTex_TexelSize.y < 0)
					uv.yw = 1 - uv.yw;

				float4 vertex = 0;
				#ifdef OUTPUT_VIN
				vertex = float4(vin.xy, _Depth, 1);
				#else
				vertex = vout.z * float4(vout.xy, _Depth, 1);
				#endif

				if (_ProjectionParams.x < 0)
					vertex.y *= -1;

				v2f o;
				o.vertex = vertex;
				o.uv = uv;
				o.bary = bary;
				return o;
			}
            v2f_if vert_if (appdata_if v) {
                v2f_if o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
			
			float blend(float4 b) {
				return 2 * min(min(b.x, b.y), min(b.z, b.w));
			}

		ENDCG

		// 0
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			float4 frag (v2f i) : SV_Target {
				float4 cmain = tex2D(_MainTex, i.uv.xy);
				float4 cblend = tex2D(_BlendTex, i.uv.zw);
				float b = cblend.r;
				#ifdef OUTPUT_VIN
				b = 1;
				#endif

				float4 c = cmain * float4(b, b, b, 1);

				return c;
			}
			ENDCG
		}

		// 1
		Pass {
			Blend One One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4 frag(v2f i) : SV_Target {
				float4 c = 0;
				c.x = blend(i.bary);
				//c.y = 1;
				return c;
			}
			ENDCG
		}

		// 2
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4 frag (v2f i) : SV_Target {
				float4 cmain = tex2D(_MainTex, i.uv);
				float b = blend(i.bary);
				float4 c = saturate(b / cmain.r);
				return c;
			}
			ENDCG
		}

		// 3
		Pass {
			CGPROGRAM
			#pragma vertex vert_if
			#pragma fragment frag

			float4 frag (v2f_if i) : SV_Target {
				float4 cblend = tex2D(_BlendTex, i.uv);
				float b = cblend.r;
				return b;
			}
			ENDCG
		}

		// 4
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4 frag (v2f i) : SV_Target {
				float4 c0 = _Wireframe_Color;
				float4 c1 = float4(c0.gbr, c0.a); //float4(1 - c0.rgb, c0.a);
				float wb = wireframe(frac(i.bary));
				float wu = wireframe(frac(i.uv.xy * _Wireframe_Repeat));

				float4 c = lerp(c0, c1, wb);
				return float4(c.rgb, c.a * saturate(wb + wu));
			}
			ENDCG
		}
	}
}
