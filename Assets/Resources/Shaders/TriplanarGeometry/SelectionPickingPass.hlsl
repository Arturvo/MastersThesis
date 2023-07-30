#ifndef SG_SELECTION_PICKING_PASS_INCLUDED
#define SG_SELECTION_PICKING_PASS_INCLUDED

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
        // This isn't defined in the sprite passes. It looks like the built-in legacy shader will use this as it's default constant
        float alphaClipThreshold = 0.01f;
        #if ALPHA_CLIP_THRESHOLD
            alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
        #endif
        clip(surfaceDescription.Alpha - alphaClipThreshold);
    #endif

    half4 outColor = 0;
    #ifdef SCENESELECTIONPASS
        // We use depth prepass for scene selection in the editor, this code allow to output the outline correctly
        outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
    #elif defined(SCENEPICKINGPASS)
        outColor = _SelectionID;
    #endif

    return outColor;
}

#endif
