using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services
{
    public interface IBlockStateParser
    {
        public BlockStateModel[] ParseBlockState(string json);
    }
}
