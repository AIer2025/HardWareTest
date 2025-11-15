using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;

namespace LabTestPlatform.Services
{
    /// <summary>
    /// 威布尔分析结果
    /// </summary>
    public class WeibullAnalysisResult
    {
        public double ShapeParameter { get; set; }  // β (beta)
        public double ScaleParameter { get; set; }  // η (eta)
        public double LocationParameter { get; set; }  // γ (gamma)
        public double MTBF { get; set; }
        public double RSquared { get; set; }
        public string EstimationMethod { get; set; } = "MLE";
        public int DataCount { get; set; }
        public Dictionary<double, double> ReliabilityFunction { get; set; } = new();
        public Dictionary<double, double> HazardFunction { get; set; } = new();
        public Dictionary<double, double> PDFFunction { get; set; } = new();
        public double B10Life { get; set; }
        public double B50Life { get; set; }
        public double ReliabilityAt1000h { get; set; }
        public double ReliabilityAt5000h { get; set; }
        public double ReliabilityAt10000h { get; set; }
    }

    /// <summary>
    /// 威布尔分析引擎
    /// 实现两参数和三参数威布尔分布分析
    /// </summary>
    public class WeibullAnalysisEngine
    {
        /// <summary>
        /// 执行威布尔分析 (两参数)
        /// </summary>
        public WeibullAnalysisResult AnalyzeTwoParameter(double[] data, string method = "MLE")
        {
            if (data == null || data.Length < 3)
                throw new ArgumentException("数据点数量不足,至少需要3个数据点");

            // 排序数据
            var sortedData = data.OrderBy(x => x).ToArray();
            
            WeibullAnalysisResult result;
            
            if (method.ToUpper() == "MLE")
            {
                result = EstimateParametersMLE(sortedData);
            }
            else // LSM (Least Squares Method)
            {
                result = EstimateParametersLSM(sortedData);
            }

            result.EstimationMethod = method.ToUpper();
            result.DataCount = data.Length;
            
            // 计算MTBF
            result.MTBF = CalculateMTBF(result.ShapeParameter, result.ScaleParameter);
            
            // 计算B寿命
            result.B10Life = CalculateBLife(0.10, result.ShapeParameter, result.ScaleParameter);
            result.B50Life = CalculateBLife(0.50, result.ShapeParameter, result.ScaleParameter);
            
            // 计算特定时间点的可靠度
            result.ReliabilityAt1000h = CalculateReliability(1000, result.ShapeParameter, result.ScaleParameter);
            result.ReliabilityAt5000h = CalculateReliability(5000, result.ShapeParameter, result.ScaleParameter);
            result.ReliabilityAt10000h = CalculateReliability(10000, result.ShapeParameter, result.ScaleParameter);
            
            // 生成函数曲线数据
            GenerateFunctionData(result, sortedData);
            
            return result;
        }

        /// <summary>
        /// 最大似然估计 (MLE)
        /// </summary>
        private WeibullAnalysisResult EstimateParametersMLE(double[] sortedData)
        {
            var result = new WeibullAnalysisResult();
            
            // 初始估计值使用线性回归方法
            var lsmResult = EstimateParametersLSM(sortedData);
            double betaInit = lsmResult.ShapeParameter;
            double etaInit = lsmResult.ScaleParameter;
            
            // 使用牛顿法优化
            int maxIterations = 1000;
            double tolerance = 1e-6;
            double beta = betaInit;
            double eta = etaInit;
            
            for (int iter = 0; iter < maxIterations; iter++)
            {
                double sumLogT = sortedData.Sum(t => Math.Log(t));
                double sumTPowBeta = sortedData.Sum(t => Math.Pow(t, beta));
                double sumTPowBetaLogT = sortedData.Sum(t => Math.Pow(t, beta) * Math.Log(t));
                double n = sortedData.Length;
                
                // 更新beta
                double f_beta = n / beta + sumLogT - n * sumTPowBetaLogT / sumTPowBeta;
                double df_beta = -n / (beta * beta) - n * (sumTPowBetaLogT * sumTPowBetaLogT - sumTPowBeta * sortedData.Sum(t => Math.Pow(t, beta) * Math.Log(t) * Math.Log(t))) / (sumTPowBeta * sumTPowBeta);
                
                double beta_new = beta - f_beta / df_beta;
                
                // 确保beta为正
                if (beta_new <= 0) beta_new = beta / 2;
                
                // 更新eta
                eta = Math.Pow(sumTPowBeta / n, 1.0 / beta_new);
                
                // 检查收敛
                if (Math.Abs(beta_new - beta) < tolerance)
                {
                    beta = beta_new;
                    break;
                }
                
                beta = beta_new;
            }
            
            result.ShapeParameter = beta;
            result.ScaleParameter = eta;
            result.LocationParameter = 0;
            
            // 计算R²
            result.RSquared = CalculateRSquared(sortedData, beta, eta);
            
            return result;
        }

        /// <summary>
        /// 最小二乘法估计 (LSM) - 使用线性回归
        /// </summary>
        private WeibullAnalysisResult EstimateParametersLSM(double[] sortedData)
        {
            var result = new WeibullAnalysisResult();
            int n = sortedData.Length;
            
            // 计算中位秩
            double[] medianRanks = new double[n];
            for (int i = 0; i < n; i++)
            {
                medianRanks[i] = (i + 1 - 0.3) / (n + 0.4);
            }
            
            // 准备线性回归数据
            // ln(ln(1/(1-F))) = β * ln(t) - β * ln(η)
            // Y = β * X - β * ln(η)
            
            double[] X = new double[n];
            double[] Y = new double[n];
            
            for (int i = 0; i < n; i++)
            {
                X[i] = Math.Log(sortedData[i]);
                Y[i] = Math.Log(-Math.Log(1 - medianRanks[i]));
            }
            
            // 线性回归
            var (slope, intercept, rSquared) = LinearRegression(X, Y);
            
            double beta = slope;
            double eta = Math.Exp(-intercept / slope);
            
            result.ShapeParameter = beta;
            result.ScaleParameter = eta;
            result.LocationParameter = 0;
            result.RSquared = rSquared;
            
            return result;
        }

        /// <summary>
        /// 线性回归
        /// </summary>
        private (double slope, double intercept, double rSquared) LinearRegression(double[] x, double[] y)
        {
            int n = x.Length;
            double sumX = x.Sum();
            double sumY = y.Sum();
            double sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
            double sumX2 = x.Sum(xi => xi * xi);
            double sumY2 = y.Sum(yi => yi * yi);
            
            double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double intercept = (sumY - slope * sumX) / n;
            
            // 计算R²
            double meanY = sumY / n;
            double ssTotal = y.Sum(yi => Math.Pow(yi - meanY, 2));
            double ssResidual = y.Zip(x, (yi, xi) => Math.Pow(yi - (slope * xi + intercept), 2)).Sum();
            double rSquared = 1 - (ssResidual / ssTotal);
            
            return (slope, intercept, rSquared);
        }

        /// <summary>
        /// 计算可靠度函数 R(t)
        /// </summary>
        public double CalculateReliability(double t, double beta, double eta)
        {
            if (t <= 0) return 1.0;
            return Math.Exp(-Math.Pow(t / eta, beta));
        }

        /// <summary>
        /// 计算失效概率密度函数 f(t)
        /// </summary>
        public double CalculatePDF(double t, double beta, double eta)
        {
            if (t <= 0) return 0;
            return (beta / eta) * Math.Pow(t / eta, beta - 1) * Math.Exp(-Math.Pow(t / eta, beta));
        }

        /// <summary>
        /// 计算失效率函数 λ(t)
        /// </summary>
        public double CalculateHazardRate(double t, double beta, double eta)
        {
            if (t <= 0) return 0;
            return (beta / eta) * Math.Pow(t / eta, beta - 1);
        }

        /// <summary>
        /// 计算MTBF (平均无故障时间)
        /// </summary>
        public double CalculateMTBF(double beta, double eta)
        {
            // MTBF = η * Γ(1 + 1/β)
            return eta * SpecialFunctions.Gamma(1 + 1 / beta);
        }

        /// <summary>
        /// 计算B寿命 (如B10, B50)
        /// </summary>
        public double CalculateBLife(double failureRate, double beta, double eta)
        {
            // B_x = η * (-ln(1-x))^(1/β)
            return eta * Math.Pow(-Math.Log(1 - failureRate), 1 / beta);
        }

        /// <summary>
        /// 计算R²拟合优度
        /// </summary>
        private double CalculateRSquared(double[] data, double beta, double eta)
        {
            int n = data.Length;
            
            // 计算中位秩
            double[] empiricalF = new double[n];
            for (int i = 0; i < n; i++)
            {
                empiricalF[i] = (i + 1 - 0.3) / (n + 0.4);
            }
            
            // 理论累积失效概率
            double[] theoreticalF = data.Select(t => 1 - CalculateReliability(t, beta, eta)).ToArray();
            
            // 计算R²
            double meanF = empiricalF.Average();
            double ssTotal = empiricalF.Sum(f => Math.Pow(f - meanF, 2));
            double ssResidual = empiricalF.Zip(theoreticalF, (e, t) => Math.Pow(e - t, 2)).Sum();
            
            return 1 - (ssResidual / ssTotal);
        }

        /// <summary>
        /// 生成函数曲线数据
        /// </summary>
        private void GenerateFunctionData(WeibullAnalysisResult result, double[] sortedData)
        {
            double maxTime = sortedData.Max() * 1.5;
            int points = 100;
            double step = maxTime / points;
            
            for (int i = 0; i <= points; i++)
            {
                double t = i * step;
                if (t == 0) t = 0.01; // 避免0
                
                result.ReliabilityFunction[t] = CalculateReliability(t, result.ShapeParameter, result.ScaleParameter);
                result.HazardFunction[t] = CalculateHazardRate(t, result.ShapeParameter, result.ScaleParameter);
                result.PDFFunction[t] = CalculatePDF(t, result.ShapeParameter, result.ScaleParameter);
            }
        }
    }
}
