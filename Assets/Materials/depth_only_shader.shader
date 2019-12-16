// Inspired by: http://wiki.unity3d.com/index.php/DepthMask

Shader "Custom/DepthOnlyShader"
{
    SubShader
    {
        Tags { "Queue"="Geometry+10" }

		// Don't draw in the RGBA channels; just the depth buffer
		ColorMask 0
		ZWrite On

		// Do nothing else.
		Pass {}
    }
}
