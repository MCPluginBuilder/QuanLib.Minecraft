using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using QuanLib.Core;
using QuanLib.IO.Zip;
using QuanLib.Minecraft.Resource.Extensions;
using QuanLib.Minecraft.Resource.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services.Implementations
{
    public class BlockModelLoader(ILogger<BlockModelLoader>? logger = null) : IBlockModelLoader
    {
        private const string BLOCK_MODEL_ID = "block/block";
        private const string MINECRAFT_BLOCK_MODEL_ID = "minecraft:block/block";

        private const int AllFacesMask = (1 << 6) - 1;  // 0b111111
        private static readonly Dictionary<string, int> FaceMaskMap = new() // 方块模型六个面对应的位掩码
        {
            ["down"] = 1 << 0,
            ["up"] = 1 << 1,
            ["north"] = 1 << 2,
            ["south"] = 1 << 3,
            ["west"] = 1 << 4,
            ["east"] = 1 << 5
        };

        public async Task<Dictionary<string, ICubeBlockModel>> LoadBlockModelsAsync(IReadOnlyDictionary<string, AssetFileEntry> entries, IReadOnlyDictionary<string, BlockStateModel> blockStates)
        {
            ArgumentNullException.ThrowIfNull(entries, nameof(entries));
            ArgumentNullException.ThrowIfNull(blockStates, nameof(blockStates));

            Dictionary<string, ICubeBlockModel> result = [];
            Dictionary<string, ICubeBlockModel> source = [];
            HashSet<string> exclude = [];

            foreach (var item in blockStates)
            {
                string blockId = item.Key;
                BlockStateModel blockStateModel = item.Value;
                BlockRotation blockRotation = blockStateModel.BlockRotation;

                ICubeBlockModel? model = await LoadModelAsync(blockStateModel.BlockModel, entries, source, exclude);
                if (model is null)
                    continue;

                if (!IsCubeTextures(model))
                    continue;

                if (!blockRotation.IsZero)
                    model = new RotatedCubeBlockModel(model, BlockRotationMapping.CreateMapping(blockRotation));

                result.Add(blockId, model);
            }

            return result;
        }

        private readonly ILogger<BlockModelLoader>? _logger = logger;

        private async Task<ICubeBlockModel?> LoadModelAsync(
            string modelId,
            IReadOnlyDictionary<string, AssetFileEntry> entries,
            IDictionary<string, ICubeBlockModel> source,
            ISet<string> exclude)
        {
            modelId = AssetHelper.GetFullName(modelId);

            if (source.TryGetValue(modelId, out var model))
                return model;

            if (exclude.Contains(modelId))
                return null;

            if (!entries.TryGetValue(modelId, out var entry))
                return null;


            JObject jObject;
            try
            {
                string json = await entry.Entry.ReadAllTextAsync(Encoding.UTF8);
                jObject = JObject.Parse(json);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning("Loading block model from '{FilePath}' failed: {Message}", entry.FilePath, ObjectFormatter.Format(ex));
                exclude.Add(modelId);
                return null;
            }

            string? parentId = jObject["parent"]?.Value<string>();
            JObject? texturesObject = jObject["textures"] as JObject;

            if (!string.IsNullOrEmpty(parentId) && parentId != BLOCK_MODEL_ID && parentId != MINECRAFT_BLOCK_MODEL_ID)
            {
                parentId = AssetHelper.GetFullName(parentId);
                if (!parentId.Split(':', 2)[^1].StartsWith("block/"))
                {
                    exclude.Add(modelId);
                    return null;
                }

                ICubeBlockModel? parentModel = await LoadModelAsync(parentId, entries, source, exclude);
                if (parentModel is null)
                {
                    exclude.Add(modelId);
                    return null;
                }

                Dictionary<string, string> textures = GetTextures(texturesObject);
                CubeBlockModel result = new(modelId, textures, parentModel);
                source.Add(modelId, result);
                return result;
            }
            else
            {
                if (jObject["elements"] is JArray elementsJArray && IsCubeElements(elementsJArray))
                {
                    Dictionary<string, string> textures = GetTextures(texturesObject);
                    CubeBlockModel result = new(modelId, textures, null);
                    source.Add(modelId, result);
                    return result;
                }
                else
                {
                    exclude.Add(modelId);
                    return null;
                }
            }
        }

        private static Dictionary<string, string> GetTextures(JObject? texturesObject)
        {
            try
            {
                return texturesObject?.ToObject<Dictionary<string, string>>() ?? [];
            }
            catch
            {
                return [];
            }
        }

        private static bool IsCubeElements(JArray? elementsJArray)
        {
            if (elementsJArray is null)
                return false;

            if (elementsJArray.Count != 1)
                return false;

            if (elementsJArray[0] is not JObject elementsObject)
                return false;

            if (elementsObject["from"] is not JArray from || elementsObject["to"] is not JArray to)
                return false;

            if (from.Count != 3 || to.Count != 3)
                return false;

            return from[0].Value<int>() == 0 &&
                   from[1].Value<int>() == 0 &&
                   from[2].Value<int>() == 0 &&
                   to[0].Value<int>() == 16 &&
                   to[1].Value<int>() == 16 &&
                   to[2].Value<int>() == 16;
        }

        private static bool IsCubeTextures(IObjectModel model)
        {
            return IsCubeTextures(model, AllFacesMask);
        }

        private static bool IsCubeTextures(IObjectModel model, int needsMask)
        {
            // 在当前模型的纹理中清除已找到的面
            foreach (var item in model.Textures)
            {
                if (FaceMaskMap.TryGetValue(item.Key, out int faceMask))
                {
                    // 清除这个面对应的 bit
                    needsMask &= ~faceMask;
                    if (needsMask == 0)
                        return true;
                }
            }

            // 还有缺失的面，向上查找父模型
            IObjectModel? parent = model.Parent;
            return parent is not null && IsCubeTextures(parent, needsMask);
        }
    }
}
