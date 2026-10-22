# EsrganTools

本目录存放 EsrganGui 构建和发布时随附的第三方工具与模型。

| 路径 | 说明 |
| --- | --- |
| `realesrgan-ncnn-vulkan.exe` | Real-ESRGAN 的 Windows Vulkan 引擎 |
| `vcomp140.dll` | Visual C++ 运行库 |
| `models/*.bin`、`models/*.param` | Real-ESRGAN 模型 |
| `ffmpeg/ffmpeg.exe` | ffmpeg 的 Windows 静态构建 |
| `ffmpeg/LICENSE.txt` | ffmpeg 许可证文本 |

上述可执行文件、运行库和模型通过 Git LFS 管理。构建时 `EsrganGui.csproj` 会把引擎、模型和 ffmpeg 复制到输出目录的 `realesrgan/`、`ffmpeg/` 下。

第三方组件的来源和许可证见根目录的 [THIRD-PARTY-NOTICES.md](../THIRD-PARTY-NOTICES.md)。
