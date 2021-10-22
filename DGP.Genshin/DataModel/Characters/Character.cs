﻿using DGP.Genshin.DataModel.Materials.GemStones;
using DGP.Genshin.DataModel.Materials.Locals;
using DGP.Genshin.DataModel.Materials.Monsters;
using DGP.Genshin.DataModel.Materials.Talents;
using DGP.Genshin.DataModel.Materials.Weeklys;

namespace DGP.Genshin.DataModel.Characters
{
    public class Character : Primitive
    {
        public string? Weapon { get; set; }
        public string? Element { get; set; }
        public string? Profile { get; set; }
        public string? GachaCard { get; set; }
        public string? GachaSplash { get; set; }
        public string? City { get; set; }
        public bool IsMondstdat() => this.City == @"https://genshin.honeyhunterworld.com/img/rep/monstadt_rep_70.png";
        public bool IsLiyue() => this.City == @"https://genshin.honeyhunterworld.com/img/rep/liyue_rep_70.png";
        public bool IsInazuma() => this.City == @"https://genshin.honeyhunterworld.com/img/rep/inazuma_rep_70.png";
        public string? AscensionStat { get; set; }
        public string? AscensionStatValue { get; set; }
        public Talent? Talent { get; set; }
        public Boss? Boss { get; set; }
        public GemStone? GemStone { get; set; }
        public Local? Local { get; set; }
        public Monster? Monster { get; set; }
        public Weekly? Weekly { get; set; }
    }
}