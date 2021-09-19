#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4 AmbientColor;
float AmbientIntensity;

float4x4 World;
float4x4 View;
float4x4 Projection;

float3 CameraPosition;

texture Texture;
sampler diffuseSampler = sampler_state
{
    Texture = (Texture);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture NormalMap;
sampler normalSampler = sampler_state
{
    Texture = (NormalMap);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float3 Binormal : BINORMAL0;
    float3 Tangent : TANGENT0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    //float3 Normal : TEXCOORD1;
    float2 Depth : TEXCOORD1;
    float4 Position2D : TEXCOORD2;
    
    float3 View : TEXCOORD3;
    float3x3 WorldToTangentSpace : TEXCOORD4;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{   
    VertexShaderOutput output;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.TexCoord = input.TexCoord;
    //output.Normal = mul(input.Normal, World);
    //output.Normal = input.Normal;
    output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w;
    output.Position2D = output.Position;
    
    output.WorldToTangentSpace[0] = mul(normalize(input.Tangent), World);
    output.WorldToTangentSpace[1] = mul(normalize(input.Binormal), World);
    output.WorldToTangentSpace[2] = mul(normalize(input.Normal), World);
    
    output.View = normalize(float4(CameraPosition, 1) - worldPosition);
    
    return output;
}

struct PixelShaderOutput
{
    float4 Color : COLOR0;
    float4 Normal : COLOR1;
    float4 Depth : COLOR2;
    float4 Position : COLOR3;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;
    output.Color = tex2D(diffuseSampler, input.TexCoord);
    
    float3 normalMap = 2.0 * (tex2D(normalSampler, input.TexCoord)) - 1;
    normalMap = normalize(mul(normalMap, input.WorldToTangentSpace));
    float4 normal = float4(normalMap, 1);
    output.Normal = normal;   
    
    //output.Depth = input.Depth.z / input.Depth.y;
    //output.Depth = float4(1, 1, 1, 1);
   
    output.Depth = input.Position2D.z / input.Position2D.w;
    
    output.Position = input.Position2D;
    
    return output;
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
};