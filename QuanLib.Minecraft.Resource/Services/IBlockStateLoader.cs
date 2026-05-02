using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services
{
    public interface IBlockStateLoader
    {
        public Task<Dictionary<string, BlockStateModel>> LoadBlockStatesAsync(IEnumerable<AssetFileEntry> entries);
    }
}
