using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services.Implementations
{
    public class BlockStateParser : IBlockStateParser
    {
        public BlockStateModel[] ParseBlockState(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

            JObject jObject = JObject.Parse(json);
            if (jObject["variants"] is not JObject variants)
                return Array.Empty<BlockStateModel>();

            List<BlockStateModel> blockStates = [];
            foreach (var item in variants)
            {
                string blockState = item.Key;
                if (item.Value is not JObject variantObject)
                    continue;

                string? blockModel = variantObject.Value<string>("model");
                if (string.IsNullOrEmpty(blockModel))
                    continue;

                int rotateX = variantObject.Value<int?>("x") ?? 0;
                int rotateY = variantObject.Value<int?>("y") ?? 0;

                if (rotateX is not (0 or 90 or 180 or 270) || rotateY is not (0 or 90 or 180 or 270))
                    continue;

                BlockStateModel blockStateModel = new(blockState, blockModel, new BlockRotate(rotateX, rotateY));
                blockStates.Add(blockStateModel);
            }

            return blockStates.ToArray();
        }
    }
}
