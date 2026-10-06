# 整理与核对记录

日期：2026-10-06。

- 完整复制 Assets、Packages、ProjectSettings、Docs。
- 逐文件 SHA-256 比对：仅 Assets/CesiumSettings/Resources/CesiumIonServers/ion.cesium.com.asset 因清除默认 Cesium 令牌及 ID 而不同，其余复制文件与源项目一致。
- Assets 文件和子目录均有对应 .meta；未发现遗漏。
- 扫描副本未发现非空的已检查 Cesium 访问令牌字段或 JWT 格式令牌；此扫描不是全面安全审计。
- 未发现超过 50 MiB 的文件，当前副本无需为大文件配置 Git LFS。
- 未复制 Library、Logs、UserSettings、.sln、.csproj、.vsconfig；未复制任何 Git 历史。
- 新增 README.md、.gitignore、.gitattributes、本说明和 FILE-MANIFEST.csv。
- 未修改源项目，未初始化仓库或上传 GitHub。
- 未运行 Unity，未验证 Mac 编辑器、Play Mode 或 macOS 构建。

FILE-MANIFEST.csv 覆盖本目录内除清单自身外的全部文件，包含相对路径、字节数及 SHA-256。原开发文档和历史验证结果被原样保留，不代表本次重新完成了那些运行验证。
