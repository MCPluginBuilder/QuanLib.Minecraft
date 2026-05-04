using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Models
{
    public interface IRotatedCubeBlockModel : ICubeBlockModel
    {
        public BlockRotationMapping BlockRotationMapping { get; }
    }
}
