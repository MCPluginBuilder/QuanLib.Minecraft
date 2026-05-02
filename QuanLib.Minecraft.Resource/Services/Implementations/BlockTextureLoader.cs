using Microsoft.Extensions.Logging;
using QuanLib.Core;
using QuanLib.Game;
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
    public class BlockTextureLoader(ILogger<BlockTextureLoader>? logger = null) : IBlockTextureLoader
    {
        private readonly ILogger<BlockTextureLoader>? _logger = logger;

        public async Task<Dictionary<string, ICubeBlockTexture>> LoadBlockTexturesAsync(
            IReadOnlyDictionary<string, AssetFileEntry> entries,
            IReadOnlyDictionary<string, ICubeBlockModel> models,
            TexturePool texturePool)
        {
            ArgumentNullException.ThrowIfNull(entries, nameof(entries));
            ArgumentNullException.ThrowIfNull(models, nameof(models));
            ArgumentNullException.ThrowIfNull(texturePool, nameof(texturePool));

            Dictionary<string, ICubeBlockTexture> result = [];
            Texture missingTexture = Texture.CreateMissingTexture();
            texturePool.Textures.TryAdd(missingTexture.TextureId, missingTexture);
            Facing[] facings = Enum.GetValues<Facing>();

            foreach (var item in models)
            {
                string blockId = item.Key;
                ICubeBlockModel model = item.Value;

                foreach (Facing facing in facings)
                {
                    string textureId = AssetHelper.GetFullName(model.GetTextureAtFacing(facing));
                    if (!texturePool.ContainsKey(textureId))
                    {
                        Texture texture = textureId == Texture.MISSING_TEXTURE ? missingTexture : await LoadTextureAsync(textureId, entries) ?? missingTexture;
                        texturePool.Textures.Add(textureId, texture);
                    }
                }

                CubeBlockTexture blockTexture = new(blockId, model, texturePool);
                result.Add(blockId, blockTexture);
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
    }
}
