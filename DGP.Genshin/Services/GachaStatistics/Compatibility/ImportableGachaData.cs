﻿using DGP.Genshin.MiHoYoAPI.Gacha;
using System.Collections.Generic;

namespace DGP.Genshin.Services.GachaStatistics.Compatibility
{
    public class ImportableGachaData
    {
        public GachaData? Data { get; set; }
        public string? Uid { get; set; }
    }
}