using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace QuanLib.Minecraft.Resource
{
    public class ResourceEntry
    {
        public ResourceEntry(string modId, IEnumerable<AssetFileEntry> assetEntries)
        {
            ModId = modId;

            ResourcePath resourcePath = new(modId);
            Dictionary<string, AssetFileEntry> blockStates = [];
            Dictionary<string, AssetFileEntry> itemMapping = [];
            Dictionary<string, AssetFileEntry> blockModels = [];
            Dictionary<string, AssetFileEntry> blockTextures = [];
            Dictionary<string, AssetFileEntry> itemModels = [];
            Dictionary<string, AssetFileEntry> itemTextures = [];

            foreach (AssetFileEntry entry in assetEntries)
            {
                if (entry.FilePath.StartsWith(resourcePath.BlockStates))
                    blockStates[entry.AssetId] = entry;
                else if (entry.FilePath.StartsWith(resourcePath.ItemMapping))
                    itemMapping[entry.AssetId] = entry;
                else if (entry.FilePath.StartsWith(resourcePath.BlockModels))
                    blockModels[entry.AssetId] = entry;
                else if (entry.FilePath.StartsWith(resourcePath.BlockTextures))
                    blockTextures[entry.AssetId] = entry;
                else if (entry.FilePath.StartsWith(resourcePath.ItemModels))
                    itemModels[entry.AssetId] = entry;
                else if (entry.FilePath.StartsWith(resourcePath.ItemTextures))
                    itemTextures[entry.AssetId] = entry;
            }

            BlockStates = blockStates.AsReadOnly();
            ItemMapping = itemMapping.AsReadOnly();
            BlockModels = blockModels.AsReadOnly();
            ItemModels = itemModels.AsReadOnly();
            BlockTextures = blockTextures.AsReadOnly();
            ItemTextures = itemTextures.AsReadOnly();
        }

        public string ModId { get; }

        public bool IsEmpty =>
            BlockStates.Count == 0 &&
            ItemMapping.Count == 0 &&
            BlockModels.Count == 0 &&
            ItemModels.Count == 0 &&
            BlockTextures.Count == 0 &&
            ItemTextures.Count == 0;

        public ReadOnlyDictionary<string, AssetFileEntry> BlockStates { get; }

        public ReadOnlyDictionary<string, AssetFileEntry> ItemMapping { get; }

        public ReadOnlyDictionary<string, AssetFileEntry> BlockModels { get; }

        public ReadOnlyDictionary<string, AssetFileEntry> ItemModels { get; }

        public ReadOnlyDictionary<string, AssetFileEntry> BlockTextures { get; }

        public ReadOnlyDictionary<string, AssetFileEntry> ItemTextures { get; }
    }
}
