# 美术资源替换操作指南

这套管理方式的目标是：以后更换角色或武器时，只改美术配置，不碰移动、攻击、伤害和场次代码。

## 日常入口

在 Unity 顶部菜单点击：

`Arena Prototype > 美术资源 > 打开美术资源管理器`

窗口分为五块：美术套装、角色身份、武器与装备、创建装备配置、验证并应用。

## 更换角色

1. 先把新资源按项目规则做成“身份预制体”。角色预制体需要包含正确的 `Character Visual`、Animator 和左右手挂点。
2. 打开美术资源管理器。
3. 在“角色身份”中，把新预制体拖到玩家、剑士、弓箭手或盾兵槽。
4. 点击“验证当前套装”。
5. 没有红色错误后，点击“重建灰盒场景并应用”。
6. 点击 Play，检查大小、朝向、脚底高度、动画和手部挂点。

如果验证提示角色配置不完整，不要直接拖进场景；先修正身份预制体。

## 更换武器、弓、箭或盾牌

1. 把新模型做成项目自己的视觉预制体，放在 `Assets/ArenaPrototype/Prefabs/Weapons/`，不要直接改 `Assets/ThirdParty/` 原包。
2. 打开美术资源管理器。
3. 在“从新模型创建装备配置”中，把视觉预制体拖入“视觉预制体”。
4. 选择装备类型，然后点击“创建配置并放入当前槽”。
5. 点击对应装备右侧的“编辑姿态”。
6. 在 Inspector 中调整：
   - `Model Local Scale`：模型大小。
   - `Equipped Local Position / Euler`：拿在手上的位置和旋转。
   - `Grounded Local Position / Euler`：掉在地上的位置和旋转。
   - `Interaction Collider Center / Size`：地面拾取范围，不是攻击命中范围。
7. 点击“验证当前套装”，再点击“重建灰盒场景并应用”。
8. 点击 Play，至少检查手持、攻击摆动、掉落、拾取交换和遮挡。

攻击伤害、攻击距离和扇形命中范围仍由原有玩法配置控制，更换模型不会自动改变这些数值。

## 临时缺少某个模型

装备槽可以留空。当前套装找不到对应资源时，会使用灰盒后备外观。KayKit 没有链刃，因此链刃目前就是这样处理的；不影响其他角色和武器继续使用正式资源。

## 一键回退

如果新资源显示异常：

1. 打开美术资源管理器。
2. 点击“切回灰盒”。
3. 点击“重建灰盒场景并应用”。

确认玩法正常后，再切回 KayKit 或继续修正新资源配置。

注意：“重建灰盒场景并应用”会重新生成 `GrayboxArena`。直接在场景里手工摆放、但没有写入场景生成器的对象会被覆盖；当前阶段应优先修改预制体和配置资源。

## 每次替换后的最小检查

- Console 没有新的红色错误。
- 玩家武器在右手，弓与盾在左手，箭的朝向正确。
- 角色脚底没有明显悬空或陷地。
- 武器掉落后可被 E 键识别并交换。
- 更换模型前后的攻击范围和伤害没有意外变化。
- 运行时没有新的周期性卡顿。

## 当前配置位置

- 总目录：`Assets/ArenaPrototype/Data/Art/ArenaArtCatalog.asset`
- KayKit 套装：`Assets/ArenaPrototype/Data/Art/Sets/ARTSET_KayKit_Adventurers.asset`
- 灰盒套装：`Assets/ArenaPrototype/Data/Art/Sets/ARTSET_Graybox.asset`
- 武器姿态配置：`Assets/ArenaPrototype/Data/Art/Profiles/Equipment/`
- 自动验收报告：`ExternalAssets/Validation/Arena_ART_SET_MANAGEMENT_RESULT.txt`
