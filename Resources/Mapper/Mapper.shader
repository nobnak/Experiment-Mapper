Shader "Hidden/Mapper" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Depth ("Depth", Range(-1, 1)) = 0.5
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			
			sampler2D _MainTex;
			float _Depth;

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
				return o;
			}

			fixed4 frag (v2f i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
