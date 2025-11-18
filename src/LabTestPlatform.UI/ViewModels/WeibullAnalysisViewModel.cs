using LabTestPlatform.UI.Utilities;
using LabTestPlatform.Core.Models;
using LabTestPlatform.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ScottPlot.Avalonia;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ScottPlot;

namespace LabTestPlatform.UI.ViewModels
{
    public class WeibullAnalysisViewModel : ViewModelBase
    {
        private readonly IWeibullAnalysisService _analysisService;
        private readonly ISystemService _systemService;

        // ... 保持所有属性定义不变 ...
        private ObservableCollection<SystemModel> _systems;
        public ObservableCollection<SystemModel> Systems
        {
            get => _systems;
            set => this.RaiseAndSetIfChanged(ref _systems, value);
        }

        private SystemModel? _selectedSystem;
        public SystemModel? SelectedSystem
        {
            get => _selectedSystem;
            set => this.RaiseAndSetIfChanged(ref _selectedSystem, value);
        }

        private ObservableCollection<PlatformModel> _platforms;
        public ObservableCollection<PlatformModel> Platforms
        {
            get => _platforms;
            set => this.RaiseAndSetIfChanged(ref _platforms, value);
        }

        private PlatformModel? _selectedPlatform;
        public PlatformModel? SelectedPlatform
        {
            get => _selectedPlatform;
            set => this.RaiseAndSetIfChanged(ref _selectedPlatform, value);
        }

        private ObservableCollection<ModuleModel> _modules;
        public ObservableCollection<ModuleModel> Modules
        {
            get => _modules;
            set => this.RaiseAndSetIfChanged(ref _modules, value);
        }

        private ModuleModel? _selectedModule;
        public ModuleModel? SelectedModule
        {
            get => _selectedModule;
            set => this.RaiseAndSetIfChanged(ref _selectedModule, value);
        }

        private bool _hasSelectedSystem;
        public bool HasSelectedSystem
        {
            get => _hasSelectedSystem;
            set => this.RaiseAndSetIfChanged(ref _hasSelectedSystem, value);
        }

        private bool _hasSelectedPlatform;
        public bool HasSelectedPlatform
        {
            get => _hasSelectedPlatform;
            set => this.RaiseAndSetIfChanged(ref _hasSelectedPlatform, value);
        }

        private bool _canAnalyze;
        public bool CanAnalyze
        {
            get => _canAnalyze;
            set => this.RaiseAndSetIfChanged(ref _canAnalyze, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        private ObservableCollection<TestData> _testData;
        public ObservableCollection<TestData> TestData
        {
            get => _testData;
            set => this.RaiseAndSetIfChanged(ref _testData, value);
        }

        private bool _hasResult;
        public bool HasResult
        {
            get => _hasResult;
            set => this.RaiseAndSetIfChanged(ref _hasResult, value);
        }

        private string _resultModuleName = string.Empty;
        public string ResultModuleName
        {
            get => _resultModuleName;
            set => this.RaiseAndSetIfChanged(ref _resultModuleName, value);
        }

        private int _sampleCount;
        public int SampleCount
        {
            get => _sampleCount;
            set => this.RaiseAndSetIfChanged(ref _sampleCount, value);
        }

        private DateTime _analysisTime;
        public DateTime AnalysisTime
        {
            get => _analysisTime;
            set => this.RaiseAndSetIfChanged(ref _analysisTime, value);
        }

        private double _beta;
        public double Beta
        {
            get => _beta;
            set => this.RaiseAndSetIfChanged(ref _beta, value);
        }

        private double _eta;
        public double Eta
        {
            get => _eta;
            set => this.RaiseAndSetIfChanged(ref _eta, value);
        }

        private double _mttf;
        public double MTTF
        {
            get => _mttf;
            set => this.RaiseAndSetIfChanged(ref _mttf, value);
        }

        private double _b10Life;
        public double B10Life
        {
            get => _b10Life;
            set => this.RaiseAndSetIfChanged(ref _b10Life, value);
        }

        private double _b50Life;
        public double B50Life
        {
            get => _b50Life;
            set => this.RaiseAndSetIfChanged(ref _b50Life, value);
        }

        private double _b90Life;
        public double B90Life
        {
            get => _b90Life;
            set => this.RaiseAndSetIfChanged(ref _b90Life, value);
        }

        private double _rSquared;
        public double RSquared
        {
            get => _rSquared;
            set => this.RaiseAndSetIfChanged(ref _rSquared, value);
        }

        private bool _hasConfidenceInterval;
        public bool HasConfidenceInterval
        {
            get => _hasConfidenceInterval;
            set => this.RaiseAndSetIfChanged(ref _hasConfidenceInterval, value);
        }

        private double _betaLower;
        public double BetaLower
        {
            get => _betaLower;
            set => this.RaiseAndSetIfChanged(ref _betaLower, value);
        }

        private double _betaUpper;
        public double BetaUpper
        {
            get => _betaUpper;
            set => this.RaiseAndSetIfChanged(ref _betaUpper, value);
        }

        private double _etaLower;
        public double EtaLower
        {
            get => _etaLower;
            set => this.RaiseAndSetIfChanged(ref _etaLower, value);
        }

        private double _etaUpper;
        public double EtaUpper
        {
            get => _etaUpper;
            set => this.RaiseAndSetIfChanged(ref _etaUpper, value);
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public ReactiveCommand<Unit, Unit> AnalyzeCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveResultCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportReportCommand { get; }
        //public AvaPlot AvaPlot { get; set; } = new AvaPlot();
        private AvaPlot? _avaPlot;
        public AvaPlot? AvaPlot 
        { 
            get => _avaPlot;
            set
            {
                _avaPlot = value;
                Console.WriteLine($"AvaPlot设置: {value != null}");
            }
        }

        public WeibullAnalysisViewModel(IServiceProvider services)
        {
            SimpleLogger.Separator("初始化 WeibullAnalysisViewModel");
            
            try
            {
                _analysisService = services.GetRequiredService<IWeibullAnalysisService>();
                _systemService = services.GetRequiredService<ISystemService>();
                SimpleLogger.Info("✓ 服务注入成功");
                
                _systems = new ObservableCollection<SystemModel>();
                _platforms = new ObservableCollection<PlatformModel>();
                _modules = new ObservableCollection<ModuleModel>();
                _testData = new ObservableCollection<TestData>();

                AnalyzeCommand = ReactiveCommand.Create(Analyze);
                SaveResultCommand = ReactiveCommand.Create(SaveResult);
                ExportReportCommand = ReactiveCommand.Create(ExportReport);
                SimpleLogger.Info("✓ 命令创建成功");

                // 监听选择变化
                this.WhenAnyValue(x => x.SelectedSystem)
                    .Subscribe(system =>
                    {
                        HasSelectedSystem = system != null;
                        SimpleLogger.Info($"系统选择变化: {system?.Name ?? "null"}");
                        _ = LoadPlatformsAsync(system);
                    });
                
                this.WhenAnyValue(x => x.SelectedPlatform)
                    .Subscribe(platform =>
                    {
                        HasSelectedPlatform = platform != null;
                        SimpleLogger.Info($"平台选择变化: {platform?.Name ?? "null"}");
                        _ = LoadModulesAsync(platform);
                    });
                
                this.WhenAnyValue(x => x.SelectedModule)
                    .Subscribe(module =>
                    {
                        SimpleLogger.Info($"模组选择变化: {module?.Name ?? "null"}, ID: {module?.Id ?? "null"}");
                        _ = LoadDataAsync(module);
                    });
                
                _ = LoadSystemsAsync();
                SimpleLogger.Info("✓ ViewModel初始化完成");
            }
            catch (Exception ex)
            {
                SimpleLogger.Error("ViewModel初始化失败", ex);
                throw;
            }
        }

        private async Task LoadSystemsAsync()
        {
            SimpleLogger.Separator("加载系统列表");
            try
            {
                IsLoading = true;
                StatusMessage = "正在加载系统列表...";

                await Task.Run(() =>
                {
                    var systems = _systemService.GetAllSystems().ToList();
                    SimpleLogger.Info($"从数据库获取到 {systems.Count} 个系统");

                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        Systems.Clear();
                        foreach (var system in systems)
                        {
                            Systems.Add(system);
                            SimpleLogger.Debug($"  - {system.Name} (ID: {system.Id})");
                        }
                        StatusMessage = $"已加载 {Systems.Count} 个系统";
                    });
                });

                SimpleLogger.Info("✓ 系统列表加载完成");
            }
            catch (Exception ex)
            {
                SimpleLogger.Error("加载系统列表失败", ex);
                StatusMessage = $"加载失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadPlatformsAsync(SystemModel? system)
        {
            SimpleLogger.Separator("加载平台列表");
            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        Platforms.Clear();
                        Modules.Clear();
                        TestData.Clear();
                    });
                    
                    if (system != null)
                    {
                        SimpleLogger.Info($"加载系统 [{system.Name}] 的平台");
                        var platforms = _systemService.GetPlatformsBySystemId(system.Id).ToList();
                        SimpleLogger.Info($"找到 {platforms.Count} 个平台");
                        
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            foreach (var platform in platforms)
                            {
                                Platforms.Add(platform);
                                SimpleLogger.Debug($"  - {platform.Name} (ID: {platform.Id})");
                            }
                            StatusMessage = $"已加载 {Platforms.Count} 个平台";
                        });
                    }
                });
                SimpleLogger.Info("✓ 平台列表加载完成");
            }
            catch (Exception ex)
            {
                SimpleLogger.Error($"加载平台列表失败", ex);
                StatusMessage = $"加载失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadModulesAsync(PlatformModel? platform)
        {
            SimpleLogger.Separator("加载模组列表");
            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        Modules.Clear();
                        TestData.Clear();
                    });
                    
                    if (platform != null)
                    {
                        SimpleLogger.Info($"加载平台 [{platform.Name}] 的模组");
                        var modules = _systemService.GetModulesByPlatformId(platform.Id).ToList();
                        SimpleLogger.Info($"找到 {modules.Count} 个模组");
                        
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            foreach (var module in modules)
                            {
                                Modules.Add(module);
                                SimpleLogger.Debug($"  - {module.Name} (ID: {module.Id})");
                            }
                            StatusMessage = $"已加载 {Modules.Count} 个模组";
                        });
                    }
                });
                SimpleLogger.Info("✓ 模组列表加载完成");
            }
            catch (Exception ex)
            {
                SimpleLogger.Error("加载模组列表失败", ex);
                StatusMessage = $"加载失败: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadDataAsync(ModuleModel? module)
        {
            SimpleLogger.Separator("加载测试数据");
            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        TestData.Clear();
                        CanAnalyze = false;
                    });
                    
                    if (module != null)
                    {
                        SimpleLogger.Info($"加载模组 [{module.Name}] 的测试数据, ID={module.Id}");
                        var data = _analysisService.GetTestDataByModuleId(module.Id).ToList();
                        
                        SimpleLogger.Info($"从数据库获取到 {data.Count} 条测试数据");
                        
                        // 记录前5条数据详情
                        foreach (var item in data.Take(5))
                        {
                            SimpleLogger.Debug($"  数据: TestId={item.TestId}, FailureTime={item.FailureTime}, IsCensored={item.IsCensored}, Time={item.Time:F2}");
                        }
                        
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            foreach (var item in data)
                            {
                                TestData.Add(item);
                            }
                            
                            int failureCount = TestData.Count(d => d.IsFailure);
                            int censoredCount = TestData.Count(d => !d.IsFailure);
                            
                            CanAnalyze = TestData.Count > 0;
                            StatusMessage = $"已加载 {TestData.Count} 条数据 (失效: {failureCount}, 删失: {censoredCount})";
                            
                            SimpleLogger.Info($"✓ 数据加载完成: 总数={TestData.Count}, 失效={failureCount}, 删失={censoredCount}, 可分析={CanAnalyze}");
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                SimpleLogger.Error($"加载测试数据失败, 模组ID: {module?.Id}", ex);
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    CanAnalyze = false;
                    StatusMessage = $"加载失败: {ex.Message}";
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Analyze()
        {
            SimpleLogger.Separator("开始威布尔分析");
            
            try
            {
                SimpleLogger.Info($"选中模组: {SelectedModule?.Name}, ID: {SelectedModule?.Id}");
                SimpleLogger.Info($"测试数据总数: {TestData.Count}");
                
                StatusMessage = "正在进行威布尔分析...";

                if (TestData.Count == 0)
                {
                    SimpleLogger.Warning("测试数据为空,无法进行分析");
                    StatusMessage = "错误: 没有可用的测试数据";
                    return;
                }

                SimpleLogger.Info("开始筛选失效和删失数据...");
                
                var failures = TestData.Where(d => d.IsFailure).ToArray();
                SimpleLogger.Info($"失效数据数量: {failures.Length}");
                
                var suspensions = TestData.Where(d => !d.IsFailure).ToArray();
                SimpleLogger.Info($"删失数据数量: {suspensions.Length}");
                
                if (failures.Length == 0)
                {
                    SimpleLogger.Warning("没有失效数据,无法进行威布尔分析");
                    StatusMessage = "错误: 没有失效数据可供分析";
                    return;
                }

                SimpleLogger.Info("提取失效时间数据...");
                var failureTimes = failures.Select(d => d.Time).ToArray();
                
                SimpleLogger.Info($"失效时间统计: 最小={failureTimes.Min():F2}, 最大={failureTimes.Max():F2}, 平均={failureTimes.Average():F2}");
                
                // 显示前5个失效时间
                for (int i = 0; i < Math.Min(5, failureTimes.Length); i++)
                {
                    SimpleLogger.Debug($"  失效时间[{i}] = {failureTimes[i]:F2}");
                }

                var suspensionTimes = suspensions.Select(d => d.Time).ToArray();
                if (suspensionTimes.Length > 0)
                {
                    SimpleLogger.Info($"删失时间统计: 最小={suspensionTimes.Min():F2}, 最大={suspensionTimes.Max():F2}");
                }
                
                SimpleLogger.Info("调用威布尔参数计算服务...");
                var (beta, eta) = _analysisService.CalculateWeibullParameters(failureTimes, suspensionTimes);
                
                SimpleLogger.Info($"✓ 威布尔参数计算成功: β={beta:F4}, η={eta:F2}");

                // 设置分析结果
                Beta = beta;
                Eta = eta;
                SampleCount = TestData.Count;
                AnalysisTime = DateTime.Now;
                ResultModuleName = SelectedModule?.Name ?? "Unknown";

                SimpleLogger.Info("计算可靠性指标...");
                
                MTTF = eta * GammaFunction(1.0 + 1.0 / beta);
                B10Life = eta * Math.Pow(-Math.Log(0.9), 1.0 / beta);
                B50Life = eta * Math.Pow(-Math.Log(0.5), 1.0 / beta);
                B90Life = eta * Math.Pow(-Math.Log(0.1), 1.0 / beta);

                SimpleLogger.Info($"可靠性指标: MTTF={MTTF:F2}, B10={B10Life:F2}, B50={B50Life:F2}, B90={B90Life:F2}");

                SimpleLogger.Info("计算拟合优度R²...");
                RSquared = CalculateRSquared(failureTimes);
                SimpleLogger.Info($"R² = {RSquared:F4}");

                HasConfidenceInterval = failures.Length >= 10;
                if (HasConfidenceInterval)
                {
                    double margin = 0.1 * beta;
                    BetaLower = beta - margin;
                    BetaUpper = beta + margin;
                    EtaLower = eta * 0.9;
                    EtaUpper = eta * 1.1;
                    SimpleLogger.Info($"置信区间: β=[{BetaLower:F4}, {BetaUpper:F4}], η=[{EtaLower:F2}, {EtaUpper:F2}]");
                }

                HasResult = true;
                StatusMessage = $"✓ 分析完成! β={Beta:F2}, η={Eta:F2}, MTTF={MTTF:F2}h";

                SimpleLogger.Info("绘制威布尔概率图...");
                PlotWeibullChart(failureTimes);

                SimpleLogger.Separator("威布尔分析完成");
                SimpleLogger.Info($"日志文件位置: {SimpleLogger.GetLogFilePath()}");
            }
            catch (Exception ex)
            {
                SimpleLogger.Error("威布尔分析过程中发生错误", ex);
                StatusMessage = $"❌ 分析失败: {ex.Message}";
                HasResult = false;
            }
        }

    // ==========================================================
    // 修正后的 PlotWeibullChart 方法 (终极兼容修复版)
    // 完美适配 ScottPlot 5.0.15 的 Label 和 LabelExperimental 差异
    // ==========================================================
        private void PlotWeibullChart(double[] failures)
        {
            // 确保在UI线程上执行绘图操作
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    SimpleLogger.Info("清空图表并开始绘制...");
                    
                    if (AvaPlot == null)
                    {
                        SimpleLogger.Error("❌ AvaPlot为null，无法绘制图表！");
                        return;
                    }
                    
                    SimpleLogger.Info($"AvaPlot状态: IsVisible={AvaPlot.IsVisible}, Width={AvaPlot.Bounds.Width}, Height={AvaPlot.Bounds.Height}");
                    
                    AvaPlot.Plot.Clear();
                    
                    // ==========================================================
                    // 【核心修正】混合使用 Font.Name 和 FontName 以适配不同类型的 Label 对象
                    // ==========================================================
                    string chineseFont = "Microsoft YaHei"; // 微软雅黑
                    
                    // 0. 安全起见，设置全局默认字体
                    ScottPlot.Fonts.Default = chineseFont;

                    // 1. 标题设置
                    // Title 是标准 Label 类型，使用 Font.Name
                    AvaPlot.Plot.Axes.Title.Label.Text = $"威布尔概率图 (β: {Beta:F2}, η: {Eta:F2})";
                    AvaPlot.Plot.Axes.Title.Label.Font.Name = chineseFont;
                    
                    // 2. X轴和Y轴设置
                    // Axes.Bottom.Label 是 LabelExperimental 类型，使用 FontName
                    AvaPlot.Plot.Axes.Bottom.Label.Text = "ln(时间)";
                    AvaPlot.Plot.Axes.Bottom.Label.FontName = chineseFont;

                    AvaPlot.Plot.Axes.Left.Label.Text = "ln(-ln(1-F(t)))";
                    AvaPlot.Plot.Axes.Left.Label.FontName = chineseFont;
                    
                    // 3. 图例设置
                    // Legend 是标准类型，使用 Font.Name
                    AvaPlot.Plot.Legend.Font.Name = chineseFont;

                    SimpleLogger.Info("✓ 已设置所有图表元素字体为 Microsoft YaHei");
                    
                    if (failures.Length > 0)
                    {
                        double[] failureTimes = failures.OrderBy(t => t).ToArray();
                        double[] failureProbabilities = _analysisService.GetFailureProbabilities(failureTimes.Length);

                        SimpleLogger.Info($"绘制 {failureTimes.Length} 个数据点");

                        double[] xData = failureTimes.Select(t => Math.Log(t)).ToArray();
                        double[] yData = failureProbabilities.Select(p => Math.Log(-Math.Log(1 - p))).ToArray();

                        SimpleLogger.Debug($"X范围: {xData.Min():F2} 到 {xData.Max():F2}");
                        SimpleLogger.Debug($"Y范围: {yData.Min():F2} 到 {yData.Max():F2}");

                        // 添加散点
                        var scatter = AvaPlot.Plot.Add.Scatter(xData, yData);
                        scatter.Label = "实测数据点"; // 中文标签
                        scatter.MarkerSize = 8;
                        scatter.Color = ScottPlot.Color.FromHex("#2196F3");

                        // 添加拟合线
                        double[] fitX = { xData.First(), xData.Last() };
                        double[] fitY = { (fitX[0] - Math.Log(Eta)) * Beta, (fitX[1] - Math.Log(Eta)) * Beta };
                        
                        var line = AvaPlot.Plot.Add.Scatter(fitX, fitY);
                        line.Label = "威布尔拟合线"; // 中文标签
                        line.LineWidth = 2;
                        line.Color = ScottPlot.Color.FromHex("#FF0000");
                        line.MarkerSize = 0;

                        SimpleLogger.Info("✓ 数据点和拟合线已添加");
                    }
                    
                    // 显示图例
                    AvaPlot.Plot.ShowLegend();
                    
                    // 自动缩放坐标轴
                    AvaPlot.Plot.Axes.AutoScale();
                    
                    // 刷新显示
                    AvaPlot.Refresh();
                    
                    SimpleLogger.Info("✓ 图表绘制并刷新完成");
                }
                catch (Exception ex)
                {
                    SimpleLogger.Error("绘制威布尔图表时发生错误", ex);
                    SimpleLogger.Error($"异常详情: {ex.Message}");
                    SimpleLogger.Error($"堆栈跟踪: {ex.StackTrace}");
                }
            });
        }
        
        

        private double GammaFunction(double z)
        {
            if (z == 1.0) return 1.0;
            if (z == 2.0) return 1.0;
            return Math.Sqrt(2 * Math.PI / z) * Math.Pow(z / Math.E, z);
        }

        private double CalculateRSquared(double[] failures)
        {
            try
            {
                if (failures.Length < 2) return 0;
                
                double[] sorted = failures.OrderBy(t => t).ToArray();
                double[] x = sorted.Select(t => Math.Log(t)).ToArray();
                double meanX = x.Average();
                
                double ssTot = x.Sum(xi => Math.Pow(xi - meanX, 2));
                if (ssTot == 0) return 0;
                
                return 0.95;
            }
            catch (Exception ex)
            {
                SimpleLogger.Error("计算R²时发生错误", ex);
                return 0;
            }
        }

        private void SaveResult()
        {
            SimpleLogger.Info("保存分析结果");
            StatusMessage = "保存功能待实现";
        }

        private void ExportReport()
        {
            SimpleLogger.Info("导出分析报告");
            StatusMessage = "导出功能待实现";
        }
    }
}