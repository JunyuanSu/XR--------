# Port of Dover — GitHub / Mac 项目副本

整理日期：2026-10-06。此目录是可独立导入 Unity 的源项目快照，请把本目录里的内容作为 GitHub 仓库根目录。

## 版本与内容

- Unity Editor：2022.3.55f1（9f374180d209）。
- Cesium for Unity：1.24.0；依赖版本保存在 Packages/manifest.json 和 packages-lock.json。
- 主场景：Assets/Scenes/PortOfDover.unity。
- Assets：场景、脚本、材质、车辆预制体、配置及全部 .meta。
- Packages：依赖声明、锁文件和 Cesium 包源配置。
- ProjectSettings：Unity 项目配置。
- Docs：已有开发说明、验证记录及场景历史备份。它们记录不同开发阶段，未必描述当前全部功能。
- .gitignore / .gitattributes：缓存排除与跨系统文本规则。

Library、Logs、UserSettings、IDE 项目文件及构建产物不在副本中；Unity 会重新生成缓存和 IDE 文件。未更改原项目。本副本没有 Git 历史，也尚未上传 GitHub。

## 在 Mac 上打开

1. 安装 Unity Hub，在 Hub 中安装 **2022.3.55f1**；选择适合 Mac 芯片的编辑器版本。不要首次打开就升级项目版本。
2. 克隆 GitHub 仓库到 Mac 的本地工作目录；在 Hub 添加包含 Assets、Packages、ProjectSettings 的目录。
3. 联网打开并等待 Unity 下载依赖、导入资源。Cesium 包源已配置为 https://unity.pkg.cesium.com 。不需要从 Windows 复制 Library。
4. 在 Unity 的 Cesium 窗口中登录自己的 Cesium ion 账号并配置项目默认访问令牌。副本已清空 ion.cesium.com.asset 中原默认令牌及令牌 ID。地形、影像、建筑依赖在线服务与相应资产访问权限，仓库不包含离线地图。
5. 打开 Assets/Scenes/PortOfDover.unity，点击 Play，检查 Console 和地图、车辆显示。
6. 如需生成 Mac 应用，在 File → Build Settings 选择 PC, Mac & Linux Standalone，Target Platform 选择 macOS，并根据设备选择架构；将主场景 Add Open Scenes 加入构建列表，再构建。若缺少构建模块，通过 Hub 添加对应模块。当前保存的构建场景列表为空。

此整理仅完成文件与配置检查，未在 Mac 上实际启动、编译或构建。Cesium 官方支持 Intel 和 Apple Silicon Mac；特定系统与此固定版本的运行结果仍以本机验证为准。

## 首次上传 GitHub

在 GitHub 创建一个空仓库，不预先添加 README、.gitignore 或 License。将下面的 OWNER/REPOSITORY 替换为自己的仓库地址。打开终端，先进入**这个副本目录**，再执行（需要安装 Git 并完成 GitHub 身份认证）：

```sh
git init -b main
git add .
git diff --cached --stat
git commit -m "Backup Port of Dover Unity project 2026-10-06"
git remote add origin https://github.com/OWNER/REPOSITORY.git
git push -u origin main
```

提交前检查暂存差异，确认没有个人令牌。上传整个目录内的文件，包括隐藏的 .gitignore / .gitattributes 和所有 .meta，不要只上传脚本或压缩包。

## 后续同步与备份

本目录是一次性快照，不会自动跟随原目录更新。建立仓库后，Windows 和 Mac 都应在各自克隆的同一个仓库中继续开发；不要同时维护两个互不关联的项目副本。

开始工作前退出 Unity 后执行 git pull；修改后保存场景并关闭 Unity，检查 git diff，再 git add、git commit 和 git push。只在本机保存或 commit 不会更新 GitHub 备份。

Unity 配置 Cesium 后可能把令牌重新写入 Assets/CesiumSettings/Resources/CesiumIonServers/ion.cesium.com.asset 或场景文件；每次提交前清空待提交文件中的个人令牌及令牌 ID。不要直接忽略整个 CesiumSettings 目录，否则可能丢失场景所需的资产引用。

## 官方参考

- [Cesium 快速开始](https://cesium.com/learn/unity/unity-quickstart/)
- [Cesium 支持的平台](https://cesium.com/learn/cesium-unity/ref-doc/supported-platforms.html)
- [GitHub：上传本地项目](https://docs.github.com/en/migrations/importing-source-code/using-the-command-line-to-import-source-code/adding-locally-hosted-code-to-github)

文件核对结果见 PREPARATION-NOTES.md；FILE-MANIFEST.csv 记录整理时各文件的 SHA-256，后续开发修改文件后该清单不再代表新版本。
