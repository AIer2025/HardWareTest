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
    /// 威布尔分析引擎 - 完整修复版
    /// 修复内容：
    /// 1. R²计算：使用相关系数平方，只使用失效数据
    /// 2. 删尾数据：正确处理失效和删尾数据
    /// 3. MOD004问题：添加β上限约束(≤15)，调整初始值[0.5,4.0]
    /// 4. 多次尝试优化机制
    /// </summary>
    public class WeibullEngine : IWeibullEngine
    {
        public WeibullResult Analyze(double[] failureTimes, bool[]? isCensored = null, double confidenceLevel = 0.95)
        {
            if (failureTimes == null || failureTimes.Length < 3)
            {
                throw new ArgumentException("至少需要3个数据点才能进行威布尔分析", nameof(failureTimes));
            }

            isCensored ??= new bool[failureTimes.Length];

            var (beta, eta) = EstimateParametersMLE(failureTimes, isCensored);

            var mttf = CalculateMTTF(beta, eta);
            var medianLife = CalculateMedianLife(beta, eta);
            var b10 = CalculateBxLife(0.10, beta, eta);
            var b50 = CalculateBxLife(0.50, beta, eta);
            var b90 = CalculateBxLife(0.90, beta, eta);

            var rSquared = CalculateRSquared(failureTimes, isCensored, beta, eta);

            var (betaLower, betaUpper, etaLower, etaUpper) = CalculateConfidenceIntervals(
                failureTimes, beta, eta, confidenceLevel);

            return new WeibullResult
            {
                Beta = beta,
                Eta = eta,
                Gamma = 0,
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

        // ===== 核心修复：MLE参数估计 =====
        private (double beta, double eta) EstimateParametersMLE(double[] failureTimes, bool[] isCensored)
        {
            var initialGuess = EstimateParametersRankRegression(failureTimes, isCensored);

            Func<Vector<double>, double> negativeLogLikelihood = (parameters) =>
            {
                double beta = parameters[0];
                double eta = parameters[1];

                // ✅ 修复1：添加β上限约束≤15，与Mathematica一致
                if (beta <= 0.2 || beta > 15.0 || eta <= 0)
                    return double.MaxValue;

                double logL = 0;
                for (int i = 0; i < failureTimes.Length; i++)
                {
                    double t = failureTimes[i];
                    if (!isCensored[i])
                    {
                        logL += Math.Log(beta) - beta * Math.Log(eta) + (beta - 1) * Math.Log(t) - Math.Pow(t / eta, beta);
                    }
                    else
                    {
                        logL += -Math.Pow(t / eta, beta);
                    }
                }
                return -logL;
            };

            // ✅ 修复2：多次尝试优化
            double bestBeta = initialGuess.beta;
            double bestEta = initialGuess.eta;
            double bestLogL = double.MaxValue;

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                double currentBeta = initialGuess.beta;
                double currentEta = initialGuess.eta;

                if (attempt == 2)
                {
                    currentBeta = Math.Min(initialGuess.beta * 1.5, 4.0);
                    currentEta = initialGuess.eta * 1.1;
                }
                else if (attempt == 3)
                {
                    currentBeta = Math.Max(initialGuess.beta * 0.7, 0.5);
                    currentEta = initialGuess.eta * 0.9;
                }

                try
                {
                    var optimizer = new NelderMeadSimplex(1e-8, 1000);
                    var initialVector = Vector<double>.Build.Dense(new[] { currentBeta, currentEta });
                    
                    var result = optimizer.FindMinimum(
                        ObjectiveFunction.Value(negativeLogLikelihood),
                        initialVector
                    );

                    double beta = result.MinimizingPoint[0];
                    double eta = result.MinimizingPoint[1];
                    double logL = result.FunctionInfoAtMinimum.Value;

                    if (beta >= 0.2 && beta <= 15.0 && eta > 0 && logL < bestLogL)
                    {
                        bestBeta = beta;
                        bestEta = eta;
                        bestLogL = logL;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return (bestBeta, bestEta);
        }

        // ===== 修复：秩回归初始估计 =====
        private (double beta, double eta) EstimateParametersRankRegression(double[] failureTimes, bool[] isCensored)
        {
            var completeData = failureTimes
                .Where((t, i) => !isCensored[i])
                .OrderBy(t => t)
                .ToArray();

            if (completeData.Length < 2)
            {
                return (2.0, failureTimes.Average());
            }

            int n = completeData.Length;
            double[] x = new double[n];
            double[] y = new double[n];

            for (int i = 0; i < n; i++)
            {
                double rank = i + 1;
                double F = (rank - 0.3) / (n + 0.4);
                
                x[i] = Math.Log(completeData[i]);
                y[i] = Math.Log(-Math.Log(1 - F));
            }

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

            // ✅ 修复3：范围改为[0.5, 4.0]
            beta = Math.Max(0.5, Math.Min(beta, 4.0));
            eta = Math.Max(completeData.Min() * 0.5, Math.Min(eta, completeData.Max() * 2.0));

            return (beta, eta);
        }

        private double CalculateMTTF(double beta, double eta)
        {
            return eta * Gamma(1.0 + 1.0 / beta);
        }

        private double CalculateMedianLife(double beta, double eta)
        {
            return eta * Math.Pow(Math.Log(2), 1.0 / beta);
        }

        private double CalculateBxLife(double x, double beta, double eta)
        {
            return eta * Math.Pow(-Math.Log(1 - x), 1.0 / beta);
        }

        // ===== 修复：R²计算 =====
        private double CalculateRSquared(double[] failureTimes, bool[] isCensored, double beta, double eta)
        {
            var failureData = failureTimes
                .Where((t, i) => !isCensored[i])
                .OrderBy(t => t)
                .ToArray();

            int n = failureData.Length;
            if (n < 2) return 0.0;

            double[] x = new double[n];
            double[] y = new double[n];

            for (int i = 0; i < n; i++)
            {
                double rank = i + 1;
                double F = (rank - 0.3) / (n + 0.4);
                x[i] = Math.Log(failureData[i]);
                y[i] = Math.Log(-Math.Log(1 - F));
            }

            double correlationCoefficient = Correlation.Pearson(x, y);
            double rSquared = correlationCoefficient * correlationCoefficient;

            return double.IsNaN(rSquared) || double.IsInfinity(rSquared) ? 0.0 : rSquared;
        }

        private (double betaLower, double betaUpper, double etaLower, double etaUpper) 
            CalculateConfidenceIntervals(double[] failureTimes, double beta, double eta, double confidenceLevel)
        {
            int n = failureTimes.Length;
            double varBeta = 1.109 * beta * beta / n;
            double varEta = 0.608 * eta * eta / n;
            double z = GetZValue(confidenceLevel);

            double betaLower = Math.Max(0.1, beta - z * Math.Sqrt(varBeta));
            double betaUpper = beta + z * Math.Sqrt(varBeta);
            double etaLower = Math.Max(failureTimes.Min() * 0.1, eta - z * Math.Sqrt(varEta));
            double etaUpper = eta + z * Math.Sqrt(varEta);

            return (betaLower, betaUpper, etaLower, etaUpper);
        }

        private double GetZValue(double confidenceLevel)
        {
            return confidenceLevel switch
            {
                0.90 => 1.645,
                0.95 => 1.960,
                0.99 => 2.576,
                _ => 1.960
            };
        }

        public double CalculateReliability(double time, double beta, double eta)
        {
            if (time < 0) throw new ArgumentException("时间不能为负", nameof(time));
            return Math.Exp(-Math.Pow(time / eta, beta));
        }

        public double CalculateHazardRate(double time, double beta, double eta)
        {
            if (time < 0) throw new ArgumentException("时间不能为负", nameof(time));
            return (beta / eta) * Math.Pow(time / eta, beta - 1);
        }

        public double CalculatePDF(double time, double beta, double eta)
        {
            if (time < 0) throw new ArgumentException("时间不能为负", nameof(time));
            return (beta / eta) * Math.Pow(time / eta, beta - 1) * Math.Exp(-Math.Pow(time / eta, beta));
        }

        public List<(double time, double probability)> GenerateProbabilityPlotData(double[] failureTimes, double beta, double eta)
        {
            var sorted = failureTimes.OrderBy(t => t).ToArray();
            var plotData = new List<(double, double)>();

            for (int i = 0; i < sorted.Length; i++)
            {
                double rank = i + 1;
                double F = (rank - 0.3) / (sorted.Length + 0.4);
                plotData.Add((sorted[i], F * 100));
            }

            return plotData;
        }
    }

    public class WeibullResult
    {
        public double Beta { get; set; }
        public double Eta { get; set; }
        public double Gamma { get; set; }
        public double MTTF { get; set; }
        public double MedianLife { get; set; }
        public double B10Life { get; set; }
        public double B50Life { get; set; }
        public double B90Life { get; set; }
        public double RSquared { get; set; }
        public double ConfidenceLevel { get; set; }
        public double BetaLower { get; set; }
        public double BetaUpper { get; set; }
        public double EtaLower { get; set; }
        public double EtaUpper { get; set; }
        public int SampleSize { get; set; }
        public int FailureCount { get; set; }
    }
}