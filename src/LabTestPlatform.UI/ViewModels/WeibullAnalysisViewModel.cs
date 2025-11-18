using LabTestPlatform.Core.Models;
using LabTestPlatform.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ScottPlot.Avalonia;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ScottPlot;

namespace LabTestPlatform.UI.ViewModels
{
    public class WeibullAnalysisViewModel : ViewModelBase
    {
        private readonly IWeibullAnalysisService _analysisService;
        private readonly ISystemService _systemService;

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

        // 添加辅助属性用于控制UI状态
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

        // 分析结果属性
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

        public ReactiveCommand<Unit, Unit> AnalyzeCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveResultCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportReportCommand { get; }
        public AvaPlot AvaPlot { get; set; } = new AvaPlot();

        public WeibullAnalysisViewModel(IServiceProvider services)
        {
            _analysisService = services.GetRequiredService<IWeibullAnalysisService>();
            _systemService = services.GetRequiredService<ISystemService>();
            
            _systems = new ObservableCollection<SystemModel>();
            _platforms = new ObservableCollection<PlatformModel>();
            _modules = new ObservableCollection<ModuleModel>();
            _testData = new ObservableCollection<TestData>();

            AnalyzeCommand = ReactiveCommand.Create(Analyze);
            SaveResultCommand = ReactiveCommand.Create(SaveResult);
            ExportReportCommand = ReactiveCommand.Create(ExportReport);

            // 监听选择变化并更新数据 - 使用异步方法
            this.WhenAnyValue(x => x.SelectedSystem)
                .Subscribe(system =>
                {
                    HasSelectedSystem = system != null;
                    _ = LoadPlatformsAsync(system);
                });
            
            this.WhenAnyValue(x => x.SelectedPlatform)
                .Subscribe(platform =>
                {
                    HasSelectedPlatform = platform != null;
                    _ = LoadModulesAsync(platform);
                });
            
            this.WhenAnyValue(x => x.SelectedModule)
                .Subscribe(module => _ = LoadDataAsync(module));

            // ✅ 异步加载初始数据，不阻塞UI
            _ = LoadSystemsAsync();
        }

        /// <summary>
        /// 异步加载系统列表
        /// </summary>
        private async Task LoadSystemsAsync()
        {
            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    var systems = _systemService.GetAllSystems();
                    
                    // 在UI线程上更新集合
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        Systems.Clear();
                        foreach (var system in systems)
                        {
                            Systems.Add(system);
                        }
                        
                        Platforms.Clear();
                        Modules.Clear();
                        TestData.Clear();
                    });
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading systems: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 异步加载平台列表
        /// </summary>
        private async Task LoadPlatformsAsync(SystemModel? system)
        {
            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    // 先清空UI
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        Platforms.Clear();
                        Modules.Clear();
                        TestData.Clear();
                    });
                    
                    if (system != null)
                    {
                        var platforms = _systemService.GetPlatformsBySystemId(system.Id);
                        
                        // 在UI线程上更新集合
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            foreach (var platform in platforms)
                            {
                                Platforms.Add(platform);
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading platforms: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 异步加载模组列表
        /// </summary>
        private async Task LoadModulesAsync(PlatformModel? platform)
        {
            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    // 先清空UI
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        Modules.Clear();
                        TestData.Clear();
                    });
                    
                    if (platform != null)
                    {
                        var modules = _systemService.GetModulesByPlatformId(platform.Id);
                        
                        // 在UI线程上更新集合
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            foreach (var module in modules)
                            {
                                Modules.Add(module);
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading modules: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 异步加载测试数据
        /// </summary>
        private async Task LoadDataAsync(ModuleModel? module)
        {
            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    // 先清空UI
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        TestData.Clear();
                        CanAnalyze = false;
                    });
                    
                    if (module != null)
                    {
                        var data = _analysisService.GetTestDataByModuleId(module.Id);
                        
                        // 在UI线程上更新集合
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            foreach (var item in data)
                            {
                                TestData.Add(item);
                            }
                            CanAnalyze = TestData.Count > 0;
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading test data: {ex.Message}");
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    CanAnalyze = false;
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Analyze()
        {
            if (TestData.Count == 0) return;

            // 需要确保TestData有IsFailure和Time属性
            var failures = TestData.Where(d => d.IsFailure).Select(d => d.Time).ToArray();
            var suspensions = TestData.Where(d => !d.IsFailure).Select(d => d.Time).ToArray();
            
            if (failures.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("No failure data available for analysis");
                return;
            }
            
            var (beta, eta) = _analysisService.CalculateWeibullParameters(failures, suspensions);

            // 设置分析结果
            Beta = beta;
            Eta = eta;
            SampleCount = TestData.Count;
            AnalysisTime = DateTime.Now;
            ResultModuleName = SelectedModule?.Name ?? "Unknown";

            // 计算可靠性指标
            MTTF = eta * Math.Pow(Math.E, 1.0 / beta) * GammaFunction(1.0 + 1.0 / beta);
            B10Life = eta * Math.Pow(-Math.Log(0.9), 1.0 / beta);
            B50Life = eta * Math.Pow(-Math.Log(0.5), 1.0 / beta);
            B90Life = eta * Math.Pow(-Math.Log(0.1), 1.0 / beta);

            // 计算拟合优度
            RSquared = CalculateRSquared(failures);

            // 设置置信区间（简化计算）
            HasConfidenceInterval = failures.Length >= 10;
            if (HasConfidenceInterval)
            {
                double margin = 0.1 * beta;
                BetaLower = beta - margin;
                BetaUpper = beta + margin;
                EtaLower = eta * 0.9;
                EtaUpper = eta * 1.1;
            }

            HasResult = true;

            // 绘制威布尔图
            PlotWeibullChart(failures);
        }

        private void PlotWeibullChart(double[] failures)
        {
            AvaPlot.Plot.Clear();
            
            if (failures.Length > 0)
            {
                double[] failureTimes = failures.OrderBy(t => t).ToArray();
                double[] failureProbabilities = _analysisService.GetFailureProbabilities(failureTimes.Length);

                double[] xData = failureTimes.Select(t => Math.Log(t)).ToArray();
                double[] yData = failureProbabilities.Select(p => Math.Log(-Math.Log(1 - p))).ToArray();

                var scatter = AvaPlot.Plot.Add.Scatter(xData, yData);
                scatter.Label = "Data Points";

                // 添加拟合线
                double[] fitX = { xData.First(), xData.Last() };
                double[] fitY = { (fitX[0] - Math.Log(Eta)) * Beta, (fitX[1] - Math.Log(Eta)) * Beta };
                
                var line = AvaPlot.Plot.Add.Scatter(fitX, fitY);
                line.Label = "Weibull Fit";
                line.LineStyle.Width = 2;
                line.LineStyle.Color = Color.FromHex("#FF0000");
                line.MarkerSize = 0;
            }

            AvaPlot.Plot.Title($"Weibull Plot (Beta: {Beta:F2}, Eta: {Eta:F2})");
            AvaPlot.Plot.XLabel("ln(Time)");
            AvaPlot.Plot.YLabel("ln(-ln(1-F(t)))");
            AvaPlot.Plot.Legend.IsVisible = true;
            AvaPlot.Refresh();
        }

        private double GammaFunction(double z)
        {
            // 简化的Gamma函数近似
            if (z == 1.0) return 1.0;
            if (z == 2.0) return 1.0;
            // 使用Stirling近似
            return Math.Sqrt(2 * Math.PI / z) * Math.Pow(z / Math.E, z);
        }

        private double CalculateRSquared(double[] failures)
        {
            if (failures.Length < 2) return 0;
            
            // 简化的R²计算
            double[] sorted = failures.OrderBy(t => t).ToArray();
            double[] x = sorted.Select(t => Math.Log(t)).ToArray();
            double meanX = x.Average();
            
            double ssTot = x.Sum(xi => Math.Pow(xi - meanX, 2));
            if (ssTot == 0) return 0;
            
            // 简化计算，实际应该计算拟合后的残差
            return 0.95; // 占位值，实际应该根据拟合结果计算
        }

        private void SaveResult()
        {
            // TODO: 实现保存结果到数据库
            System.Diagnostics.Debug.WriteLine("SaveResult called");
        }

        private void ExportReport()
        {
            // TODO: 实现导出报告
            System.Diagnostics.Debug.WriteLine("ExportReport called");
        }
    }
}
