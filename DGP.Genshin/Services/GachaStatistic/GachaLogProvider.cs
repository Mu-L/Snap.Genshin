﻿using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.Gacha;
using DGP.Genshin.Models.MiHoYo.Request;
using DGP.Snap.Framework.Data.Privacy;
using DGP.Snap.Framework.Extensions.System;
using DGP.Snap.Framework.Extensions.System.Windows.Threading;
using DGP.Snap.Framework.Net.Web.QueryString;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DGP.Genshin.Services.GachaStatistic
{
    /// <summary>
    /// 联机抽卡记录提供器
    /// </summary>
    public class GachaLogProvider
    {
        private const string gachaLogBaseUrl = "https://hk4e-api.mihoyo.com/event/gacha_info/api/getGachaLog";
        private string logFilePath;

        private string gachaLogUrl;
        private string configListUrl;

        private Config gachaConfig;
        public Config GachaConfig
        {
            get
            {
                if (this.gachaConfig == null)
                    this.gachaConfig = GetGachaConfig();
                return this.gachaConfig;
            }
        }

        public LocalGachaLogProvider LocalGachaLogProvider { get; private set; }

        public GachaStatisticService Service { get; set; }

        #region Initialization
        public GachaLogProvider(GachaStatisticService service)
        {
            this.Service = service;
            this.LocalGachaLogProvider = new LocalGachaLogProvider(service);
        }

        #endregion

        /// <summary>
        /// 尝试在日志文件中寻找url
        /// </summary>
        /// <returns></returns>
        public bool TryFindUrlInLogFile()
        {
            //combine into locallow
            string LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            this.logFilePath = LocalPath + @"Low\miHoYo\原神\output_log.txt";
            //share the file to make genshin access it so it doesn't crash when game is running
            using (StreamReader sr = new StreamReader(File.Open(this.logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                string str;
                //check till the log file end to make sure
                while (sr.Peek() >= 0)
                {
                    str = sr.ReadLine();
                    if (str.StartsWith("OnGetWebViewPageFinish:") && str.EndsWith("#/log"))
                    {
                        str = str.Replace("OnGetWebViewPageFinish:", "").Replace("#/log", "");
                        string[] splitedUrl = str.Split('?');
                        splitedUrl[0] = gachaLogBaseUrl;
                        this.gachaLogUrl = String.Join("?", splitedUrl);
                    }
                }
            }
            if (this.gachaLogUrl == null)
            {
                return false;
            }
            else
            {
                this.configListUrl = this.gachaLogUrl.Replace("getGachaLog?", "getConfigList?");
                return true;
            }
        }

        /// <summary>
        /// 获取祈愿池信息
        /// </summary>
        /// <returns></returns>
        private Config GetGachaConfig()
        {
            Response<Config> resp = new Requester(new RequestOptions
            {
                {"Accept", RequestOptions.Json },
                {"User-Agent", RequestOptions.CommonUA }
            }).Get<Config>(this.configListUrl);
            return resp.ReturnCode == 0 ? resp.Data : null;
        }

        /// <summary>
        /// 获取单个奖池的祈愿记录增量信息，能改变当前uid
        /// </summary>
        /// <param name="type">卡池类型</param>
        public void FetchGachaLogIncrement(ConfigType type)
        {
            lock (LocalGachaLogProvider.processing)
            {
                List<GachaLogItem> increment = new List<GachaLogItem>();
                int currentPage = 0;
                long endId = 0;
                do
                {
                    if (TryGetBatch(out GachaLog gachaLog, type, endId, ++currentPage))
                    {
                        foreach (GachaLogItem item in gachaLog.List)
                        {
                            App.Current.Invoke(() => Service.SwitchUidContext(item.Uid));
                            //this one is increment
                            if (item.TimeId > this.LocalGachaLogProvider.GetNewestTimeId(type, item.Uid))
                            {
                                increment.Add(item);
                            }
                            else//already done the new item
                            {
                                MergeIncrement(type, increment);
                                return;
                            }
                        }
                        //last page
                        if (gachaLog.List.Count < 20)
                        {
                            break;
                        }
                        endId = gachaLog.List.Last().TimeId;
                    }
                    else
                    {
                        //url not valid
                        break;
                    }
                } while (true);
                //first time fecth could go here
                MergeIncrement(type, increment);
            }
        }

        /// <summary>
        /// 合并增量
        /// </summary>
        /// <param name="type">卡池类型</param>
        /// <param name="increment">增量</param>
        private void MergeIncrement(ConfigType type, List<GachaLogItem> increment)
        {
            //简单的将老数据插入到增量后侧，最后重置数据
            GachaData dict = this.LocalGachaLogProvider.Data[this.Service.SelectedUid.UnMaskedValue];
            if (dict.ContainsKey(type.Key))
            {
                List<GachaLogItem> local = dict[type.Key];
                increment.AddRange(local);
            }
            dict[type.Key] = increment;
        }

        public void SaveAllLogs() => this.LocalGachaLogProvider.SaveAllLogs();

        public event Action<FetchProgress> OnFetchProgressed;

        /// <summary>
        /// try to get a segment contains 20 log items
        /// </summary>
        /// <param name="result"></param>
        /// <param name="type"></param>
        /// <param name="endId"></param>
        /// <returns></returns>
        private bool TryGetBatch(out GachaLog result, ConfigType type, long endId, int currentPage)
        {
            this.Log($"try to get batch of {type.Name} with end_id:{endId}");
            OnFetchProgressed?.Invoke(new FetchProgress { Type = type.Name, Page = currentPage });
            //modify the url
            string[] splitedUrl = this.gachaLogUrl.Split('?');
            string baseUrl = splitedUrl[0];

            //parse querystrings
            QueryString query = QueryString.Parse(splitedUrl[1]);
            query.Set("gacha_type", type.Key);
            //20 is the max size the api can return
            query.Set("size", "20");
            query.Set("lang", "zh-cn");
            query.Set("end_id", endId.ToString());
            string finalUrl = $"{baseUrl}?{query}";

            Response<GachaLog> resp = new Requester(new RequestOptions
            {
                {"Accept", RequestOptions.Json },
                {"User-Agent", RequestOptions.CommonUA }
            }).Get<GachaLog>(finalUrl);

            if (resp.ReturnCode == 0)
            {
                result = resp.Data;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
    }
}