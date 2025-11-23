using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.Optimization;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using static MathNet.Numerics.SpecialFunctions;

namespace LabTestPlatform.Analysis
{
    /// <summary>
    /// 威布尔分析引擎 - 修复版
    /// 实现威布尔分布参数估计和可靠性计算
    /// 修复内容：
    /// 1. R²计算方法：使用相关系数的平方，而非残差平方和
    /// 2. R²计算时只使用失效数据（排除删尾数据）
    /// 3. 与MATLAB版本的CalcR2函数对齐
    /// </summary>
    public class WeibullEngine : IWeibullEngine
    {
        /// <summary>
        /// 执行威布尔分析
        /// </summary>
        /// <param name="failureTimes">失效时间数据</param>
        /// <param name="isCensored">截尾标记（可选，true表示删尾，false表示失效）</param>
        /// <param name="confidenceLevel">置信水平（默认95%）</param>
        /// <returns>威布尔分析结果</returns>
        public WeibullResult Analyze(double[] failureTimes, bool[]? isCensored = null, double confidenceLevel = 0.95)
        {
            if (failureTimes == null || failureTimes.Length < 3)
            {
                throw new ArgumentException("至少需要3个数据点才能进行威布尔分析", nameof(failureTimes));
            }

            // 如果没有提供截尾标记,默认全部为完整数据（失效数据）
            isCensored ??= new bool[failureTimes.Length];

            // 使用最大似然估计法（MLE）估计参数
            var (beta, eta) = EstimateParametersMLE(failureTimes, isCensored);

            // 计算可靠性指标
            var mttf = CalculateMTTF(beta, eta);
            var medianLife = CalculateMedianLife(beta, eta);
            var b10 = CalculateBxLife(0.10, beta, eta);
            var b50 = CalculateBxLife(0.50, beta, eta);
            var b90 = CalculateBxLife(0.90, beta, eta);

            // 计算拟合优度 - 修复版：只使用失效数据，使用相关系数平方
            var rSquared = CalculateRSquared(failureTimes, isCensored, beta, eta);

            // 计算置信区间（使用Fisher信息矩阵）
            var (betaLower, betaUpper, etaLower, etaUpper) = CalculateConfidenceIntervals(
                failureTimes, beta, eta, confidenceLevel);

            return new WeibullResult
            {
                Beta = beta,
                Eta = eta,
                Gamma = 0, // 当前实现不考虑位置参数
                MTTF = mttf,
                MedianLife = medianLife,
                B10Life = b10,
                B50Life = b50,
                B90Life = b90,
                RSquared = rSquared,
                ConfidenceLevel = confidenceLevel,
                BetaLower = betaLower,
                BetaUpper = betaUpper,
                EtaLower = etaLower,
                EtaUpper = etaUpper,
                SampleSize = failureTimes.Length,
                FailureCount = isCensored.Count(c => !c)
            };
        }

        /// <summary>
        /// 使用最大似然估计法（MLE）估计威布尔参数
        /// </summary>
        private (double beta, double eta) EstimateParametersMLE(double[] failureTimes, bool[] isCensored)
        {
            // 初始猜测值（使用矩估计或秩回归的结果）
            var initialGuess = EstimateParametersRankRegression(failureTimes, isCensored);

            // 定义负对数似然函数
            Func<Vector<double>, double> negativeLogLikelihood = (parameters) =>
            {
                double beta = parameters[0];
                double eta = parameters[1];

                if (beta <= 0 || eta <= 0)
                    return double.MaxValue;

                double logL = 0;
                for (int i = 0; i < failureTimes.Length; i++)
                {
                    double t = failureTimes[i];
                    if (!isCensored[i]) // 失效数据
                    {
                        logL += Math.Log(beta) - beta * Math.Log(eta) + (beta - 1) * Math.Log(t) - Math.Pow(t / eta, beta);
                    }
                    else // 删尾数据
                    {
                        logL += -Math.Pow(t / eta, beta);
                    }
                }
                return -logL; // 返回负对数似然
            };

            // 使用Nelder-Mead优化算法
            var optimizer = new NelderMeadSimplex(1e-8, 1000);
            
            // 创建初始向量
            var initialVector = Vector<double>.Build.Dense(new[] { initialGuess.beta, initialGuess.eta });
            
            var result = optimizer.FindMinimum(
                ObjectiveFunction.Value(negativeLogLikelihood),
                initialVector
            );

            return (result.MinimizingPoint[0], result.MinimizingPoint[1]);
        }

        /// <summary>
        /// 使用秩回归法估计威布尔参数（用于初始估计）
        /// </summary>
        private (double beta, double eta) EstimateParametersRankRegression(double[] failureTimes, bool[] isCensored)
        {
            // 只使用失效数据进行秩回归
            var completeData = failureTimes
                .Where((t, i) => !isCensored[i])
                .OrderBy(t => t)
                .ToArray();

            if (completeData.Length < 2)
            {
                // 如果失效数据太少，使用简单估计
                return (2.0, failureTimes.Average());
            }

            int n = completeData.Length;
            double[] x = new double[n]; // ln(t)
            double[] y = new double[n]; // ln(ln(1/(1-F)))

            for (int i = 0; i < n; i++)
            {
                double rank = i + 1;
                // Bernard中位秩估计: (i - 0.3) / (n + 0.4)
                double F = (rank - 0.3) / (n + 0.4);
                
                x[i] = Math.Log(completeData[i]);
                y[i] = Math.Log(-Math.Log(1 - F));
            }

            // 线性回归: y = beta * x - beta * ln(eta)
            double xMean = x.Average();
            double yMean = y.Average();

            double sumXY = 0, sumXX = 0;
            for (int i = 0; i < n; i++)
            {
                sumXY += (x[i] - xMean) * (y[i] - yMean);
                sumXX += (x[i] - xMean) * (x[i] - xMean);
            }

            double beta = sumXY / sumXX;
            double eta = Math.Exp((beta * xMean - yMean) / beta);

            // 确保参数在合理范围内
            beta = Math.Max(0.1, Math.Min(beta, 10.0));
            eta = Math.Max(completeData.Min() * 0.5, Math.Min(eta, completeData.Max() * 2.0));

            return (beta, eta);
        }

        /// <summary>
        /// 计算平均故障时间（MTTF）
        /// MTTF = η * Γ(1 + 1/β)
        /// </summary>
        private double CalculateMTTF(double beta, double eta)
        {
            return eta * Gamma(1.0 + 1.0 / beta);
        }

        /// <summary>
        /// 计算中位寿命
        /// Median = η * (ln(2))^(1/β)
        /// </summary>
        private double CalculateMedianLife(double beta, double eta)
        {
            return eta * Math.Pow(Math.Log(2), 1.0 / beta);
        }

        /// <summary>
        /// 计算Bx寿命（x%的产品失效时的寿命）
        /// Bx = η * (-ln(1-x))^(1/β)
        /// </summary>
        private double CalculateBxLife(double x, double beta, double eta)
        {
            return eta * Math.Pow(-Math.Log(1 - x), 1.0 / beta);
        }

        /// <summary>
        /// 计算拟合优度R² - 修复版
        /// 修复内容：
        /// 1. 只使用失效数据（isCensored = false），排除删尾数据
        /// 2. 计算线性化数据(X, Y)的Pearson相关系数的平方
        /// 3. X = ln(t), Y = ln(-ln(1-F))
        /// 4. 与MATLAB版本的CalcR2函数完全对齐
        /// </summary>
        private double CalculateRSquared(double[] failureTimes, bool[] isCensored, double beta, double eta)
        {
            // 1. 只提取失效数据（删尾数据不参与R²计算）
            var failureData = failureTimes
                .Where((t, i) => !isCensored[i])
                .OrderBy(t => t)
                .ToArray();

            int n = failureData.Length;

            // 如果失效数据少于2个，无法计算R²
            if (n < 2)
            {
                return 0.0;
            }

            // 2. 准备线性化数据
            double[] x = new double[n]; // X = ln(t)
            double[] y = new double[n]; // Y = ln(-ln(1-F))

            for (int i = 0; i < n; i++)
            {
                // Bernard中位秩估计: F = (rank - 0.3) / (n + 0.4)
                double rank = i + 1;
                double F = (rank - 0.3) / (n + 0.4);
                
                // 线性化Weibull分布
                x[i] = Math.Log(failureData[i]);
                y[i] = Math.Log(-Math.Log(1 - F));
            }

            // 3. 计算Pearson相关系数
            // 使用MathNet.Numerics的Correlation函数
            double correlationCoefficient = Correlation.Pearson(x, y);
            
            // 4. R² = r²（相关系数的平方）
            double rSquared = correlationCoefficient * correlationCoefficient;

            // 确保R²在合理范围内
            if (double.IsNaN(rSquared) || double.IsInfinity(rSquared))
            {
                return 0.0;
            }

            return rSquared;
        }

        /// <summary>
        /// 计算参数的置信区间（使用Fisher信息矩阵）
        /// </summary>
        private (double betaLower, double betaUpper, double etaLower, double etaUpper) 
            CalculateConfidenceIntervals(double[] failureTimes, double beta, double eta, double confidenceLevel)
        {
            int n = failureTimes.Length;
            
            // Fisher信息矩阵的近似（仅考虑完整数据的简化版本）
            // 对于威布尔分布，方差的近似公式
            double varBeta = 1.109 * beta * beta / n; // 简化的方差估计
            double varEta = 0.608 * eta * eta / n;

            // 置信水平对应的Z值（正态分布）
            double z = GetZValue(confidenceLevel);

            double betaLower = Math.Max(0.1, beta - z * Math.Sqrt(varBeta));
            double betaUpper = beta + z * Math.Sqrt(varBeta);
            double etaLower = Math.Max(failureTimes.Min() * 0.1, eta - z * Math.Sqrt(varEta));
            double etaUpper = eta + z * Math.Sqrt(varEta);

            return (betaLower, betaUpper, etaLower, etaUpper);
        }

        /// <summary>
        /// 获取置信水平对应的Z值
        /// </summary>
        private double GetZValue(double confidenceLevel)
        {
            // 常用置信水平的Z值
            return confidenceLevel switch
            {
                0.90 => 1.645,
                0.95 => 1.960,
                0.99 => 2.576,
                _ => 1.960 // 默认95%
            };
        }

        /// <summary>
        /// 计算可靠度函数 R(t)
        /// R(t) = exp(-(t/η)^β)
        /// </summary>
        public double CalculateReliability(double time, double beta, double eta)
        {
            if (time < 0) throw new ArgumentException("时间不能为负", nameof(time));
            return Math.Exp(-Math.Pow(time / eta, beta));
        }

        /// <summary>
        /// 计算失效率函数 h(t)
        /// h(t) = (β/η) * (t/η)^(β-1)
        /// </summary>
        public double CalculateHazardRate(double time, double beta, double eta)
        {
            if (time < 0) throw new ArgumentException("时间不能为负", nameof(time));
            return (beta / eta) * Math.Pow(time / eta, beta - 1);
        }

        /// <summary>
        /// 计算概率密度函数 f(t)
        /// f(t) = (β/η) * (t/η)^(β-1) * exp(-(t/η)^β)
        /// </summary>
        public double CalculatePDF(double time, double beta, double eta)
        {
            if (time < 0) throw new ArgumentException("时间不能为负", nameof(time));
            return (beta / eta) * Math.Pow(time / eta, beta - 1) * Math.Exp(-Math.Pow(time / eta, beta));
        }

        /// <summary>
        /// 生成威布尔概率图数据点
        /// </summary>
        public List<(double time, double probability)> GenerateProbabilityPlotData(double[] failureTimes, double beta, double eta)
        {
            var sorted = failureTimes.OrderBy(t => t).ToArray();
            var plotData = new List<(double, double)>();

            for (int i = 0; i < sorted.Length; i++)
            {
                double rank = i + 1;
                // Bernard中位秩
                double F = (rank - 0.3) / (sorted.Length + 0.4);
                plotData.Add((sorted[i], F * 100));
            }

            return plotData;
        }
    }

    /// <summary>
    /// 威布尔分析结果
    /// </summary>
    public class WeibullResult
    {
        /// <summary>形状参数β</summary>
        public double Beta { get; set; }

        /// <summary>尺度参数η（特征寿命）</summary>
        public double Eta { get; set; }

        /// <summary>位置参数γ</summary>
        public double Gamma { get; set; }

        /// <summary>平均故障时间</summary>
        public double MTTF { get; set; }

        /// <summary>中位寿命</summary>
        public double MedianLife { get; set; }

        /// <summary>B10寿命（10%失效寿命）</summary>
        public double B10Life { get; set; }

        /// <summary>B50寿命（50%失效寿命）</summary>
        public double B50Life { get; set; }

        /// <summary>B90寿命（90%失效寿命）</summary>
        public double B90Life { get; set; }

        /// <summary>拟合优度R²</summary>
        public double RSquared { get; set; }

        /// <summary>置信水平</summary>
        public double ConfidenceLevel { get; set; }

        /// <summary>β的置信下限</summary>
        public double BetaLower { get; set; }

        /// <summary>β的置信上限</summary>
        public double BetaUpper { get; set; }

        /// <summary>η的置信下限</summary>
        public double EtaLower { get; set; }

        /// <summary>η的置信上限</summary>
        public double EtaUpper { get; set; }

        /// <summary>样本数量</summary>
        public int SampleSize { get; set; }

        /// <summary>失效数量</summary>
        public int FailureCount { get; set; }
    }
}
