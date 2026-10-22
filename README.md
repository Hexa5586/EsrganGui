# ESRGAN GUI v0.1

> **Fully created by DeepSeek V4.1 Flash with OpenCode**

一个面向 Windows 的 Real-ESRGAN 批量图片放大 GUI，使用 WPF / .NET 10 编写，封装了 `realesrgan-ncnn-vulkan` 与 `ffmpeg`。

仓库地址：<https://github.com/Hexa5586/EsrganGui>

## 功能

- 批量添加图片，支持文件选择和拖拽导入。
- 支持多选、Ctrl 多选和鼠标框选。
- 从 `realesrgan/models` 自动读取模型，支持 x2、x3、x4 放大。
- 可选使用 ffmpeg 将结果压缩为 JPG，质量范围 1–100。
- 输出到原图目录或指定目录，支持追加子目录。
- 可自定义文件名后缀模板：
  - `%MODEL%`：模型名
  - `%SCALE%`：放大倍数，一位小数
  - `%COMPRESS%`：压缩质量的小数表示；未压缩时为空
- 可选去除文件名末尾的下划线。
- 可选在压缩完成后删除未压缩的 PNG。
- 批量任务顺序执行，显示进度和状态；完成后对应行变绿。
- “停止执行”会终止当前 Real-ESRGAN / ffmpeg 进程树。
- 路径单元格悬浮显示缩略预览，双击打开文件所在位置。
- 浅色、深色和跟随系统三种主题。
- 首选项自动保存到可执行文件同级的 `preferences.json`。

## 环境要求

- Windows 10 / 11 x64
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)（构建时需要）
- 支持 Vulkan 的显卡与驱动
- Git LFS（克隆仓库时需要）

运行和发布不需要单独安装 ffmpeg，仓库和发布目录中已经包含所需二进制文件。

## 克隆仓库

`EsrganTools` 中的可执行文件、运行库和模型通过 **Git LFS** 管理。请先安装 Git LFS，否则克隆下来的会是 LFS 指针文件：

```powershell
git lfs install
git clone https://github.com/Hexa5586/EsrganGui.git
cd EsrganGui
git lfs pull
```

如果已经克隆但工具文件只有一百多字节，可以执行：

```powershell
git lfs checkout
```

## 构建

```powershell
dotnet build .\EsrganGui.slnx -v q
```

调试运行：

```powershell
dotnet run --project .\EsrganGui\EsrganGui.csproj
```

构建输出位于：

```text
EsrganGui\bin\Debug\net10.0-windows\
```

构建过程会自动把工具复制到输出目录：

```text
EsrganGui.exe
realesrgan\
  realesrgan-ncnn-vulkan.exe
  vcomp140.dll
  models\...
ffmpeg\
  ffmpeg.exe
  LICENSE.txt
```

## 发布单文件版本

Release 配置已经启用单文件、自包含和 ReadyToRun；架构通过 `-r` 指定：

```powershell
dotnet publish .\EsrganGui\EsrganGui.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -o .\publish\win-x64
```

发布目录大致如下：

```text
publish\win-x64\
  EsrganGui.exe          # 单文件主程序（x64）
  EsrganGui.pdb
  LICENSE.txt
  THIRD-PARTY-NOTICES.md
  realesrgan\            # 引擎、运行库和模型
  ffmpeg\                # ffmpeg
```

`EsrganGui.exe` 已包含 .NET / WPF 运行时，但 `realesrgan/` 和 `ffmpeg/` 需要与它放在同一目录，因为程序会以独立进程方式调用它们。

## 使用说明

1. 点击“添加项...”选择图片，或直接把图片拖入列表。
2. 在“模型”中选择放大模型。
3. 如需压缩，勾选“压缩输出”并设置质量。
4. 选择输出位置和子目录/输出目录。
5. 点击“开始执行”开始批量处理；处理期间可以点击“停止执行”中止。
6. 双击原图或输出路径单元格，可在系统文件管理器中打开所在位置。

支持的输入格式：PNG、JPG/JPEG、BMP、WebP、TIF/TIFF。

### 输出命名

默认后缀模板是 `%MODEL%`，输出文件名为：

```text
原文件名_模型名.扩展名
```

例如：

```text
photo_realesrgan-x4plus.png
```

如果后缀模板为 `%MODEL%_%SCALE%`，结果类似：

```text
photo_realesrgan-x4plus_4.0.png
```

启用压缩时输出扩展名为 `.jpg`，否则为 `.png`。程序不会覆盖原图；极端同名情况下会追加 `_out1` 等后缀。

### 首选项

首选项保存在可执行文件同级的 `preferences.json`，包括：

- 主题：浅色 / 深色 / 跟随系统
- 后缀模板
- 去除文件名末尾的下划线
- 压缩后删除未压缩的 PNG

## 项目结构

```text
EsrganGui.slnx
EsrganGui\
  App.xaml
  MainWindow.xaml
  MainWindow.xaml.cs
  PreferencesWindow.xaml
  PreferencesWindow.xaml.cs
  Themes\
  Models\
  Assets\app.ico
EsrganTools\              # 第三方工具与模型（Git LFS）
  realesrgan-ncnn-vulkan.exe
  vcomp140.dll
  ffmpeg\
  models\
TASK.md
THIRD-PARTY-NOTICES.md
LICENSE
```

## 第三方组件

本项目自身代码采用 **MIT** 许可证，详见 [LICENSE](LICENSE)。

随项目分发或调用的第三方组件包括：

- **Real-ESRGAN / realesrgan-ncnn-vulkan**：BSD 3-Clause
- **ncnn**：BSD 3-Clause（包含在 Real-ESRGAN 可执行文件中）
- **ffmpeg**：GPLv3，独立进程调用
- **vcomp140.dll**：Microsoft Visual C++ 运行库

完整版权和许可证文本见 [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md)。再次分发本项目时，请同时遵守第三方组件的许可证要求。

## 说明

- 当前 GUI 面向 Windows；`EsrganTools` 中只保留 Windows 版引擎和工具。
- `EsrganTools` 的大文件使用 Git LFS 存储，普通 Git 历史中只保留指针。
- `publish/`、`bin/`、`obj/` 和运行时的 `preferences.json` 不纳入 Git 跟踪。

## Credits

**Fully created by DeepSeek V4.1 Flash with OpenCode**
