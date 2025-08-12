# SliderWatch

SliderWatch是一个BepInEx插件，用于监控和修改Unity游戏中的滑块(Slider)组件的值范围。

## 功能

- 从配置文件加载滑块ID和对应的最小/最大值范围
- 监控游戏中滑块值的变化
- 当检测到特定ID的滑块时，强制应用预定义的最小/最大值范围

## 安装

1. 确保你的游戏已安装[BepInEx](https://github.com/BepInEx/BepInEx)
2. 下载最新的SliderWatch发布版本
3. 将`SliderWatch.dll`放入游戏的`BepInEx/plugins`目录中
4. 在插件目录中创建`data.json`文件，配置需要监控的滑块ID和值范围

## 配置

在插件目录中创建`data.json`文件，格式如下：

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

## 技术细节

- 使用Harmony库对Unity的Slider类进行补丁
- 通过监控滑块的OnValueChanged事件来检测值变化
- 使用Newtonsoft.Json库解析配置文件

## 依赖项

- BepInEx.Unity.IL2CPP
- Unity引擎相关库
- Newtonsoft.Json

## 版本

当前版本：1.2.0

## 开发

### 环境要求

- .NET 6.0
- Visual Studio 2022或JetBrains Rider

### 构建

1. 克隆仓库
2. 使用Visual Studio或Rider打开SliderPlugin.sln
3. 构建解决方案

## 许可证

[MIT](LICENSE)
