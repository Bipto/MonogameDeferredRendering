#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4 LightColor;
float3 LightDirection;
float LightPower;

float3 CameraPosition;

float Shininess = 16;

texture ColorMap;
sampler ColorSampler = sampler_state
{
    texture = <ColorMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};

texture NormalMap;
sampler NormalSampler = sampler_state
{
    texture = <NormalMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};

texture PositionMap;
sampler PositionSampler = sampler_state
{
    texture = <PositionMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};

struct VertexShaderInput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
};

struct VertexToPixel
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
    float3 Position3D : TEXCOORD2;
};

struct PixelToFrame
{
    float4 Color : COLOR0;
};

float DotProduct(float3 lightPos, float3 pos3D, float3 normal)
{
    float3 lightDir = normalize(pos3D - lightPos);
    return dot(-lightDir, normal);
}

VertexToPixel VertexShaderFunction(VertexShaderInput input)
{
    VertexToPixel output;
    output.Position = input.Position;
    output.TexCoords = input.TexCoords;
    output.Position3D = output.Position;
    return output;
}

PixelToFrame PixelShaderFunction(VertexToPixel input)
{
    PixelToFrame output;
           
    float4 color = tex2D(ColorSampler, input.TexCoords);
    float4 normalCol = tex2D(NormalSampler, input.TexCoords);
    float4 position = tex2D(PositionSampler, input.TexCoords);
   
    float3 normal = normalize(2.0f * normalCol.xyz - 1.0f);
    float3 lighting = color;
    
    float3 viewDir = normalize(CameraPosition - position);
    float3 lightDir = normalize(LightDirection);
    
    float3 diffuse = max(dot(normal, lightDir), 0) * LightColor * LightPower;
    lighting += diffuse;
    
    float specularStrength = 0.1;
    float3 reflectDirection = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDirection), 0.0), Shininess);
    float3 specular = specularStrength * spec * LightColor;
    
    lighting += specular;
    
    output.Color = saturate(float4(lighting, 1));
    
    return output;
}

technique DirectionalLight
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
};