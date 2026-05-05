using Newtonsoft.Json;
using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Models
{
    public record class BlockFace(string Texture, string Cullface, int Rotation, UV UV)
    {
        public const string MISSINGNO = "#missingno";

        public static BlockFace CreateMissingno(string face)
        {
            return new BlockFace(MISSINGNO, face, 0, UV.FullUV);
        }

        public static BlockFace CreateFromJson(JsonObject jsonObject)
        {
            NullValidator.ValidateObject(jsonObject, nameof(jsonObject));

            UV uv = jsonObject.UV is null ? UV.FullUV : ParseUV(jsonObject.UV);
            return new BlockFace(jsonObject.Texture, jsonObject.Cullface, jsonObject.Rotation, uv);
        }

        private static UV ParseUV(int[] uvArray)
        {
            if (uvArray.Length != 4)
                throw new InvalidOperationException("uv array must have exactly 4 elements");

            return new UV(new Point(uvArray[0], uvArray[1]), new Point(uvArray[2], uvArray[3]));
        }

        public class JsonObject
        {
            [JsonProperty("texture")]
            public required string Texture { get; set; }

            [JsonProperty("cullface")]
            public required string Cullface { get; set; }

            [JsonProperty("rotation")]
            public required int Rotation { get; set; }

            [Nullable]
            [JsonProperty("uv")]
            public required int[]? UV { get; set; }
        }
    }
}
