# 更新日志

## [1.0.22] - 2025-08-19

### Bug Fixes

- 还原番剧解析代码 #354

### Miscellaneous Tasks

- 优化ci构建，使trimmed正确移除程序集

### Refactor

- 消除对WebClient程序集的依赖 (#350)

## [1.0.21] - 2025-08-10

### Bug Fixes

- 优化剪切板相关代码
- 修复番剧无法解析高清晰度问题
- 使用新版bv av互转算法
- 禁止SYSLIB5005警告
- 修复去水印页改变文本状态影响外部滚动容器 (#334)
- 修复部分接口未登录风控问题
- 修复下载过程中退出导致的数据错乱 #321
- 删除已下载视频时同步删除粗base表数据
- 优化cookie处理
- 修复返回引用类型导致的设置保存失败
- 规范路径拼接
- 修复下载中关闭软件导致的数据异常 Fixes: #321
- 优化设计器启动 (#320)
- 修复数据迁移失败 (#313)
- 将音频编码字段改为可空类型 (#311)
- 修复flv视频无法解析下载 (#306)
- 修复选择驱动器根目录闪退 #305
- 修复内建下载器暂停恢复的功能
- 修复修改aria2端口后无法正常通信问题
- 优化视频下载，避免二次解析 (#304)
- 修复切换选中合集会丢失选中高亮色 (#302)
- 调试无法启用DevTools
- 修复无法通过监听剪贴板进入用户空间
- 修复无后缀字幕文件重名可能导致复制失败的问题
- 修复代理设置项排列错误

### Documentation

- 加入issue模板 (#347)

### Features

- 增强崩溃日志记录 (#348)
- 加入生成视频元数据功能 (#322)
- 去水印工具加入水印定位 (#319)
- 加入启动自动更新检测 (#317)
- 支持保留窗口大小和位置 #312 #261
- 新增导航对鼠标回退键的支持

### Miscellaneous Tasks

- Ci发布增加windows trimmed并优化macos签名
- 将裁剪发布分离到独立发布配置文件

### Refactor

- 修改弹幕下载不再生成临时文件 (#329)
- 使用信号量控制下载器同时下载数 (#328)
- 减少配置文件io次数并去除加密 (#324)
- 重构数据库并优化下载列表相关逻辑
- 请求由WebRequest替换为HttpClient (#314)
- 去除inter字体包 (#308)

### Styling

- 代码清理

## [1.0.20] - 2025-05-08

### Bug Fixes

- 修复部分番剧解析异常

### Miscellaneous Tasks

- Macos打包添加公证
- 升级avalonia到最新版

## [1.0.19] - 2025-04-30

### Bug Fixes

- 修复日志无限膨胀 #273

### Refactor

- 代理设置重构，默认无代理,可选无代理、系统代理、自定义代理

## [1.0.18] - 2025-04-25

### Bug Fixes

- 修复编译问题和升级avalonia版本
- 优化datagrid宽度
- 优化asyncImageLoader内存占用
- 修复vs打开项目兼容性
- 修复字幕下载 Fixes: #254
- 修复收藏夹页面返回概率会出现的崩溃
- 修复我的订阅页面显示效果
- 修复个人空间-我的订阅 点击出现的崩溃
- 修复解析闪退 Fixes: #244
- 下载闪退 #244

### Features

- 视频解析页DataGrid新增排序功能
- 调整主窗口默认位置至屏幕中心
- 加入页面跳转
- 完善观看历史无限滚动功能
- 个人空间-历史记录 加入增量加载，支持无限滚动

### Miscellaneous Tasks

- 升级avalonia到最新版并替换Xaml.Behaviors包
- 升级avalonia到最新版并替换Xaml.Behaviors包

### Refactor

- 更改解析页DataGrid选中行高亮颜色
- 触发加载数据修改为异步命令
- 重新实现无限滚动功能

## [1.0.17] - 2025-03-19

### Bug Fixes

- 优化部分代码并修复hires音频问题 Fixes: #235
- 修复精简aria2后导致的参数问题
- 修复错别字 Fixes: #180

### Features

- 在视频解析页面中，新增支持视频av或bv号搜索
- 导航离开或页面刷新重置分割器的影响

### Miscellaneous Tasks

- 升级action版本

### Refactor

- 优化视频解析页页面布局，增强合集数较多时的体验
- 投稿视频页图片展示代码改进
- 改进视频投稿页面，先展示完信息后设置图片
- 移除macos aria2二进制文件，改为构建时下载专用版本并同步修改打包脚本

## [1.0.16] - 2024-11-29

### Bug Fixes

- 加入bvuid3和bvuid4可能解决无法解析的问题

## [1.0.15] - 2024-11-26

### Bug Fixes

- 修复macos闪退问题 #179

## [1.0.14] - 2024-11-25

### Bug Fixes

- 优化部分nullable代码
- 修复视频标题显示为合集标题
- 合集选中使用cid区分
- 修复合集选中

### Features

- 合集选中使用cid区分

### Refactor

- 改进GetVideoSections代码

## [1.0.13] - 2024-10-14

### Bug Fixes

- 修复个人空间加载完成前退出登录导致的崩溃
- 小修复登录二维码容易生成失败
- 修复在已下载中，只存在弹幕或字幕文件夹会打不开

### Features

- 加入版本检测

### Refactor

- 补全打开文件夹后缀

## [1.0.12] - 2024-10-05

### Bug Fixes

- 去掉字幕的自动生成后缀 Fixes: #144
- 修复windows无法打开相关网址的问题 Fixes: #147
- 修复选择下载路径时引发的另一个问题
- 修复选择下载位置时，列表未触发正确的更新
- 升级avalonia至11.1.3以使用DataGrid新的IsSelected binding

### Features

- 加入版本检测

### Refactor

- 版本检测修改
- 检查更新完善

## [1.0.11] - 2024-08-05

### Bug Fixes

- 修复视频发布时间错误问题Fixes: #103
- 使用webpage解析失败回退api方式
- 修复解析选中项失败的问题
- 加入使用webpage无法解析会回退到api解析解决webpage覆盖面的问题 Fixes: #115

### Features

- 升级Avalonia到11.1版本
- 替换内建下载器
- 加入已下载列表按下载时间倒序 Fixes: #114

## [1.0.10] - 2024-05-07

### Bug Fixes

- 修复多分类错误应用解析项问题 Fixes: #79
- 修复没有可以打开网址的程序导致的打开链接崩溃的问题 Fixes: #84
- 移除TextTrimming="CharacterEllipsis"导致的带有emoji的textblock崩溃 Fixes: #86
- 修复样式问题 Fixes: #80
- 修复下载时开启自动添加后缀文件夹不存在时闪退问题 Fixes: #78 #74

### Miscellaneous Tasks

- Update github workflows

## [1.0.9] - 2024-04-08

### Bug Fixes

- 优化暗黑模式表现并修复dialog暗黑模式异常问题
- 弹幕下载添加参数修复弹幕不完整 Fixes: #52

### Features

- 添加下载纯音频时使用mp3格式以修复各个播放器对aac的兼容问题 Fixes: #49 #56
- 基本支持暗黑模式(还有部分图标和dialog没有支持)
- 支持主题切换并修复部分问题
- 修复多次选择存在的重复下载问题

## [1.0.8] - 2024-03-05

### Bug Fixes

- 修复自动添加后缀重启无法正确显示设置状态 Fixes: #33

### Features

- 仅允许单实例同时运行
- 游客添加随机buvid3解决部分接口风控(真实buvid3暂未实现、后续可以考虑)
- 添加使用WebPage方式解析解决风控问题(仅限普通视频，番剧、课程未实现) #37 #36

## [1.0.7] - 2024-02-05

### Bug Fixes

- 使用avalonia旧版本修复windows7报错 Fixes: #31
- 修复剪切板中的收藏夹url无法获取到信息
- 修复收藏夹无法显示视频列表的问题
- 使用avalonia的nightly版本
- 文件命名av标签添加av的前缀使意义更加明确
- 修复特殊文件名导致闪退问题 Fixes: #18

### Features

- 添加一种收藏夹url的支持 Fixes: #30
- 添加文件已存在自动重命名的开关 Fixes: #27 Fixes: #21
- 加入linux arm64构建脚本

### Miscellaneous Tasks

- 去掉rpm arm64构建
- 去掉rpm arm64构建

## [1.0.6] - 2024-01-15

### Bug Fixes

- 修复windows上部分输入框样式不正确
- 修复异常出现的杜比音频
- 登录页面优化动画显示
- 首页未登录添加登录文字
- 修复输入错误参数导致ffmpeg异常而导致的软件闪退问题 Fixes #14
- 优化模态框示显示效果
- 去掉窗口边框以优化显示效果

### Features

- 修改ffmpeg为自编译版本。减少包大小

### Miscellaneous Tasks

- 将linux编译ci基础环境更换为20.04

### Revert

- "fix: 优化模态框示显示效果"

### Styling

- 修改部分代码样式

## [1.0.5] - 2024-01-01

### Bug Fixes

- 修复复制封面链接无效
- 暂时修复由于AvaloniaUI的bug导致的带有padding的scrollViewer 里面的textBlock裁剪不正确

### Features

- 优化工具箱滚动代码
- 优化边框ui
- 加入剪贴板监听支持(实验性)
- MacOS加入应用签名以修复应用程序已损坏 Fixes #12
- 加入自定义aria2支持

### Revert

- "feat: 优化边框ui"

## [1.0.4] - 2023-12-27

### Bug Fixes

- 修复全选无效的问题 Fixed: #11
- 修复debug模式报错
- 修复只下载音频无法混流 Fixes #10
- 修复工具箱错误

### Features

- 指定字体优化windows上的显示效果
- 添加ffmpeg日志记录

### Miscellaneous Tasks

- 更新readme
- 更新readme
- 更新readme
- Ffmpeg固定版本
- 修改readme
- 去掉每次提交运行ci

## [1.0.3] - 2023-12-24

### Bug Fixes

- 修复dialog字体问题
- 优化代码命名
- 修复关注列表首次加载空白 Fixes #8
- 修改图片生成质量

### Features

- 添加重复下载提醒的设置
- 添加弹幕屏蔽类型

## [1.0.2] - 2023-12-21

### Bug Fixes

- 修复基本设置和弹幕设置部分无法保存的问题 Fixes #5
- 优化dialog样式和修复dialog的相关bug
- 修复下载设置磁盘剩余空间和取消浏览的bug
- 修复历史记录显示bug
- 修复视频设置样式问题
- 修复无法保存编码、画质、音质 Fixes #3  [ci skip]

## [1.0.1] - 2023-12-20

### Bug Fixes

- 修复投稿页面样式问题
- 修复关注页面点击up不跳转
- 修复多分片不显示
- 命名修复
- 修复下载管理tips显示错误
- 修复下载全部无效的bug

### Features

- Linux支持打开文件和文件夹
- 修改macOS最低支持版本为10.15

### Miscellaneous Tasks

- 发布v1.0.1
- 添加更新日志

## [1.0.0] - 2023-12-19

### Bug Fixes

- Linux图标问题
- 修复发布在macOS和linux上的一些问题
- 修复下载列表不显示和提示框异步问题
- 修改发布配置
- 正确处理各个系统的文件路径并兼容8.0中断性变更
- Macos和linux正确获取相关目录。windows继续使用当前目录
- 正确获取临时文件路径
- 修复ui上的问题
- 修复下载列表删除无效
- 修复提示框异步问题
- 修复视频详情页滚动导致选项丢失
- 修复解析中滚动闪退
- 修复拼写错误

### Features

- 添加图标
- 修改icon
- 修改ci
- 修改ci
- 一些属性
- 修正命名
- 完善构建
- 添加windows获取aria2和ffmpeg脚本
- Macos打包脚本
- 添加统一的跨平台处理函数
- Windows加入图标
- Windows和macOS添加aria2支持
- 支持aria2
- 升级AvaloniaUI版本
- 修改referer和origin为移动端，目前测试是可以减少风控。未经详细测试
- 保存当前页面服务，减少new的开销和接口调用次数
- 更新wbi签名
- 支持文件名设置支持拖动
- Bump Avalonia version
- Update README.md
- 首次完成测试

### Miscellaneous Tasks

- Release v1.0.0
- 修复macOS构建
- 修复构建
- 修复构建
- 修复构建
- 修复构建
- 修复构建
- 去掉代理

<!-- generated by git-cliff -->
