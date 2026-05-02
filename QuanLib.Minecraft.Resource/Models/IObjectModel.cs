using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Models
{
    public interface IObjectModel
    {
        public IObjectModel? Parent { get; }

        public string ModelId { get; }

        public IReadOnlyDictionary<string, string> Textures { get; }
    }
}
