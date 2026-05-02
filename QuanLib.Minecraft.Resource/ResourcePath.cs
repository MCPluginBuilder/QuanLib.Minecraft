using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource
{
    public class ResourcePath
    {
        public ResourcePath(string modId)
        {
            ArgumentException.ThrowIfNullOrEmpty(modId, nameof(modId));

            BlockStates = $"/assets/{modId}/blockstates/";
            ItemMapping = $"/assets/{modId}/items/";
            BlockModels = $"/assets/{modId}/models/block/";
            BlockTextures = $"/assets/{modId}/textures/block/";
            ItemModels = $"/assets/{modId}/models/item/";
            ItemTextures = $"/assets/{modId}/textures/item/";
            Languages = $"/assets/{modId}/lang/";
        }

        public string BlockStates { get; }

        public string ItemMapping { get; }

        public string BlockModels { get; }

        public string BlockTextures { get; }

        public string ItemModels { get; }

        public string ItemTextures { get; }

        public string Languages { get; }
    }
}
