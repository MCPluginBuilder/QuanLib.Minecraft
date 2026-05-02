using Microsoft.Extensions.Logging;
using QuanLib.Core;
using QuanLib.IO.Zip;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services.Implementations
{
    public class ItemMappingLoader : IItemMappingLoader
    {
        public ItemMappingLoader(IItemMappingParser parser, ILogger<ItemMappingLoader>? logger = null)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));

            _parser = parser;
            _logger = logger;
        }

        private readonly IItemMappingParser _parser;
        private readonly ILogger<ItemMappingLoader>? _logger;

        public async Task<Dictionary<string, string>> LoadItemMappingsAsync(IEnumerable<AssetFileEntry> entries)
        {
            ArgumentNullException.ThrowIfNull(entries, nameof(entries));

            Dictionary<string, string> result = [];
            foreach (AssetFileEntry entry in entries)
            {
                string? modelId;
                try
                {
                    string json = await entry.Entry.ReadAllTextAsync(Encoding.UTF8);
                    modelId = _parser.ParseItemMapping(json);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning("Loading item mapping from '{FilePath}' failed: {Message}", entry.FilePath, ObjectFormatter.Format(ex));
                    continue;
                }

                if (string.IsNullOrEmpty(modelId))
                    continue;

                string[] assetIdParts = entry.AssetId.Split(':', 2);
                if (assetIdParts.Length != 2)
                    throw new FormatException($"Invalid asset ID format: {entry.AssetId}");

                StringBuilder itemIdBuilder = new();
                itemIdBuilder.Append(assetIdParts[0]);
                itemIdBuilder.Append(':');
                itemIdBuilder.Append(Path.GetFileNameWithoutExtension(entry.FilePath));

                result[itemIdBuilder.ToString()] = modelId;
            }

            return result;
        }
    }
}
