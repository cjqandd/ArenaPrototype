# 美术资源管理方案

本方案适用于当前单人开发、先完成灰盒玩法、后续再购买或制作资源的工作方式。核心目标是：买来的原包不被改坏，正式资源可以逐步替换灰盒，战斗代码不依赖某个具体模型包。

## 1. 资源分区

| 区域 | 位置 | 用途 | 规则 |
| --- | --- | --- | --- |
| 待检查资源 | `ExternalAssets/` | 下载的压缩包、UnityPackage、许可证明和商品页截图 | 不直接投入游戏；先检查格式、依赖和许可 |
| 可编辑源文件 | `ArtSource/` | Blender、PSD、原始贴图、参考图等制作源文件 | 不放入 `Assets`，避免 Unity 每次导入大型源文件 |
| 第三方原包 | `Assets/ThirdParty/` | 已通过检查并导入的第三方资源 | 原样保留，原则上不重命名、不移动、不直接修改 |
| 项目正式美术 | `Assets/ArenaPrototype/Art/Production/` | 从原包选出并适配后的模型、材质、动画、贴图、特效和界面资源 | 只放真正会被本项目使用的版本 |
| 灰盒资源 | `Assets/ArenaPrototype/Art/GeneratedMaterials/` | 当前自动生成的占位材质 | 暂时保留原位置，避免场景生成工具失效 |
| 游戏预制体 | `Assets/ArenaPrototype/Prefabs/` | 角色、武器、场景、特效和界面最终预制体 | 玩法场景只引用这里的项目预制体，不直接引用商店原包 |

如果第三方包强制导入到自己的固定目录，不要为了统一目录而移动它。保留原目录，并在下方资源台账记录实际位置即可。

## 2. 建议目录

资源真正开始介入时，再按需创建以下目录，不提前制造大量空文件夹：

```text
Assets/ArenaPrototype/
  Art/
    Production/
      Characters/
        Player/
        Enemies/
      Weapons/
      Environment/
      Animation/
      Materials/
      Textures/
      VFX/
      UI/
        Fonts/
        Icons/
  Prefabs/
    Characters/
    Weapons/
    Environment/
    VFX/
    UI/
```

音频不混入美术目录，后续单独使用 `Assets/ArenaPrototype/Audio/`，分为音乐、环境、界面和战斗音效。

## 3. 资源进入项目的流程

1. 将下载文件放入 `ExternalAssets/Incoming/`，同时保留商品链接、订单或许可截图。
2. 先在一个空白 Unity URP 测试工程中导入，不直接污染主项目。
3. 检查 Unity 版本、URP 兼容性、模型格式、骨骼、动画、材质、脚本、插件和外部依赖。
4. 删除或停用不需要的演示场景、编辑器扩展和 DLL；发现来源不明的可执行代码时停止导入。
5. 在主项目中保留一份未修改的第三方原包。
6. 只把会实际使用的内容复制或制作成项目正式资源，并建立项目自己的预制体。
7. 在预制体上配置本项目的碰撞体、武器挂点、攻击特效点和玩法组件。
8. 重新生成/打开灰盒场景，分别测试外观、碰撞、动画、攻击范围和性能。

即使商品链接无法直接分析，也可以把下载后的文件、目录截图、说明文档或 Unity 导入结果交给 Codex 检查。购买前则至少提供商品页截图和格式说明。

## 4. 购买前检查清单

### 必须满足

- 许可允许商业游戏使用，并保留许可凭证。
- 提供 Unity 包或 FBX 等实际资源文件，不能只有渲染图。
- 支持 URP，或者至少提供标准 PBR 贴图，能够转换到 URP/Lit。
- 不强制依赖另一个付费插件、专有渲染管线或不明 DLL。
- 角色优先支持 Unity Humanoid；同一批角色尽量共用骨架体系。
- 动画能够作为独立片段使用，不被某个演示控制器锁死。
- 商品说明包含面数、贴图分辨率、动画列表和 Unity 版本。

### 本项目特别关注

- 玩家和敌人的攻击动作要有清楚的前摇、命中段和后摇。
- 角色必须能挂载长剑、盾牌和链刃，手部姿态不能完全写死在武器上。
- 武器模型的握柄枢轴要合理；最好是米制、Y 轴向上、正前方为 +Z。
- 竞技场环境优先选择模块化套件，墙、地面、入口和观众席可以拆分。
- 链刃资源若没有真实链条结构，只能用作静态武器外观，不能冒充完整链刃玩法资源。

第一次不要一次性买齐。优先用“一个角色 + 一组动画 + 一把武器”验证整个导入和替换流程，确认成功后再扩充同系列资源。

## 5. 购买优先级

1. 角色和战斗动画：风险最高，也最直接决定攻击前摇、闪避窗口和手感。
2. 武器：先长剑、盾牌，再确认链刃是否需要定制模型和动画。
3. 模块化中古奇幻竞技场：玩法尺寸稳定后再替换墙体、地面和看台。
4. 战斗 VFX 与 UI：在动作和画面风格确定后统一购买或制作。
5. 音效与音乐：最后按最终动作节奏和场景气氛匹配。

## 6. 命名规则

使用“类型_对象_变体”的方式，名称只描述资源身份，不写临时状态：

```text
CHR_Player_Gladiator_A
CHR_Enemy_Swordsman_A
CHR_Enemy_Archer_A
WPN_Sword_A
WPN_ChainBlade_A
ENV_Arena_Wall_A
PFB_Player_A
PFB_Enemy_Swordsman_A
MAT_Steel_A
T_Steel_BaseColor_A
AN_Player_Sword_Attack_01
VFX_Hit_Slash_A
UI_Icon_Boon_Ferocity
FONT_MainChinese
```

常用前缀：`CHR` 角色、`WPN` 武器、`ENV` 环境、`PFB` 预制体、`MAT` 材质、`T` 贴图、`AN` 动画、`VFX` 特效、`UI` 界面、`FONT` 字体。

## 7. 导入基线

- 模型先按 1 Unity 单位 = 1 米检查，导入缩放尽量保持 1。
- 角色默认关闭 Root Motion，由现有移动代码控制位移；正式动画试验后再决定是否局部启用。
- 初次导入不急于压缩网格，确认外观和动画无误后再优化。
- 普通颜色贴图使用 sRGB；法线贴图必须标记为 Normal Map。
- 角色和主要环境贴图先以最高 2048 为基线，小型道具和图标通常不超过 1024；最终按实机画面调整。
- 材质统一转为 URP/Lit 或项目自有 Shader，不长期依赖来源不明的自定义 Shader。
- 动画按动作切分，逐项检查循环、脚底滑动、朝向、武器穿模和事件时间点。
- `.meta` 文件和资源必须成对保留，绝不删除后重新生成 GUID。

## 8. 灰盒到正式美术的替换边界

第一套正式资源进入项目后，替换边界已经落地：

- `ArenaArtCatalog` 只保存“当前套装”和“后备套装”。
- `ArenaArtSet` 保存玩家、剑士、弓箭手、盾兵及各类装备槽。
- `EquipmentVisualProfile` 保存武器模型、大小、手持姿态、落地姿态和拾取碰撞范围。
- KayKit 与程序灰盒是同一接口下的两套实现；正式槽为空时可回退到灰盒。
- 移动、攻击、喝彩、掉落、箭矢和场次逻辑只读取统一视觉配置，不直接引用第三方原包。

非技术操作统一从 Unity 顶部菜单 `Arena Prototype > 美术资源 > 打开美术资源管理器` 进入。角色直接更换身份预制体；武器先从视觉预制体创建装备配置，再调整姿态并放入对应槽。最后点击“验证当前套装”和“重建场景并应用”。详细步骤见 `ART_REPLACEMENT_GUIDE.md`。

## 9. 资源台账

每次引入资源时在下表新增一行：

| 资源 | 来源/订单 | 许可凭证 | 版本与格式 | 原包位置 | 项目适配位置 | 状态 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| KayKit - Character Pack: Adventurers（FREE） | [itch.io 商品页](https://kaylousberg.itch.io/kaykit-adventurers)；免费版 | `ExternalAssets/Licenses/KayKit_Adventurers_2.0_FREE_LICENSE.txt`；CC0 1.0，可商用且无需署名 | Free 2.0；FBX 7.4、glTF/GLB、OBJ；12.42 MiB；SHA256 `ABE48F4763FBA0896BAB486EE9E6D08CA6B5B3884B9601F235C8847AE94DC479` | 压缩包：`ExternalAssets/Incoming/KayKit_Adventurers_2.0_FREE.zip`；已筛选 FBX 原包：`Assets/ThirdParty/KayKit/Adventurers_2.0_FREE/` | 材质：`Assets/ArenaPrototype/Art/Production/Materials/KayKitAdventurers/`；通用视觉预制体：`Assets/ArenaPrototype/Prefabs/Characters/`、`Assets/ArenaPrototype/Prefabs/Weapons/`；身份预制体：`Assets/ArenaPrototype/Prefabs/Characters/Roles/`；Art Set：`Assets/ArenaPrototype/Data/Art/Sets/`；装备配置：`Assets/ArenaPrototype/Data/Art/Profiles/Equipment/` | 已投入使用 | Unity 6000.3.21f1 + URP 17.3.0 验证通过。玩家（Barbarian）、剑士（Rogue）、弓箭手（Ranger）、盾兵（Knight）共 8 个角色实例已接入；基础待机、行走、奔跑已接入，Root Motion 关闭。KayKit 单手剑、弓、箭和圆盾已通过统一装备配置接入；场景验收为 6 剑、2 弓、2 搭弦箭、2 盾，运行无新增错误。链刃因原包没有对应资源继续使用灰盒。玩法数值和命中判定未改；战斗、受击、死亡动作仍待替换。`Rig_Medium_General.fbx` 导入时会触发 Unity FBX 曲线 `IsFinite` 断言，但导入后逐条检查未发现非有限时间/数值关键帧。 |

状态统一使用：`待评估`、`测试工程通过`、`已导入原包`、`适配中`、`已投入使用`、`已停用`。

## 10. 当前决定

- 当前灰盒资源和自动生成场景不移动。
- 当前正式套装使用 `ARTSET_KayKit_Adventurers`，后备套装使用 `ARTSET_Graybox`。
- 角色与武器后续统一通过美术资源管理器更换，不直接改战斗脚本。
- 界面临时使用 Windows 系统中文字体回退；正式发布前必须导入一款许可明确、包含所需中文字形的项目字体。
- 用户购买前可以先给商品信息；购买后优先检查本地文件，再决定是否导入主项目。
