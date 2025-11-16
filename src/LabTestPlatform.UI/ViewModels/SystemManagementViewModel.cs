using LabTestPlatform.Core.Services;
using LabTestPlatform.UI.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq; // 修正：添加了 using
using LabTestPlatform.Core.Models; // 修正：添加了 using (修复 CS1061)

namespace LabTestPlatform.UI.ViewModels
{
    public class SystemManagementViewModel : ViewModelBase
    {
        private readonly ISystemService _systemService;
        private ObservableCollection<SystemNode> _systemNodes;
        
        // 修正：声明为可空类型以修复警告
        private SystemNode? _selectedNode;

        public ObservableCollection<SystemNode> SystemNodes
        {
            get => _systemNodes;
            set => this.RaiseAndSetIfChanged(ref _systemNodes, value);
        }

        // 修正：声明为可空类型以修复警告
        public SystemNode? SelectedNode
        {
            get => _selectedNode;
            set => this.RaiseAndSetIfChanged(ref _selectedNode, value);
        }

        public ReactiveCommand<Unit, Unit> AddSystemCommand { get; }
        public ReactiveCommand<Unit, Unit> AddPlatformCommand { get; }
        public ReactiveCommand<Unit, Unit> AddModuleCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteNodeCommand { get; }
        public ReactiveCommand<Unit, Unit> UpdateNodeCommand { get; }

        public SystemManagementViewModel(ISystemService systemService)
        {
            _systemService = systemService;
            
            _systemNodes = new ObservableCollection<SystemNode>();
            _selectedNode = null; 
            
            LoadSystemTree();

            // 修正：修正 WhenAnyValue 的用法，避免二义性
            var canAddPlatform = this.WhenAnyValue(x => x.SelectedNode)
                                     .Select(node => node?.Type == "System");
            
            var canAddModule = this.WhenAnyValue(x => x.SelectedNode)
                                   .Select(node => node?.Type == "Platform");

            var canDeleteOrUpdate = this.WhenAnyValue(x => x.SelectedNode)
                                        .Select(node => node != null);

            AddSystemCommand = ReactiveCommand.Create(AddSystem);
            AddPlatformCommand = ReactiveCommand.Create(AddPlatform, canAddPlatform);
            AddModuleCommand = ReactiveCommand.Create(AddModule, canAddModule);
            DeleteNodeCommand = ReactiveCommand.Create(DeleteNode, canDeleteOrUpdate);
            UpdateNodeCommand = ReactiveCommand.Create(UpdateNode, canDeleteOrUpdate);
        }

        private void LoadSystemTree()
        {
            // 修正：此代码现在可以正确找到 Type 属性和 Service 方法
            var systems = _systemService.GetAllSystems();
            var nodes = systems.Select(s => new SystemNode
            {
                Id = s.Id,
                Name = s.Name,
                Type = "System",
                Children = new ObservableCollection<SystemNode>(
                    _systemService.GetPlatformsBySystemId(s.Id).Select(p => new SystemNode
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Type = "Platform",
                        Children = new ObservableCollection<SystemNode>(
                            _systemService.GetModulesByPlatformId(p.Id).Select(m => new SystemNode
                            {
                                Id = m.Id,
                                Name = m.Name,
                                Type = "Module"
                            })
                        )
                    })
                )
            });
            SystemNodes = new ObservableCollection<SystemNode>(nodes);
        }

        private void AddSystem()
        {
            var newSystem = new SystemNode { Id = "new_system_" + SystemNodes.Count, Name = "New System", Type = "System" };
            SystemNodes.Add(newSystem);
        }

        private void AddPlatform()
        {
            if (SelectedNode?.Type == "System")
            {
                var newPlatform = new SystemNode { Id = "new_platform_" + SelectedNode.Children.Count, Name = "New Platform", Type = "Platform" };
                SelectedNode.Children.Add(newPlatform);
            }
        }

        private void AddModule()
        {
            if (SelectedNode?.Type == "Platform")
            {
                var newModule = new SystemNode { Id = "new_module_" + SelectedNode.Children.Count, Name = "New Module", Type = "Module" };
                SelectedNode.Children.Add(newModule);
            }
        }

        private void DeleteNode()
        {
            if (SelectedNode == null) return;

            var parent = FindParent(SystemNodes, SelectedNode);
            if (parent != null)
            {
                parent.Children.Remove(SelectedNode);
            }
            else if (SystemNodes.Contains(SelectedNode))
            {
                SystemNodes.Remove(SelectedNode);
            }
        }
        
        // 修正：返回类型改为可空以修复警告
        private SystemNode? FindParent(ObservableCollection<SystemNode> nodes, SystemNode nodeToFind)
        {
            foreach (var node in nodes)
            {
                if (node.Children.Contains(nodeToFind))
                {
                    return node;
                }
                var parent = FindParent(node.Children, nodeToFind);
                if (parent != null)
                {
                    return parent;
                }
            }
            return null;
        }


        private void UpdateNode()
        {
            if (SelectedNode == null) return;
            // _systemService.UpdateNode(SelectedNode);
        }
    }
}