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

# 任务 5

加大 压缩图片 CheckBox 左侧的 Margin。

将列标题“输出图路径”改为“输出路径”。

添加“删除所选项”按钮，放在“添加图片...”的右侧。如果在放大完成后按下这个按钮，则询问是否一并删除放大后的图片，默认为否。无论如何不删除原始图片。

将“中”栏的高度变为现在的2倍左右，下面的新行包含一个输出选项 ComboBox 和一个输出目录 TextBox。他们发生变更后，应当立即重新计算 ListView 中的所有输出路径。

ComboBox 有2个选项：

## 选项 1

“输出到原图所在目录”。选择此项时，TextBox 的 Label 注释文本变更为 “追加子目录”，最终的输出路径就是 原图所在目录/追加子目录/图片_后缀.拓展名。

## 选项 2

“输出到指定目录”，启用 TextBox。TextBox 的 Label 注释文本变更为 “输出目录”，最终的输出路径就是 输出目录/图片_后缀.拓展名。

若不压缩，拓展名统一变为 png，否则统一变为 jpg。

后缀暂定为 模型名。

# 任务 6

设计“首选项”窗口，文件名定为 PreferencesWindow。首选项保存到 ./preferences.json（相对可执行文件）。

主容器可滚动，内部选项由若干个 GroupBox 分类。下方则有 Apply, OK, Cancel 三个按钮。目前设计以下几个 Group：

## Group 1：外观

目前仅设置一个“主题”ComboBox，可保存选项，但具体功能待以后实现。

## Group 2：输出

目前仅设置一个“文件后缀选项”ComboBox以及紧邻的“后缀”TextBox。文件后缀选项有三个：

### 选项 1

“使用模型名”，TextBox禁用；

### 选项 2

“使用放大倍数”，TextBox禁用；

### 选项 3

“使用给定后缀”，TextBox 启用。

# 任务 7

编写暗色主题，命名为 Dark，整体配色参考 Dracula，主题色仍然选定为 Light 主题中的蓝色。

# 任务 8

开始实现核心的放大功能。

关于NCNN的调用方法，参考 ./EsrganTools/upscale.cmd。

完成放大的图片，其对应的项背景变为 #55FF55 (Light) #00DD00 (Dark)，前景变为 #000000。

在开始放大作业后，NCNN 进程应该由本程序掌握，在点击强制停止时 KILL NCNN 进程。

# 任务 9

通过调整 Margin 让“模型”的 ComboBox 左侧和输出选项的 ComboBox 左侧对齐；

缩短 追加子目录（或输出目录）的 TextBox 的长度，同时通过调整 Margin 让“追加子目录” Label 与 “压缩输出” 的 CheckBox 对齐。

在上栏和中栏之间加一个蓝色主题的 ProgressBar，值为 完成放大的图片数，最大值为要处理的总图片数。

# 任务 10

在首选项的“输出”组中，去掉原有的“文件后缀选项”ComboBox，为后缀 TextBox 添加如下几个字符串模板：

1. %MODEL%：模型名；

2. %SCALE%：放大倍数，使用一位小数；

3. %COMPRESS%：压缩质量，使用小数。如果没有被压缩则为空字符串。

同时，提供“去除文件名末尾的下划线”功能的 CheckBox。

提供“删除未压缩的输出图片”功能的 CheckBox。

# 任务 11

PS: 首先，我自己对窗体的一些文本进行了修改，请不要改回去。

将目前位于操作栏的 STATUS LABEL 和上面的 ProgressBar 一起放到窗口最下方的“状态”栏（同样使用卡片样式），共 2 行，第一行为 ProgressBar，第二行为 Label。