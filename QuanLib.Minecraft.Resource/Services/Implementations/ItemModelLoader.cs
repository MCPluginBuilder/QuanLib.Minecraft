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
    public class ItemModelLoader(ILogger<ItemModelLoader>? logger = null) : IItemModelLoader
    {
        private const string BUILTIN_GENERATED = "minecraft:builtin/generated";
        private const string ITEM_GENERATED = "minecraft:item/generated";

        private readonly ILogger<ItemModelLoader>? _logger = logger;

        public Task<Dictionary<string, IGeneratedItemModel>> LoadItemModelsAsync(IReadOnlyDictionary<string, AssetFileEntry> entries)
        {
            ArgumentNullException.ThrowIfNull(entries, nameof(entries));

            Dictionary<string, string> itemMappings = [];
            foreach (var item in entries)
            {
                AssetFileEntry entry = item.Value;
                string[] assetIdParts = entry.AssetId.Split(':', 2);
                if (assetIdParts.Length != 2)
                    throw new FormatException($"Invalid asset ID format: {entry.AssetId}");

                string itemId = $"{assetIdParts[0]}:{Path.GetFileNameWithoutExtension(entry.FilePath)}";
                string modelId = entry.AssetId;
                itemMappings.Add(itemId, modelId);
            }

            return LoadItemModelsAsync(entries, itemMappings);
        }

        public async Task<Dictionary<string, IGeneratedItemModel>> LoadItemModelsAsync(IReadOnlyDictionary<string, AssetFileEntry> entries, IReadOnlyDictionary<string, string> itemMappings)
        {
            ArgumentNullException.ThrowIfNull(entries, nameof(entries));
            ArgumentNullException.ThrowIfNull(itemMappings, nameof(itemMappings));

            Dictionary<string, IGeneratedItemModel> result = [];
            Dictionary<string, IGeneratedItemModel> source = [];
            HashSet<string> exclude = [];

            GeneratedItemModel rootModel = new(ITEM_GENERATED, ReadOnlyDictionary<string, string>.Empty, null);
            source.Add(rootModel.ModelId, rootModel);
            exclude.Add(BUILTIN_GENERATED);

            foreach (var item in itemMappings)
            {
                string itemId = item.Key;
                string modelId = item.Value;

                IGeneratedItemModel? model = await LoadModelAsync(modelId, entries, source, exclude);
                if (model is null)
                    continue;

                result.Add(itemId, model);
            }

            return result;
        }

        private async Task<IGeneratedItemModel?> LoadModelAsync(
            string modelId,
            IReadOnlyDictionary<string, AssetFileEntry> entries,
            IDictionary<string, IGeneratedItemModel> source,
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
                _logger?.LogWarning("Loading item model from '{FilePath}' failed: {Message}", entry.FilePath, ObjectFormatter.Format(ex));
                exclude.Add(modelId);
                return null;
            }

            if (jObject["parent"]?.Value<string>() is not string parentId)
            {
                exclude.Add(modelId);
                return null;
            }

            parentId = AssetHelper.GetFullName(parentId);
            if (parentId == BUILTIN_GENERATED || !parentId.Split(':', 2)[^1].StartsWith("item/"))
            {
                exclude.Add(modelId);
                return null;
            }

            IGeneratedItemModel? parentModel = await LoadModelAsync(parentId, entries, source, exclude);
            if (parentModel is null)
            {
                exclude.Add(modelId);
                return null;
            }

            JObject? texturesObject = jObject["textures"] as JObject;
            Dictionary<string, string> textures;
            try
            {
                textures = texturesObject?.ToObject<Dictionary<string, string>>() ?? [];
            }
            catch
            {
                textures = [];
            }

            GeneratedItemModel result = new(modelId, textures, parentModel);
            source.Add(modelId, result);
            return result;
        }
    }
}
