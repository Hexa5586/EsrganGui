# 第三方组件与许可证

本项目自身代码采用 **MIT** 许可证，详见根目录的 [LICENSE](LICENSE)。

以下第三方组件随项目一同分发，各自遵循其原有许可证。它们与本项目代码相互独立，不影响本项目代码的 MIT 授权。

## Real-ESRGAN / realesrgan-ncnn-vulkan

- 组件：`EsrganTools/realesrgan-ncnn-vulkan.exe`（Windows）、`EsrganTools/models/*`
- 许可证：BSD 3-Clause
- 版权：Copyright (c) 2021, Xintao Wang
- 项目主页：<https://github.com/xinntao/Real-ESRGAN>

```text
BSD 3-Clause License

Copyright (c) 2021, Xintao Wang
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

## ncnn

- 组件：包含在 realesrgan-ncnn-vulkan 可执行文件中
- 许可证：BSD 3-Clause
- 版权：Copyright (c) 2017 Tencent
- 项目主页：<https://github.com/Tencent/ncnn>

## ffmpeg

- 组件：`EsrganTools/ffmpeg/ffmpeg.exe`（gyan.dev 7.0.2 essentials 静态构建）
- 许可证：GNU General Public License v3.0，完整文本见 `EsrganTools/ffmpeg/LICENSE.txt`
- 构建来源：<https://www.gyan.dev/ffmpeg/builds/>，源码：<https://ffmpeg.org/download.html>

说明：ffmpeg 以独立可执行程序的形式随附，本项目仅通过命令行调用它，不与其链接。根据 GPL 的“聚合”（aggregate）条款，本项目自身代码仍可采用 MIT 许可证；ffmpeg 本身继续受 GPLv3 约束。如果再次分发本项目（尤其是对外发布），请同时遵守 GPLv3 对 ffmpeg 的要求，包括提供其对应源代码或有效的源代码获取途径。

## vcomp140.dll

- 组件：`EsrganTools/vcomp140.dll`
- 来源：Microsoft Visual C++ Redistributable
- 按 Microsoft 的可再分发条款随附，不属于本项目 MIT 授权的范围。
