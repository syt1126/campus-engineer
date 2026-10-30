# 素材与公开仓库边界

## 包含的内容

| 内容 | 来源与说明 |
| --- | --- |
| 校园生成、教学逻辑、互动和声音合成 | 本项目代码生成 |
| 小浣熊/小熊猫连续造型与网格 | 本项目生成，附网格生成脚本与哈希 |
| 校园插画、桌面图标 | 为项目生成的 AI 原创插画，未复制参考游戏美术 |
| 地板、墙面、木桌贴图 | Poly Haven，CC0；见资源清单 |
| 系统字体 | 运行时访问本机字体，没有复制商业字体文件 |

详细路径与 SHA-256：[`Documentation/ASSET_SOURCES.json`](../Documentation/ASSET_SOURCES.json)。Poly Haven 许可：https://polyhaven.com/license 。

## 未包含的原始素材

- Unity Asset Store 的 **School assets**，作者 A.R.S|T.。官方页面：https://assetstore.unity.com/packages/3d/environments/school-assets-146253 。原始资源未公开上传，本地开发构建的截图可能显示其装饰道具。
- 已退役的 VRoid 人物、Rocketbox 人物与历史导入数据，均不属于当前公开运行资源。
- 编辑器缓存、用户设置、机器日志、许可证文件、私人交接历史、试玩存档及开发机路径。

### 可选恢复 School assets

自行从官方页面获取并遵守 Unity Asset Store EULA：https://unity.com/legal/as-terms 。本项目用 `Resources.Load<GameObject>("SchoolAssets/Prefabs/props/<name>")` 加载道具。若需要恢复本地装饰效果，应在私人工作副本将相应包结构放到 `Assets/Campus/Resources/SchoolAssets/`；不要将原始包提交进公开仓库。

未安装时，游戏会跳过这些装饰。学习主线、原创教室、小浣熊与课间互动仍可运行。

## 使用范围

公开可见不等于全部素材获得通用开源许可。原创部分采用 [LICENSE.md](../LICENSE.md) 所述展示范围，第三方许可独立适用。截图、图标与角色生成资料也应保留来源，不应改标为其他作者作品。
