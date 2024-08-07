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

    PixelInput MainVs( VertexInput i )
    {
        PixelInput o = ProcessVertex( i );
        // Add your vertex manipulation functions here
        return FinalizeVertex( o );
    }
}

//=========================================================================================================================

PS
{
    #include "common/pixel.hlsl"
    #include "procedural.hlsl"

    CreateInputTexture2D( NormalMapA, Srgb, 8, "", "_normal", "Water,10/10", Default3( 1.0, 1.0, 1.0 ) );
    Texture2D g_tNormalMapA < Channel( RGB, Box( NormalMapA ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;

    CreateInputTexture2D( NormalMapB, Srgb, 8, "", "_normal", "Water,10/10", Default3( 1.0, 1.0, 1.0 ) );
    Texture2D g_tNormalMapB < Channel( RGB, Box( NormalMapB ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;

    SamplerState g_sSampler < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;

    float3 g_vColorTintA < UiType( Color ); Default3( 1.0, 1.0, 1.0 ); UiGroup( "Water,10/20" ); >;
    float3 g_vColorTintB < UiType( Color ); Default3( 1.0, 1.0, 1.0 ); UiGroup( "Water,10/20" ); >;
    float3 g_vColorTintDeep < UiType( Color ); Default3( 1.0, 1.0, 1.0 ); UiGroup( "Water,10/20" ); >;

    float2 g_flSpeedA < UiType( Slider ); Range2( 0, 0, 10, 10 ); UiGroup( "Water,10/30" ); >;
    float2 g_flSpeedB < UiType( Slider ); Range2( 0, 0, 10, 10 ); UiGroup( "Water,10/30" ); >;

    float g_flScale < UiType( Slider ); Range( 1, 10 ); UiGroup( "Water,10/30" ); >;
    float g_fNoiseScale < UiType( Slider ); Range( 5, 50 ); UiGroup( "Water,10/30" ); >;
    float g_fDepthScale < UiType( Slider ); Range( 5, 50 ); UiGroup( "Water,10/30" ); >;

    BoolAttribute(bWantsFBCopyTexture, true);
    BoolAttribute(translucent, true);

    CreateTexture2D( g_tFrameBufferCopyTexture ) < Attribute("FrameBufferCopyTexture");   SrgbRead( false ); Filter(MIN_MAG_MIP_LINEAR);    AddressU( MIRROR );     AddressV( MIRROR ); >;

    float3 GetNormalForPixel( float2 pixel )
    {
        float2 pixelA = pixel.xy + ( g_flSpeedA * float2( g_flTime, g_flTime ) );
        float2 pixelB = pixel.xy + ( g_flSpeedB * float2( g_flTime, g_flTime ) );

        float3 a = g_tNormalMapA.Sample( g_sSampler, pixelA.xy ).xyz;
        float3 b = g_tNormalMapB.Sample( g_sSampler, pixelB.xy ).xyz;

        return normalize( (a + b) / float3( 2, 2, 2 ) );
    }
    
    float4 MainPs( PixelInput i ) : SV_Target0
    {
        Material m = Material::From( i );

        float2 vTextureCoords = i.vTextureCoords.xy / float2( g_flScale, g_flScale );
        m.Normal = GetNormalForPixel( vTextureCoords ).xyz;
		
		float2 noiseUV = vTextureCoords.xy + ((g_flSpeedA + g_flSpeedB) / 2.xx * g_flTime);
        float fNoiseSample = Simplex2D(noiseUV / g_fNoiseScale);
		m.Albedo = lerp( g_vColorTintA, g_vColorTintB, fNoiseSample );

		float depth = Depth::GetNormalized( i.vPositionSs );
		float invCameraToDepth = -abs( dot( i.vPositionWithOffsetWs, -g_vCameraDirWs ) ) + ( 1 / depth );
		float waterDepth = saturate( invCameraToDepth / (pow(2, g_fDepthScale ) ) );

		m.Albedo = lerp( m.Albedo, g_vColorTintDeep, waterDepth );

        m.Roughness = 0.1;
        m.Metalness = 0;
        m.AmbientOcclusion = 1;
        m.Opacity = 0.1;

        return ShadingModelStandard::Shade( i, m );
    }
}