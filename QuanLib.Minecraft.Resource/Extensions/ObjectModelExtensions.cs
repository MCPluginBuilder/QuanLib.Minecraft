using QuanLib.Game;
using QuanLib.Minecraft.Resource.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace QuanLib.Minecraft.Resource.Extensions
{
    public static class ObjectModelExtensions
    {
        public static string GetTextureAtFacing(this ICubeBlockModel model, Facing facing)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));

            return facing switch
            {
                Facing.Xp => model.East,
                Facing.Xm => model.West,
                Facing.Yp => model.Up,
                Facing.Ym => model.Down,
                Facing.Zp => model.South,
                Facing.Zm => model.North,
                _ => throw new InvalidEnumArgumentException(nameof(facing), (int)facing, typeof(Facing)),
            };
        }

        public static int GetTextureRotation(this ICubeBlockModel model, Facing facing)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));

            BlockModelElement? element = model.Element;
            if (model is IRotatedCubeBlockModel rotatedModel && !rotatedModel.BlockRotationMapping.IsZero)
            {
                RotationMapping rotationMapping = rotatedModel.BlockRotationMapping.GetRotationMapping(facing);
                int textureRotation = rotationMapping.TextureRotation;
                if (element is not null)
                {
                    textureRotation += element.Faces.GetFace(rotationMapping.TargetFacing).Rotation;
                    textureRotation %= 360;
                }

                return textureRotation;
            }

            if (element is not null)
                return element.Faces.GetFace(facing).Rotation;

            return 0;
        }

        public static string? GetTexture(this IObjectModel model, string face)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));

            return model.GetTexture(face, []);
        }

        private static string? GetTexture(this IObjectModel model, string face, Dictionary<string, string> childTextures)
        {
            if (model.Textures.TryGetValue(face, out var value))
                return GetTexture(value, childTextures);

            if (model.Parent is not null)
            {
                Dictionary<string, string> mergeTextures = new(model.Textures);
                foreach (var item in childTextures)
                    mergeTextures[item.Key] = item.Value;

                return GetTexture(model.Parent, face, mergeTextures);
            }

            return null;
        }

        private static string GetTexture(string value, IReadOnlyDictionary<string, string> childTextures)
        {
            if (!value.StartsWith('#'))
                return value;

            if (!childTextures.TryGetValue(value[1..], out var childValue))
                return value;

            return GetTexture(childValue, childTextures);
        }
    }
}
