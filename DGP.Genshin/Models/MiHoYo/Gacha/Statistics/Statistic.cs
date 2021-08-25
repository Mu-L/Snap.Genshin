﻿using DGP.Snap.Framework.Attributes.DataModel;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Gacha.Statistics
{
    /// <summary>
    /// 统计信息
    /// </summary>
    [InterModel]
    public class Statistic
    {
        public string Uid { get; set; }
        public StatisticBanner Permanent { get; set; }
        public StatisticBanner CharacterEvent { get; set; }
        public StatisticBanner WeaponEvent { get; set; }
        public List<StatisticItem> Characters { get; set; }
        public List<StatisticItem> Weapons { get; set; }
        public List<SpecificBanner> SpecificBanners { get; set; }
    }
}