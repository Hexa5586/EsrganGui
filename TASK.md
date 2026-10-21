# 背景叙述

你是一个精通 C# 的软件工程师，你要使用 WPF 框架编写一个封装 ESRGAN 模型以及 NCRR_ESRGAN_VULKAN.EXE 命令行程序的 GUI 界面。

# 任务 1

编辑 MainWindow.xaml，页面分为上、中、下三个部分。标题为 ESRGAN GUI by inori_koishi

## 上

一个 ListView 或类似的列表容器，标题栏可点击，可拖动；共有 4 列，分别对应序号、原图路径、输出图路径。

### 序号

十进制整数序号，从 1 开始递增；

### 原图路径、输出图路径

悬浮在该格时通过类似 Tooltips 的方式显示低画质预览，点击时打开文件所在位置并选中（这可能不是这一步要完成的）

## 中

配置栏，主要包括模型选择 ComboBox、是否进行压缩的 Checkbox 以及压缩质量的 TextBox。

## 下

操作栏，包括开始执行、强制停止、关于三个按钮。

# 任务 2

让 ListView 支持多选，支持鼠标的矩形选框选择，支持按住 Ctrl 点击来多选。

在“下”栏添加“添加图片...”按钮。

# 任务 3

将“关于”改为“首选项...”，删除 MessageBox 事件，具体行为待定。

移除 ListView 中的测试项，尝试从 E:\Images\Collections 中选择一些项 通过“添加图片...”按钮加入列表。

让 ListView 支持来自 Explorer 的拖拽，这个功能由我来测试。

# 任务 4

我将 ESRGAN 组件放在了 ./EsrganTools 中。请你更改构建设置，将 realesrgan-ncnn-vulkan.exe，vcomp140.dll 和 models/ 组合为 realesrgan 目录，然后获取一个 ffmpeg，将它们放进构建目录（realesrgan/ 和 ffmpeg/ 目录与 EsrganGui.exe 同级）。

去除 ComboBox 中的测试项，让其从 ./realesrgan/models（相对可执行文件） 中读取所有可用模型。不显示拓展名。