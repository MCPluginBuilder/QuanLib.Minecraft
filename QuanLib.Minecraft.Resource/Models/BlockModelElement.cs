using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Models
{
    public class BlockModelElement
    {
        public BlockModelElement(Vector3<int> from, Vector3<int> to, BlockFaces faces)
        {
            ArgumentNullException.ThrowIfNull(faces, nameof(faces));

            From = from;
            To = to;
            Faces = faces;
        }

        public Vector3<int> From { get; }

        public Vector3<int> To { get; }

        public BlockFaces Faces { get; }

        public static BlockModelElement CreateFromJson(JsonObject jsonObject)
        {
            NullValidator.ValidateObject(jsonObject, nameof(jsonObject));

            int[] fromArray = jsonObject.From;
            int[] toArray = jsonObject.To;
            if (fromArray.Length != 3 || toArray.Length != 3)
                throw new InvalidOperationException("The 'from' and 'to' arrays must have exactly 3 elements.");

            Vector3<int> from = new(fromArray[0], fromArray[1], fromArray[2]);
            Vector3<int> to = new(toArray[0], toArray[1], toArray[2]);
            BlockFaces faces = new(jsonObject.Faces.ToDictionary(kvp => kvp.Key, kvp => BlockFace.CreateFromJson(kvp.Value)));

            return new BlockModelElement(from, to, faces);
        }

        public class JsonObject
        {
            [JsonProperty("from")]
            public required int[] From { get; set; }

            [JsonProperty("to")]
            public required int[] To { get; set; }

            [JsonProperty("faces")]
            public required Dictionary<string, BlockFace.JsonObject> Faces { get; set; }
        }
    }
}
