using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using QuanLib.Core;
using QuanLib.Game;
using QuanLib.IO.Zip;
using QuanLib.Minecraft.Resource.Extensions;
using QuanLib.Minecraft.Resource.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
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
                CubeBlockModel result = new(modelId, textures, element: null, parentModel);
                source.Add(modelId, result);
                return result;
            }
            else
            {
                if (jObject["elements"] is JArray elementsJArray && IsCubeElements(elementsJArray, out var element))
                {
                    Dictionary<string, string> textures = GetTextures(texturesObject);
                    CubeBlockModel result = new(modelId, textures, element, parent: null);
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

        private bool IsCubeElements(JArray? elementsJArray, [MaybeNullWhen(false)] out BlockModelElement element)
        {
            element = null;

            if (elementsJArray is null)
                return false;

            if (elementsJArray.Count != 1)
                return false;

            if (elementsJArray[0] is not JObject elementsObject)
                return false;

            try
            {
                var elementObject = elementsObject.ToObject<BlockModelElement.JsonObject>();
                if (elementObject is null)
                    return false;

                element = BlockModelElement.CreateFromJson(elementObject);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning("Parsing block model element failed: {Message}", ObjectFormatter.Format(ex));
                return false;
            }

            if (element.From != new Vector3<int>(0, 0, 0) || element.To != new Vector3<int>(16, 16, 16))
                return false;

            foreach (string faceKey in FaceMaskMap.Keys)
            {
                if (!element.Faces.AllFaces.ContainsKey(faceKey))
                    return false;
            }

            foreach (BlockFace face in element.Faces.AllFaces.Values)
            {
                if (!face.UV.IsFullUV)
                    return false;
            }

            return true;
        }

        private static bool IsCubeTextures(ICubeBlockModel model)
        {
            BlockModelElement? element = model.Element;
            if (element is null)
                return false;

            return IsCubeTextures(model, element, AllFacesMask);
        }

        private static bool IsCubeTextures(IObjectModel model, BlockModelElement element, int needsMask)
        {
            // 在当前模型的纹理中清除已找到的面
            foreach (var item in element.Faces.AllFaces)
            {
                if (model.Textures.ContainsKey(item.Value.Texture.TrimStart('#')) &&
                    FaceMaskMap.TryGetValue(item.Key, out int faceMask))
                {
                    // 清除这个面对应的 bit
                    needsMask &= ~faceMask;
                    if (needsMask == 0)
                        return true;
                }
            }

            // 还有缺失的面，向上查找父模型
            IObjectModel? parent = model.Parent;
            return parent is not null && IsCubeTextures(parent, element, needsMask);
        }
    }
}
