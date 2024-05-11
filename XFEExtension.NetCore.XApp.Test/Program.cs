using XFEExtension.NetCore.StringExtension;
using XFEExtension.NetCore.XApp.Core;
using XFEExtension.NetCore.XApp.Extensions;
using XFEExtension.NetCore.XFETransform;

internal class Program
{
    [SMTest]
    private static async Task MainTest()
    {
        try
        {
            var app = CreateFirstXApp();
            var page = await app.GetMainPage();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static XApp CreateFirstXApp()
    {
        var appInfo = new XAppInformation(string.Empty, "be98276c-48d6-451d-8a0a-a311d782d9d6", "第一个小程序", new Version(1, 0, 5), "这是第一个小程序", "XFE工作室室长", "4174240069", string.Empty, string.Empty, string.Empty, string.Empty);
        var xApp = new XApp
        {
            AppInformation = appInfo
        };
        xApp.AppFiles.Add(XAppCode.FromCode(XAppFileType.XFCS, "XApp.xfcs", """
            namespace MyFirstXApp;

            public class XAppProgram
            {
                public static void Main(string[] args)
                {
                    XAppBuilder.CreateBuilder()
                               .WithShellDev(new AppShell())
                               .BuildAndRun();
                }
            }
            """));
        xApp.AppFiles.Add(XAppCode.FromCode(XAppFileType.CS, "GlobalUsing.g.cs", """
            global using global::Microsoft.Maui;
            global using global::Microsoft.Maui.Controls;
            global using global::Microsoft.Maui.Controls.Hosting;
            global using global::Microsoft.Maui.Controls.Xaml;
            global using global::Microsoft.Maui.Dispatching;
            global using global::Microsoft.Maui.Graphics;
            global using global::System;
            global using global::System.Collections.Generic;
            global using global::System.IO;
            global using global::System.Linq;
            global using global::XFEExtension.NetCore.XApp;
            global using global::System.Threading;
            global using global::System.Threading.Tasks;
            """));
        xApp.AppFiles.Add(XAppCode.FromCode(XAppFileType.XFCS, "MainPage.xfcs", """"
            namespace MyFirstXApp;

            public partial class MainPage : ContentPage
            {
                public MainPage()
                {
                    InitXAppPage();
                }
            }
            """"));
        xApp.AppFiles.Add(XAppCode.FromCode(XAppFileType.CS, "MainPage.g.cs", """
            namespace MyFirstXApp;

            public partial class MainPage
            {
                internal void InitXAppPage()
                {
                    this.LoadFromXaml(@"[XFMLTOLOAD]");
                }
            }
            """));
        xApp.AppFiles.Add(XAppCode.FromCode(XAppFileType.XFML, "MainPage.xfml", """
            <?xml version="1.0" encoding="utf-8" ?>
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                         x:Class="MyFirstXApp.MainPage"
                         Title="第一个小程序">
                <StackLayout>
                    <Label Text="这是一个小程序" FontSize="22" HorizontalOptions="Center" TextColor="#000"/>
                    <Label Text="这是一个小程序" FontSize="18" HorizontalOptions="Center"/>
                    <Label Text="这是一个小程序" FontSize="12" HorizontalOptions="Center"/>
                </StackLayout>
            </ContentPage>
            """));
        xApp.AppFiles.Add(XAppCode.FromCode(XAppFileType.XFCS, "AppShell.xfcs", """"
            namespace MyFirstXApp;

            public partial class AppShell : Shell
            {
                public AppShell()
                {
                    InitXAppPage();
                }
            }
            """"));
        xApp.AppFiles.Add(XAppCode.FromCode(XAppFileType.CS, "AppShell.g.cs", """
            namespace MyFirstXApp;

            public partial class AppShell
            {
                internal void InitXAppPage()
                {
                    this.LoadFromXaml(@"[XFMLTOLOAD]");
                    this.AddLogicalChild(new ShellContent()
                        {
                            ContentTemplate = new DataTemplate(typeof(MainPage))
                        });
                }
            }
            """));
        xApp.AppFiles.Add(XAppCode.FromCode(XAppFileType.XFML, "AppShell.xfml", """
            <?xml version="1.0" encoding="UTF-8" ?>
            <Shell
                x:Class="MyFirstXApp.AppShell"
                xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                xmlns:local="clr-namespace:MyFirstXApp">
            </Shell>
            """));
        return xApp;
    }
}