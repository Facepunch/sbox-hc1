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
	// Foam
	//
    CreateInputTexture2D( Foam, Srgb, 8, "", "_foam", "Foam,10/10", Default3( 1.0, 1.0, 1.0 ) );
    Texture2D g_tFoam < Channel( RGB, Box( Foam ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;

	float2 g_flFoamSpeed < UiType( Slider ); Range2( 0, 0, 10, 10 ); UiGroup( "Foam,10/30" ); >;
	float g_flFoamStrength < UiType( Slider ); Range( 0, 1 ); UiGroup( "Foam,10/30" ); >;
	float g_flFoamScale < UiType( Slider ); Range( 1, 20 ); UiGroup( "Foam,10/30" ); >;
    float3 g_vFoamColor < UiType( Color ); Default3( 1.0, 1.0, 1.0 ); UiGroup( "Foam,10/20" ); >;

	//
	// Engine
	//
    BoolAttribute( bWantsFBCopyTexture, true );
    BoolAttribute( translucent, true );
    CreateTexture2D( g_tFrameBufferCopyTexture ) < Attribute("FrameBufferCopyTexture");   SrgbRead( false ); Filter(MIN_MAG_MIP_LINEAR);    AddressU( MIRROR );     AddressV( MIRROR ); >;

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
    float3 GetNormalForPixel( float2 screenPixel, float2 pixel )
    {
        float2 pixelA = pixel.xy + ( g_flSpeedA * float2( g_flTime, g_flTime ) );
        float2 pixelB = pixel.xy + ( g_flSpeedB * float2( g_flTime, g_flTime ) );

        float3 a = g_tNormalMapA.Sample( g_sSampler, pixelA.xy ).xyz;
        float3 b = g_tNormalMapB.Sample( g_sSampler, pixelB.xy ).xyz;

		// Blend
		a = a * 2.0 - 1.0;
		b = b * 2.0 - 1.0;

		float3 vNorm = lerp(a,b,0.5);

		// Scale
		float3 vDefault = float3( 0.0, 0.0, 1.0 );
		return normalize( lerp( vDefault, vNorm, g_flNormalScale ) );
    }

	//
	// Foam sampling
	//
	float GetFoamSample( float2 vTextureCoords )
	{
		float2 noiseUV = float2( vTextureCoords.xy );
		noiseUV += g_flTime * 0.001;
		float flNoise = saturate( GetLayeredSimplex( noiseUV / g_flFoamScale ) );

		float2 vOffset = float2( g_flFoamSpeed * g_flTime.xx );
		return saturate( g_tFoam.Sample( g_sSampler, vTextureCoords + vOffset ) / flNoise );
	}

	//
	// Foam noise
	//
	float GetFoamNoise( float2 vTextureCoords )
	{
		float fNoise = 0.0;

		float2 noiseUV = float2( vTextureCoords.xy );
		noiseUV *= 5;
		noiseUV += g_flTime * float2( 0.25, 0.25 );
		return saturate( GetLayeredSimplex( noiseUV / g_flFoamScale ) );
	}
    
    float4 MainPs( PixelInput i ) : SV_Target0
    {
        Material m = Material::From( i );
		
		//
		// Gather everything we need
		//
		float depth = Depth::GetNormalized( i.vPositionSs );		
		float invCameraToDepth = -abs( dot( i.vPositionWithOffsetWs, -g_vCameraDirWs ) ) + ( 1 / depth );

        float2 vTextureCoords = i.vTextureCoords.xy / float2( g_flScale, g_flScale );
		
		//
		// 1. Base color & shoreline
		//
		{			
			float flShorelineDepth = saturate( invCameraToDepth / ( pow( 2, g_flShorelineDepthScale ) ) );
			flShorelineDepth += GetFoamSample( vTextureCoords );
			m.Albedo = lerp( g_vFoamColor, g_vColor, saturate( flShorelineDepth ) );
			
			float flWaterDepth = saturate( invCameraToDepth / ( pow( 2, g_flWaterDepthScale ) ) );
			m.Albedo = lerp( m.Albedo, g_vDeepColor, saturate( flWaterDepth ) );
		}

		//
		// 3. Extra foam based on noise
		//
		{
			float flFoamNoise = GetFoamNoise( vTextureCoords );
			float vFoamSample = g_tFoam.Sample( g_sSampler, vTextureCoords + ( g_flFoamSpeed * g_flTime.xx ) ).x;
			m.Albedo += lerp( 0, vFoamSample * g_flFoamStrength, saturate( flFoamNoise ) ).xxx;
		}

		//
		// 4. Use the framebuffer copy texture to apply fake alpha to the water
		//
		{
			float2 vScreenUv = CalculateViewportUv( i.vPositionSs );
			float3 vClear = Tex2DLevel( g_tFrameBufferCopyTexture, vScreenUv, 0 ).xyz;
			float waterFeatheringOffset = 1.0f + GetFoamSample( vTextureCoords );
			float waterFeathering = saturate( (invCameraToDepth / ( pow( 2, g_flShorelineFeatheringScale ) )) - waterFeatheringOffset );
			m.Albedo = lerp( vClear, m.Albedo, waterFeathering );

			m.Albedo = lerp( vClear, m.Albedo, g_flOpacity );
		}

		//
		// Finalize material
		//
		float3 vNormalSample = GetNormalForPixel( i.vPositionSs.xy, vTextureCoords.xy ); 
		m.Normal = TransformNormal( vNormalSample, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
        m.Roughness = g_flRoughness;
        m.Metalness = g_flMetallic;

        m.AmbientOcclusion = 1;
		m.Opacity = 0;

        return ShadingModelStandard::Shade( i, m );
    }
}