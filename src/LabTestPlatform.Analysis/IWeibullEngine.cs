using System.Collections.Generic;

namespace LabTestPlatform.Analysis
{
    /// <summary>
    /// 威布尔分析引擎接口
    /// </summary>
    public interface IWeibullEngine
    {
        /// <summary>
        /// 执行威布尔分析
        /// </summary>
        /// <param name="failureTimes">失效时间数据</param>
        /// <param name="isCensored">截尾标记（可选）</param>
        /// <param name="confidenceLevel">置信水平（默认95%）</param>
        /// <returns>威布尔分析结果</returns>
        WeibullResult Analyze(double[] failureTimes, bool[]? isCensored = null, double confidenceLevel = 0.95);

        /// <summary>
        /// 计算可靠度函数 R(t)
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="beta">形状参数</param>
        /// <param name="eta">尺度参数</param>
        /// <returns>可靠度值</returns>
        double CalculateReliability(double time, double beta, double eta);

        /// <summary>
        /// 计算失效率函数 h(t)
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="beta">形状参数</param>
        /// <param name="eta">尺度参数</param>
        /// <returns>失效率值</returns>
        double CalculateHazardRate(double time, double beta, double eta);

        /// <summary>
        /// 计算概率密度函数 f(t)
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="beta">形状参数</param>
        /// <param name="eta">尺度参数</param>
        /// <returns>概率密度值</returns>
        double CalculatePDF(double time, double beta, double eta);

        /// <summary>
        /// 生成威布尔概率图数据点
        /// </summary>
        /// <param name="failureTimes">失效时间数据</param>
        /// <param name="beta">形状参数</param>
        /// <param name="eta">尺度参数</param>
        /// <returns>概率图数据点列表</returns>
        List<(double time, double probability)> GenerateProbabilityPlotData(double[] failureTimes, double beta, double eta);
    }
}
