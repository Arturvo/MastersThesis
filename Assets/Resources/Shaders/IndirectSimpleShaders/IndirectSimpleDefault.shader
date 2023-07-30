Shader "Custom/IndirectSimpleDefault"
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

			StructuredBuffer<int> _Triangles;
			StructuredBuffer<float3> _Positions;
			StructuredBuffer<float3> _Normals;
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

				uint cmdID = GetCommandID(0);
				uint instanceID = GetIndirectInstanceID(svInstanceID);
				float3 pos = _Positions[_Triangles[GetIndirectVertexID(svVertexID)] + _BaseVertexIndex];
				output.worldPos = mul(_ObjectToWorld, float4(pos + float3(instanceID, cmdID, 0.0f), 1.0f));
				output.vertex = TransformObjectToHClip(output.worldPos);
				output.normal = _Normals[_Triangles[GetIndirectVertexID(svVertexID)] + _BaseVertexIndex];

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
