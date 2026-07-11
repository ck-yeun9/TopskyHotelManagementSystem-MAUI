<h1 align="center"><img src="https://foruda.gitee.com/avatar/1677165732744604624/7158691_java-and-net_1677165732.png!avatar100" alt="Organization Logo.png" /></h1>
<h1 align="center">TopskyHotelManagementSystem-MAUI</h1>
<p align="center">
	<a href='https://gitee.com/java-and-net/topsky-hotel-management-system-maui/stargazers'><img src='https://gitee.com/java-and-net/topsky-hotel-management-system-maui/badge/star.svg?theme=white' alt='star'></img></a>
        <a href='https://gitee.com/java-and-net/topsky-hotel-management-system-maui/members'><img src='https://gitee.com/java-and-net/topsky-hotel-management-system-maui/badge/fork.svg?theme=white' alt='fork'></img></a>
        <a href='https://img.shields.io/badge/license-MIT-000000.svg'><img src="https://img.shields.io/badge/license-MIT-000000.svg" alt=""></img></a>
        <a href='https://img.shields.io/badge/language-C#-red.svg'><img src="https://img.shields.io/badge/language-CSharp-red.svg" alt=""></img></a>
</p>
<div align="center">
	<p>中文文档 | <a href="./README.en.md">English Document</a></p>
</div>



#  :exclamation: 重要说明：

**项目基于.NET 10的MAUI进行开发，目前仅考虑安卓端，其他暂不具备测试条件**

#  :pray: 引用的开源项目：

1. ##### MAUI——.NET MAUI is the .NET Multi-platform App UI, a framework for building native device applications spanning mobile, tablet, and desktop.[MAUI,MIT开源协议](https://github.com/dotnet/maui)

1. ##### UraniumUI——Uranium is a Free & Open-Source UI Kit for MAUI.[UraniumUI,Apache-2.0 开源协议](https://github.com/enisn/UraniumUI)

1. ##### Plugin.Fingerprint——Xamarin and MvvMCross plugin for authenticate a user via fingerprint sensor.[Plugin.Fingerprint,MS-PL 开源协议](https://github.com/smstuebe/xamarin-fingerprint)

1. ##### RestSharp——Simple REST and HTTP API Client for .NET。[RestSharp,Apache-2.0开源协议](https://github.com/restsharp/RestSharp)


#  :exclamation: 本项目说明：

1、在对本项目进行二次开发时，请遵循 MIT 开源协议。所有引用的其他开源项目均采用其各自的开源协议。使用这些开源项目时，请务必在项目介绍中添加相应的声明，并按照各自的开源协议进行开源等操作。

2、有bug欢迎提出issue！或进行评论

3、本系统UI框架目前主要基于MAUI原生控件进行创建，后续可能考虑引入第三方控件，在此特别声明！

#  :thought_balloon: 开发目的：

与酒店管理系统配套开发的手机程序，主要帮助用户查询空房情况以及预约房间、了解酒店最新的新闻公告、对入住体验进行评价等等。

#  :mag_right: 系统开发环境：

操作系统：Windows 11(x64)

开发工具：Microsoft Visual Studio 2022(系统最新版本)

数据库：SQLite

开发语言：C#语言、T-SQL语言

开发平台：.Net

开发框架：.Net 10

开发技术：.NET 10 MAUI

调试环境：Xiaomi 17 (Android 16)

#  :open_file_folder: 系统结构：

```tree
EOM.TSHotelManagementSystem.Mobile
├─ .git
├─ .gitignore
├─ EOM.TSHotelManagementSystem.Mobile.sln
├─ LICENSE
├─ README.md
├─ EOM.TSHotelManagementSystem.Mobile.UI
│    ├─ App.xaml
│    ├─ AppShell.xaml
│    ├─ MauiProgram.cs
│    ├─ Views
│    ├─ ViewModels
│    ├─ Service
│    ├─ Helper
│    ├─ Converter
│    ├─ Program.cs
│    ├─ Properties
│    ├─ Resources
├─ README.en.md
```

#  :books: 系统功能模块汇总：

| 功能汇总 |              |              |          |          |      |      |
| -------- | ------------ | ------------ | -------- | -------- | ---- | ---- |
| 新闻     | 新闻列表     | 新闻跳转查看 |          |          |      |      |
| 入住     | 预约房间     | 房间入住     | 商品消费 | 历史评价 |      |      |
| 我的     | 个人信息设置 | 系统设置     | 退出登录 |          |      |      |


#  :family: 项目作者：

**原创团队：Jackson、Benjamin、Bin、Jonathan**

**维护团队：易开元(Easy Open Meta)**

#  :computer: 项目运行部署：

**下载并安装.NET 10及以上SDK版本。**
**下载并安装Microsoft Visual Studio Professional 2022及以上版本，并通过下载Zip包解压，打开.sln后缀格式文件运行。**

# 项目效果图：

 ![news](.\preview\app-news.png) ![news-detail](.\preview\app-news-detail.png)

# License Information
This project is licensed under the MIT License - see [LICENSE](LICENSE) file.

Contains third-party components licensed under Apache 2.0:
- See [licenses/THIRD-PARTY-NOTICE.md](licenses/THIRD-PARTY-NOTICE.md)

[![java-and-net/TopskyHotelManagementSystem-MAUI](https://gitee.com/java-and-net/topsky-hotel-management-system-maui/widgets/widget_card.svg?colors=4183c4,ffffff,ffffff,e3e9ed,666666,9b9b9b)](https://gitee.com/java-and-net/topsky-hotel-management-system-maui)
