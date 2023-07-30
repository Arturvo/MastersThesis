#ifndef __POPULATE_ATTRIBUTES_INCLUDED__
#define __POPULATE_ATTRIBUTES_INCLUDED__

#define UNITY_INDIRECT_DRAW_ARGS IndirectDrawArgs
#include "UnityIndirect.cginc"

Attributes PopulateAttributes(Attributes input)
{
    InitIndirectDrawArgs(0);

    uint cmdID = GetCommandID(0);
    uint instanceID = GetIndirectInstanceID(input.instanceID);
    float3 pos = _Positions[_Triangles[GetIndirectVertexID(input.id)] + _BaseVertexIndex];

    input.positionOS = mul(_ObjectToWorld, float4(pos + float3(instanceID, cmdID, 0.0f), 1.0f));
    input.normalOS = _Normals[_Triangles[GetIndirectVertexID(input.id)] + _BaseVertexIndex];
 
    float3 t1 = cross(input.normalOS, float3(0, 0, 1));
    float3 t2 = cross(input.normalOS, float3(0, 1, 1));
    if (length(t1) > length(t2))
    {
        input.tangentOS = float4(t1, 1);
    }
    else
    {
        input.tangentOS = float4(t2, 1);
    }
    return input;
}
#endif