using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using LabTestPlatform.Core.Services;
using LabTestPlatform.Core.Models;
using LabTestPlatform.UI.Models;

namespace LabTestPlatform.UI.ViewModels;

public class SystemManagementViewModel : ViewModelBase
{
    private readonly ISystemService _systemService;
    private ObservableCollection<SystemNode> _systems;
    private ObservableCollection<ModuleModel> _modules;
    private SystemNode? _selectedNode;
    private ModuleModel? _selectedModule;
    private string _searchText = string.Empty;
    private string _selectedNodeInfo = string.Empty;
    private int _systemCount;
    private int _platformCount;
    private int _moduleCount;

    public SystemManagementViewModel(IServiceProvider serviceProvider)
    {
        _systemService = serviceProvider.GetRequiredService<ISystemService>();
        _systems = new ObservableCollection<SystemNode>();
        _modules = new ObservableCollection<ModuleModel>();

        LoadDataCommand = ReactiveCommand.CreateFromTask(LoadDataAsync);
        AddSystemCommand = ReactiveCommand.CreateFromTask(AddSystemAsync);
        AddPlatformCommand = ReactiveCommand.CreateFromTask(AddPlatformAsync, this.WhenAnyValue(x => x.CanAddPlatform));
        AddModuleCommand = ReactiveCommand.CreateFromTask(AddModuleAsync, this.WhenAnyValue(x => x.CanAddModule));
        EditCommand = ReactiveCommand.CreateFromTask(EditAsync, this.WhenAnyValue(x => x.CanEdit));
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync, this.WhenAnyValue(x => x.CanDelete));
        RefreshCommand = ReactiveCommand.CreateFromTask(LoadDataAsync);

        // 监听选中节点的变化
        this.WhenAnyValue(x => x.SelectedNode)
            .Subscribe(async node => await OnSelectedNodeChanged(node));

        LoadDataCommand.Execute().Subscribe();
    }

    public ObservableCollection<SystemNode> Systems
    {
        get => _systems;
        set => this.RaiseAndSetIfChanged(ref _systems, value);
    }

    public ObservableCollection<ModuleModel> Modules
    {
        get => _modules;
        set => this.RaiseAndSetIfChanged(ref _modules, value);
    }

    public SystemNode? SelectedNode
    {
        get => _selectedNode;
        set => this.RaiseAndSetIfChanged(ref _selectedNode, value);
    }

    public ModuleModel? SelectedModule
    {
        get => _selectedModule;
        set => this.RaiseAndSetIfChanged(ref _selectedModule, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public string SelectedNodeInfo
    {
        get => _selectedNodeInfo;
        set => this.RaiseAndSetIfChanged(ref _selectedNodeInfo, value);
    }

    public int SystemCount
    {
        get => _systemCount;
        set => this.RaiseAndSetIfChanged(ref _systemCount, value);
    }

    public int PlatformCount
    {
        get => _platformCount;
        set => this.RaiseAndSetIfChanged(ref _platformCount, value);
    }

    public int ModuleCount
    {
        get => _moduleCount;
        set => this.RaiseAndSetIfChanged(ref _moduleCount, value);
    }

    public bool CanAddPlatform => SelectedNode?.NodeType == "System";
    public bool CanAddModule => SelectedNode?.NodeType == "Platform";
    public bool CanEdit => SelectedNode != null;
    public bool CanDelete => SelectedNode != null;

    public ReactiveCommand<Unit, Unit> LoadDataCommand { get; }
    public ReactiveCommand<Unit, Unit> AddSystemCommand { get; }
    public ReactiveCommand<Unit, Unit> AddPlatformCommand { get; }
    public ReactiveCommand<Unit, Unit> AddModuleCommand { get; }
    public ReactiveCommand<Unit, Unit> EditCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    private async Task LoadDataAsync()
    {
        try
        {
            var systems = await _systemService.GetAllSystemsAsync();
            Systems.Clear();

            int totalPlatforms = 0;
            int totalModules = 0;

            foreach (var system in systems)
            {
                var systemNode = new SystemNode
                {
                    Id = system.SystemId.ToString(),
                    Name = $"{system.SystemCode} - {system.SystemName}",
                    NodeType = "System",
                    Data = system
                };

                var platforms = await _systemService.GetPlatformsBySystemIdAsync(system.SystemId);
                totalPlatforms += platforms.Count();

                foreach (var platform in platforms)
                {
                    var platformNode = new SystemNode
                    {
                        Id = platform.PlatformId.ToString(),
                        Name = $"{platform.PlatformCode} - {platform.PlatformName}",
                        NodeType = "Platform",
                        Data = platform,
                        Parent = systemNode
                    };

                    var modules = await _systemService.GetModulesByPlatformIdAsync(platform.PlatformId);
                    totalModules += modules.Count();

                    foreach (var module in modules)
                    {
                        var moduleNode = new SystemNode
                        {
                            Id = module.ModuleId.ToString(),
                            Name = $"{module.ModuleCode} - {module.ModuleName}",
                            NodeType = "Module",
                            Data = module,
                            Parent = platformNode
                        };
                        platformNode.Children.Add(moduleNode);
                    }

                    systemNode.Children.Add(platformNode);
                }

                Systems.Add(systemNode);
            }

            // 更新统计信息
            SystemCount = systems.Count();
            PlatformCount = totalPlatforms;
            ModuleCount = totalModules;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载数据失败: {ex.Message}");
        }
    }

    private async Task OnSelectedNodeChanged(SystemNode? node)
    {
        if (node == null)
        {
            SelectedNodeInfo = string.Empty;
            Modules.Clear();
            this.RaisePropertyChanged(nameof(CanAddPlatform));
            this.RaisePropertyChanged(nameof(CanAddModule));
            this.RaisePropertyChanged(nameof(CanEdit));
            this.RaisePropertyChanged(nameof(CanDelete));
            return;
        }

        // 更新选中节点信息
        SelectedNodeInfo = node.NodeType switch
        {
            "System" => $"系统: {node.Name}",
            "Platform" => $"平台: {node.Name}",
            "Module" => $"模组: {node.Name}",
            _ => string.Empty
        };

        // 加载模组列表
        try
        {
            Modules.Clear();

            if (node.NodeType == "Platform" && node.Data is PlatformModel platform)
            {
                var modules = await _systemService.GetModulesByPlatformIdAsync(platform.PlatformId);
                foreach (var module in modules)
                {
                    Modules.Add(module);
                }
            }
            else if (node.NodeType == "System" && node.Data is SystemModel system)
            {
                var platforms = await _systemService.GetPlatformsBySystemIdAsync(system.SystemId);
                foreach (var platform in platforms)
                {
                    var modules = await _systemService.GetModulesByPlatformIdAsync(platform.PlatformId);
                    foreach (var module in modules)
                    {
                        Modules.Add(module);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载模组失败: {ex.Message}");
        }

        // 更新命令的可用状态
        this.RaisePropertyChanged(nameof(CanAddPlatform));
        this.RaisePropertyChanged(nameof(CanAddModule));
        this.RaisePropertyChanged(nameof(CanEdit));
        this.RaisePropertyChanged(nameof(CanDelete));
    }

    private async Task AddSystemAsync()
    {
        // TODO: 实现添加系统的逻辑
        await Task.CompletedTask;
    }

    private async Task AddPlatformAsync()
    {
        // TODO: 实现添加平台的逻辑
        await Task.CompletedTask;
    }

    private async Task AddModuleAsync()
    {
        // TODO: 实现添加模组的逻辑
        await Task.CompletedTask;
    }

    private async Task EditAsync()
    {
        // TODO: 实现编辑的逻辑
        await Task.CompletedTask;
    }

    private async Task DeleteAsync()
    {
        // TODO: 实现删除的逻辑
        await Task.CompletedTask;
    }
}
