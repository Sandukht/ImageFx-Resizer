using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
<<<<<<< HEAD
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
=======
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;



namespace ImageFxFramework
{
        public sealed class ImageData
    {
        public string Id { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Channels { get; private set; } 
        public List<string> History { get; } = new();

        public ImageData(string id, int width, int height, int channels)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Width = width;
            Height = height;
            Channels = channels;
            History.Add($"Loaded image '{id}' ({width}x{height}, ch:{channels})");
        }

        public void Resize(int target)
        {
            Width = target;
            Height = target;
            History.Add($"Simulated Resize -> {target}x{target}");
>>>>>>> 281b56d9c93b25e32db2dfa0afc78020b1cf0a67
        }

        public void Blur(int radius)
        {
<<<<<<< HEAD
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
=======
            History.Add($"Simulated Blur -> radius:{radius}");
        }

        public void Grayscale()
        {
            Channels = 1;
            History.Add("Simulated Grayscale");
        }

        public override string ToString()
            => $"Image[{Id}] {Width}x{Height} ch:{Channels}";
    }

    public interface ILogger
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message);
    }

    public sealed class ConsoleLogger : ILogger
    {
        public void Info(string message)  => Console.WriteLine($"[INFO ] {message}");
        public void Warn(string message)  => Console.WriteLine($"[WARN ] {message}");
        public void Error(string message) => Console.WriteLine($"[ERROR] {message}");
    }

    public interface IExecutionContext
    {
        ILogger Logger { get; }
    }

    public sealed class ExecutionContext : IExecutionContext
    {
        public ILogger Logger { get; }
        public ExecutionContext(ILogger logger) => Logger = logger;
    }

      public interface IImageEffect
    {
        string Name { get; }
        bool HasParameter { get; }           // true if effect expects a parameter
        Type? ParameterType { get; }         // the CLR type of the parameter if any
        void Apply(ImageData image, object? parameter, IExecutionContext context);
    }

       public interface IImageEffectFactory
    {
        string EffectName { get; }
        IImageEffect Create();
    }

    public sealed class PluginManager
    {
        private readonly Dictionary<string, IImageEffectFactory> _factories =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly ILogger _logger;

        public PluginManager(ILogger logger) => _logger = logger;

        public void Register(IImageEffectFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            _factories[factory.EffectName] = factory;
            _logger.Info($"Registered effect: {factory.EffectName}");
        }

        public bool TryGetFactory(string name, out IImageEffectFactory? factory)
            => _factories.TryGetValue(name, out factory);

        public IReadOnlyCollection<string> ListEffects() => _factories.Keys.ToList().AsReadOnly();

              public void LoadFromFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                _logger.Warn($"Plugins folder '{folderPath}' not found. Skipping.");
                return;
            }

            foreach (var dll in Directory.EnumerateFiles(folderPath, "*.dll"))
            {
                try
                {
                    var asm = Assembly.LoadFrom(dll);
                    var factories = asm
                        .GetTypes()
                        .Where(t => !t.IsAbstract && typeof(IImageEffectFactory).IsAssignableFrom(t))
                        .Select(t => Activator.CreateInstance(t) as IImageEffectFactory)
                        .Where(f => f != null)!
                        .ToList();

                    foreach (var f in factories)
                    {
                        Register(f!);
                        _logger.Info($"Loaded plugin factory '{f!.EffectName}' from '{Path.GetFileName(dll)}'");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed loading '{dll}': {ex.Message}");
                }
            }
        }
    }

    public sealed class PipelineEngine
    {
        private readonly PluginManager _plugins;
        private readonly IExecutionContext _context;

        public PipelineEngine(PluginManager plugins, IExecutionContext context)
        {
            _plugins = plugins;
            _context = context;
        }

        public void Run(IEnumerable<ImageJob> jobs, IDictionary<string, ImageData> imageStore)
        {
            foreach (var job in jobs)
            {
                if (!imageStore.TryGetValue(job.ImageId, out var image))
                {
                    _context.Logger.Warn($"Image '{job.ImageId}' not found. Skipping job.");
                    continue;
                }

                _context.Logger.Info($"\nProcessing {image}");

                foreach (var step in job.Effects)
                {
                    if (!_plugins.TryGetFactory(step.Name, out var factory) || factory == null)
                    {
                        _context.Logger.Warn($"Effect '{step.Name}' not registered. Skipping.");
                        continue;
                    }

                    var effect = factory.Create();
                    object? parameter = null;

                    if (effect.HasParameter)
                    {
                        parameter = step.ParameterRaw?.Deserialize(effect.ParameterType!, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (parameter == null)
                        {
                            throw new InvalidOperationException(
                                $"Effect '{effect.Name}' requires parameter of type {effect.ParameterType}.");
                        }
                    }

                    _context.Logger.Info($" → Applying '{effect.Name}'" + (parameter != null ? $" with param: {JsonSerializer.Serialize(parameter)}" : string.Empty));
                    effect.Apply(image, parameter, _context);
                }

                _context.Logger.Info($"Done. Final: {image}");
                foreach (var h in image.History)
                    _context.Logger.Info($"   • {h}");
            }
        }
    }

      public sealed class EffectsConfig
    {
        public List<string>? PluginFolders { get; set; }
        public List<ImageJob>? Images { get; set; }
    }

    public class ImageEngine
{
    private readonly PluginManager _plugins;

    public ImageEngine(PluginManager plugins)
    {
        _plugins = plugins;
    }

    public void Run(ImageJob job)
    {
        Console.WriteLine($"Running job on {job.ImageId}...");

            foreach (var effect in job.Effects)
            {
                Console.WriteLine($"Applying effect: {effect.Name}");
                   }

        File.Copy(job.ImageId, "output.jpg", overwrite: true);
    }
}


    public sealed class ImageJob
    {
        public ImageJob(string imagePath, object effects, Dictionary<string, object> parameters) { }
        public string ImageId { get; set; } = string.Empty;
        public List<EffectRequest> Effects { get; set; } = new();
    }

    public sealed class EffectRequest
    {
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("Parameter")]
        
        public JsonElement? ParameterRaw { get; set; } 
    }
    public sealed class ResizeEffect : IImageEffect
    {
        public string Name => "Resize";
        public bool HasParameter => true;
        public Type? ParameterType => typeof(ResizeParams);
        public void Apply(ImageData image, object? parameter, IExecutionContext context)
        {
            var p = (ResizeParams)parameter!;
            if (p.TargetPixels <= 0) throw new ArgumentOutOfRangeException(nameof(p.TargetPixels));
            image.Resize(p.TargetPixels);
        }
    }

    public sealed class ResizeEffectFactory : IImageEffectFactory
    {
        public string EffectName => "Resize";
        public IImageEffect Create() => new ResizeEffect();
    }

    public sealed class ResizeParams
    {
        public int TargetPixels { get; set; }
    }

    public sealed class BlurEffect : IImageEffect
    {
        public string Name => "Blur";
        public bool HasParameter => true;
        public Type? ParameterType => typeof(BlurParams);
        public void Apply(ImageData image, object? parameter, IExecutionContext context)
        {
            var p = (BlurParams)parameter!;
            if (p.Radius < 0) throw new ArgumentOutOfRangeException(nameof(p.Radius));
            image.Blur(p.Radius);
        }
    }

    public sealed class BlurEffectFactory : IImageEffectFactory
    {
        public string EffectName => "Blur";
        public IImageEffect Create() => new BlurEffect();
    }

    public sealed class BlurParams
    {
        public int Radius { get; set; }
    }

    public sealed class GrayscaleEffect : IImageEffect
    {
        public string Name => "Grayscale";
        public bool HasParameter => false;
        public Type? ParameterType => null;
        public void Apply(ImageData image, object? parameter, IExecutionContext context)
        {
            image.Grayscale();
        }
    }

    public sealed class GrayscaleEffectFactory : IImageEffectFactory
    {
        public string EffectName => "Grayscale";
        public IImageEffect Create() => new GrayscaleEffect();
    }

       public static class ImageRepository
    {
        public static IDictionary<string, ImageData> Seed()
        {
            return new Dictionary<string, ImageData>(StringComparer.OrdinalIgnoreCase)
            {
                ["Image#1"] = new ImageData("Image#1", 400, 300, 3),
                ["Image#2"] = new ImageData("Image#2", 800, 600, 3),
                ["Image#3"] = new ImageData("Image#3", 1024, 768, 3),
            };
>>>>>>> 281b56d9c93b25e32db2dfa0afc78020b1cf0a67
        }
    }

    public static class Program
    {
<<<<<<< HEAD
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
=======
        public static void Main(string[] args)
        {
            ILogger logger = new ConsoleLogger();
            var context = new ExecutionContext(logger);
            var plugins = new PluginManager(logger);

            plugins.Register(new ResizeEffectFactory());
            plugins.Register(new BlurEffectFactory());
            plugins.Register(new GrayscaleEffectFactory());

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run -- <imagePath>");
                return;
            }

            string imagePath = args[0];
            if (!File.Exists(imagePath))
            {
                Console.WriteLine($"[ERROR] File not found: {imagePath}");
                return;
            }
            var engine = new ImageEngine(plugins);

            var effects = new List<EffectRequest>
            {
                new EffectRequest
                {
                    Name = "resize",
                    ParameterRaw = JsonDocument.Parse("{\"width\":500,\"height\":500}").RootElement
                },
                new EffectRequest
                {
                    Name = "grayscale",
                    ParameterRaw = JsonDocument.Parse("{}").RootElement
                }
            };
                var job = new ImageJob(imagePath, effects, new Dictionary<string, object>());
                job.ImageId = imagePath;

                engine.Run(job);

            Console.WriteLine("Processing complete. Output: output.jpg");
    }

        private static EffectsConfig LoadConfig(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            return JsonSerializer.Deserialize<EffectsConfig>(json, options)
                   ?? new EffectsConfig { Images = new List<ImageJob>() };
        }
        private const string SampleJsonConfig = @"
        {
            ""images"": [
            {
                ""name"": ""Image#1"",
                ""effects"": [
                { ""name"": ""resize"", ""parameter"": 100 },
                { ""name"": ""blur"", ""parameter"": 2 }
                ]
            },
            {
                ""name"": ""Image#2"",
                ""effects"": [
                { ""name"": ""resize"", ""parameter"": 100 }
                ]
            },
            {
                ""name"": ""Image#3"",
                ""effects"": [
                { ""name"": ""resize"", ""parameter"": 150 },
                { ""name"": ""blur"", ""parameter"": 5 },
                { ""name"": ""grayscale"" }
                ]
            }
            ]
        }"; 
    }
 }

>>>>>>> 281b56d9c93b25e32db2dfa0afc78020b1cf0a67
