texture InputTexture;

sampler inputSampler = sampler_state
{
   Texture = (InputTexture);
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

const int steps = 4;

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
   float4 color = tex2D(inputSampler, input.texCoord);
   
   //color.r = int(color.r * steps) / float(steps);
   //color.g = int(color.g * steps) / float(steps);
   //color.b = int(color.b * steps) / float(steps);

   return color;
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