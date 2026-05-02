using QuanLib.Minecraft.Resource.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services
{
    public interface IItemModelLoader
    {
        public Task<Dictionary<string, IGeneratedItemModel>> LoadItemModelsAsync(IReadOnlyDictionary<string, AssetFileEntry> entries);

        public Task<Dictionary<string, IGeneratedItemModel>> LoadItemModelsAsync(IReadOnlyDictionary<string, AssetFileEntry> entries, IReadOnlyDictionary<string, string> itemMappings);
    }
}
