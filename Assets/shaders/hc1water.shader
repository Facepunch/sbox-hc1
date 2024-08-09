HEADER
{
    Description = "Template Shader for S&box";
    DevShader = true;
}

FEATURES
{
    #include "common/features.hlsl"
}

COMMON
{
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

    float g_flWaveHeight < UiType( Slider ); Range( 0, 128 ); Default( 16 ); UiGroup( "Vertex Animation,10/10" ); >;
    float g_flWaveFrequency < UiType( Slider ); Range( 0, 10 ); Default( 2.0 ); UiGroup( "Vertex Animation,10/20" ); >;
    float g_flWaveSpeed < UiType( Slider ); Range( 0, 5 ); Default( 1.0 ); UiGroup( "Vertex Animation,10/30" ); >;

    PixelInput MainVs( VertexInput i )
    {
        PixelInput o = ProcessVertex( i );

        // Add vertex animation for water effect
        float3 worldPos = i.vPositionOs.xyz;
        float wave = sin(g_flWaveFrequency * worldPos.x + g_flTime * g_flWaveSpeed) * 
                     cos(g_flWaveFrequency * worldPos.z + g_flTime * g_flWaveSpeed);
        
        o.vPositionPs.y += wave * g_flWaveHeight;

        return FinalizeVertex( o );
    }
}

//=========================================================================================================================

PS
{
    #include "common/pixel.hlsl"
    #include "procedural.hlsl"

	//
	// Material
	//
	float g_flTilingScale < UiType( Slider ); Range( 0, 12 ); Default( 9 ); UiGroup( "Material,10/30" ); >;
    float g_flRoughness < UiType( Slider ); Range( 0, 1 ); Default( 0.9 ); UiGroup( "Material,10/30" ); >;
    float g_flMetallic < UiType( Slider ); Range( 0, 1 ); Default( 0.0 ); UiGroup( "Material,10/30" ); >;
    float g_flOpacity < UiType( Slider ); Range( 0, 1 ); Default( 1.0 ); UiGroup( "Material,10/30" ); >;
    float g_flNormalScale < UiType( Slider ); Range( 0, 1 ); Default( 1.0 ); UiGroup( "Material,10/30" ); >;

	//
	// General
	//
    SamplerState g_sSampler < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
    CreateInputTexture2D( NormalMapA, Linear, 8, "", "_normal", "General,10/10", Default3( 1.0, 1.0, 1.0 ) );
    Texture2D g_tNormalMapA < Channel( RGB, Box( NormalMapA ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;

    CreateInputTexture2D( NormalMapB, Linear, 8, "", "_normal", "General,10/10", Default3( 1.0, 1.0, 1.0 ) );
    Texture2D g_tNormalMapB < Channel( RGB, Box( NormalMapB ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;

    float3 g_vColor < UiType( Color ); Default3( 1.0, 1.0, 1.0 ); UiGroup( "General,10/20" ); >;
    float3 g_vDeepColor < UiType( Color ); Default3( 1.0, 1.0, 1.0 ); UiGroup( "General,10/20" ); >;

    float g_flScale < UiType( Slider ); Range( 1, 10 ); UiGroup( "General,10/30" ); >;
    float2 g_flSpeedA < UiType( Slider ); Range2( -1, -1, 1, 1 ); UiGroup( "General,10/30" ); >;
    float2 g_flSpeedB < UiType( Slider ); Range2( -1, -1, 1, 1 ); UiGroup( "General,10/30" ); >;

	float g_flWaterDepthScale < UiType( Slider ); Range( 1, 30 ); Default( 14.0 ); UiGroup( "Water,10/30" ); >;

	//
	// Shoreline
	//
    float g_flShorelineDepthScale < UiType( Slider ); Range( 1, 10 ); Default( 7.0 ); UiGroup( "Water,10/30" ); >;
    float g_flShorelineFeatheringScale < UiType( Slider ); Range( 1, 10 ); Default( 5.0 ); UiGroup( "Water,10/30" ); >;

	//
	// Engine
	//
    BoolAttribute( bWantsFBCopyTexture, true );
    BoolAttribute( translucent, true );
    CreateTexture2D( g_tFrameBufferCopyTexture ) < Attribute("FrameBufferCopyTexture");   SrgbRead( true ); Filter(MIN_MAG_MIP_LINEAR);    AddressU( MIRROR );     AddressV( MIRROR ); >;

	//
	// Planar reflections
	//
	CreateTexture2D( g_tPlanarReflectionTexture ) < Attribute("PlanarReflectionTexture");   SrgbRead( true ); Filter(MIN_MAG_MIP_LINEAR);    AddressU( MIRROR );     AddressV( MIRROR ); >;
	bool g_bPlanarReflections < UiType( CheckBox ); UiGroup( "General,10/30" ); >;
	BoolAttribute( g_bPlanarReflections, g_bPlanarReflections );

	float GetLayeredSimplex( float2 noiseUV )
	{
		float fNoise = 0.0;
		float fScale = 1.0;
		float fWeight = 0.0;

		for ( int i = 0; i < 4; i++ )
		{
			fNoise += Simplex2D( noiseUV * fScale ) * fWeight;
			fWeight += 0.1;
			fScale *= 4.0;
		}

		return fNoise;
	}

	//
	// Mixed/blended and animated wave normal maps
	//
    float3 GetNormalForPixel( float3 normal, float2 pixel )
    {
        float2 pixelA = pixel.xy + ( g_flSpeedA * float2( g_flTime, g_flTime ) );
        float2 pixelB = pixel.xy + ( g_flSpeedB * float2( g_flTime, g_flTime ) );

        float3 a = DecodeNormal( g_tNormalMapA.Sample( g_sSampler, pixelA.xy ).xyz );
        float3 b = DecodeNormal( g_tNormalMapB.Sample( g_sSampler, pixelB.xy ).xyz );

		float3 vNorm = lerp(a,b,0.5);

		// Scale
		float3 vDefault = float3( 0, 0, 1.0 );
		return normalize( lerp( vDefault, vNorm, g_flNormalScale * 0.5 ) );
    }

    float4 MainPs( PixelInput i ) : SV_Target0
    {
        Material m = Material::From( i );
		i.vTextureCoords *= pow( 2, g_flTilingScale );
		
		//
		// Gather everything we need
		//
		float depth = Depth::GetNormalized( i.vPositionSs );		
		float2 vScreenUv = CalculateViewportUv( i.vPositionSs );
		float invCameraToDepth = -abs( dot( i.vPositionWithOffsetWs, -g_vCameraDirWs ) ) + ( 1 / depth );
        float2 vTextureCoords = i.vTextureCoords.xy / float2( g_flScale, g_flScale );
		float flTranslucency = 1.0 - saturate( g_flOpacity );
		
		//
		// Material
		//
        m.Roughness = g_flRoughness;
        m.Metalness = g_flMetallic;
		float3 vNormalSample = GetNormalForPixel( i.vNormalWs, vTextureCoords.xy ); 
		m.Normal = TransformNormal( vNormalSample, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		float2 vRefractionUv = vScreenUv + vNormalSample.xy;

		//
		// Translucency and refraction
		//
		float3 vClear = Tex2DLevel( g_tFrameBufferCopyTexture, vScreenUv, 0 ).xyz;
		vClear = SrgbGammaToLinear( vClear );
		float3 vClearRefracted = Tex2DLevel( g_tFrameBufferCopyTexture, vRefractionUv, 0 ).xyz;
		vClearRefracted = SrgbGammaToLinear( vClearRefracted );

		//
		// 1. Refraction
		//
		{
			m.Albedo = lerp( m.Albedo, vClearRefracted, flTranslucency );
		}

		//
		// 3. Reflection
		//
		if ( g_bPlanarReflections )
		{
			m.Albedo += g_tPlanarReflectionTexture.SampleLevel( g_sSampler, ( vRefractionUv * float2( -1, 1 ) ), 0 );
		}

		//
		// 4. Tinting
		//
		{
			float flWaterDepth = saturate( invCameraToDepth / ( pow( 2, g_flWaterDepthScale ) ) );
			m.Albedo *= lerp( g_vColor, g_vDeepColor, saturate( flWaterDepth ) );
		}

        m.AmbientOcclusion = 1;
		m.Opacity = 0;
        float3 vRes = ShadingModelStandard::Shade( i, m ).xyz;

		//
		// 5. Feathering
		//
		{
			float waterFeathering = saturate( (invCameraToDepth / ( pow( 2, g_flShorelineFeatheringScale ) ) ) );
			vRes = lerp( vClear, vRes, waterFeathering );
		}

		return float4( vRes, 1.0 );
    }
}