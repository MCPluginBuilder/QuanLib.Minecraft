using QuanLib.IO.Zip;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services
{
    public interface IResourceEntryLoader
    {
        public ResourceEntryManager LoadResourceEntry(ZipPack[] zipPacks);
    }
}
