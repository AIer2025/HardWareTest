# 实验室硬件测试平台 - Weibull可靠性分析系统

## 项目简介

本项目是一个基于 C# 和 Avalonia UI 框架开发的硬件可靠性测试与威布尔分析平台。系统用于管理硬件模组的测试数据,进行威布尔分布分析,评估产品可靠性指标,并生成专业的分析报告。

**主要功能:**
- 系统层级管理(系统-平台-模组三级结构)
- 测试数据导入(手工输入/Excel导入/ModBus-TCP)
- 威布尔可靠性分析(MLE最大似然估计法)
- 可靠性指标计算(MTTF、B10、B50、B90寿命等)
- 分析报告导出

## 技术栈

### 前端框架
- **Avalonia 11.0.5** - 跨平台UI框架
- **ReactiveUI** - MVVM响应式编程框架
- **ScottPlot 5.0.15** - 数据可视化图表库

### 后端技术
- **.NET 8.0** - 应用程序框架
- **Dapper** - 轻量级ORM框架
- **MathNet.Numerics** - 数学计算库(威布尔参数估计)
- **ClosedXML** - Excel文件处理
- **MySql.Data** - MySQL数据库连接

### 数据库
- **MySQL 8.0+** - 关系型数据库

## 系统架构

### 项目结构

```
LabTestPlatform/
├── src/
│   ├── LabTestPlatform.UI/           # 用户界面层
│   │   ├── Views/                    # AXAML视图文件
│   │   ├── ViewModels/               # MVVM视图模型
│   │   └── Models/                   # UI数据模型
│   │
│   ├── LabTestPlatform.Core/         # 业务逻辑层
│   │   ├── Services/                 # 业务服务
│   │   │   ├── SystemService.cs      # 系统管理服务
│   │   │   ├── DataImportService.cs  # 数据导入服务
│   │   │   ├── WeibullAnalysisService.cs  # 威布尔分析服务
│   │   │   └── ReportService.cs      # 报告生成服务
│   │   └── Models/                   # 业务模型
│   │
│   ├── LabTestPlatform.Data/         # 数据访问层
│   │   ├── Repositories/             # 数据仓储
│   │   ├── Entities/                 # 数据实体
│   │   └── Context/                  # 数据库连接
│   │
│   └── LabTestPlatform.Analysis/     # 分析引擎层
│       ├── WeibullEngine.cs          # 威布尔分析引擎
│       └── IWeibullEngine.cs         # 分析引擎接口
│
├── appsettings.json                  # 应用配置
└── README.md                         # 项目文档
```

### 分层架构

```
┌─────────────────────────────────────┐
│     UI Layer (Avalonia MVVM)       │
│  Views, ViewModels, UI Models      │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│      Business Logic Layer          │
│  Services, Domain Models           │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│      Data Access Layer             │
│  Repositories, Entities, Dapper    │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│      Analysis Engine Layer         │
│  WeibullEngine (MathNet.Numerics)  │
└────────────────────────────────────┘
```

## 核心功能模块

### 1. 系统管理模块

**功能描述:**
- 管理三级层次结构:系统(System) → 平台(Platform) → 模组(Module)
- 支持系统、平台、模组的增删改查操作
- 模组信息包括:编码、名称、类型、制造商、型号、序列号、生产日期、额定寿命等

**数据模型:**
- **System(系统)**: 顶层测试系统,如"深圳公司测试中心"
- **Platform(平台)**: 测试平台,如"血液中心平台"、"盐湖所平台"
- **Module(模组)**: 具体测试模组,如"振荡模组"、"加盐模组"

### 2. 数据导入模块

**支持的导入方式:**

1. **手工输入**
   - 通过UI界面直接录入测试数据
   
2. **Excel导入**
   - 支持批量导入测试数据
   - 自动解析Excel文件格式
   - 数据验证和错误处理
   
3. **ModBus-TCP导入**
   - 支持从ModBus设备实时采集数据
   - 自动记录导入批次信息

**导入数据字段:**
- 模组ID、测试时间、测试值、测试单位、测试类型
- 测试周期数、失效时间、失效模式
- 是否截尾数据、温度、湿度、操作员、备注

### 3. 威布尔分析模块

**分析方法:**
- **最大似然估计法(MLE)**: 主要参数估计方法
- **秩回归法(Rank Regression)**: 用于初始参数估计

**威布尔分布参数:**
- **β(Beta)**: 形状参数
  - β < 1: 早期失效(婴儿期)
  - β = 1: 随机失效(偶然失效期)
  - β > 1: 磨损失效(耗损期)
- **η(Eta)**: 尺度参数(特征寿命)
- **γ(Gamma)**: 位置参数(当前版本默认为0)

**可靠性指标计算:**
- **MTTF**: 平均故障时间 = η × Γ(1 + 1/β)
- **中位寿命**: Median = η × (ln(2))^(1/β)
- **B10寿命**: 10%产品失效时的寿命
- **B50寿命**: 50%产品失效时的寿命(中位寿命)
- **B90寿命**: 90%产品失效时的寿命
- **R²**: 拟合优度(相关系数的平方)

**置信区间计算:**
- 基于Fisher信息矩阵
- 支持90%、95%、99%置信水平
- 计算β和η的置信上下限

**核心算法:**

```csharp
// 威布尔可靠度函数
R(t) = exp(-(t/η)^β)

// 失效率函数
h(t) = (β/η) × (t/η)^(β-1)

// 概率密度函数
f(t) = (β/η) × (t/η)^(β-1) × exp(-(t/η)^β)

// Bx寿命计算
Bx = η × (-ln(1-x))^(1/β)
```

### 4. 报告导出模块

**报告内容:**
- 模组基本信息
- 测试数据统计
- 威布尔参数(β、η、γ)
- 可靠性指标(MTTF、B10、B50、B90)
- 拟合优度(R²)
- 置信区间
- 威布尔概率图

**导出格式:**
- Excel格式(.xlsx)
- 包含图表和详细分析结果

## 数据库设计

### 核心表结构

#### 1. tb_system (系统表)
```sql
- system_id: 系统ID(主键)
- system_code: 系统编码(唯一)
- system_name: 系统名称
- description: 系统描述
- location: 位置
```

#### 2. tb_platform (平台表)
```sql
- platform_id: 平台ID(主键)
- system_id: 系统ID(外键)
- platform_code: 平台编码(唯一)
- platform_name: 平台名称
- platform_type: 平台类型
```

#### 3. tb_module (模组表)
```sql
- module_id: 模组ID(主键)
- platform_id: 平台ID(外键)
- module_code: 模组编码(唯一)
- module_name: 模组名称
- module_type: 模组类型
- manufacturer: 制造商
- rated_life: 额定寿命
```

#### 4. tb_test_data (测试数据表)
```sql
- test_id: 测试ID(主键)
- module_id: 模组ID(外键)
- test_time: 测试时间
- test_value: 测试值
- failure_time: 失效时间
- is_censored: 是否截尾数据
- test_type: 测试类型
```

#### 5. tb_weibull_analysis (威布尔分析结果表)
```sql
- analysis_id: 分析ID(主键)
- module_id: 模组ID(外键)
- beta: 形状参数
- eta: 尺度参数
- mttf: 平均故障时间
- b10_life, b50_life, b90_life: Bx寿命
- r_squared: 拟合优度
```

#### 6. tb_import_batch (导入批次表)
```sql
- batch_id: 批次ID(主键)
- batch_code: 批次编码
- import_type: 导入类型
- record_count: 记录总数
```

#### 7. tb_data_dictionary (数据字典表)
```sql
- dict_id: 字典ID(主键)
- dict_type: 字典类型
- dict_code: 字典编码
- dict_value: 字典值
```

### 视图

1. **v_system_hierarchy**: 系统层级视图
2. **v_test_data_summary**: 测试数据汇总视图
3. **v_weibull_analysis_data**: 威布尔分析数据视图
4. **v_weibull_latest**: 最新威布尔分析结果视图

## 安装和配置

### 环境要求

- .NET 8.0 SDK
- MySQL 8.0+
- Windows / Linux / macOS

### 安装步骤

1. **克隆项目**
```bash
git clone https://github.com/yourusername/HardWareTest.git
cd HardWareTest
```

2. **恢复NuGet包**
```bash
dotnet restore
```

3. **配置数据库**

编辑 `appsettings.json` 文件:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=syshardtestdb;User=root;Password=yourpassword;"
  }
}
```

4. **导入数据库**
```bash
mysql -u root -p < dump-syshardtestdb-202511231248.sql
```

5. **编译项目**
```bash
dotnet build
```

6. **运行应用**
```bash
cd src/LabTestPlatform.UI
dotnet run
```

## 使用说明

### 1. 系统管理

1. 启动应用后,默认进入"系统管理"页面
2. 点击"添加系统"创建新的测试系统
3. 为系统添加平台
4. 为平台添加模组
5. 支持树形结构展示和管理

### 2. 数据导入

1. 切换到"数据导入"页面
2. 选择模组
3. 选择导入方式:
   - **手工输入**: 逐条录入数据
   - **Excel导入**: 选择Excel文件批量导入
   - **ModBus-TCP**: 配置设备地址实时采集
4. 填写测试数据:测试时间、测试值、失效时间等
5. 标记是否为截尾数据
6. 提交保存

### 3. 威布尔分析

1. 切换到"威布尔分析"页面
2. 选择要分析的模组
3. 系统自动加载该模组的测试数据
4. 点击"执行分析"按钮
5. 查看分析结果:
   - 威布尔参数(β、η)
   - 可靠性指标(MTTF、B10、B50、B90)
   - 拟合优度(R²)
   - 置信区间
6. 查看威布尔概率图

### 4. 报告导出

1. 切换到"报告导出"页面
2. 选择要导出的模组
3. 选择分析结果(可选择历史分析记录)
4. 点击"导出Excel"
5. 选择保存路径
6. 生成包含图表的Excel报告

## 威布尔分析原理

### 威布尔分布概述

威布尔分布是可靠性工程中最常用的寿命分布模型,适用于描述产品从早期失效到磨损失效的全生命周期。

**概率密度函数:**
```
f(t) = (β/η) × (t/η)^(β-1) × exp(-(t/η)^β)
```

**可靠度函数:**
```
R(t) = exp(-(t/η)^β)
```

**失效率函数:**
```
h(t) = (β/η) × (t/η)^(β-1)
```

### 参数意义

- **β(形状参数)**:
  - β < 1: 失效率递减,早期失效
  - β = 1: 失效率恒定,随机失效,退化为指数分布
  - β > 1: 失效率递增,磨损失效
  - β ≈ 3.5: 近似正态分布

- **η(尺度参数)**:
  - 特征寿命,约63.2%的产品在此时间失效
  - η越大,产品寿命越长

### 参数估计方法

#### 最大似然估计法(MLE)

MLE是威布尔分析的标准方法,通过最大化似然函数来估计参数:

```
L(β,η) = ∏[f(ti) for 失效数据] × ∏[R(ti) for 截尾数据]
```

优点:
- 统计效率最高
- 可处理截尾数据
- 提供置信区间

#### 秩回归法(Rank Regression)

通过威布尔概率纸的线性化进行回归分析:

```
ln(ln(1/(1-F))) = β×ln(t) - β×ln(η)
```

用于MLE的初始估计。

### 拟合优度评估

**R²(决定系数)**:
- 使用线性化数据的Pearson相关系数的平方
- R² > 0.95: 拟合优秀
- R² > 0.90: 拟合良好
- R² < 0.90: 需要检查数据质量

### 置信区间

基于Fisher信息矩阵计算参数的置信区间,评估估计的不确定性。

## 数据字典

### 模组类型 (module_type)
- POWER: 电源模组
- SENSOR: 传感器模组
- CONTROLLER: 控制器模组
- MOTOR: 电机模组
- DISPLAY: 显示模组
- COMMUNICATION: 通信模组
- STORAGE: 存储模组
- OTHER: 其他模组

### 测试类型 (test_type)
- LIFE_TEST: 寿命测试
- STRESS_TEST: 应力测试
- BURN_IN: 老化测试
- ENVIRONMENTAL: 环境测试
- PERFORMANCE: 性能测试
- RELIABILITY: 可靠性测试

### 失效模式 (failure_mode)
- WEAR_OUT: 磨损失效
- FATIGUE: 疲劳失效
- CORROSION: 腐蚀失效
- ELECTRICAL: 电气失效
- THERMAL: 热失效
- MECHANICAL: 机械失效
- DEGRADATION: 性能退化
- UNKNOWN: 未知失效

### 平台类型 (platform_type)
- TESTING: 测试平台
- PRODUCTION: 生产平台
- DEVELOPMENT: 研发平台
- QUALITY: 质检平台

### 分析方法 (analysis_method)
- MLE: 最大似然估计
- RRX: 秩回归X轴
- RRY: 秩回归Y轴

### 导入类型 (import_type)
- MANUAL: 手工输入
- EXCEL: Excel导入
- MODBUS: ModBus-TCP

## 常见问题

### Q1: 数据库连接失败?
**A:** 检查 `appsettings.json` 中的连接字符串配置,确保MySQL服务已启动。

### Q2: 威布尔分析提示数据不足?
**A:** 威布尔分析至少需要3个数据点。确保选择的模组有足够的测试数据。

### Q3: 拟合优度R²很低?
**A:** 可能原因:
- 数据质量问题
- 数据不符合威布尔分布
- 存在离群点
建议检查原始数据,剔除异常值。

### Q4: 如何处理截尾数据?
**A:** 在数据导入时,将 `is_censored` 字段设置为 `true`。系统会在MLE分析时正确处理截尾数据。

### Q5: Excel导入格式要求?
**A:** Excel文件应包含以下列:
- 模组编码
- 测试时间
- 测试值
- 失效时间
- 是否截尾
详见模板文件。

## 技术参考

### 威布尔分析参考资料
- 《可靠性数据分析》 - 茆诗松
- 《威布尔分布及其应用》
- IEC 61649: 威布尔分析标准

### 相关标准
- GB/T 5080.7: 威布尔分布
- MIL-HDBK-217: 可靠性预计手册
- IEC 61124: 可靠性增长

## 版本历史

### V3.3 (当前版本)
- 修复威布尔分析R²计算方法
- 优化MLE参数估计
- 改进置信区间计算
- 增强Excel导入功能
- 优化UI交互体验

### V3.2
- 添加截尾数据支持
- 实现置信区间计算
- 优化数据库查询性能

### V3.0
- 完整实现MLE分析
- 重构代码架构
- 引入依赖注入

### V2.2
- 初始版本发布
- 基本功能实现

## 贡献指南

欢迎提交Issue和Pull Request!

### 开发规范
- 遵循C# 编码规范
- 使用MVVM模式
- 编写单元测试
- 提交前运行代码检查

## 许可证

本项目采用 [MIT License](LICENSE)

## 联系方式

- 项目主页: https://github.com/yourusername/HardWareTest
- 问题反馈: https://github.com/yourusername/HardWareTest/issues

---

**Mathematica Lab Test Platform - Weibull Reliability Analysis System**

*Version 3.3 - 2025*
