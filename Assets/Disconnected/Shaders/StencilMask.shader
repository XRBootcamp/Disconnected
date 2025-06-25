Shader "Unlit/StencilMask"
{
    Properties
    {
        _StencilRef("Stencil Ref", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-10" }
        LOD 100

        Pass
        {
            // Don't draw to color or depth buffers
            ColorMask 0
            ZWrite Off

            // Stencil operation
            Stencil
            {
                Ref [_StencilRef]
                Comp Always
                Pass Replace
            }
        }
    }
} 