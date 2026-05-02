using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Textures
{
    public interface IGeneratedItemTexture
    {
        public string ItemId { get; }

        public Texture GetTexture();
    }
}
