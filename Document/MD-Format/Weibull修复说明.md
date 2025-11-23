# Weibull分析程序修复说明

## 问题概述

C#项目中的Weibull分析程序在计算拟合优度R²时存在两个关键问题：

1. **使用了所有数据而非仅失效数据**：计算R²时包含了删尾数据
2. **R²计算方法错误**：使用了残差平方和而非相关系数的平方

## 详细对比

### 问题1：数据过滤

#### ❌ 原始代码（错误）
```csharp
private double CalculateRSquared(double[] failureTimes, double beta, double eta)
{
    var sorted = failureTimes.OrderBy(t => t).ToArray();  // 使用所有数据！
    int n = sorted.Length;
    // ...
}
```

#### ✅ 修复后代码（正确）
```csharp
private double CalculateRSquared(double[] failureTimes, bool[] isCensored, double beta, double eta)
{
    // 只提取失效数据（删尾数据不参与R²计算）
    var failureData = failureTimes
        .Where((t, i) => !isCensored[i])  // 过滤掉删尾数据
        .OrderBy(t => t)
        .ToArray();
    // ...
}
```

### 问题2：R²计算方法

#### ❌ 原始代码（错误方法）
```csharp
// 计算观测值和预测值
for (int i = 0; i < n; i++)
{
    double F_empirical = (i + 1 - 0.3) / (n + 0.4);
    observed[i] = Math.Log(-Math.Log(1 - F_empirical));
    
    double F_weibull = 1 - Math.Exp(-Math.Pow(sorted[i] / eta, beta));
    predicted[i] = Math.Log(-Math.Log(1 - Math.Max(F_weibull, 1e-10)));
}

// 使用残差平方和方法（错误！）
double observedMean = observed.Average();
double ssTot = observed.Sum(y => Math.Pow(y - observedMean, 2));
double ssRes = observed.Zip(predicted, (o, p) => Math.Pow(o - p, 2)).Sum();
return 1 - (ssRes / ssTot);
```

**问题说明**：
- 这个方法计算的是回归分析中的决定系数
- 但实现有误：`predicted`是基于拟合后的参数计算的，而不是线性回归的预测值
- 导致R²值不准确

#### ✅ 修复后代码（正确方法）
```csharp
// 准备线性化数据
double[] x = new double[n]; // X = ln(t)
double[] y = new double[n]; // Y = ln(-ln(1-F))

for (int i = 0; i < n; i++)
{
    // Bernard中位秩估计
    double rank = i + 1;
    double F = (rank - 0.3) / (n + 0.4);
    
    // 线性化Weibull分布
    x[i] = Math.Log(failureData[i]);
    y[i] = Math.Log(-Math.Log(1 - F));
}

// 计算Pearson相关系数的平方（正确！）
double correlationCoefficient = Correlation.Pearson(x, y);
double rSquared = correlationCoefficient * correlationCoefficient;
```

**正确方法说明**：
- 对于Weibull概率纸拟合，R²定义为线性化数据的Pearson相关系数的平方
- X = ln(t)，Y = ln(-ln(1-F))
- 这与MATLAB版本的实现完全一致

## MATLAB参考实现

MATLAB代码中的正确实现（第76-102行）：

```matlab
CalcR2[times_, censoredRaw_, beta_, eta_] := Module[
  {censored, failTimes, n, ranks, medianRanks, x, y, r2},
  
  censored = Map[ConvertToNumeric, censoredRaw];
  (* 仅使用失效数据进行拟合优度计算 *)
  failTimes = Sort[Pick[times, censored, 0]];
  n = Length[failTimes];
  
  If[n < 2, Return[0]];
  
  (* 使用贝纳德近似法计算中位秩 *)
  ranks = Range[n];
  medianRanks = (ranks - 0.3)/(n + 0.4);
  
  (* 线性化 Weibull *)
  x = Log[failTimes];
  y = Log[-Log[1 - medianRanks]];
  
  (* 计算相关系数的平方作为 R2 *)
  r2 = Correlation[x, y]^2;
  
  If[!NumericQ[r2], 0, r2]
];
```

## 修复影响

### 对R²值的影响

修复前后R²值会有显著差异：

1. **删尾数据较少时**：影响较小
   - 原因：删尾数据占比小，对整体拟合影响有限
   
2. **删尾数据较多时**：影响显著
   - 原因：删尾数据会扭曲拟合优度评估
   - 修复后R²更准确地反映失效数据的拟合情况

3. **计算方法差异**：
   - 原方法：基于残差的决定系数（实现有误）
   - 新方法：基于相关系数的平方（标准方法）

### 典型示例

假设有以下数据：
- 失效数据：[100, 200, 300, 400, 500]（5个）
- 删尾数据：[1000, 1100, 1200]（3个）

**修复前**：
- 使用全部8个数据计算R²
- 删尾数据的高值会影响拟合评估
- R²可能偏低或不准确

**修复后**：
- 仅使用5个失效数据计算R²
- 准确反映失效数据的拟合质量
- R²值更可靠

## 使用方法

### 更新文件

将修复后的`WeibullEngine_Fixed.cs`替换到以下位置：

```
/src/LabTestPlatform.Analysis/WeibullEngine.cs
```

### 依赖项

确保项目引用了以下NuGet包：
- `MathNet.Numerics` (最新版本)
- `MathNet.Numerics.LinearAlgebra` (包含在上述包中)

### 调用示例

```csharp
var engine = new WeibullEngine();

// 示例数据
double[] failureTimes = { 100, 200, 300, 400, 500, 1000, 1100, 1200 };
bool[] isCensored = { false, false, false, false, false, true, true, true };
//                    失效    失效    失效    失效    失效    删尾  删尾  删尾

// 执行分析
var result = engine.Analyze(failureTimes, isCensored);

Console.WriteLine($"β = {result.Beta:F3}");
Console.WriteLine($"η = {result.Eta:F1}");
Console.WriteLine($"R² = {result.RSquared:F4}");  // 修复后的R²值
Console.WriteLine($"MTTF = {result.MTTF:F1}");
Console.WriteLine($"B10 = {result.B10Life:F1}");
```

## 验证建议

1. **与MATLAB结果对比**：
   - 使用相同数据集
   - 对比β、η、R²值
   - 应该完全一致（允许浮点误差）

2. **边界条件测试**：
   - 全部失效数据（无删尾）
   - 部分删尾数据
   - 大量删尾数据

3. **R²合理性检查**：
   - R²应在0-1之间
   - 拟合良好时R²接近1
   - 拟合较差时R²接近0

## 技术要点

### Bernard中位秩公式

```
F(i) = (i - 0.3) / (n + 0.4)
```

其中：
- i：秩次（1到n）
- n：失效样本总数
- F(i)：第i个失效样本的累积失效概率估计

### Weibull线性化

```
Y = ln(-ln(1-F)) = β·ln(t) - β·ln(η)
```

即：Y = β·X - β·ln(η)

这是一个线性关系，斜率为β。

### R²的物理意义

在Weibull分析中，R²表示：
- 线性化后的实际数据点与理论直线的拟合程度
- R²越接近1，说明数据越符合Weibull分布
- R²显著偏低（<0.9）可能表示：
  - 数据不符合Weibull分布
  - 存在多种失效模式
  - 数据质量问题

## 总结

此次修复解决了两个关键问题：

1. ✅ 删尾数据正确处理：R²计算时排除删尾数据
2. ✅ R²计算方法正确：使用相关系数的平方

修复后的代码与MATLAB参考实现完全对齐，确保了分析结果的准确性和一致性。

---

**修复日期**: 2024年11月23日
**修复人**: Claude AI Assistant
**参考标准**: MATLAB V3.3 完整版本
