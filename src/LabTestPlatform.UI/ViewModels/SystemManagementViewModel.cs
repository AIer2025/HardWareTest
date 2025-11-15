using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using LabTestPlatform.Core.Services;
using LabTestPlatform.UI.Models;

namespace LabTestPlatform.UI.ViewModels;

public class SystemManagementViewModel : ViewModelBase
{
    private readonly ISystemService _systemService;
    private ObservableCollection<SystemNode> _systems;
    private SystemNode? _selectedNode;
    private string _searchText = string.Empty;

    public SystemManagementViewModel(IServiceProvider serviceProvider)
    {
        _systemService = serviceProvider.GetRequiredService<ISystemService>();
        _systems = new ObservableCollection<SystemNode>();

        LoadDataCommand = ReactiveCommand.CreateFromTask(LoadDataAsync);
        AddSystemCommand = ReactiveCommand.CreateFromTask(AddSystemAsync);
        RefreshCommand = ReactiveCommand.CreateFromTask(LoadDataAsync);

        LoadDataCommand.Execute().Subscribe();
    }

    public ObservableCollection<SystemNode> Systems
    {
        get => _systems;
        set => this.RaiseAndSetIfChanged(ref _systems, value);
    }

    public SystemNode? SelectedNode
    {
        get => _selectedNode;
        set => this.RaiseAndSetIfChanged(ref _selectedNode, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public ReactiveCommand<Unit, Unit> LoadDataCommand { get; }
    public ReactiveCommand<Unit, Unit> AddSystemCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    private async Task LoadDataAsync()
    {
        try
        {
            var systems = await _systemService.GetAllSystemsAsync();
            Systems.Clear();

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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载数据失败: {ex.Message}");
        }
    }

    private async Task AddSystemAsync()
    {
        await Task.CompletedTask;
    }
}
