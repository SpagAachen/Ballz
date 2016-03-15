
texture WaterTexture;

const float4 waterColor = float4(0, 0.2, 1, 0.75);
const float waterThreshold = 0.35;

sampler waterSampler = sampler_state
{
   Texture = (WaterTexture);
   MipFilter = POINT;
   MinFilter = LINEAR;
   MagFilter = LINEAR;
};


struct VertexShaderInput
{
   float3 position : SV_Position0;
   float2 texCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
   float4 position : SV_Position0;
   float2 texCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
   VertexShaderOutput output;
   output.position = float4(input.position * 2 - 1, 1);
   output.texCoord = input.texCoord;
   return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
   //return tex2D(waterSampler, input.texCoord);
   float water = tex2D(waterSampler, input.texCoord).r;
   float4 c = float4(0, 0, 0, 0);
   if (water > waterThreshold)
      c = waterColor;
   return c;
}

technique Technique1
{
   pass Pass1
   {
      VertexShader = compile vs_4_0 VertexShaderFunction();
      PixelShader = compile ps_4_0 PixelShaderFunction();
   }
}