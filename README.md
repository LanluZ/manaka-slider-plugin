# SliderWatch

此插件调整了真菜香的基础身体能力阈值, 现在的她简直是一个超人

## 安装

1. 安装 [BepInEx](https://github.com/BepInEx/BepInEx) IL2CPP版本 (从以下选择安装, 版本要求6.x)
    - [BepInEx Bleeding Edge (BE) 构建](https://builds.bepinex.dev/projects/bepinex_be)
    - [BepInEx Release 构建](https://github.com/BepInEx/BepInEx/releases)
2. 安装后运行游戏等待 BepInEx 初始化目录后关闭游戏
3. 下载 [Release]()
4. 解压插件放在目录`/BepInEx/plugins`
5. 运行游戏

## 配置

此插件允许自行修改滑块范围

在插件目录中自行修改`data.json`文件：

```json
{
  "sliders": [
    {
      "id": "滑块ID1",
      "min": 最小值,
      "max": 最大值
    },
    {
      "id": "滑块ID2",
      "min": 最小值,
      "max": 最大值
    }
  ]
}
```

## 技术

- 使用Harmony库对Unity的Slider类进行补丁
- 通过监控滑块的OnValueChanged事件来检测值变化
- 使用Newtonsoft.Json库解析配置文件

## 依赖项

- BepInEx.Unity.IL2CPP
- Unity引擎相关库
- Newtonsoft.Json

## 开发

### BeplnEX版本

- 6.x

### 环境要求

- .NET 6.0
- Visual Studio 2022或JetBrains Rider

### 构建

1. 克隆仓库
2. 使用Visual Studio或Rider打开SliderPlugin.sln
3. 构建解决方案

## 许可证

[MIT](LICENSE)
