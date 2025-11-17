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
using ScottPlot; // 确保此 using 存在

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

        private ObservableCollection<TestData> _testData;
        public ObservableCollection<TestData> TestData
        {
            get => _testData;
            set => this.RaiseAndSetIfChanged(ref _testData, value);
        }

        public ReactiveCommand<Unit, Unit> AnalyzeCommand { get; }
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

            // 监听选择变化并更新数据
            this.WhenAnyValue(x => x.SelectedSystem)
                .Subscribe(system =>
                {
                    HasSelectedSystem = system != null;
                    LoadPlatforms(system);
                });
            
            this.WhenAnyValue(x => x.SelectedPlatform)
                .Subscribe(platform =>
                {
                    HasSelectedPlatform = platform != null;
                    LoadModules(platform);
                });
            
            this.WhenAnyValue(x => x.SelectedModule)
                .Subscribe(module => LoadData(module));

            LoadSystems();
        }

        private void LoadSystems()
        {
            var systems = _systemService.GetAllSystems();
            Systems = new ObservableCollection<SystemModel>(systems);
            Platforms.Clear();
            Modules.Clear();
            TestData.Clear();
        }

        private void LoadPlatforms(SystemModel? system)
        {
            Platforms.Clear();
            Modules.Clear();
            TestData.Clear();
            if (system != null)
            {
                var platforms = _systemService.GetPlatformsBySystemId(system.Id);
                Platforms = new ObservableCollection<PlatformModel>(platforms);
            }
        }

        private void LoadModules(PlatformModel? platform)
        {
            Modules.Clear();
            TestData.Clear();
            if (platform != null)
            {
                var modules = _systemService.GetModulesByPlatformId(platform.Id);
                Modules = new ObservableCollection<ModuleModel>(modules);
            }
        }

        private void LoadData(ModuleModel? module)
        {
            TestData.Clear();
            if (module != null)
            {
                var data = _analysisService.GetTestDataByModuleId(module.Id);
                TestData = new ObservableCollection<TestData>(data);
            }
        }

        private void Analyze()
        {
            if (TestData.Count == 0) return;

            var failures = TestData.Where(d => d.IsFailure).Select(d => d.Time).ToArray();
            var suspensions = TestData.Where(d => !d.IsFailure).Select(d => d.Time).ToArray();
            
            var (beta, eta) = _analysisService.CalculateWeibullParameters(failures, suspensions);

            AvaPlot.Plot.Clear();
            
            if (failures.Length > 0)
            {
                double[] failureTimes = failures.OrderBy(t => t).ToArray();
                double[] failureProbabilities = _analysisService.GetFailureProbabilities(failureTimes.Length);

                double[] xData = failureTimes.Select(t => Math.Log(t)).ToArray();
                double[] yData = failureProbabilities.Select(p => Math.Log(-Math.Log(1 - p))).ToArray();

                // 修正：Add.Scatter 可以接受 (double[] x, double[] y)
                var scatter = AvaPlot.Plot.Add.Scatter(xData, yData);
                scatter.Label = "Data Points";

                // 添加拟合线
                double[] fitX = { xData.First(), xData.Last() };
                double[] fitY = { (fitX[0] - Math.Log(eta)) * beta, (fitX[1] - Math.Log(eta)) * beta };
                
                // 修正：使用 Add.Scatter 绘制拟合线 (修复 CS1503)
                var line = AvaPlot.Plot.Add.Scatter(fitX, fitY);
                line.Label = "Weibull Fit";
                line.LineStyle.Width = 2;
                line.LineStyle.Color = Color.FromHex("#FF0000");
                line.MarkerSize = 0; // 隐藏拟合线上的点
            }

            AvaPlot.Plot.Title($"Weibull Plot (Beta: {beta:F2}, Eta: {eta:F2})");
            AvaPlot.Plot.XLabel("ln(Time)");
            AvaPlot.Plot.YLabel("ln(-ln(1-F(t)))");
            AvaPlot.Plot.Legend.IsVisible = true;
            AvaPlot.Refresh();
        }
    }
}