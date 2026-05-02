using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services.Implementations
{
    public class ItemMappingParser : IItemMappingParser
    {
        public string? ParseItemMapping(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

            JObject jObject = JObject.Parse(json);
            if (jObject["model"] is not JObject model)
                return null;

            if (model["type"]?.Value<string>() != "minecraft:model")
                return null;

            return model["model"]?.Value<string>();
        }
    }
}
