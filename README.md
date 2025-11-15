# 深圳实验室测试平台 - Avalonia项目

## 快速开始

### 1. 环境要求
- .NET 8.0 SDK
- MySQL 8.0
- Windows 11 Enterprise

### 2. 安装步骤

```bash
# 还原NuGet包
dotnet restore

# 构建项目
dotnet build

# 运行项目
cd src/LabTestPlatform.UI
dotnet run
```

### 3. 数据库配置

修改 `src/LabTestPlatform.UI/appsettings.json` 中的连接字符串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SysHardTestDB;User=root;Password=hml2023PLTKIB;"
  }
}
```

### 4. 项目结构

```
LabTestPlatform/
├── src/
│   ├── LabTestPlatform.UI/          # Avalonia UI项目
│   ├── LabTestPlatform.Core/        # 业务逻辑层
│   ├── LabTestPlatform.Data/        # 数据访问层
│   └── LabTestPlatform.Analysis/    # 威布尔分析引擎
```

## 主要功能

- ✅ 系统/平台/模组管理
- ✅ 测试数据导入（手工输入 + Excel导入）
- ✅ 威布尔可靠性分析
- ⚠️ 报告导出（待实现）

## 技术栈

- .NET 8.0
- Avalonia 11.0.5
- MySQL 8.0
- Dapper
- EPPlus
- MathNet.Numerics

## 许可证

仅供内部使用
