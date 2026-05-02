using QuanLib.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Textures
{
    public interface ICubeBlockTexture
    {
        public string BlockId { get; }

        public bool IsCubeAll { get; }

        public Texture GetTexture(Facing facing);
    }
}
