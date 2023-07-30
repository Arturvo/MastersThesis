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
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(unpacked);
    SurfaceDescription surfaceDescription = BuildSurfaceDescription(unpacked);

    #if _ALPHATEST_ON
        half alpha = surfaceDescription.Alpha;
        clip(alpha - surfaceDescription.AlphaClipThreshold);
    #elif _SURFACE_TYPE_TRANSPARENT
        half alpha = surfaceDescription.Alpha;
    #else
        half alpha = 1;
    #endif

    half4 color = half4(surfaceDescription.BaseColor, alpha);
    return color;
}
