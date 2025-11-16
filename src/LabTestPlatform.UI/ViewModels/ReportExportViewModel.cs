using LabTestPlatform.Core.Models;
using LabTestPlatform.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using System.Reactive.Linq; // 确保此 using 存在

namespace LabTestPlatform.UI.ViewModels
{
    public class ReportExportViewModel : ViewModelBase
    {
        private readonly IReportService _reportService;
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

        private string? _selectedFormat = "Excel"; // 默认格式
        public string? SelectedFormat
        {
            get => _selectedFormat;
            set => this.RaiseAndSetIfChanged(ref _selectedFormat, value);
        }

        public Interaction<Unit, IStorageFile?> ShowSaveFileDialog { get; }

        public ReactiveCommand<Unit, Unit> GenerateReportCommand { get; }

        public ReportExportViewModel(IServiceProvider services)
        {
            _reportService = services.GetRequiredService<IReportService>();
            _systemService = services.GetRequiredService<ISystemService>();
            
            _systems = new ObservableCollection<SystemModel>();
            _platforms = new ObservableCollection<PlatformModel>();
            _modules = new ObservableCollection<ModuleModel>();

            ShowSaveFileDialog = new Interaction<Unit, IStorageFile?>();

            GenerateReportCommand = ReactiveCommand.CreateFromTask(GenerateReport);

            this.WhenAnyValue(x => x.SelectedSystem)
                .Subscribe(system => LoadPlatforms(system));
            this.WhenAnyValue(x => x.SelectedPlatform)
                .Subscribe(platform => LoadModules(platform));

            LoadSystems();
        }

        private void LoadSystems()
        {
            var systems = _systemService.GetAllSystems();
            Systems = new ObservableCollection<SystemModel>(systems);
            Platforms.Clear();
            Modules.Clear();
        }

        private void LoadPlatforms(SystemModel? system)
        {
            Platforms.Clear();
            Modules.Clear();
            if (system != null)
            {
                var platforms = _systemService.GetPlatformsBySystemId(system.Id);
                Platforms = new ObservableCollection<PlatformModel>(platforms);
            }
        }

        private void LoadModules(PlatformModel? platform)
        {
            Modules.Clear();
            if (platform != null)
            {
                var modules = _systemService.GetModulesByPlatformId(platform.Id);
                Modules = new ObservableCollection<ModuleModel>(modules);
            }
        }

        private async Task GenerateReport()
        {
            var file = await ShowSaveFileDialog.Handle(Unit.Default).FirstAsync();
            if (file == null) return;

            try
            {
                string reportType = "FullReport"; // 示例类型
                string? entityId = _selectedModule?.Id ?? _selectedPlatform?.Id ?? _selectedSystem?.Id;
                
                if (entityId == null || _selectedFormat == null)
                {
                    // 修正：删除了我上次留下的 'Services' 拼写错误
                    return; 
                }

                // 这是您仓库中的正确名称
                await _reportService.GenerateReportAsync(reportType, entityId, file.Path.LocalPath, _selectedFormat);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}