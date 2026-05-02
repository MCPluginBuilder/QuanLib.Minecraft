using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Extensions
{
    public static class AssetHelper
    {
        public static string GetFullName(string assetId)
        {
            ArgumentNullException.ThrowIfNull(assetId, nameof(assetId));

            if (assetId.Contains(':'))
                return assetId;
            else
                return "minecraft:" + assetId;
        }
    }
}
