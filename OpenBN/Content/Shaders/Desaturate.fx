float ColourAmount;
Texture coloredTexture;

sampler coloredTextureSampler = sampler_state
{
	texture = <coloredTexture>;
};

float4 GreyscalePixelShaderFunction(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 coords: TEXCOORD0) : COLOR0
{
	float4 color = tex2D(coloredTextureSampler, coords);
	float3 colrgb = color.rgb;
	float greycolor = dot(colrgb, float3(0.3, 0.59, 0.11));

	colrgb.rgb = lerp(dot(greycolor, float3(0.3, 0.59, 0.11)), colrgb, ColourAmount);

	return float4(colrgb.rgb, color.a);
}

technique Grayscale
{
	pass GreyscalePass
	{
		PixelShader = compile ps_4_0_level_9_1 GreyscalePixelShaderFunction();
	}
}

