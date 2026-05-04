using Microsoft.Extensions.Logging;
using QuanLib.Core;
using QuanLib.Game;
using QuanLib.Minecraft.Resource.Extensions;
using QuanLib.Minecraft.Resource.Models;
using QuanLib.Minecraft.Resource.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
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
                IRotatedCubeBlockModel? rotatedModel = model as IRotatedCubeBlockModel;

                foreach (Facing facing in facings)
                {
                    string textureId = AssetHelper.GetFullName(model.GetTextureAtFacing(facing));
                    if (!texturePool.ContainsKey(textureId))
                    {
                        Texture texture = textureId == Texture.MISSING_TEXTURE ? missingTexture : await LoadTextureAsync(textureId, entries) ?? missingTexture;
                        texturePool.Textures.Add(textureId, texture);
                    }

                    if (rotatedModel is not null && !rotatedModel.BlockRotationMapping.IsZero)
                    {
                        int textureRotation = rotatedModel.BlockRotationMapping.GetRotationMapping(facing).TextureRotation;
                        if (textureRotation is 90 or 180 or 270)
                        {
                            string rotatedTextureId = $"{textureId}[rotation={textureRotation}]";
                            if (!texturePool.ContainsKey(rotatedTextureId))
                            {
                                Texture texture = new(rotatedTextureId, RotateImage(texturePool[textureId].Image.Clone(), textureRotation));
                                texturePool.Textures.Add(rotatedTextureId, texture);
                            }
                        }
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

        private static Image<Rgba32> RotateImage(Image<Rgba32> image, int rotation)
        {
            switch (rotation)
            {
                case 0:
                    break;
                case 90:
                    image.Mutate(x => x.Rotate(RotateMode.Rotate90));
                    break;
                case 180:
                    image.Mutate(x => x.Rotate(RotateMode.Rotate180));
                    break;
                case 270:
                    image.Mutate(x => x.Rotate(RotateMode.Rotate270));
                    break;
                default:
                    throw new ArgumentException("Rotation must be 0, 90, 180, or 270 degrees.", nameof(rotation));
            }

            return image;
        }
    }
}
