Shader "RukhankaDebugThickLineDrawer"
{
SubShader
{
    PackageRequirements
    {
        "com.unity.render-pipelines.high-definition": "1.0.0"
    }
	Tags
	{
		"RenderPipeline" = "HDRenderPipeline"
		"RenderType" = "HDUnlitShader"
		"Queue" = "Transparent+0"
	}

	Pass
	{
		Name "ForwardOnly"
		Tags
		{
			"LightMode" = "ForwardOnly"
		}

        Blend SrcAlpha OneMinusSrcAlpha
		ZTest off
        Cull off

		HLSLPROGRAM
		#pragma target 3.0

		#pragma vertex VSThickLines
		#pragma fragment PS
        #define IS_HDRP

		#include "RukhankaDebugDrawer.hlsl"

		ENDHLSL
	}
}

/////////////////////////////////////////////////////////////////////////////////

SubShader
{
    PackageRequirements
    {
        "com.unity.render-pipelines.universal": "1.0.0"
    }
	Tags
	{
        "RenderPipeline"="UniversalPipeline"
		"Queue" = "Transparent+0"
	}

	Pass
	{
		Tags
		{
            "LightMode" = "UniversalForward"
		}

        Blend SrcAlpha OneMinusSrcAlpha
		ZTest off
        Cull off

		HLSLPROGRAM
		#pragma target 3.0

		#pragma vertex VSThickLines
		#pragma fragment PS

		#include "RukhankaDebugDrawer.hlsl"

		ENDHLSL
	}
}
}
