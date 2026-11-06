using Vulcan;
using Vulcan.Graphics;

namespace CraftyNative.Rendering;

public sealed class RenderResources : IDisposable
{
    public IShader VertexShader { get; }
    public IShader FragmentShader { get; }
    public IShader TranslucentFragmentShader { get; }

    public IVertexLayout VertexLayout { get; }

    public IRasterizerState OpaqueRasterizer { get; }
    public IRasterizerState TranslucentRasterizer { get; }

    public IDepthStencilState OpaqueDepthState { get; }
    public IDepthStencilState TranslucentDepthState { get; }

    public IBlendState OpaqueBlendState { get; }
    public IBlendState AlphaBlendState { get; }

    public IPipeline OpaquePipeline { get; }
    public IPipeline TranslucentPipeline { get; }

    public IBuffer ConstantBuffer { get; }
    public ISampler Sampler { get; }

    public RenderResources(IGraphicsDevice device)
    {
        const string shaderSource = """
        struct VSInput
        {
            float3 Position : POSITION;
            float2 UV : TEXCOORD0;
            float Light : TEXCOORD1;
        };

        struct VSOutput
        {
            float4 Position : SV_Position;
            float2 UV : TEXCOORD0;
            float Light : TEXCOORD1;
        };

        cbuffer Transform : register(b0)
        {
            row_major matrix MVP;
            float Daylight;
            float Moonlight;
            float2 Padding;
            float4 ClipRect;
        };

        VSOutput VSMain(VSInput input)
        {
            VSOutput output;
            output.Position = mul(float4(input.Position, 1.0), MVP);
            output.UV = input.UV;
            output.Light = input.Light;
            return output;
        }

        Texture2D Texture : register(t0);
        SamplerState Sampler : register(s0);

        float4 PSMain(VSOutput input) : SV_Target
        {
            clip(float4(
                input.Position.x - ClipRect.x,
                input.Position.y - ClipRect.y,
                ClipRect.z - input.Position.x,
                ClipRect.w - input.Position.y));

            float4 color = Texture.Sample(Sampler, input.UV);
            clip(color.a - 0.5f);

            float ambient = 0.04f + saturate(Daylight) * 0.96f + saturate(Moonlight) * 0.12f;

            color.rgb *= saturate(input.Light) * saturate(ambient);

            return color;
        }

        float4 PSMainTranslucent(VSOutput input) : SV_Target
        {
            clip(float4(
                input.Position.x - ClipRect.x,
                input.Position.y - ClipRect.y,
                ClipRect.z - input.Position.x,
                ClipRect.w - input.Position.y));

            float4 color = Texture.Sample(Sampler, input.UV);
            clip(color.a - 0.004f);

            float ambient = 0.04f + saturate(Daylight) * 0.96f + saturate(Moonlight) * 0.12f;

            color.rgb *= saturate(input.Light) * saturate(ambient);

            return color;
        }
        """;

        var vertexShaderCode = Shaders.CompileShader(shaderSource, "VSMain", "vs_5_0");

        var fragmentShaderCode = Shaders.CompileShader(shaderSource, "PSMain", "ps_5_0");

        var translucentFragmentShaderCode = Shaders.CompileShader(shaderSource, "PSMainTranslucent", "ps_5_0");

        VertexShader = device.CreateShader(new()
        {
            Code = vertexShaderCode,
            Stage = ShaderStage.Vertex,
            EntryPoint = "VSMain"
        });

        FragmentShader = device.CreateShader(new()
        {
            Code = fragmentShaderCode,
            Stage = ShaderStage.Fragment,
            EntryPoint = "PSMain"
        });

        TranslucentFragmentShader = device.CreateShader(new()
        {
            Code = translucentFragmentShaderCode,
            Stage = ShaderStage.Fragment,
            EntryPoint = "PSMainTranslucent"
        });

        VertexLayout = device.CreateVertexLayout(new()
        {
            VertexShader = VertexShader,
            Elements = new VertexElement[]
            {
                new()
                {
                    Semantic = "POSITION",
                    Location = 0,
                    Format = TextureFormat.R32G32B32Float,
                    Offset = 0
                },
                new()
                {
                    Semantic = "TEXCOORD",
                    Location = 0,
                    Format = TextureFormat.R32G32Float,
                    Offset = 12
                },
                new()
                {
                    Semantic = "TEXCOORD",
                    Location = 1,
                    Format = TextureFormat.R32Float,
                    Offset = 20
                }
            }
        });

        OpaqueRasterizer = device.CreateRasterizerState(new()
        {
            CullMode = CullMode.None,
            FrontFace = FrontFace.CounterClockwise,
            FillMode = FillMode.Solid,
            DepthClipEnable = true,
            ScissorEnable = true
        });

        OpaqueDepthState = device.CreateDepthStencilState(new()
        {
            DepthTestEnable = true,
            DepthWriteEnable = true,
            DepthCompare = CompareOperation.Less
        });

        OpaqueBlendState = device.CreateBlendState(new()
        {
            Enable = false
        });

        OpaquePipeline = device.CreatePipeline(new()
        {
            VertexShader = VertexShader,
            FragmentShader = FragmentShader,
            VertexLayout = VertexLayout,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            Rasterizer = OpaqueRasterizer,
            DepthStencil = OpaqueDepthState,
            Blend = OpaqueBlendState
        });

        AlphaBlendState = device.CreateBlendState(new()
        {
            Enable = true,
            SourceColor = BlendFactor.SourceAlpha,
            DestinationColor = BlendFactor.InverseSourceAlpha,
            ColorOperation = BlendOperation.Add,
            SourceAlpha = BlendFactor.One,
            DestinationAlpha = BlendFactor.InverseSourceAlpha,
            AlphaOperation = BlendOperation.Add
        });

        TranslucentRasterizer = device.CreateRasterizerState(new()
        {
            CullMode = CullMode.Back,
            FrontFace = FrontFace.CounterClockwise,
            FillMode = FillMode.Solid,
            DepthClipEnable = true,
            ScissorEnable = true
        });

        TranslucentDepthState = device.CreateDepthStencilState(new()
        {
            DepthTestEnable = true,
            DepthWriteEnable = false,
            DepthCompare = CompareOperation.Less
        });

        TranslucentPipeline = device.CreatePipeline(new()
        {
            VertexShader = VertexShader,
            FragmentShader = TranslucentFragmentShader,
            VertexLayout = VertexLayout,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            Rasterizer = TranslucentRasterizer,
            DepthStencil = TranslucentDepthState,
            Blend = AlphaBlendState
        });

        ConstantBuffer = device.CreateBuffer(new()
        {
            Size = 96,
            Usage = BufferUsage.Uniform,
            MemoryUsage = MemoryUsage.Upload
        });

        Sampler = device.CreateSampler(new()
        {
            MinFilter = Filter.Nearest,
            MagFilter = Filter.Nearest,
            MipmapFilter = Filter.Nearest,
            AddressU = AddressMode.Repeat,
            AddressV = AddressMode.Repeat,
            AddressW = AddressMode.Repeat,
            MipLodBias = 0,
            MinLod = 0,
            MaxLod = float.MaxValue
        });
    }

    public void Dispose()
    {
        TranslucentPipeline.Dispose();
        OpaquePipeline.Dispose();

        TranslucentDepthState.Dispose();
        OpaqueDepthState.Dispose();

        AlphaBlendState.Dispose();
        OpaqueBlendState.Dispose();

        TranslucentRasterizer.Dispose();
        OpaqueRasterizer.Dispose();

        VertexLayout.Dispose();

        TranslucentFragmentShader.Dispose();
        FragmentShader.Dispose();
        VertexShader.Dispose();

        ConstantBuffer.Dispose();
        Sampler.Dispose();
    }
}