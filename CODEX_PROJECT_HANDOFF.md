# 3D RPG — Codex 新对话交接摘要

更新时间：2026-08-15（Asia/Shanghai）

## 新对话第一句话

建议在新对话中直接发送：

> 我的 Unity 项目位于 `D:\Unity\Project\3D RPG`。请先完整阅读项目根目录的 `CODEX_PROJECT_HANDOFF.md`，然后重新检测 Unity MCP、Editor 状态和 Console。请保留现有用户修改，只处理我当前明确提出的问题。

## 协作约定

- 使用中文沟通。
- 项目根目录：`D:\Unity\Project\3D RPG`。
- 以后 Unity 项目统一放在 `D:\Unity\Project` 下。
- 用户希望能由 MCP 完成的检查和操作尽量直接完成，但修改前先确认真实场景、Prefab、Inspector 和 Console 状态。
- 解释/诊断请求默认只检查并给最小修复，不自动大改代码；明确要求“修复、创建、搭建”时才实施。
- 不要罗列大量猜测；先找最直接、经过项目证据支持的原因。
- 不要修改无关代码，不要重置或覆盖用户现有工作。
- 每次脚本修改后：等待编译/Domain Reload 完成，检查 Console，再保存和验证。
- 工作树很脏，包含大量用户修改和新增资产；禁止 `git reset --hard`、`git checkout --` 或批量清理。

## 技术基线（已从磁盘核对）

- Unity：`2022.3.62f3`。
- 平台：Windows Standalone。
- 渲染管线：URP `14.0.12`。
- 脚本后端：Mono。
- 输入：旧版 `UnityEngine.Input`，没有采用新 Input System。
- AI Navigation：`1.1.7`，使用 `NavMeshSurface` 组件工作流。
- Cinemachine：`2.10.7`。
- ProBuilder：`5.2.4`。
- Polybrush：`1.1.8`。
- ProGrids：`3.0.3-preview.6`。
- Unity MCP 包来自 `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`。

## Build Settings（已从 `EditorBuildSettings.asset` 核对）

1. 索引 0：`Assets/Scenes/SampleScene.unity`，启用。
2. 索引 1：`Assets/Scenes/DungeonRoom.unity`，启用。

## 场景状态

### SampleScene

- 主学习场景，约 24 MB，包含 50×50 森林/草原地形、树木、岩石、怪物、玩家、传送门和 NavMesh。
- 用户正跟随 B 站 M_Studio 教程 `BV1ez4y1U7Qx`。
- 场景文件有用户修改，不能覆盖。

### DungeonRoom

- 文件：`Assets/Scenes/DungeonRoom.unity`。
- 使用 `Assets/Assets Pack/LowPolyDungeonsLite` 搭建。
- 结构：地板、三面墙、正面开放、顶部不封顶。
- 四张桌子，每张有黄色蜡烛火焰粒子与暖色点光。
- 中央篝火和墙面火把使用黄白—金黄色火焰，不使用明显红色火焰。
- 玩家出生点上方保留不可见的冷白向下聚光；可见灯具、发光球和灯罩已删除。
- 已创建 `PlayerSpawn` 和 `PortalArrivalPoint` 定位对象。
- 阻挡型家具参与碰撞/NavMesh；书、瓶子、蜡烛等小装饰不切割 NavMesh。
- 最后一次已知 Bake 验证：NavMesh 顶点数 256；新对话应重新验证。
- LowPolyDungeonsLite 的 3 个原 Standard 材质已转换为 URP/Lit。
- 生成的粒子/灯光材质位于 `Assets/Materials/DungeonGenerated`。

## Portal Shader 状态

- 材质：`Assets/Materials/GreenPortal.mat`。
- Shader Graph：`Assets/Materials/Shader Graph/Portal Shader.shadergraph`。
- 用户明确要求继续使用原 Shader Graph，不能替换成新手写 Shader。
- 曾经临时创建的 `PortalFixed.shader` 已删除。
- 原 Shader Graph 已将 `Sample Texture 2D` 的 A 通道接入 Alpha。
- 已开启 Alpha Clipping，并补充 `Alpha Clip Threshold ≈ 0.08`，用于清除 Quad 外围低透明度方框。
- 近距离相机验证曾显示圆形传送门无方框；Scene 视图选中时的橙色 Quad 轮廓只是编辑器选择框。

## 鼠标点击系统

文件：`Assets/Scripts/Managers/MouseManager.cs`。

### 已应用

- 玩家位于 `Player` Layer（第 7 层）。
- `MouseManager` 当前已设置：

```csharp
mouseRaycastMask = ~LayerMask.GetMask("Player");
```

- Raycast 当前使用该 Mask，因此鼠标射线会穿过玩家 Collider，能够点击被玩家模型遮挡的敌人。

### 注意

- 不要删除玩家 CapsuleCollider。
- 若修改 Raycast，继续保持敌人、Ground、Portal、Attackable 的点击行为。

## 玩家攻击与 Golem 石头

关键文件：

- `Assets/Scripts/Characters/PlayerController.cs`
- `Assets/Scripts/Characters/Enemy/Gloem.cs`
- `Assets/Scripts/Characters/Enemy/Rock.cs`
- `Assets/Prefab/Weapon/Rock.prefab`

实际核对过的数值：

- Rock 使用动态 Rigidbody + Convex MeshCollider。
- Rock Renderer 大小约 `2.39 × 1.99 × 1.81`。
- 玩家 `attackRange = 2`。
- 玩家 NavMeshAgent radius 约 `0.35`。

### 未修复问题

`PlayerController.MoveToAttackTarget()` 仍然使用：

```csharp
Vector3.Distance(attackTarget.transform.position, transform.position)
agent.destination = attackTarget.transform.position;
```

这会让玩家尝试走向石头 Collider 内部的中心点，石头斜着落地时可能在前面绕圈、无法进入攻击条件。

### 已给出但尚未实施的最小修复

- 使用 `Collider.ClosestPoint(transform.position)` 计算玩家到目标表面的距离和接近点。
- 距离判断时将目标点 Y 设置成玩家 Y，忽略动态石头的高度差。
- 循环中持续更新目标点和朝向。
- 目标被销毁时 `yield break`。
- 不建议优先缩小 Rock Collider，因为会影响投射物撞击玩家、Boss 和墙体的判定。

新对话若用户要求实施，只修改这段接近逻辑并完成编译、Console 和运行验证。

## 传送系统

关键文件：

- `Assets/Scripts/Transition/SceneController.cs`
- `Assets/Scripts/Transition/TransitionPoint.cs`
- `Assets/Scripts/Transition/TransitionDestination.cs`

### 已应用

- `SceneController.Transition()` 当前已经通过：

```csharp
player = GameManager.Instance.playerStats.gameObject;
```

取得玩家，而不是错误地移动 GameManager。
- 传送前临时禁用 NavMeshAgent，设置位置/旋转后重新启用。

### 仍未完成

- `DifferentScene` 分支当前仍为空，没有调用 `LoadSceneAsync`。
- 当前协程中的 `yield return null` 位于移动之后，不等于“等待场景加载完成”。
- 跨场景时需要让执行协程的 SceneController 和玩家在加载期间保持存活，或采用 `sceneLoaded` 回调架构。
- `DungeonRoom/PortalArrivalPoint` 最初只是定位对象；新对话应检查它现在是否已挂 `TransitionDestination`，不能依赖旧推断。
- SampleScene 中曾核对到两个 Portal 均为 `SameScene`、sceneName 为空；当前 Inspector 必须重新检查。

## 战斗/动画已知原则

- 玩家和敌人的伤害由动画事件触发，事件应放在武器实际命中帧，不能仅按协程到达立即扣血。
- 攻击 Clip 不应无限 Loop；返回 Locomotion 应使用一次性 Exit Time。
- 玩家在 Hit/Dizzy 状态时应阻止攻击动画事件继续造成伤害。
- 敌人被销毁后应及时清空 attackTarget/停止攻击协程。
- 以前的相关代码已经多次被用户手动改动，处理新问题前必须读取当前 Animator、Animation Event 和脚本，不能只依赖本摘要。

## NavMesh 与碰撞约定

- Unity 2022 使用 `NavMeshSurface`，不是旧 Navigation Bake 页签。
- 树木、石头、木桩、墙、桌椅、箱子等大型阻挡物应影响 NavMesh。
- 花、灌木、小装饰可让鼠标射线穿过，但不应因为 Ignore Raycast Layer 自动变成 NavMesh 障碍。
- Raycast Layer 与 NavMesh 几何收集是两套独立逻辑。
- 动态目标接近应考虑 `pathPending`、`pathStatus`、`remainingDistance`、`stoppingDistance` 和目标 Collider 表面。

## Git/文件安全

- 项目有 Git，但当前工作树包含大量 Modified 和 Untracked 文件。
- 这些修改和新资产属于用户；不得执行破坏性回滚或清理。
- 包括但不限于：Animator、角色数据、脚本、SampleScene、NavMesh、LowPolyDungeonsLite、Golem/Grunt/TurtleShell、Portal、DungeonRoom、Shader Graph 和截图。
- 若要提交 Git，先只读核对 diff 和目标范围，再由用户明确授权。

## 性能诊断结果

用户曾出现整机和鼠标周期性卡顿。只读采样结果：

- 物理内存约 16 GB，空闲一度只有 1.5–1.9 GB。
- Windows Memory Compression 约 1.5 GB。
- 页面换页峰值约 847 pages/sec。
- Codex/ChatGPT 多进程合计超过 1 GB，并有明显 CPU 占用；关闭 Codex 后用户确认恢复流畅。
- `wallpaper64` 动态壁纸曾占用约 12–23% CPU。
- 页面文件被手动设得异常巨大：C 约 117 GB、D 约 293 GB；AutomaticManagedPagefile=False。
- C 盘一度只剩约 22.7 GB，D 盘只剩约 10.1 GB。
- GPU RTX 4060 Laptop 温度约 51°C、显存约 2.45/8 GB；未发现近期 Display/Disk/WHEA 严重错误。

建议：使用新对话、退出动态壁纸、减少并行应用、把页面文件恢复为系统自动管理并重启、为 C/D 盘释放空间。不要直接关闭页面文件。

## 当前验证边界

- 生成本交接时，Unity MCP `http://localhost:8080/mcp` 握手失败，因此最后一次实时 Editor/Console 状态没有取得。
- 磁盘上的 Unity 版本、Packages、Build Settings、场景和脚本已重新读取。
- 新对话第一步必须重新检测 Unity MCP；连接成功后读取 Editor state、当前场景和 Console，再继续工作。
- 不要把 Editor.log 中历史错误直接当成当前错误；应以重新连接后的 Console 为准。

## 新对话推荐检查顺序

1. 检测 Unity MCP 实例与 Editor ready 状态。
2. 读取当前活动场景和 Console Error/Warning。
3. 确认用户当前问题属于场景、传送、战斗、AI、Shader 还是性能。
4. 只读取该问题相关的脚本、Prefab、Inspector 和 Animator。
5. 给出最小修复；用户明确要求实施时再修改。
6. 修改后等待编译/Domain Reload，检查 Console，保存并做运行或截图验证。
