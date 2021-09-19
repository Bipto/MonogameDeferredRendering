#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

texture colorMap;
texture normalMap;
texture depthMap;
texture skyboxTexture;

sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

sampler normalSampler = sampler_state
{
    Texture = (normalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

sampler depthSampler = sampler_state
{
    Texture = (depthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

sampler skyboxSampler = sampler_state
{
    Texture = (skyboxTexture);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

float4 AmbientColor = float4(1, 0, 0, 1);
float AmbientIntensity = 0.2;

float3 DiffuseLightDirection = float3(1, 0, 0);
float4 DiffuseColor = float4(1, 0, 0, 1);
float DiffuseIntensity = 1;

float4x4 WorldInverseTranspose;
float2 halfPixel;

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;    
    output.Position = float4(input.Position, 1);    
    output.TexCoord = input.TexCoord - halfPixel;      
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    //lighting
    float4 color = tex2D(colorSampler, input.TexCoord);   
    float4 skybox = tex2D(skyboxSampler, input.TexCoord);
    float4 depth = tex2D(depthSampler, input.TexCoord);
    
    if (depth.r == 0)
    {
        color = skybox;
    }
    
    float4 ambient = AmbientColor * AmbientIntensity;    
     
    float4 outputColor = saturate(color);
    
    return outputColor;

}

technique Combine
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
};