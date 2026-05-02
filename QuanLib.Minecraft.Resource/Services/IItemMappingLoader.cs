using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services
{
    public interface IItemMappingLoader
    {
        public Task<Dictionary<string, string>> LoadItemMappingsAsync(IEnumerable<AssetFileEntry> entries);
    }
}
