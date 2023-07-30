#ifndef __POPULATE_ATTRIBUTES_INCLUDED__
#define __POPULATE_ATTRIBUTES_INCLUDED__

Attributes PopulateAttributes(Attributes input)
{
    Triangle triangleStruct = triangleBuffer[input.id / 3];
    float3 localPos;
    float3 normal;
    if (input.id % 3 == 0)
    {
        localPos = triangleStruct.v1;
        normal = triangleStruct.n1;
    }
    else if (input.id % 3 == 1)
    {
        localPos = triangleStruct.v2;
        normal = triangleStruct.n2;
    }
    else
    {
        localPos = triangleStruct.v3;
        normal = triangleStruct.n3;
    }
    input.positionOS = localPos;
    input.normalOS = normal;

    float3 t1 = cross(normal, float3(0, 0, 1));
    float3 t2 = cross(normal, float3(0, 1, 1));
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