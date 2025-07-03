FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	Forward();
	Depth();
}

COMMON
{
	#define S_SPECULAR 1
	#define F_DYNAMIC_REFLECTIONS 0
	#include "common/shared.hlsl"
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		
		PixelInput i = ProcessVertex( v );
		return FinalizeVertex( i );
	}
}

PS
{
	#include "common/pixel.hlsl"


    // Good old SDF functions from Inigo Quilez
    float sdCircle(float2 p, float r)
    {
        return length(p) - r;
    }

    float sdBox(in float2 p, in float2 b)
    {
        float2 d = abs(p) - b;
        return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
    }

	CreateTexture2D( g_ReflectionTexture ) < Attribute( "ReflectionTexture" ); SrgbRead( true ); > ;    

    float ScopeFOV < Attribute( "ScopeFOV" ); >;
    float2 ScopeEyeOffset < Attribute( "ScopeEyeOffset" ); >;

    float4 MainPs( PixelInput i ) : SV_Target0
    {
        float2 uv = i.vTextureCoords.xy;
        float dist = distance( uv, float2( 0.5, 0.5 ) );

        // Hard circular mask
        float radius = 0.5;
        if ( dist > radius )
            discard;

        // Vignette (black in center → transparent)
        float vignetteStart = 0.0;
        float vignetteEnd = 0.5;
        float vignetteStrength = 1.0;

        // Vignette mask centered on projected reticle (collimator style)
        float2 vignetteCenter = 0.5 + ScopeEyeOffset * 0.6;
        float vignetteDist = distance( uv, vignetteCenter );

        float vignetteRadius = 0.43;       // radius of visible circular window
        float vignetteEdge = 0.05;         // softness of the edge

        float vignette = 1.0 - smoothstep( vignetteRadius, vignetteRadius + vignetteEdge, vignetteDist );

        // Sample fixed scope image (does NOT shift)
        float3 texColor = g_ReflectionTexture.Sample( g_sAniso, uv ).rgb;
        texColor = lerp( float3( 0, 0, 0 ), texColor, vignette );

        // Apply reticle shift — this affects only reticle SDF space
        float2 centerUv = (uv - 0.5) - (ScopeEyeOffset * 0.3);

        float lineWidth = 0.001;
        float lineLength = 0.5;

        // Central crosshair
        float dH = sdBox( centerUv, float2( lineLength, lineWidth ) );
        float dV = sdBox( centerUv, float2( lineWidth, lineLength ) );
        float lineAlpha = 1.0 - smoothstep( 0.0, 0.001, min(dH, dV) );

        float3 reticleColor = float3( 0, 0, 0 );
        texColor = lerp( texColor, reticleColor, lineAlpha );

        // -- Mildots --
        float baseSpacing = 0.025;
        float spacing = baseSpacing * (40.0 / ScopeFOV);
        float dotRadius = 0.008;
        float dotAlpha = 0.0;

        int dotCount = 6;

        for ( int x = 1; x <= dotCount; ++x )
        {
            float offset = x * spacing;

            float dTop    = sdCircle( centerUv - float2( 0, offset ), dotRadius );
            float dBot    = sdCircle( centerUv + float2( 0, offset ), dotRadius );
            float dLeft   = sdCircle( centerUv - float2( offset, 0 ), dotRadius );
            float dRight  = sdCircle( centerUv + float2( offset, 0 ), dotRadius );

            dotAlpha += 1.0 - smoothstep( 0.0, 0.001, dTop );
            dotAlpha += 1.0 - smoothstep( 0.0, 0.001, dBot );
            dotAlpha += 1.0 - smoothstep( 0.0, 0.001, dLeft );
            dotAlpha += 1.0 - smoothstep( 0.0, 0.001, dRight );
        }

        texColor = lerp( texColor, reticleColor, saturate( dotAlpha ) );

        // -- Thick outer lines --
        float outerLineWidth  = 0.007;
        float outerLineLength = 0.5;
        float innerLineCut = spacing * (dotCount + 0.5);

        float thickLineAlpha = 0.0;

        if ( abs(centerUv.y) > innerLineCut )
        {
            float dTopBar    = sdBox( centerUv, float2( outerLineWidth, outerLineLength ) );
            thickLineAlpha += 1.0 - smoothstep( 0.0, 0.001, dTopBar );
        }

        if ( abs(centerUv.x) > innerLineCut )
        {
            float dSideBar = sdBox( centerUv, float2( outerLineLength, outerLineWidth ) );
            thickLineAlpha += 1.0 - smoothstep( 0.0, 0.001, dSideBar );
        }

        texColor = lerp( texColor, reticleColor, saturate( thickLineAlpha ) );

        return float4( texColor.rgb, 1 );
    }

}