sampler s0;
texture lightMask;
sampler lightSampler = sampler_state{Texture = lightMask;AddressU = Clamp; AddressV = Clamp;};
float param1;

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
	float4 color = tex2D(s0, coords);
	float4 lightColor = tex2D(lightSampler,coords);
	return color * lightColor;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

