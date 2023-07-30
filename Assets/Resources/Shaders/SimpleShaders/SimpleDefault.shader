Shader "Custom/SimpleDefault"
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

			#include "ShaderStructures.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
			#include "NMGGeometryHelpers.hlsl"

			float4 _AlbedoColor;
			
			struct Attributes
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
			};

			struct V2F {
				float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
			};

			V2F vert(Attributes input)
			{
				V2F output;

				output.worldPos = input.positionOS;
				output.vertex = TransformObjectToHClip(input.positionOS);
				output.normal = input.normalOS;

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
