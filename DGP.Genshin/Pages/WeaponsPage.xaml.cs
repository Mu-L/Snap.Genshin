﻿using DGP.Genshin.Data.Weapons;
using DGP.Genshin.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// WeaponsPage.xaml 的交互逻辑
    /// </summary>
    public partial class WeaponsPage : Page
    {
        public WeaponsPage()
        {
            this.DataContext = DataService.Instance;
            this.InitializeComponent();
        }
    }
}
