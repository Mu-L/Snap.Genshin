﻿using Microsoft.Web.WebView2.Core;
using Snap.Data.Utility;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace DGP.Genshin.Control.GenshinElement
{
    public sealed partial class AnnouncementWindow : Window, IDisposable
    {
        private const string mihoyoSDKDefinition = 
            "window.miHoYoGameJSSDK = {" +
            "openInBrowser: function(url){ window.chrome.webview.postMessage(url); }," +
            "openInWebview: function(url){ location.href = url }}";
        private readonly string? targetContent;
        public AnnouncementWindow(string? content)
        {
            //不需要在此处检查WebView2可用性，由使用方代为检查
            targetContent = content;
            InitializeComponent();
        }

        public void Dispose()
        {
            WebView?.Dispose();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await WebView.EnsureCoreWebView2Async();
                WebView.CoreWebView2.ProcessFailed += (s, e) => WebView?.Dispose();
                //support click open browser.
                await WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(mihoyoSDKDefinition);
                WebView.CoreWebView2.WebMessageReceived += (s, e) => Browser.Open(e.TryGetWebMessageAsString);
            }
            catch
            {
                Close();
                return;
            }

            WebView.NavigateToString(targetContent);
        }
    }
}