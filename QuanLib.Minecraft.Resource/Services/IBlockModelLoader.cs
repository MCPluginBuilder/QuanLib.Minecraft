using QuanLib.Minecraft.Resource.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services
{
    public interface IBlockModelLoader
    {
        public Task<Dictionary<string, ICubeBlockModel>> LoadBlockModelsAsync(IReadOnlyDictionary<string, AssetFileEntry> entries, IReadOnlyDictionary<string, BlockStateModel> blockStates);
    }
}
