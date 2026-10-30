# 验证记录

## 历史本地 0.3

2026-10-29，本地完整开发构建通过 46 项 Unity 场景集成检查，覆盖主线、反例、存档、三语、连续动画、七项发现和角色回应。历史报告保存在 [`Documentation/Validation/local-v0.3-baseline.json`](../Documentation/Validation/local-v0.3-baseline.json)。

最终 Windows 程序另行核对了英文标题、简中与繁中切换、新游戏进入、暂停、保存确认和正常退出，未发现日志异常。完整三段挑战与所有七项发现没有在该最终 exe 中逐项人工重走；其他设备、教师评审和目标学生试用尚未完成。

## 公开源码版

公开版保留游戏逻辑，排除可选授权模型和私人开发记录。集成检查中的装饰断言调整为：只有安装了 School assets 时才要求相应物件存在。其余教学逻辑与检查保持一致。

2026-10-30，公开版在 Unity 2022.3.46f1c1 的图形批处理模式下重新完成 **46 项场景集成检查**，errors 为空；独立规则测试 **15 项断言通过**。报告见 [`public-v0.3-integration.json`](../Documentation/Validation/public-v0.3-integration.json) 和 [`publication-checks.json`](../Documentation/Validation/publication-checks.json)。公开版截图与回应 GIF 也从此次实际运行重新生成。

## 自动检查的范围

- `SimulationChecks`：15 项纯 C# 断言，包括规则边界、噪声场景、能耗单位、全开/全关反例、确定性和异常输入。
- `Tools/check_publication.py`：检查公开目录不含受限制模型、机器路径、许可证文件或常见凭据。
- GitHub Actions 只执行以上两类检查，不进行 Unity 渲染或完整 Windows 试玩。

测试通过不等于美术完成、学习效果得到验证或具备正式发行质量。

## Windows 公开试玩包

公开源码版 Windows 构建成功，压缩包 CRC 与 SHA-256 已检查。该包的完整人工试玩没有在此次上传中重走；Unity 场景集成和历史本地 Windows 人工检查的范围分别见上文。

由于 GitHub Release 附件接口从此电脑传输大文件超时，校验过的 Windows ZIP 直接存放在仓库 downloads/ 目录；下载后用同目录 SHA256SUMS.txt 校验。
