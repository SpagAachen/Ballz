
float2 GridSize;
texture VectorField;
texture ArrowSymbol;

sampler fieldSampler = sampler_state
{
   Texture = (VectorField);
   MipFilter = POINT;
   MinFilter = POINT;
   MagFilter = POINT;
};

sampler arrowSampler = sampler_state
{
   Texture = (ArrowSymbol);
   MipFilter = LINEAR;
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
   uint2 cell = uint2(input.texCoord * GridSize);
   float2 innerCellPos = (input.texCoord * GridSize) - cell;

   float4 data = tex2D(fieldSampler, cell / GridSize);

   float pressure = data.w;

   float2 vl = data.xy * 2.0 - 1.0;
   float l = length(vl);
   float2 v = vl/l;
   float2 n = float2(v.y, -v.x);

   float3x3 arrowMat = /*float3x3(float3(1, 0, 0), float3(0, 1, 0), float3(-0.5, -0.5, 1))
      **/ float3x3(float3(v, 0), float3(n, 0), float3(0, 0, 1));
      //* float3x3(float3(1, 0, 0), float3(0, 1, 0), float3(0.5, 0.5, 1));

   float2 arrowTexCoords = mul(float3(innerCellPos, 1), arrowMat).xy;
   float3 arrowBase = tex2D(arrowSampler, arrowTexCoords).rgb;
   float3 arrowColor = arrowBase * l;
   if (l >= 1)
      arrowColor = float3(arrowColor.r, 0, 0);

   float pressureColorMix = arrowBase.r;
   float3 pressureColor = float3(0, pressure, 0);

   return float4(lerp(pressureColor, arrowColor, pressureColorMix), 1);
}

technique Technique1
{
   pass Pass1
   {
      // TODO: set renderstates here.

      VertexShader = compile vs_4_0 VertexShaderFunction();
      PixelShader = compile ps_4_0 PixelShaderFunction();
   }
}