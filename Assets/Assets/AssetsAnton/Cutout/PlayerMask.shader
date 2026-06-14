Shader "Custom/PlayerMask"
{
    Properties {}

    SubShader
    {
        //Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Pass
        {
            // Keine Farb- oder Alpha-Outputs nötig — wir schreiben nur Stencil
            ColorMask 0

            // Stelle sicher, dass die Stencil-Writes auch dann passieren,
            // wenn die Sphäre hinter Geometrie liegt.
            ZTest Always
            ZWrite Off

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }
        }
    }

    //FallBack Off
}
