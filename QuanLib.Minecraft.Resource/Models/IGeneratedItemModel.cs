using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Models
{
    public interface IGeneratedItemModel : IObjectModel
    {
        public string Layer0 { get; }

        public string? Layer1 { get; }

        public string? Layer2 { get; }

        public string? Layer3 { get; }

        public string? Layer4 { get; }

        public string? Layer5 { get; }

        public IEnumerable<string> GetLayers();
    }
}
