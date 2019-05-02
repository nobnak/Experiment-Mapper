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

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 bary : TEXCOORD1;
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

				float2 uvin = saturate(0.5 * (vin + 1.0));
				if (_MainTex_TexelSize.y < 0)
					uvin.y = 1 - uvin.y;

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
				o.uv = uvin;
				o.bary = bary;
				return o;
			}
			
			float blend(float4 b) {
				return 2 * min(min(b.x, b.y), min(b.z, b.w));
			}

		ENDCG

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			float4 frag (v2f i) : SV_Target {
				float4 c = 0;
				float4 cmain = tex2D(_MainTex, i.uv);
				float4 cblend = tex2D(_BlendTex, i.uv);
				float b = blend(i.bary);

				if ((_Feature & 1) != 0)
					c += float4(cmain.rgb, 1) * cmain.a;

				if ((_Feature & 2) != 0)
					c += saturate(b / cblend.r);

				if ((_Feature & 4) != 0)
					c +=  float4(i.uv, 0, 1);
				if ((_Feature & 8 )!= 0)
					c = lerp(c, _Wireframe_Color, wireframe(frac(i.bary * _Wireframe_Repeat)));

				return c;
			}
			ENDCG
		}

		Pass {
			Blend One One

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			float4 frag(v2f i) : SV_Target {
				float4 c;
				c.x = blend(i.bary);
				c.y = 1;
				return c;
			}
			ENDCG
		}
	}
}
