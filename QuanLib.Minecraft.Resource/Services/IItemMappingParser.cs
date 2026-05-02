using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services
{
    public interface IItemMappingParser
    {
        public string? ParseItemMapping(string json);
    }
}
