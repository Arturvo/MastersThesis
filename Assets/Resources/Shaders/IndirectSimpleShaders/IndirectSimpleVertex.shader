Shader "Custom/IndirectSimpleVertex"
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

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT

			#pragma vertex vert
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

			struct V2F {
				float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
			};

			V2F vert(uint svVertexID: SV_VertexID, uint svInstanceID : SV_InstanceID)
			{
				InitIndirectDrawArgs(0);
				V2F output;

				uint id = GetIndirectVertexID(svVertexID) + _BaseVertexIndex;
				Triangle triangleStruct = triangleBuffer[id / 3];

				if (id % 3 == 0)
				{
					output.worldPos = triangleStruct.v1;
					output.vertex = TransformObjectToHClip(triangleStruct.v1.xyz);
					output.normal = triangleStruct.n1;
				}
				else if (id % 3 == 1)
				{
					output.worldPos = triangleStruct.v2;
					output.vertex = TransformObjectToHClip(triangleStruct.v2.xyz);
					output.normal = triangleStruct.n2;
				}
				else
				{
					output.worldPos = triangleStruct.v3;
					output.vertex = TransformObjectToHClip(triangleStruct.v3.xyz);
					output.normal = triangleStruct.n3;
				}

				return output;
			}

			float4 frag(V2F input) : COLOR
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
