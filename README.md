

README.md文件内容如下：

![](https://foruda.gitee.com/avatar/1677165732744604624/7158691_java-and-net_1677165732.png!avatar100)

# TopskyHotelManagementSystem-MAUI

[![Gitee Fork](https://gitee.com/java-and-net/topsky-hotel-management-system-maui/badge/fork.svg?theme=white)](https://gitee.com/java-and-net/topsky-hotel-management-system-maui/fork)
[![License](https://img.shields.io/badge/license-MIT-000000.svg)](https://opensource.org/licenses/MIT)
[![Language](https://img.shields.io/badge/language-CSharp-red.svg)](https://gitee.com/java-and-net/topsky-hotel-management-system-maui)

<div align="center">
	<a href="./README.en.md">English Document</a>
</div>

#  :exclamation: 重要说明：
本项目是使用.NET 8 MAUI开发的，目前仅专注于Android平台，因为现阶段其他平台无法进行测试。

# :pray: 引用的开源项目：
1. ##### MAUI——.NET MAUI 是 .NET 多平台应用 UI，是一个用于构建原生设备应用程序的框架，支持移动、平板和桌面应用。[MAUI, MIT License](https://github.com/dotnet/maui)

1. ##### Plugin.Toolkit.Fonts.MaterialIcons [Plugin.Toolkit.Fonts.MaterialIcons, 未知协议](https://github.com/andyapin/Plugin.Toolkit.Fonts.MaterialIcons)

# :exclamation: 本项目说明：
1. 二次开发本项目时，请遵循MIT开源协议。所有引用的开源项目都有他们自己的开源协议。使用这些开源项目时，务必在项目说明中加入相应的声明，并依从各自的开源协议进行任何开源行为。
2. 欢迎提交bug和意见！
3. 本系统的UI框架目前主要使用原生MAUI控件构建，请注意未来可能会加入第三方控件。

# :thought_balloon: 开发目的：
与酒店管理系统配套开发的移动应用，主要帮助用户进行：
- 查询客房情况
- 进行预订
- 获取最新的酒店新闻和公告
- 评价入住体验
等操作。

# :mag_right: 系统开发环境：
操作系统：Windows 11(x64)

开发工具：Microsoft Visual Studio 2022（最新系统版本）

数据库：SQLite

编程语言：C#语言、T-SQL语言

开发平台：.Net

开发框架：.Net 8

开发技术：.NET 8 MAUI

# :open_file_folder: 系统结构：
```
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

# :books: 系统功能模块汇总：
| 功能汇总 |              |              |          |          |      |      |
| ---------------- | ----------------- | --------------- | ------- | -------------- | ---- | ---- |
| 新闻             | 新闻列表         | 查看新闻       |          |                |      |      |
| 入住             | 预订              | 费用账单        | 评论     |                |      |      |
| 我的             | 个人设置          | 系统设置        | 注销     | 删除账户         |      |      |

# :family: 项目作者：
**原团队：Jackson, Benjamin, Bin, Jonathan**

**开发&维护团队：Easy Open Meta (易开元)**

# :computer: 项目运行部署：
**下载并安装.NET SDK 8版本或以上。**

**下载并安装Microsoft Visual Studio Professional 2022或以上版本，解压下载的Zip包，运行.sln文件。**

[![java-and-net/TopskyHotelManagementSystem-MAUI](https://gitee.com/java-and-net/topsky-hotel-management-system-maui/widgets/widget_card.svg?colors=4183c4,ffffff,ffffff,e3e9ed,666666,9b9b9b)](https://gitee.com/java-and-net/topsky-hotel-management-system-maui)