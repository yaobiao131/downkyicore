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

