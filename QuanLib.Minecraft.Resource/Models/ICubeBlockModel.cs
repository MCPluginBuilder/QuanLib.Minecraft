using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Models
{
    public interface ICubeBlockModel : IObjectModel
    {
        public BlockModelElement? Element { get; }

        public bool IsCubeAll { get; }

        public string Down { get; }

        public string Up { get; }

        public string North { get; }

        public string East { get; }

        public string South { get; }

        public string West { get; }
    }
}
