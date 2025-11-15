using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using LabTestPlatform.Models;
using LabTestPlatform.Data;
using LabTestPlatform.Services;

namespace LabTestPlatform.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly TestDataRepository _repository;
        private readonly WeibullAnalysisEngine _analysisEngine;
        private readonly ExcelImportService _excelService;
        private readonly ReportGenerationService _reportService;

        private ObservableCollection<SystemEntity> _systems = new();
        private ObservableCollection<PlatformEntity> _platforms = new();
        private ObservableCollection<ModuleEntity> _modules = new();
        private ObservableCollection<TestDataEntity> _testData = new();

        private SystemEntity? _selectedSystem;
        private PlatformEntity? _selectedPlatform;
        private ModuleEntity? _selectedModule;

        private string _statusMessage = "就绪";
        private bool _isBusy = false;

        public MainWindowViewModel(
            TestDataRepository repository,
            WeibullAnalysisEngine analysisEngine,
            ExcelImportService excelService,
            ReportGenerationService reportService)
        {
            _repository = repository;
            _analysisEngine = analysisEngine;
            _excelService = excelService;
            _reportService = reportService;

            // 初始化命令
            LoadSystemsCommand = ReactiveCommand.CreateFromTask(LoadSystemsAsync);
            LoadPlatformsCommand = ReactiveCommand.CreateFromTask(LoadPlatformsAsync);
            LoadModulesCommand = ReactiveCommand.CreateFromTask(LoadModulesAsync);
            LoadTestDataCommand = ReactiveCommand.CreateFromTask(LoadTestDataAsync);
            
            ImportExcelCommand = ReactiveCommand.CreateFromTask(ImportFromExcelAsync);
            PerformWeibullAnalysisCommand = ReactiveCommand.CreateFromTask(PerformWeibullAnalysisAsync);
            ExportReportCommand = ReactiveCommand.CreateFromTask(ExportReportAsync);
            GenerateTemplateCommand = ReactiveCommand.CreateFromTask(GenerateTemplateAsync);

            // 自动加载系统列表
            _ = LoadSystemsAsync();
        }

        // 属性
        public ObservableCollection<SystemEntity> Systems
        {
            get => _systems;
            set => this.RaiseAndSetIfChanged(ref _systems, value);
        }

        public ObservableCollection<PlatformEntity> Platforms
        {
            get => _platforms;
            set => this.RaiseAndSetIfChanged(ref _platforms, value);
        }

        public ObservableCollection<ModuleEntity> Modules
        {
            get => _modules;
            set => this.RaiseAndSetIfChanged(ref _modules, value);
        }

        public ObservableCollection<TestDataEntity> TestData
        {
            get => _testData;
            set => this.RaiseAndSetIfChanged(ref _testData, value);
        }

        public SystemEntity? SelectedSystem
        {
            get => _selectedSystem;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedSystem, value);
                if (value != null)
                    _ = LoadPlatformsAsync();
            }
        }

        public PlatformEntity? SelectedPlatform
        {
            get => _selectedPlatform;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedPlatform, value);
                if (value != null)
                    _ = LoadModulesAsync();
            }
        }

        public ModuleEntity? SelectedModule
        {
            get => _selectedModule;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedModule, value);
                if (value != null)
                    _ = LoadTestDataAsync();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }

        // 命令
        public ReactiveCommand<Unit, Unit> LoadSystemsCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadPlatformsCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadModulesCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadTestDataCommand { get; }
        public ReactiveCommand<Unit, Unit> ImportExcelCommand { get; }
        public ReactiveCommand<Unit, Unit> PerformWeibullAnalysisCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportReportCommand { get; }
        public ReactiveCommand<Unit, Unit> GenerateTemplateCommand { get; }

        // 方法实现
        private async Task LoadSystemsAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "加载系统列表...";
                
                var systems = await _repository.GetAllSystemsAsync();
                Systems = new ObservableCollection<SystemEntity>(systems);
                
                StatusMessage = $"已加载 {Systems.Count} 个系统";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载系统失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadPlatformsAsync()
        {
            if (SelectedSystem == null) return;

            try
            {
                IsBusy = true;
                StatusMessage = "加载平台列表...";
                
                var platforms = await _repository.GetPlatformsBySystemIdAsync(SelectedSystem.SystemID);
                Platforms = new ObservableCollection<PlatformEntity>(platforms);
                
                StatusMessage = $"已加载 {Platforms.Count} 个平台";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载平台失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadModulesAsync()
        {
            if (SelectedPlatform == null) return;

            try
            {
                IsBusy = true;
                StatusMessage = "加载模组列表...";
                
                var modules = await _repository.GetModulesByPlatformIdAsync(SelectedPlatform.PlatformID);
                Modules = new ObservableCollection<ModuleEntity>(modules);
                
                StatusMessage = $"已加载 {Modules.Count} 个模组";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载模组失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadTestDataAsync()
        {
            if (SelectedModule == null) return;

            try
            {
                IsBusy = true;
                StatusMessage = "加载测试数据...";
                
                var testData = await _repository.GetTestDataByModuleIdAsync(SelectedModule.ModuleID);
                TestData = new ObservableCollection<TestDataEntity>(testData);
                
                StatusMessage = $"已加载 {TestData.Count} 条测试数据";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载测试数据失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ImportFromExcelAsync()
        {
            if (SelectedModule == null)
            {
                StatusMessage = "请先选择模组";
                return;
            }

            // 这里应该打开文件选择对话框
            // 由于Avalonia的文件对话框需要在View层处理,这里只是示例
            StatusMessage = "请在View层实现文件选择对话框";
        }

        private async Task PerformWeibullAnalysisAsync()
        {
            if (SelectedModule == null || TestData.Count < 10)
            {
                StatusMessage = "请选择模组并确保至少有10条测试数据";
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "执行威布尔分析...";

                // 获取测试值数据
                var dataValues = TestData.Select(t => t.TestValue).ToArray();
                
                // 执行威布尔分析
                var analysisResult = _analysisEngine.AnalyzeTwoParameter(dataValues, "MLE");

                // 保存分析结果
                var resultEntity = new WeibullAnalysisResultEntity
                {
                    ModuleID = SelectedModule.ModuleID,
                    AnalysisTime = DateTime.Now,
                    AnalysisType = "Weibull",
                    DataCount = dataValues.Length,
                    ShapeParameter = analysisResult.ShapeParameter,
                    ScaleParameter = analysisResult.ScaleParameter,
                    LocationParameter = analysisResult.LocationParameter,
                    MTBF = analysisResult.MTBF,
                    ConfidenceLevel = 0.95,
                    RSquared = analysisResult.RSquared,
                    EstimationMethod = analysisResult.EstimationMethod,
                    ReliabilityAt1000h = analysisResult.ReliabilityAt1000h,
                    ReliabilityAt5000h = analysisResult.ReliabilityAt5000h,
                    ReliabilityAt10000h = analysisResult.ReliabilityAt10000h,
                    B10Life = analysisResult.B10Life,
                    B50Life = analysisResult.B50Life,
                    Analyst = Environment.UserName,
                    Status = "Confirmed"
                };

                var resultId = await _repository.SaveWeibullAnalysisResultAsync(resultEntity);

                StatusMessage = $"威布尔分析完成! β={analysisResult.ShapeParameter:F4}, η={analysisResult.ScaleParameter:F2}, MTBF={analysisResult.MTBF:F2}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"分析失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExportReportAsync()
        {
            if (SelectedModule == null)
            {
                StatusMessage = "请先选择模组";
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "生成报告...";

                // 获取最新的分析结果
                var results = await _repository.GetWeibullResultsByModuleIdAsync(SelectedModule.ModuleID);
                var latestResult = results.FirstOrDefault();

                if (latestResult == null)
                {
                    StatusMessage = "没有可用的分析结果";
                    return;
                }

                // 生成PDF报告
                var reportPath = await _reportService.GenerateWeibullReportAsync(
                    latestResult,
                    SelectedModule,
                    TestData.ToArray());

                StatusMessage = $"报告已生成: {reportPath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"生成报告失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GenerateTemplateAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "生成导入模板...";

                var templatePath = await _excelService.GenerateImportTemplateAsync();
                StatusMessage = $"模板已生成: {templatePath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"生成模板失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
