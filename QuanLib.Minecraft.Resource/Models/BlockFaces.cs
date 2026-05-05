using QuanLib.Game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace QuanLib.Minecraft.Resource.Models
{
    public class BlockFaces
    {
        public BlockFaces(IReadOnlyDictionary<string, BlockFace> faces)
        {
            ArgumentNullException.ThrowIfNull(faces, nameof(faces));

            AllFaces = faces;
        }

        public IReadOnlyDictionary<string, BlockFace> AllFaces { get; }

        public BlockFace Down => AllFaces.GetValueOrDefault("down", BlockFace.CreateMissingno("down"));

        public BlockFace Up => AllFaces.GetValueOrDefault("up", BlockFace.CreateMissingno("up"));

        public BlockFace North => AllFaces.GetValueOrDefault("north", BlockFace.CreateMissingno("north"));

        public BlockFace South => AllFaces.GetValueOrDefault("south", BlockFace.CreateMissingno("south"));

        public BlockFace West => AllFaces.GetValueOrDefault("west", BlockFace.CreateMissingno("west"));

        public BlockFace East => AllFaces.GetValueOrDefault("east", BlockFace.CreateMissingno("east"));

        public BlockFace GetFace(Facing facing)
        {
            return facing switch
            {
                Facing.Xp => East,
                Facing.Xm => West,
                Facing.Yp => Up,
                Facing.Ym => Down,
                Facing.Zp => South,
                Facing.Zm => North,
                _ => throw new InvalidEnumArgumentException(nameof(facing), (int)facing, typeof(Facing)),
            };
        }
    }
}
