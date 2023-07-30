#ifndef SG_LIT_META_INCLUDED
#define SG_LIT_META_INCLUDED

V2G vert(Attributes input)
{
    return PopulateGeometryInput(input);
}

[maxvertexcount(3)]
void geom(point V2G IN[1], inout TriangleStream<PackedVaryings> OUT)
{
    OUT.Append(PopulateVaryingsFromGeometry(IN[0], IN[0].v1, IN[0].n1));
    OUT.Append(PopulateVaryingsFromGeometry(IN[0], IN[0].v2, IN[0].n2));
    OUT.Append(PopulateVaryingsFromGeometry(IN[0], IN[0].v3, IN[0].n3));
}

half4 frag(PackedVaryings packedInput) : SV_TARGET
{
    Varyings unpacked = UnpackVaryings(packedInput);
    UNITY_SETUP_INSTANCE_ID(unpacked);
    SurfaceDescription surfaceDescription = BuildSurfaceDescription(unpacked);

    #if _ALPHATEST_ON
        clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
    #endif

    MetaInput metaInput = (MetaInput)0;
    metaInput.Albedo = surfaceDescription.BaseColor;
    metaInput.Emission = surfaceDescription.Emission;
#ifdef EDITOR_VISUALIZATION
    metaInput.VizUV = unpacked.texCoord1.xy;
    metaInput.LightCoord = unpacked.texCoord2;
#endif

    return UnityMetaFragment(metaInput);
}

#endif
