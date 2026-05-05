# QuanLib.Minecraft.ResourcePack → QuanLib.Minecraft.Resource 重构报告

## 概述

`QuanLib.Minecraft.Resource` 是对 `QuanLib.Minecraft.ResourcePack` 的**完全重构版本**。从源文件规模上看，由 21 个文件扩展至 56 个文件；从架构范式上看，这是一次从脚本式静态代码到企业级可测试架构的根本转变。

---

## 1. 架构范式变更

### 1.1 静态类 → 服务接口

**旧（ResourcePack）：** 所有功能通过静态类暴露，调用方与实现强耦合。

```csharp
// 无法替换、无法 mock、无法测试
ResourcePackReader.Load(ZipPack[]);
BlockTextureReader.Load(ResourceEntryManager);
LanguageReader.Load(ResourceEntryManager, "zh_cn");
```

**新（Resource）：** 每个职责抽象为接口，实现类通过构造函数注入依赖。

```csharp
IResourceEntryLoader.LoadResourceEntry(ZipPack[]);
IBlockStateLoader.LoadBlockStatesAsync(entries);
IBlockModelLoader.LoadBlockModelsAsync(entries, blockStates);
IBlockTextureLoader.LoadBlockTexturesAsync(entries, models, texturePool);
ILanguageTemplateParser.ParseLanguageTemplates(languageEntries);
```

### 1.2 并发模型：Parallel.ForEach → 顺序 async/await

| 维度     | ResourcePack                                              | Resource                                                             |
| -------- | --------------------------------------------------------- | -------------------------------------------------------------------- |
| 执行模型 | `Parallel.ForEach` + `ConcurrentDictionary`           | 顺序 `foreach` + `await`                                         |
| ZIP 读取 | `lock` 包裹解压+解析，I/O 在锁内导致串行化              | 顺序 `await`，每次只处理一个文件，无锁争用                         |
| 忙等同步 | `while (count < total) Thread.Sleep(10)` 脆弱的完成检测 | 无忙等——`await` 本身即完成信号                                   |
| 错误处理 | `AggregateException` 收集异常，可能部分失败难定位       | per-entry catch +`ILogger` 警告 + `continue`，单个失败不影响整体 |

**说明：** Resource 并未采用 `Task.WhenAll` 或 `SemaphoreSlim` 并发调度，所有 Loader 实现均为顺序 `foreach` + `await`。这虽然牺牲了一定的并行吞吐能力，但换来了更简单的错误处理模型和确定性的执行顺序。ResourcePack 的 `Parallel.ForEach` 虽然名义上并行，但在 `BlockTextureReader` 中 ZIP 流读取被 `lock` 完全串行化，实际并行收益有限；而 `LanguageReader` 的 `Parallel.ForEach` 配合 `Thread.Sleep` 忙等，在异常场景下会永久挂死。Resource 选择顺序异步 I/O 是正确性优先于吞吐量的取舍。

### 1.3 服务目录结构

```
Services/
├── IResourceEntryLoader.cs
├── IBlockStateParser.cs
├── IBlockStateLoader.cs
├── IBlockModelLoader.cs
├── IBlockTextureLoader.cs
├── IItemMappingParser.cs
├── IItemMappingLoader.cs
├── IItemModelLoader.cs
├── IItemTextureLoader.cs
├── ILanguageFormatConverter.cs
├── ILanguageTemplateParser.cs
└── Implementations/
    ├── ResourceEntryLoader.cs
    ├── BlockStateParser.cs
    ├── BlockStateLoader.cs
    ├── BlockModelLoader.cs
    ├── BlockTextureLoader.cs
    ├── ItemMappingParser.cs
    ├── ItemMappingLoader.cs
    ├── ItemModelLoader.cs
    ├── ItemTextureLoader.cs
    ├── LanguageFormatConverter.cs
    └── LanguageTemplateParser.cs
```

所有加载/解析能力接口化，共 **11 个接口 + 11 个实现**，天然支持依赖注入和单元测试。

---

## 2. 新增功能

### 2.1 完整的物品纹理管线

ResourcePack 只处理方块纹理。Resource 新增了完整的物品支持：

```
ItemMappingParser / ItemMappingLoader  →  解析 items/<id>.json 的 model 引用
    ↓
ItemModelLoader                       →  递归加载生成式物品模型 (minecraft:item/generated)
    ↓
ItemTextureLoader                     →  加载层纹理，多层 Porter-Duff Alpha 合成
    ↓
ItemTextureManager（单例）            →  IReadOnlyDictionary<string, IGeneratedItemTexture>
```

- 支持最多 6 层纹理 (Layer0 ~ Layer5)
- 多层纹理使用 **Porter-Duff Alpha 合成**合并为单张图像
- `GeneratedItemTexture` 以 `"layer0+layer1+..."` 的复合键在 `TexturePool` 中缓存

### 2.2 ItemMapping 解析

新增 `IItemMappingParser` 解析 Minecraft 的 `items/<id>.json` 文件。识别 `"minecraft:model"` 类型的条目并提取 `model.model` 引用路径，从而建立物品 ID → 模型 ID 的映射关系。

### 2.3 图像只读保护

新增 `IReadOnlyImage<TPixel>` 泛型接口和 `ReadOnlyImage<TPixel>` 代理类，完整镜像 ImageSharp `Image<TPixel>` 的读取 API。底层可变图像由 `Texture` 持有，外部代码只能通过只读接口访问，防止误修改。

### 2.4 TexturePool（纹理去重池）

新增 `TexturePool` 类，作为 `IReadOnlyDictionary<string, Texture>` 集中管理所有已加载纹理。同一纹理路径只加载一次，各 `CubeBlockTexture` / `GeneratedItemTexture` 通过引用共享，减少内存占用。

### 2.5 缺失纹理（Missing Texture）

新增 `Texture.CreateMissingTexture()` 生成标准紫色/黑色棋盘格纹理，在所有纹理加载失败时作为降级备用。避免 NRE，同时通过日志发出警告。

---

## 3. 类型体系重构

### 3.1 模型系统的引入

**ResourcePack 的模型解析方式：** 通过 `TextureMap` 策略类直接从 JSON 提取纹理键名 → 加载图像 → 构造 `BlockTexture`。**模型本身不作为独立概念存在。**

**Resource 引入完整的模型层次：**

```
IObjectModel
├── ModelId, Parent, Textures 字典
├── ICubeBlockModel（方块）
│   ├── Down, Up, North, East, South, West（6 面纹理 ID）
│   ├── IsCubeAll
│   └── 实现：CubeBlockModel, RotatedCubeBlockModel
└── IGeneratedItemModel（物品）
    ├── Layer0 ~ Layer5
    └── 实现：GeneratedItemModel
```

关键机制：

- **递归解析：** `BlockModelLoader` / `ItemModelLoader` 递归遍历 `parent` 链，维护 `source` 和 `exclude` 字典处理循环引用与重复解析
- **纹理引用解析：** `ObjectModelExtensions.GetTexture` 递归解析 `#texture` 引用链
- **旋转装饰器：** `RotatedCubeBlockModel` 包装 `ICubeBlockModel`，通过预计算的 `BlockRotationMapping` 重映射 6 个面的纹理，无需修改基础模型

### 3.2 BlockModelLoader 加载机制详解

`BlockModelLoader` 是方块模型加载的核心，负责判断一个模型是否为合法的立方体方块（CubeBlock），并递归解析其父模型链。其判定机制由两个独立的方法组成：

#### 3.2.1 几何判定：IsCubeElements

```csharp
private bool IsCubeElements(JArray? elementsJArray, [MaybeNullWhen(false)] out BlockModelElement element)
```

从模型 JSON 的 `elements` 数组判断该模型在几何上是否为一个完整的立方体，同时将解析出的元素数据结构化为 `BlockModelElement` 输出。判定条件：

1. `elements` 非 null
2. `elements` 数组恰好包含 **1 个**元素（多元素模型为非标准方块）
3. 将元素 JSON 反序列化为 `BlockModelElement.JsonObject`，再通过 `BlockModelElement.CreateFromJson` 构造强类型对象（异常则记录日志并返回 false）
4. 该元素的 `from` 坐标为 `[0, 0, 0]`
5. 该元素的 `to` 坐标为 `[16, 16, 16]`
6. 六个面（down/up/north/south/west/east）必须全部存在
7. 每个面的 UV 必须为完整 UV（`IsFullUV`），不能有自定义 UV 裁剪

只有满足全部条件的才是标准的 CubeBlock——即一个从原点 (0,0,0) 到对角顶点 (16,16,16) 的完整方块。任何偏离（多元素、非满尺寸、缺面、自定义 UV）的模型均被排除。

与旧版本的关键区别：该方法不再只是返回 bool 的"门卫"，而是通过 `out` 参数将解析出的 `BlockModelElement` 传递给 `CubeBlockModel` 构造函数。Element 成为模型的持久数据，驱动下游的 `FindTexture`（纹理引用解析）和 `GetTextureRotation`（纹理旋转计算）。

#### 3.2.2 纹理完整性判定：IsCubeTextures

使用 **6-bit 位掩码**验证方块模型沿父子链能解析出全部 6 个面的纹理。两个方法重载：

```csharp
// 公开入口：从模型提取 Element，null 则直接返回 false
private static bool IsCubeTextures(ICubeBlockModel model)
{
    BlockModelElement? element = model.Element;
    if (element is null)
        return false;
    return IsCubeTextures(model, element, AllFacesMask);
}

// 私有递归：使用 element 的面数据查找纹理
private static bool IsCubeTextures(IObjectModel model, BlockModelElement element, int needsMask)
```

位掩码定义：

```csharp
private const int AllFacesMask = (1 << 6) - 1;  // 0b111111

private static readonly Dictionary<string, int> FaceMaskMap = new()
{
    ["down"]  = 1 << 0,  // bit 0
    ["up"]    = 1 << 1,  // bit 1
    ["north"] = 1 << 2,  // bit 2
    ["south"] = 1 << 3,  // bit 3
    ["west"]  = 1 << 4,  // bit 4
    ["east"]  = 1 << 5,  // bit 5
};
```

算法流程：

1. 初始 `needsMask = 0b111111`（所有 6 位均置 1，表示所有面都缺失）
2. 遍历 `element.Faces.AllFaces` 中的每个面（element 在整个递归链中保持同一引用——都是最底层模型解析出来的那个）
3. 取该面的 `Texture` 值（去除 `#` 前缀，如 `"#all"` → `"all"`），检查 `model.Textures` 是否包含该纹理键
4. 若包含且面名在 `FaceMaskMap` 中，将对应 bit 清零：`needsMask &= ~faceMask`
5. 若 `needsMask == 0`（所有面已找到），立即返回 `true`
6. 若仍有面缺失，沿 `model.Parent` 链向上递归查找

与旧版本的关键区别：纹理查找不再遍历 `model.Textures` 的键名，而是遍历 `element.Faces` 的面数据。这使得面名与纹理变量名解耦——例如 `"north"` 面引用 `"#all"` 纹理，只需模型定义了 `"all"` 纹理键即可匹配，不再要求纹理变量名恰好等于面名。这种设计也使得父子模型共享同一套 `BlockModelElement` 面定义来验证纹理完整性。

#### 3.2.3 递归加载流程：LoadModelAsync

```
LoadModelAsync(modelId, entries, source, exclude)
│
├── 1. 补全命名空间（AssetHelper.GetFullName）
├── 2. 检查 source 缓存 → 命中则直接返回
├── 3. 检查 exclude 集合 → 在列表中则返回 null（已排除）
├── 4. 从 entries 查找对应 JSON 文件 → 未找到则返回 null
├── 5. 异步读取并解析 JSON（ReadAllTextAsync + JObject.Parse）
│      └── 异常 → 记日志，加入 exclude，返回 null
│
├── 6. 提取 parent 字段和 textures 对象
│
├── 7. 若 parent 非空且非 "minecraft:block/block"：
│      ├── 验证 parent 路径在 block/ 下 → 否则排除
│      ├── 递归调用 LoadModelAsync 加载父模型
│      ├── 解析当前模型的 textures 字典
│      └── 构造 CubeBlockModel(modelId, textures, element: null, parentModel)
│
└── 8. 若 parent 为空或为根模型 "minecraft:block/block"：
       ├── 调用 IsCubeElements 检查 elements 字段，解析出 BlockModelElement
       │    ├── 通过 → 构造 CubeBlockModel(modelId, textures, element, parent: null)（此为根模型）
       │    └── 未通过 → 加入 exclude，返回 null
```

整个递归过程通过 `source`（已成功加载的模型）和 `exclude`（已确认不可用的模型）两个集合防止循环引用和重复加载。`source` 在成功构造模型对象后立即添加，确保同一模型在递归链中被多次引用时直接返回缓存结果。

#### 3.2.4 加载后的后处理

`LoadBlockModelsAsync` 在 `LoadModelAsync` 返回后执行两项检查：

```csharp
// 验证纹理完整性（父模型链级别）
if (!IsCubeTextures(model))
    continue;

// 应用方块旋转
if (!blockRotation.IsZero)
    model = new RotatedCubeBlockModel(model, BlockRotationMapping.CreateMapping(blockRotation));
```

即使单个模型的 `elements` 合格，也必须确保沿整个父模型链能解析出全部 6 个面的纹理；否则该方块被跳过。然后对于带有旋转的 block state（如 `facing=east` 的楼梯），用 `RotatedCubeBlockModel` 装饰器包裹基础模型。

### 3.3 BlockState 解析分离

**ResourcePack：** `TextureMap` 的 `CubeColumnTextureMap` / `OrientableTextureMap` 子类在其构造函数内解析 block state 字符串（如 `axis=x`、`facing=south`），纹理映射逻辑与状态解析耦合。

**Resource：** 彻底分离关注点：

1. `IBlockStateParser` → `BlockStateModel[]`（含 `BlockRotation`）
2. `IBlockStateLoader` → `Dictionary<string, BlockStateModel>`（复合键 `"blockid[state=value]"`）
3. `RotatedCubeBlockModel` 装饰器独立处理旋转
4. `BlockRotationMapping.CreateMapping` 和 `RotationMapping` 处理面向旋转数学与纹理旋转

新增值类型 `BlockRotation`（`record struct`）简洁替代原有的离散 X/Y 旋转值。

#### 3.3.1 旋转映射与纹理预旋转

最初的 `RotatedCubeBlockModel` 通过 `BlockRotateHelper.RotateFacing` 在**每次获取纹理时动态计算**朝向映射，且无法处理纹理本身的旋转（如方块旋转后面上纹理方向错误）。本次重构将其替换为 **"预计算映射 + 预旋转纹理"** 两步方案：

**第一步：预计算 RotationMapping**

```
BlockRotation(X=0/90/180/270, Y=0/90/180/270)
    → BlockRotationMapping.CreateMapping()
        → 对 X 轴逐次调用 RotateFromXaxis()
        → 对 Y 轴逐次调用 RotateFromYaxis()
            → 每个面的 RotationMapping { SourceFacing, TargetFacing, TextureRotation }
```

`RotationMapping` 记录"源朝向映射到哪个目标朝向 + 纹理需要旋转多少度"的完整信息。`BlockRotationMapping` 管理全部 6 个面的映射，通过 `RotateFromXaxis` / `RotateFromYaxis` 模拟方块绕轴旋转时各面的重新分配和纹理角度变化。

**第二步：纹理预旋转**

`BlockTextureLoader` 和 `CubeBlockTexture` 统一通过 `ObjectModelExtensions.GetTextureRotation(facing)` 扩展方法计算纹理旋转角度。该方法内部综合了 `BlockRotationMapping` 的旋转映射和 `BlockModelElement` 的面级旋转，对外暴露统一接口。若旋转角度为 90°/180°/270°，则克隆原始纹理图像并调用 ImageSharp 的 `Rotate` 进行旋转，以 `"<textureId>[rotation=N]"` 键名存入 `TexturePool`。

这种设计将旋转计算集中在模型创建阶段，纹理旋转集中在加载阶段，查询时仅做字典查找——消除了运行时动态计算的性能开销。与早期版本的区别在于，调用方不再需要手动检查 `IRotatedCubeBlockModel` 类型，`GetTextureRotation` 扩展方法内部已统一处理所有旋转来源。

新增类型：
- `IRotatedCubeBlockModel`：扩展 `ICubeBlockModel`，暴露 `BlockRotationMapping` 属性
- `RotationMapping`：值类型，记录 {SourceFacing, TargetFacing, TextureRotation}
- `BlockRotationMapping`：值类型，管理 6 个面的 `RotationMapping`，提供 `CreateMapping` 工厂方法和旋转操作
- 删除 `BlockRotateHelper`（其功能已被上述类型取代）

### 3.4 TextureMap 策略模式 → 递归模型解析

**ResourcePack 的 TextureMap 层次结构：**

```
TextureMap（抽象基类）
├── BlockTextureMap     → minecraft:block/block
├── CubeTextureMap      → minecraft:block/cube
├── CubeAllTextureMap   → minecraft:block/cube_all
├── CubeColumnTextureMap → minecraft:block/cube_column（axis 感知）
├── CubeBottomTopTextureMap → minecraft:block/cube_bottom_top
└── OrientableTextureMap → minecraft:block/orientable（facing 感知）
```

每新增一种 Minecraft 模型父类型就需要新子类。`CubeColumnTextureMap` 和 `OrientableTextureMap` 还包含了 block state 解析逻辑，违反了单一职责原则。

**Resource 完全抛弃此模式：**

- `BlockModelLoader` 递归解析模型 JSON 的 `parent` 链
- 纹理键名直接从模型 `textures` 字典提取，通过 `ObjectModelExtensions.GetTexture` 递归解析
- Block state 解析由 `IBlockStateParser` 独立处理
- 旋转由 `RotatedCubeBlockModel` 装饰器统一处理
- 消除了 `BlockType` 枚举，以 `ICubeBlockModel.IsCubeAll` 布尔属性替代

### 3.5 完整类型对照表

| ResourcePack 类型        | Resource 对应物                                      | 变化性质                   |
| ------------------------ | ---------------------------------------------------- | -------------------------- |
| `ResourcePath`         | `ResourcePath`                                     | 保留                       |
| `ResourceEntry`        | `ResourceEntry` + `AssetFileEntry`               | 重构，6 个具体属性替代字典 |
| `ResourceEntryManager` | `ResourceEntryManager` + `IResourceEntryLoader`  | 拆分                       |
| `Texture`              | `Texture` + `IReadOnlyImage` + `ReadOnlyImage` | 增强                       |
| —                       | `TexturePool`                                      | **新增**             |
| `BlockType` 枚举       | `ICubeBlockModel.IsCubeAll`                        | 消除枚举                   |
| `BlockTexture`         | `ICubeBlockTexture` + `CubeBlockTexture`         | 接口化                     |
| `TextureMap` + 6 子类  | `IObjectModel` + 递归解析                          | 完全重写                   |
| `TextureInfo` DTO      | 消除，嵌入模型对象                                   | 简化                       |
| `BlockTextureReader`   | 4 个接口 + 4 个实现                                  | 拆分                       |
| `BlockTextureManager`  | `BlockTextureManager`                              | 增强                       |
| —                       | `RotatedCubeBlockModel`                            | **新增（装饰器）**   |
| —                       | `BlockRotation`                                      | **新增（值类型）**   |
| —                       | `BlockStateModel`                                  | **新增（record）**   |
| —                       | `BlockRotateHelper`                                | **已删除**           |
| —                       | `RotationMapping`                                  | **新增（值类型）**   |
| —                       | `BlockRotationMapping`                             | **新增（值类型）**   |
| —                       | `IRotatedCubeBlockModel`                           | **新增（接口）**     |
| —                       | `BlockFace`                                        | **新增（record）**   |
| —                       | `BlockFaces`                                       | **新增**             |
| —                       | `BlockModelElement`                                | **新增**             |
| —                       | `UV`                                               | **新增（record struct）** |
| `TextTemplate`         | `LanguageTemplate` + `ILanguageFormatConverter`  | record 化                  |
| `LanguageReader`       | `ILanguageTemplateParser`                          | 接口化                     |
| `LanguageManager`      | `LanguageManager`                                  | 增强                       |
| —                       | `ItemTextureManager`                               | **新增**             |
| —                       | `IGeneratedItemModel` / `GeneratedItemModel`     | **新增**             |
| —                       | `IGeneratedItemTexture` / `GeneratedItemTexture` | **新增**             |
| —                       | `AssetFileEntry`                                   | **新增**             |
| —                       | 4 个 Extensions 类                                   | **新增**（`BlockRotateHelper` 已删除，余 3 个）|

---

## 4. 语言系统重构

### 4.1 模板类型：可变类 → 不可变 record

```csharp
// 旧：可变类，字段可被修改
public class TextTemplate {
    public string PatternText { get; }
    public string FormatText { get; }
    private int[] _orders;
}

// 新：不可变 record class
public record class LanguageTemplate {
    public string JavaFormat { get; init; }
    public string CSharpFormat { get; init; }
    public string RegexPattern { get; init; }
    public IReadOnlyList<int> InterpolatedOrder { get; init; }
}
```

### 4.2 格式化逻辑：内嵌解析 → 源生成器服务

**ResourcePack：** `TextTemplate.TryParse` 用动态正则 `Regex.Replace` 处理占位符，然后反向用正则 `\(\[\\s\\S\]\+\)` 匹配自己生成的模式文本进行二次替换——耦合脆弱。

**Resource：** `ILanguageFormatConverter` 的实现使用 `[GeneratedRegex]` 源生成器，编译时生成高性能正则代码。三个方法分离关注点：

| 方法                      | 职责                                   |
| ------------------------- | -------------------------------------- |
| `ConvertToCSharpFormat` | `%s` / `%1$s` → `{0}` / `{1}` |
| `ConvertToRegexPattern` | 格式字符串 → 匹配正则                 |
| `GetInterpolatedOrder`  | 提取显式参数序号                       |

### 4.3 占位符匹配：贪婪 → 非贪婪

```csharp
// ResourcePack
@"([\s\S]+)"   // 贪婪捕获，可能跨越多个占位符

// Resource
@"([\s\S]+?)"  // 非贪婪捕获，精确匹配单个模板参数
```

### 4.4 多语言 fallback 链

**ResourcePack：** 简单的两步合并（指定语言覆盖 `en_us`），元素据模式引发 NRE。

**Resource：** 三级 fallback 链：

1. 在指定语言中查找
2. 在 `en_us` 基础语言中查找
3. 自动解析 `MissingKey%` 格式的降级模板

缺失 key 被缓存到 `ConcurrentDictionary<string, LanguageTemplate>`，通过日志警告记录。

`LanguageManager` 内部以 `Dictionary<string, Dictionary<string, LanguageTemplate>>` 存储多语言数据（外层 key 为语言代码如 `"zh_cn"`），`ILanguageTemplateParser.ParseLanguageTemplates` 每次处理单一语言的 `IReadOnlyDictionary<string, string>` 条目集合，由调用方组织多次调用来构建多语言字典。

### 4.5 方法组织：实例方法 → 扩展方法

**ResourcePack：** `TryFormat` / `TryMatch` 作为 `TextTemplate` 的实例方法。

**Resource：** 使用 C# `extension class` 语法将 `TryFormat` / `TryMatch` 组织为 `LanguageTemplate` 的扩展方法，并新增 `ParameterCount` 属性。核心数据结构与操作行为解耦。

---

## 5. 依赖变更

| 包                                             | ResourcePack | Resource | 说明           |
| ---------------------------------------------- | ------------ | -------- | -------------- |
| `Newtonsoft.Json`                            | 13.0.4       | 13.0.4   | 不变           |
| `SixLabors.ImageSharp`                       | 3.1.11       | 3.1.11   | 不变           |
| `Microsoft.Extensions.Logging.Abstractions`  | —           | 10.0.7   | **新增** |
| `System.Text.RegularExpressions`（源生成器） | —           | 隐式     | **新增** |
| `QuanLib.Minecraft`                          | ✓           | ✓       | 不变           |
| 目标框架                                       | net10.0      | net10.0  | 不变           |

`ILogger<T>` 的引入使每个服务实现都具有结构化日志能力，警告和错误对被调用方可追踪。

---

## 6. 已修复的问题

| 严重度         | 问题                                                                                     | 旧位置                                              | 修复方式                                                                                            |
| -------------- | ---------------------------------------------------------------------------------------- | --------------------------------------------------- | --------------------------------------------------------------------------------------------------- |
| **致命** | `while (count < total) Thread.Sleep(10)` 忙等——`Parallel.ForEach` 异常导致永久挂死 | `LanguageReader.Load`                             | 完全移除 `Parallel.ForEach` 和忙等，改为顺序 `foreach` + `await`，异常通过 try-catch 即时处理 |
| **高**   | `goto err` 非惯用错误处理模式                                                          | `ResourceEntryManager.TryGetModel/TryGetTexture`  | 标准 try-catch + early return                                                                       |
| **高**   | `lock` 内执行 ZIP 解压 I/O，串行化所有线程                                             | `BlockTextureReader.GetBlockStates`               | 改为顺序 `foreach` + `await`，每次只处理一个文件，无需锁                                        |
| **中**   | 正则反向匹配自身生成的模式文本，耦合脆弱                                                 | `TextTemplate.TryParse`                           | `[GeneratedRegex]` 源生成器                                                                       |
| **中**   | Block state 解析耦合在 TextureMap 子类构造函数中                                         | `CubeColumnTextureMap` / `OrientableTextureMap` | 分离为 `IBlockStateParser` + `RotatedCubeBlockModel`                                            |
| **中**   | TextureMap 策略模式每个父类型需要一个子类，扩展成本高                                    | `TextureMap` + 6 子类                             | 废弃策略模式，改用递归模型解析 +`IsCubeElements` 几何判定                                         |
| **低**   | 整型除法损失精度                                                                         | `Texture.GetAverageColor`                         | 保留（实际影响可忽略）                                                                              |

---

## 7. 仍存在的改进空间

| 严重度       | 问题                                                          | 建议                                                           |
| ------------ | ------------------------------------------------------------- | -------------------------------------------------------------- |
| **中** | 无 DI 注册扩展                                                | 添加 `AddMinecraftResource(IServiceCollection)` 组合根方法   |
| **中** | `TexturePool.Textures` 为 `internal` 可变字典             | 改为 `private readonly`，仅通过 `IReadOnlyDictionary` 暴露 |
| **低** | 缺失纹理静默降级可能掩盖真实资产缺失                          | 考虑配置开关：strict mode 抛异常 / lenient mode 降级           |
| **低** | `LanguageManager` 的 `_missingLanguageTemplates` 永不清理 | 考虑使用 `MemoryCache` 限制大小或过期策略                    |
| **低** | `ResourceEntryLoader` 是同步的而其他 Loader 全异步          | 统一为异步                                                     |
| **低** | 公共 API 无 XML 文档注释                                      | 为接口和公共方法添加文档                                       |
| **低** | 无单元测试项目                                                | 接口化已为测试铺平道路，可开始添加                             |

---

## 8. 设计模式总结

| 模式       | 旧项目使用                                   | 新项目使用                                                                     |
| ---------- | -------------------------------------------- | ------------------------------------------------------------------------------ |
| 单例模式   | `BlockTextureManager`, `LanguageManager` | 保留 + 新增 `ItemTextureManager`                                             |
| 策略模式   | `TextureMap` 层次结构（6 子类）            | 废弃，改用递归模型解析                                                         |
| 服务接口   | 无                                           | 11 个接口 + 11 个实现                                                          |
| 装饰器模式 | 无                                           | `RotatedCubeBlockModel`（内部由 `BlockRotationMapping` 驱动）              |
| 代理模式   | 无                                           | `ReadOnlyImage`                                                              |
| 工厂模式   | `ISingletonFactory`                        | 保留                                                                           |
| 扩展方法   | 无                                           | 3 个 Extensions 类                                                             |
| 记录类型   | 无                                           | `BlockStateModel`, `BlockRotation`, `AssetFileEntry`, `LanguageTemplate` |

---

## 9. 数据流对比

### ResourcePack 数据流

```
ZipPack[]
  → ResourcePackReader.Load（同步）
    → ResourceEntryManager（持有 ZipArchiveEntry 引用）
      → BlockTextureReader.Load（同步）
        → Parallel.ForEach 解析 blockstate JSON
        → TextureMap 策略模式解析纹理键
        → 加载 ImageSharp 图像
        → BlockTextureManager.Instance（单例）
      → LanguageReader.Load（同步）
        → Parallel.ForEach 解析语言 JSON
        → TextTemplate.Parse
        → LanguageManager.Instance（单例）
```

### Resource 数据流

```
ZipPack[]
  → IResourceEntryLoader.LoadResourceEntry（同步）
    → ResourceEntryManager（持有 ResourceEntry[] + 语言原始条目）

调用方分别启动独立管道：

方块管线（由调用方编排）：
  IBlockStateParser.ParseBlockState（JSON → BlockStateModel[]，同步）
  → IBlockStateLoader.LoadBlockStatesAsync（foreach await ReadAllTextAsync）
  → IBlockModelLoader.LoadBlockModelsAsync（foreach await 递归解析）
    ├── IsCubeElements（elements.from=[0,0,0] to=[16,16,16] 判定几何完整性）
    └── IsCubeTextures（6-bit 位掩码验证纹理覆盖全部 6 面）
  → IBlockTextureLoader.LoadBlockTexturesAsync（foreach await Image.LoadAsync）
    └── TexturePool 纹理去重
  → BlockTextureManager.Instance（单例）

物品管线（由调用方编排）：
  IItemMappingParser.ParseItemMapping（JSON → model ID，同步）
  → IItemMappingLoader.LoadItemMappingsAsync（foreach await ReadAllTextAsync）
  → IItemModelLoader.LoadItemModelsAsync（foreach await 递归解析）
  → IItemTextureLoader.LoadItemTexturesAsync（foreach await，多层 Porter-Duff 合成）
    └── TexturePool 纹理去重
  → ItemTextureManager.Instance（单例）

语言管线（由调用方编排）：
  ResourceEntryLoader 从 ZIP 提取 Dictionary<string, Dictionary<string, string>>
    （外层 key = 语言代码如 "en_us"/"zh_cn"，内层 key = 翻译键）
  → ILanguageTemplateParser.ParseLanguageTemplates（单语言条目集 → LanguageTemplate 字典）
  → LanguageManager.Instance（单例，三级 fallback）
```

---

## 10. 结论

`QuanLib.Minecraft.Resource` 是一次从**脚本式代码到企业级架构**的全面重构。核心变化方向：

1. **可测试性：** 所有功能接口化，天然支持 mock 和单元测试
2. **可扩展性：** 新模型类型、新资产格式可通过实现接口扩展，无需修改现有代码；BlockModelLoader 通过通用的 `IsCubeElements` 几何判定 + `IsCubeTextures` 位掩码验证取代硬编码的 TextureMap 子类
3. **异步 I/O：** 全管道 `async/await`，使用顺序 `foreach` + `await` 替代 `Parallel.ForEach` + `lock` 的脆弱组合，消除忙等和锁内 I/O
4. **功能完整性：** 从仅支持方块扩展到方块 + 物品双管线
5. **代码健壮性：** 修复了忙等循环、goto 模式、锁内 I/O 等实际问题
6. **不可变性：** 关键数据结构使用 `record` 类型，减少状态相关的 bug
7. **可观测性：** `ILogger` 集成提供结构化日志

重构后的代码虽然文件数增加了 2.7 倍，但每个文件的职责更单一、实现更内聚。接口与实现分离、模型与加载分离、数据与行为分离——这些基础架构决策使项目具备了在生产环境中持续演进的工程基础。
