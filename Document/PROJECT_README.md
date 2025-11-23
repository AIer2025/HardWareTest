# 实验室测试平台 (Lab Test Platform)

[![.NET Version](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-11.0.5-blue)](https://avaloniaui.net/)
[![License](https://img.shields.io/badge/license-Internal%20Use-red)](LICENSE)

一个基于 Avalonia UI 的跨平台硬件可靠性测试与分析平台，专注于威布尔（Weibull）分布分析和产品寿命评估。

---

## 📋 目录

- [项目概述](#项目概述)
- [核心功能](#核心功能)
- [系统架构](#系统架构)
- [技术栈](#技术栈)
- [快速开始](#快速开始)
- [详细配置](#详细配置)
- [使用指南](#使用指南)
- [开发指南](#开发指南)
- [重要更新](#重要更新)
- [数据库架构](#数据库架构)
- [常见问题](#常见问题)
- [贡献指南](#贡献指南)
- [许可证](#许可证)

---

## 🎯 项目概述

实验室测试平台（Lab Test Platform）是一个专业的硬件可靠性测试与分析系统，旨在帮助工程师和质量管理人员：

- 📊 **管理测试数据**：系统化管理测试平台、系统、模组及其测试数据
- 📈 **可靠性分析**：使用威布尔分布进行寿命数据分析和可靠性评估
- 📄 **报告生成**：自动生成专业的分析报告和可视化图表
- 🔄 **数据导入**：支持手工输入和Excel批量导入
- 💾 **数据持久化**：基于MySQL的可靠数据存储

### 应用场景

- 硬件模组寿命测试与评估
- 产品可靠性分析与预测
- 失效模式统计与分析
- 测试数据管理与追溯
- B10/B50/B90寿命计算

---

## ⭐ 核心功能

### 1. 系统管理 (System Management)

```
平台管理 → 系统管理 → 模组管理
   ↓           ↓           ↓
多平台    多系统配置   模组信息
```

**功能特性**：
- ✅ 三级层次结构管理（平台-系统-模组）
- ✅ 树形视图展示系统关系
- ✅ 模组编码、名称、类型管理
- ✅ 批量导入和导出
- ✅ 数据字典维护

### 2. 数据导入 (Data Import)

**支持方式**：
- **手工输入**：单条数据录入
- **Excel导入**：批量数据导入
- **ModBus-TCP**：自动采集（规划中）

**数据类型**：
- 失效时间数据
- 删尾数据（Censored Data）
- 测试类型和失效模式
- 批次信息

**Excel导入格式**：
```
| 模组编码 | 失效时间(h) | 是否删尾 | 测试类型 | 失效模式 |
|---------|------------|---------|---------|---------|
| MOD001  | 1000       | 否      | LIFE    | WEAR    |
| MOD001  | 1500       | 是      | LIFE    | -       |
```

### 3. 威布尔分析 (Weibull Analysis) ⭐

**核心算法**：
- 最大似然估计（MLE）参数估计
- 支持删尾数据处理
- Bernard中位秩法
- 拟合优度R²计算

**分析结果**：
- **形状参数 β**：失效率趋势指标
  - β < 1：早期失效（浴盆曲线左侧）
  - β = 1：随机失效（指数分布）
  - β > 1：耗损失效（浴盆曲线右侧）
  
- **尺度参数 η**：特征寿命（63.2%失效时间）

- **可靠性指标**：
  - MTTF（平均故障时间）
  - 中位寿命
  - B10寿命（10%失效寿命）
  - B50寿命（50%失效寿命）
  - B90寿命（90%失效寿命）

- **拟合优度 R²**：线性化拟合质量评估

**可视化**：
- 威布尔概率图
- 可靠度曲线
- 失效率曲线
- 概率密度函数

### 4. 报告导出 (Report Export)

**支持格式**：
- Excel (`.xlsx`)
- PDF报告
- 图表导出（PNG/SVG）

**报告内容**：
- 分析参数汇总
- 可靠性指标表
- 统计图表
- 置信区间
- 分析日志

---

## 🏗️ 系统架构

### 架构设计

本项目采用经典的四层架构设计：

```
┌─────────────────────────────────────────────┐
│           Presentation Layer                │
│      (LabTestPlatform.UI - Avalonia)        │
│   ┌──────────┬──────────┬──────────────┐   │
│   │ Views    │ViewModels│   Models     │   │
│   └──────────┴──────────┴──────────────┘   │
└─────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────┐
│            Business Logic Layer             │
│         (LabTestPlatform.Core)              │
│   ┌──────────────┬──────────────────────┐  │
│   │  Services    │      Models          │  │
│   └──────────────┴──────────────────────┘  │
└─────────────────────────────────────────────┘
                     ↓
┌──────────────────────┬──────────────────────┐
│   Data Access Layer  │   Analysis Engine    │
│ (LabTestPlatform.    │  (LabTestPlatform.   │
│      Data)           │     Analysis)        │
│  ┌────────────────┐  │  ┌────────────────┐ │
│  │ Repositories   │  │  │ Weibull Engine │ │
│  │ Entities       │  │  │ MLE Estimator  │ │
│  │ DbFactory      │  │  │ R² Calculator  │ │
│  └────────────────┘  │  └────────────────┘ │
└──────────────────────┴──────────────────────┘
                     ↓
┌─────────────────────────────────────────────┐
│              Database Layer                 │
│             (MySQL 8.0)                     │
└─────────────────────────────────────────────┘
```

### 项目结构

```
LabTestPlatform/
├── src/
│   ├── LabTestPlatform.UI/              # 🖥️ UI层 (Avalonia)
│   │   ├── Views/                       # XAML视图
│   │   │   ├── MainWindow.axaml
│   │   │   ├── SystemManagementView.axaml
│   │   │   ├── DataImportView.axaml
│   │   │   ├── WeibullAnalysisView.axaml
│   │   │   └── ReportExportView.axaml
│   │   ├── ViewModels/                  # 视图模型（MVVM）
│   │   │   ├── MainWindowViewModel.cs
│   │   │   ├── SystemManagementViewModel.cs
│   │   │   ├── DataImportViewModel.cs
│   │   │   ├── WeibullAnalysisViewModel.cs
│   │   │   └── ReportExportViewModel.cs
│   │   ├── Models/                      # UI模型
│   │   ├── Utilities/                   # 工具类
│   │   └── appsettings.json            # 配置文件
│   │
│   ├── LabTestPlatform.Core/            # 💼 业务逻辑层
│   │   ├── Models/                      # 业务模型
│   │   │   ├── PlatformModel.cs
│   │   │   ├── SystemModel.cs
│   │   │   ├── ModuleModel.cs
│   │   │   └── TestData.cs
│   │   └── Services/                    # 业务服务
│   │       ├── ISystemService.cs
│   │       ├── SystemService.cs
│   │       ├── IDataImportService.cs
│   │       ├── DataImportService.cs
│   │       ├── IWeibullAnalysisService.cs
│   │       ├── WeibullAnalysisService.cs
│   │       ├── IReportService.cs
│   │       └── ReportService.cs
│   │
│   ├── LabTestPlatform.Data/            # 💾 数据访问层
│   │   ├── Context/                     # 数据库上下文
│   │   │   ├── IDbConnectionFactory.cs
│   │   │   └── DbConnectionFactory.cs
│   │   ├── Entities/                    # 数据实体
│   │   │   ├── PlatformEntity.cs
│   │   │   ├── SystemEntity.cs
│   │   │   ├── ModuleEntity.cs
│   │   │   └── TestDataEntity.cs
│   │   └── Repositories/                # 数据仓储
│   │       ├── IPlatformRepository.cs
│   │       ├── PlatformRepository.cs
│   │       ├── ISystemRepository.cs
│   │       ├── SystemRepository.cs
│   │       ├── IModuleRepository.cs
│   │       ├── ModuleRepository.cs
│   │       ├── ITestDataRepository.cs
│   │       ├── TestDataRepository.cs
│   │       ├── IWeibullAnalysisRepository.cs
│   │       └── WeibullAnalysisRepository.cs
│   │
│   └── LabTestPlatform.Analysis/        # 📊 分析引擎层
│       ├── IWeibullEngine.cs
│       └── WeibullEngine.cs             # Weibull分析核心
│
├── LabTestPlatform.sln                  # 解决方案文件
└── README.md                            # 本文件
```

---

## 🛠️ 技术栈

### 前端技术

| 技术 | 版本 | 用途 |
|------|------|------|
| **Avalonia UI** | 11.0.5 | 跨平台UI框架 |
| **ReactiveUI** | 11.0.5 | MVVM框架 |
| **Fluent Theme** | 11.0.5 | 现代化UI主题 |
| **ScottPlot** | 5.0.15 | 数据可视化图表 |

### 后端技术

| 技术 | 版本 | 用途 |
|------|------|------|
| **.NET** | 8.0 | 运行时框架 |
| **C#** | 12.0 | 编程语言 |
| **MySQL** | 8.0 | 关系数据库 |
| **Dapper** | 2.1.24 | ORM框架（轻量级） |
| **MySqlConnector** | 2.3.1 | MySQL驱动 |

### 分析与处理

| 技术 | 版本 | 用途 |
|------|------|------|
| **MathNet.Numerics** | 5.0.0 | 数值计算库 |
| **EPPlus** | 7.0.3 | Excel处理 |

### 开发工具

- **IDE**: Visual Studio 2022 / JetBrains Rider / VS Code
- **版本控制**: Git
- **包管理**: NuGet

---

## 🚀 快速开始

### 环境要求

#### 必需软件

- **操作系统**: Windows 10/11, macOS, Linux
- **.NET 8.0 SDK**: [下载地址](https://dotnet.microsoft.com/download/dotnet/8.0)
- **MySQL 8.0**: [下载地址](https://dev.mysql.com/downloads/mysql/)

#### 推荐配置

- **CPU**: 4核及以上
- **内存**: 8GB及以上
- **磁盘**: 500MB可用空间

### 安装步骤

#### 1. 克隆项目

```bash
git clone <repository-url>
cd LabTestPlatform
```

#### 2. 数据库初始化

```bash
# 登录MySQL
mysql -u root -p

# 创建数据库
CREATE DATABASE syshardtestdb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

# 导入数据库架构（使用提供的SQL文件）
mysql -u root -p syshardtestdb < dump-syshardtestdb-202511231112.sql
```

#### 3. 配置连接字符串

编辑 `src/LabTestPlatform.UI/appsettings.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=syshardtestdb;User=root;Password=你的密码;SslMode=None;AllowPublicKeyRetrieval=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

#### 4. 还原依赖包

```bash
# 在项目根目录执行
dotnet restore
```

#### 5. 编译项目

```bash
# 编译整个解决方案
dotnet build

# 或编译特定项目
dotnet build src/LabTestPlatform.UI/LabTestPlatform.UI.csproj
```

#### 6. 运行应用

```bash
# 切换到UI项目目录
cd src/LabTestPlatform.UI

# 运行应用
dotnet run
```

或者使用Visual Studio/Rider直接打开解决方案文件 `LabTestPlatform.sln` 并运行。

---

## ⚙️ 详细配置

### 数据库配置

#### 连接字符串参数说明

```
Server=localhost          # MySQL服务器地址
Port=3306                # 端口（默认3306）
Database=syshardtestdb   # 数据库名称
User=root                # 用户名
Password=***             # 密码
SslMode=None             # SSL模式
AllowPublicKeyRetrieval=True  # 允许公钥检索
ServerTimezone=UTC       # 服务器时区
```

#### 数据库表结构

主要数据表：

- `tb_platform` - 测试平台表
- `tb_system` - 系统表
- `tb_module` - 模组表
- `tb_test_data` - 测试数据表
- `tb_weibull_analysis` - 威布尔分析结果表
- `tb_import_batch` - 导入批次表
- `tb_data_dictionary` - 数据字典表

### 应用配置

#### 日志配置

在 `appsettings.json` 中配置日志级别：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

#### Excel导入配置

支持的Excel格式：
- 文件扩展名: `.xlsx`, `.xls`
- 最大文件大小: 10MB
- 最大行数: 10,000行

---

## 📖 使用指南

### 1. 系统管理

#### 创建测试平台

1. 打开应用，点击 **系统管理** 标签
2. 点击 **新建平台** 按钮
3. 填写平台信息：
   - 平台编码（必填）
   - 平台名称（必填）
   - 平台类型
   - 备注

#### 创建系统

1. 选择目标平台
2. 点击 **新建系统** 按钮
3. 填写系统信息

#### 创建模组

1. 选择目标系统
2. 点击 **新建模组** 按钮
3. 填写模组信息：
   - 模组编码（必填，唯一）
   - 模组名称（必填）
   - 模组类型
   - 设计寿命

### 2. 数据导入

#### 手工输入

1. 打开 **数据导入** 标签
2. 选择目标模组
3. 填写测试数据：
   - 失效时间（小时）
   - 是否删尾
   - 测试类型
   - 失效模式
4. 点击 **添加** 保存

#### Excel批量导入

1. 准备Excel文件，确保包含以下列：
   ```
   模组编码 | 失效时间 | 是否删尾 | 测试类型 | 失效模式
   ```

2. 点击 **Excel导入** 按钮
3. 选择Excel文件
4. 系统自动验证并导入数据
5. 查看导入结果报告

### 3. 威布尔分析

#### 执行分析

1. 打开 **威布尔分析** 标签
2. 选择要分析的模组（可多选）
3. 设置分析参数：
   - 置信水平（默认95%）
   - 分析方法（MLE）
4. 点击 **开始分析** 按钮
5. 查看分析结果

#### 查看结果

分析完成后，系统显示：

- **参数表格**：
  - 形状参数 β
  - 尺度参数 η
  - 拟合优度 R²
  - MTTF
  - B10/B50/B90寿命

- **可视化图表**：
  - 威布尔概率图
  - 可靠度曲线
  - 失效率曲线

#### 保存结果

分析结果自动保存到数据库 `tb_weibull_analysis` 表。

### 4. 报告导出

1. 打开 **报告导出** 标签
2. 选择要导出的分析记录
3. 选择导出格式（Excel/PDF）
4. 点击 **导出** 按钮
5. 选择保存位置

---

## 🔧 开发指南

### 开发环境设置

#### IDE配置

**Visual Studio 2022**：
1. 安装 **.NET desktop development** 工作负载
2. 安装 **Avalonia for Visual Studio** 扩展

**JetBrains Rider**：
1. 安装 **Avalonia** 插件
2. 配置 **.NET 8.0 SDK**

#### 编码规范

- 遵循 C# 编码规范
- 使用有意义的变量和方法名
- 添加必要的XML文档注释
- 使用异步编程（async/await）
- 实现依赖注入（DI）

### 添加新功能

#### 1. 创建数据模型

在 `LabTestPlatform.Core/Models/` 中创建：

```csharp
namespace LabTestPlatform.Core.Models
{
    public class YourModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        // ... 其他属性
    }
}
```

#### 2. 创建数据实体

在 `LabTestPlatform.Data/Entities/` 中创建：

```csharp
namespace LabTestPlatform.Data.Entities
{
    public class YourEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        // ... 对应数据库字段
    }
}
```

#### 3. 创建Repository

```csharp
// 接口
public interface IYourRepository
{
    Task<YourEntity> GetByIdAsync(int id);
    Task<IEnumerable<YourEntity>> GetAllAsync();
    Task<int> InsertAsync(YourEntity entity);
    Task<bool> UpdateAsync(YourEntity entity);
    Task<bool> DeleteAsync(int id);
}

// 实现
public class YourRepository : IYourRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    public YourRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    // 实现接口方法...
}
```

#### 4. 创建Service

```csharp
public interface IYourService
{
    Task<YourModel> GetByIdAsync(int id);
    // ... 其他业务方法
}

public class YourService : IYourService
{
    private readonly IYourRepository _repository;
    
    public YourService(IYourRepository repository)
    {
        _repository = repository;
    }
    
    // 实现业务逻辑...
}
```

#### 5. 注册依赖注入

在 `Program.cs` 中注册：

```csharp
services.AddScoped<IYourRepository, YourRepository>();
services.AddScoped<IYourService, YourService>();
```

#### 6. 创建ViewModel

```csharp
public class YourViewModel : ViewModelBase
{
    private readonly IYourService _yourService;
    
    public YourViewModel(IYourService yourService)
    {
        _yourService = yourService;
    }
    
    // 属性和命令...
}
```

#### 7. 创建View

在 `Views/` 中创建 `.axaml` 文件。

### 调试技巧

#### 1. 启用详细日志

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

#### 2. 使用断点调试

在 Visual Studio/Rider 中设置断点，F5启动调试。

#### 3. 数据库调试

使用 MySQL Workbench 或 DBeaver 查看数据库状态。

---

## 🔄 重要更新

### Weibull分析引擎修复 (2024-11-23)

#### 修复内容

修复了 `WeibullEngine.cs` 中R²（拟合优度）计算的两个关键问题：

**问题1**: R²计算使用了删尾数据
- **影响**: R²值不准确，无法正确反映拟合质量
- **修复**: 只使用失效数据计算R²

**问题2**: R²计算方法错误
- **影响**: 与标准Weibull分析方法不一致
- **修复**: 使用Pearson相关系数的平方

#### 修复详情

查看修复包中的文档：
- `Weibull修复说明.md` - 详细技术说明
- `快速修复指南.md` - 应用修复步骤
- `修复前后代码对比.md` - 代码对比
- `WeibullEngine_Fixed.cs` - 修复后的代码

#### 应用修复

1. 备份原文件：
   ```bash
   cp src/LabTestPlatform.Analysis/WeibullEngine.cs \
      src/LabTestPlatform.Analysis/WeibullEngine.cs.bak
   ```

2. 替换文件：
   ```bash
   cp WeibullEngine_Fixed.cs \
      src/LabTestPlatform.Analysis/WeibullEngine.cs
   ```

3. 重新编译：
   ```bash
   dotnet build
   ```

#### 修复效果

- ✅ R²值更准确
- ✅ 与MATLAB标准方法一致
- ✅ 仅失效数据参与拟合评估
- ✅ β、η等参数不受影响

---

## 💾 数据库架构

### ER图概述

```
tb_platform (平台表)
    ↓ 1:N
tb_system (系统表)
    ↓ 1:N
tb_module (模组表)
    ↓ 1:N
tb_test_data (测试数据表)
    ↓ 1:N
tb_weibull_analysis (威布尔分析表)
```

### 核心表结构

#### tb_module (模组表)

| 字段 | 类型 | 说明 |
|------|------|------|
| module_id | INT | 主键 |
| platform_id | INT | 平台ID |
| module_code | VARCHAR(50) | 模组编码 |
| module_name | VARCHAR(200) | 模组名称 |
| module_type | VARCHAR(20) | 模组类型 |
| design_life | INT | 设计寿命(小时) |
| is_active | TINYINT | 是否启用 |

#### tb_test_data (测试数据表)

| 字段 | 类型 | 说明 |
|------|------|------|
| test_id | INT | 主键 |
| module_id | INT | 模组ID |
| failure_time | DECIMAL(10,2) | 失效时间(小时) |
| is_censored | TINYINT | 是否删尾 |
| test_type | VARCHAR(20) | 测试类型 |
| failure_mode | VARCHAR(50) | 失效模式 |

#### tb_weibull_analysis (威布尔分析表)

| 字段 | 类型 | 说明 |
|------|------|------|
| analysis_id | INT | 主键 |
| module_id | INT | 模组ID |
| beta | DECIMAL(10,6) | 形状参数 |
| eta | DECIMAL(10,6) | 尺度参数 |
| mttf | DECIMAL(10,2) | 平均故障时间 |
| median_life | DECIMAL(10,2) | 中位寿命 |
| b10_life | DECIMAL(10,2) | B10寿命 |
| r_squared | DECIMAL(10,6) | 拟合优度 |
| analysis_time | DATETIME | 分析时间 |

---

## ❓ 常见问题

### Q1: 数据库连接失败

**问题**: 提示 "Unable to connect to MySQL server"

**解决方案**:
1. 检查MySQL服务是否启动
2. 验证连接字符串是否正确
3. 确认用户名和密码
4. 检查防火墙设置

### Q2: Excel导入失败

**问题**: 导入Excel时提示格式错误

**解决方案**:
1. 确保Excel文件格式正确（.xlsx）
2. 检查列标题是否匹配
3. 验证数据类型（数值、文本）
4. 确保模组编码存在

### Q3: Weibull分析失败

**问题**: 分析时提示 "数据不足"

**解决方案**:
1. 至少需要3个失效数据点
2. 检查是否有有效的失效时间
3. 确认数据没有异常值

### Q4: R²值异常

**问题**: R²值为负数或大于1

**解决方案**:
1. 应用Weibull分析修复包
2. 检查数据质量
3. 确认是否有足够的失效数据

### Q5: 界面显示异常

**问题**: 界面元素显示不正常

**解决方案**:
1. 更新到最新版Avalonia
2. 检查DPI缩放设置
3. 重启应用程序

---

## 🤝 贡献指南

### 贡献流程

1. Fork 项目
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 提交 Pull Request

### 代码审查标准

- 代码符合C#编码规范
- 包含必要的单元测试
- 更新相关文档
- 通过所有CI检查

### Bug报告

提交Bug时请包含：
- 操作系统和版本
- .NET版本
- 重现步骤
- 预期行为
- 实际行为
- 错误日志

---

## 📄 许可证

本项目仅供内部使用，未经授权不得对外分发。

---

## 📞 联系方式

- **项目负责人**: [姓名]
- **技术支持**: [邮箱]
- **问题反馈**: [Issue追踪系统]

---

## 🙏 致谢

- **Avalonia Team** - 优秀的跨平台UI框架
- **MathNet.Numerics** - 强大的数值计算库
- **ScottPlot** - 简洁的数据可视化工具

---

## 📚 附录

### A. 威布尔分布公式

**概率密度函数 (PDF)**:
```
f(t) = (β/η) * (t/η)^(β-1) * exp(-(t/η)^β)
```

**累积分布函数 (CDF)**:
```
F(t) = 1 - exp(-(t/η)^β)
```

**可靠度函数**:
```
R(t) = exp(-(t/η)^β)
```

**失效率函数**:
```
h(t) = (β/η) * (t/η)^(β-1)
```

### B. Bernard中位秩公式

```
F(i) = (i - 0.3) / (n + 0.4)
```

其中：
- i: 秩次（1到n）
- n: 失效样本总数
- F(i): 第i个失效样本的累积失效概率估计

### C. 参考资料

- [Weibull Analysis Handbook](https://example.com)
- [Avalonia Documentation](https://docs.avaloniaui.net/)
- [.NET 8.0 Documentation](https://docs.microsoft.com/dotnet/)

---

**最后更新**: 2024-11-23  
**版本**: 1.1.0  
**状态**: 生产就绪
