Shader "Custom/IndirectSimpleGeometry"
{
	Properties
	{
		_AlbedoColor("Color", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

		Pass
		{
			Name "ForwardLit"
			Tags { "LightMode" = "UniversalForward" }
			Cull Back

			HLSLPROGRAM

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0
			#pragma require geometry

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT

			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#define UNITY_INDIRECT_DRAW_ARGS IndirectDrawArgs
			#include "UnityIndirect.cginc"

			#include "ShaderStructures.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
			#include "NMGGeometryHelpers.hlsl"

			uniform StructuredBuffer<Triangle> triangleBuffer;
			uniform uint _BaseVertexIndex;
			uniform float4x4 _ObjectToWorld;
			float4 _AlbedoColor;

			struct V2G {

				float3 v1 : TEXCOORD0;
				float3 v2 : TEXCOORD1;
				float3 v3 : TEXCOORD2;
				float3 n1 : TEXCOORD3;
				float3 n2 : TEXCOORD4;
				float3 n3 : TEXCOORD5;
			};

			struct G2F {
				float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
			};

			V2G vert(uint svVertexID: SV_VertexID, uint svInstanceID : SV_InstanceID)
			{
				InitIndirectDrawArgs(0);
				V2G output;

				uint id = GetIndirectVertexID(svVertexID) + _BaseVertexIndex;
				Triangle triangleStruct = triangleBuffer[id/3];

				output.v1 = triangleStruct.v1;
				output.v2 = triangleStruct.v2;
				output.v3 = triangleStruct.v3;
				output.n1 = triangleStruct.n1;
				output.n2 = triangleStruct.n2;
				output.n3 = triangleStruct.n3;

				return output;
			}

			[maxvertexcount(3)]
			void geom(point V2G IN[1], inout TriangleStream<G2F> OUT)
			{
				G2F output;

				output.worldPos = IN[0].v1;

				output.vertex = TransformObjectToHClip(IN[0].v1.xyz);
				output.normal = IN[0].n1;
				OUT.Append(output);

				output.worldPos = IN[0].v2;
				output.vertex = TransformObjectToHClip(IN[0].v2.xyz);
				output.normal = IN[0].n2;
				OUT.Append(output);

				output.worldPos = IN[0].v3;
				output.vertex = TransformObjectToHClip(IN[0].v3.xyz);
				output.normal = IN[0].n3;
				OUT.Append(output);
			}

			float4 frag(G2F input) : COLOR
			{
			   InputData lightingInput = (InputData)0;
			   lightingInput.positionWS = input.worldPos;
			   lightingInput.normalWS = normalize(input.normal);
			   lightingInput.viewDirectionWS = GetViewDirectionFromPosition(input.worldPos);
			   lightingInput.shadowCoord = CalculateShadowCoord(input.worldPos, input.vertex);

			   return UniversalFragmentBlinnPhong(lightingInput, _AlbedoColor.xyz, 1, 0, 0, 1, input.normal);
			}

			ENDHLSL
		}
	}
}
