Shader "Hidden/Mapper" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Depth ("Depth", Range(-1, 1)) = 0.5

		_Wireframe_Color("Wire Color", Color) = (1,1,1,1)
		_Wireframe_Gain("Wire Gain", Float) = 1
		_Wireframe_Repeat("Wire Repeat", Range(1, 10)) = 1

		_Feature("Feature Flags", int) = -1
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Assets/Packages/Gist/CGIncludes/Wireframe.cginc"
			struct v2f {
				float2 uv : TEXCOORD0;
				float4 bary : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};
			
			sampler2D _MainTex;
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
				if (_ProjectionParams.x < 0)
					uvin.y = 1 - uvin.y;

				v2f o;
				o.vertex = vout.z * float4(vout.xy, _Depth, 1);
				o.uv = uvin;
				o.bary = bary * _Wireframe_Repeat;
				return o;
			}

			float4 frag (v2f i) : SV_Target {
				float4 c = 0;
				float4 cmain = tex2D(_MainTex, i.uv);

				if ((_Feature & 1) != 0)
					c += float4(cmain.rgb, 1) * cmain.a;

				if ((_Feature & 4) != 0)
					c +=  float4(i.uv, 0, 1);
				if ((_Feature & 8 )!= 0)
					c = lerp(c, _Wireframe_Color, wireframe(frac(i.bary)));

				return c;
			}
			ENDCG
		}
	}
}
