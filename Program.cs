using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageFxFramework
{
    public sealed class ImageData
    {
        public string Id { get; }
        public Image<Rgba32> Image { get; private set; }

        public ImageData(string path)
{
    if (!File.Exists(path)) throw new FileNotFoundException(path);
    Id = path;
    Image = SixLabors.ImageSharp.Image.Load<Rgba32>(path); 
}

        public void Resize(int target)
        {
            Image.Mutate(x => x.Resize(target, target));
        }

        public void Grayscale()
        {
            Image.Mutate(x => x.Grayscale());
        }

        public void Blur(int radius)
        {
            Image.Mutate(x => x.GaussianBlur(radius));
        }

        public void SaveOutput()
        {
            string outputPath = Path.GetFileNameWithoutExtension(Id) + "_out.jpg";
            Image.Save(outputPath);
            Console.WriteLine($"Saved output: {outputPath}");
        }
    }

    public interface IImageEffect
    {
        string Name { get; }
        void Apply(ImageData image, JsonElement? parameter);
    }

    public sealed class ResizeEffect : IImageEffect
    {
        public string Name => "resize";
        public void Apply(ImageData image, JsonElement? parameter)
        {
            var p = parameter?.Deserialize<ResizeParams>()
                    ?? throw new ArgumentException("Resize requires parameter");
            image.Resize(p.TargetPixels);
        }
    }

    public sealed class BlurEffect : IImageEffect
    {
        public string Name => "blur";
        public void Apply(ImageData image, JsonElement? parameter)
        {
            var p = parameter?.Deserialize<BlurParams>()
                    ?? throw new ArgumentException("Blur requires parameter");
            image.Blur(p.Radius);
        }
    }

    public sealed class GrayscaleEffect : IImageEffect
    {
        public string Name => "grayscale";
        public void Apply(ImageData image, JsonElement? parameter) => image.Grayscale();
    }

    public sealed class ResizeParams { public int TargetPixels { get; set; } }
    public sealed class BlurParams { public int Radius { get; set; } }

    public sealed class EffectRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("parameter")]
        public JsonElement? Parameter { get; set; }
    }

    public sealed class ImageJob
    {
        [JsonPropertyName("name")]
        public string ImageId { get; set; } = string.Empty;
        [JsonPropertyName("effects")]
        public List<EffectRequest> Effects { get; set; } = new();
    }

    public sealed class EffectsConfig
    {
        [JsonPropertyName("images")]
        public List<ImageJob>? Images { get; set; }
    }

    // ---------------- Engine ----------------
    public sealed class PipelineEngine
    {
        private readonly Dictionary<string, IImageEffect> _effects;

        public PipelineEngine()
        {
            _effects = new Dictionary<string, IImageEffect>(StringComparer.OrdinalIgnoreCase)
            {
                ["resize"] = new ResizeEffect(),
                ["blur"] = new BlurEffect(),
                ["grayscale"] = new GrayscaleEffect()
            };
        }

        public void Run(ImageJob job)
        {
            if (!File.Exists(job.ImageId))
            {
                Console.WriteLine($"[ERROR] File not found: {job.ImageId}");
                return;
            }

            var image = new ImageData(job.ImageId);
            Console.WriteLine($"\nProcessing {job.ImageId}...");

            foreach (var effectReq in job.Effects)
            {
                if (_effects.TryGetValue(effectReq.Name, out var effect))
                {
                    Console.WriteLine($" → Applying effect: {effectReq.Name}");
                    effect.Apply(image, effectReq.Parameter);
                }
                else
                {
                    Console.WriteLine($"[WARN] Effect '{effectReq.Name}' not found. Skipping.");
                }
            }

            image.SaveOutput();
        }
    }

    public static class Program
    {
        private const string SampleJsonConfig = @"
        {
            ""images"": [
                {
                    ""name"": ""Image#1.jpg"",
                    ""effects"": [
                        { ""name"": ""resize"", ""parameter"": { ""TargetPixels"": 100 } },
                        { ""name"": ""blur"", ""parameter"": { ""Radius"": 2 } }
                    ]
                },
                {
                    ""name"": ""Image#2.jpg"",
                    ""effects"": [
                        { ""name"": ""resize"", ""parameter"": { ""TargetPixels"": 100 } }
                    ]
                },
                {
                    ""name"": ""Image#3.jpg"",
                    ""effects"": [
                        { ""name"": ""resize"", ""parameter"": { ""TargetPixels"": 150 } },
                        { ""name"": ""blur"", ""parameter"": { ""Radius"": 5 } },
                        { ""name"": ""grayscale"" }
                    ]
                }
            ]
        }";

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run -- <image1> <image2> ...");
                return;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var config = JsonSerializer.Deserialize<EffectsConfig>(SampleJsonConfig, options);
            if (config?.Images == null) return;

            var selectedJobs = config.Images
                .Where(img => args.Contains(img.ImageId, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (selectedJobs.Count == 0)
            {
                Console.WriteLine("[WARN] No matching images found in configuration.");
                return;
            }

            var engine = new PipelineEngine();
            foreach (var job in selectedJobs)
                engine.Run(job);

            Console.WriteLine("\nProcessing complete for all images.");
            var unmatched = args.Except(config.Images.Select(img => img.ImageId), StringComparer.OrdinalIgnoreCase);
            foreach (var name in unmatched)
                Console.WriteLine($"[WARN] Image {name} not found in config.");

        }
    }
}
