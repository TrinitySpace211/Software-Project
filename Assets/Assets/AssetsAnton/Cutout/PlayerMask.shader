Shader "Custom/PlayerMask"
{
    Properties {}

    SubShader
    {
        Tags { "Queue"="Geometry-10" "RenderType"="Transparent" "IgnoreProjector"="True" }

        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            ColorMask 0

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
                ReadMask 1
                WriteMask 1
            }
        }
    }

    FallBack Off
}
