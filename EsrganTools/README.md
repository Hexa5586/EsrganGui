# tools — 图片放大（Linux 原生版）

这是 `upscale.cmd` / `upscale.ps1` 的 Linux 原生等价实现，
**不依赖** `dim2clear.exe` / `dim2clear_cmd.exe`、PowerShell 或 wine。

## 文件

| 文件 | 说明 |
| --- | --- |
| `realesrgan-ncnn-vulkan` | Real-ESRGAN 的 Linux(Vulkan) 引擎，原生 ELF 可执行文件 |
| `upscale.sh` | 放大脚本，功能与 `upscale.ps1` 一致 |
| `realesrgan-ncnn-vulkan.exe` / `upscale.cmd` / `upscale.ps1` / `vcomp140.dll` | 原有 Windows 工具，保持不变 |

模型与 ffmpeg 不再重复打包：

* 模型优先取 `tools/models/`，没有则取上级的 `../models/`；
* 压缩改用系统 `ffmpeg`（可用环境变量 `FFMPEG` 指定其它路径）。

## 用法

```bash
./tools/upscale.sh <img_path> [选项]
```

`<img_path>` 可以是单张图片，也可以是一个图片文件夹。

| 选项 | 说明 |
| --- | --- |
| `-h`, `--help` | 显示帮助 |
| `-Type <model>` | 模型类型，默认 `anix4` |
| `-OutDir <dir>` | 输出目录（默认：文件夹输入 → `<folder>/out_x<scale>`；单文件 → 原目录） |
| `-Compress` | 用 ffmpeg 重新编码（PNG → JPG） |
| `-Quality <n>` | JPEG 质量 1..100，配合 `-Compress`（默认 80） |
| `-Format <fmt>` | `png` \| `jpg` \| `webp`（默认 `png`） |
| `-DelOld` | 成功后删除原图 |
| `-TileSize <n>` | 分块大小，0 = 自动（默认 0） |
| `-Gpu <id>` | Vulkan 显卡编号（默认 0；用引擎的 `-v` 可列出可用编号） |

选项名不区分大小写，`-Type` 与 `--type`、`-t` 等价，也支持 `-Type=x4` 这类写法。

模型类型：

| `-Type` | 模型 | 倍率 |
| --- | --- | --- |
| `anividx2` | realesr-animevideov3 | x2 |
| `anividx3` | realesr-animevideov3 | x3 |
| `anividx4` | realesr-animevideov3 | x4 |
| `x4` | realesrgan-x4plus | x4 |
| `anix4` | realesrgan-x4plus-anime | x4 |

## 示例

```bash
# 单张图，动漫模型 x4
./tools/upscale.sh ./pics/a.jpg

# 文件夹整体处理，2 倍，输出 webp，并删除原图
./tools/upscale.sh ./pics -Type anividx2 -Format webp -DelOld

# 处理后压缩为 JPG，质量 70
./tools/upscale.sh ./pics/a.png -Compress -Quality 70
```

## 依赖

* 支持 Vulkan 的显卡 + 驱动（本机 Intel Iris Xe 走 GPU 0）；
* 可选：系统 `ffmpeg`（仅 `-Compress` 时需要）。
