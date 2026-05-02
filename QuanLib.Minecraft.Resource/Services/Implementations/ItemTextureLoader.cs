using Microsoft.Extensions.Logging;
using QuanLib.Core;
using QuanLib.Minecraft.Resource.Extensions;
using QuanLib.Minecraft.Resource.Models;
using QuanLib.Minecraft.Resource.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services.Implementations
{
    public class ItemTextureLoader(ILogger<ItemTextureLoader>? logger = null) : IItemTextureLoader
    {
        private readonly ILogger<ItemTextureLoader>? _logger = logger;

        public async Task<Dictionary<string, IGeneratedItemTexture>> LoadItemTexturesAsync(
            IReadOnlyDictionary<string, AssetFileEntry> entries,
            IReadOnlyDictionary<string, IGeneratedItemModel> models,
            TexturePool texturePool)
        {
            ArgumentNullException.ThrowIfNull(entries, nameof(entries));
            ArgumentNullException.ThrowIfNull(models, nameof(models));
            ArgumentNullException.ThrowIfNull(texturePool, nameof(texturePool));

            Dictionary<string, IGeneratedItemTexture> result = [];
            Texture missingTexture = Texture.CreateMissingTexture();
            texturePool.Textures.TryAdd(missingTexture.TextureId, missingTexture);

            foreach (var item in models)
            {
                string itemId = item.Key;
                IGeneratedItemModel model = item.Value;
                string[] layers = model.GetLayers().Select(AssetHelper.GetFullName).ToArray();

                foreach (string layer in layers)
                {
                    string textureId = layer;
                    if (!texturePool.ContainsKey(textureId))
                    {
                        Texture texture = textureId == Texture.MISSING_TEXTURE ? missingTexture : await LoadTextureAsync(textureId, entries) ?? missingTexture;
                        texturePool.Textures.Add(textureId, texture);
                    }
                }

                if (layers.Length > 1)
                {
                    string textureId = string.Join('+', layers);
                    if (!texturePool.ContainsKey(textureId))
                    {
                        Texture[] layerTextures = new Texture[layers.Length];
                        for (int i = 0; i < layers.Length; i++)
                            layerTextures[i] = texturePool[layers[i]];

                        Image<Rgba32> mergedImage = MergeLayers(layerTextures);
                        Texture mergedTexture = new(textureId, mergedImage);
                        texturePool.Textures.Add(textureId, mergedTexture);
                    }
                }

                GeneratedItemTexture itemTexture = new(itemId, model, texturePool);
                result.Add(itemId, itemTexture);
            }

            return result;
        }

        private async Task<Texture?> LoadTextureAsync(string textureId, IReadOnlyDictionary<string, AssetFileEntry> entries)
        {
            if (entries.TryGetValue(textureId, out var entry))
            {
                try
                {
                    using Stream stream = entry.Entry.Open();
                    Image<Rgba32> image = await Image.LoadAsync<Rgba32>(stream);
                    return new Texture(textureId, image);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning("Loading texture from '{FilePath}' failed: {Message}", entry.FilePath, ObjectFormatter.Format(ex));
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private static Image<Rgba32> MergeLayers(Texture[] layerTextures)
        {
            if (layerTextures.Length < 2)
                throw new ArgumentException("At least two layers are required to merge.", nameof(layerTextures));

            List<Image<Rgba32>> mergedImages = [];
            IReadOnlyImage<Rgba32> background =  layerTextures[0].Image;

            try
            {
                for (int i = 1; i < layerTextures.Length; i++)
                {
                    IReadOnlyImage<Rgba32> foreground = layerTextures[i].Image;
                    Image<Rgba32> mergedImage = MergeLayers(background, foreground);
                    background = new ReadOnlyImage<Rgba32>(mergedImage);
                    mergedImages.Add(mergedImage);
                }

                return mergedImages[^1];
            }
            finally
            {
                for (int i = 0; i < mergedImages.Count - 1; i++)
                    mergedImages[i].Dispose();
            }
        }

        private static Image<Rgba32> MergeLayers(IReadOnlyImage<Rgba32> background, IReadOnlyImage<Rgba32> foreground)
        {
            if (background.Size != foreground.Size)
                throw new InvalidOperationException("Cannot merge textures with different sizes.");

            Image<Rgba32> result = new(background.Width, background.Height);
            for (int y = 0; y < background.Height; y++)
            {
                for (int x = 0; x < background.Width; x++)
                {
                    Rgba32 fgPixel = foreground[x, y];
                    Rgba32 bgPixel = background[x, y];

                    // 获取前景和背景的 RGBA 值，转换为浮点数以便计算
                    float fgA = fgPixel.A / 255f;
                    float bgA = bgPixel.A / 255f;

                    // 标准 Alpha 合成公式（Porter-Duff Over 操作）
                    // resultAlpha = fgAlpha + bgAlpha * (1 - fgAlpha)
                    float outA = fgA + bgA * (1 - fgA);

                    if (outA > 0)
                    {
                        // resultColor = (fgColor * fgAlpha + bgColor * bgAlpha * (1 - fgAlpha)) / resultAlpha
                        float outR = (fgPixel.R * fgA + bgPixel.R * bgA * (1 - fgA)) / outA;
                        float outG = (fgPixel.G * fgA + bgPixel.G * bgA * (1 - fgA)) / outA;
                        float outB = (fgPixel.B * fgA + bgPixel.B * bgA * (1 - fgA)) / outA;

                        result[x, y] = new Rgba32(
                            (byte)Math.Clamp(outR, 0, 255),
                            (byte)Math.Clamp(outG, 0, 255),
                            (byte)Math.Clamp(outB, 0, 255),
                            (byte)Math.Clamp(outA * 255, 0, 255)
                        );
                    }
                    else
                    {
                        // 完全透明的情况
                        result[x, y] = new Rgba32(0, 0, 0, 0);
                    }
                }
            }

            return result;
        }
    }
}
