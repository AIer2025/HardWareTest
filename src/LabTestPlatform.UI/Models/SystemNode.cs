using System.Collections.ObjectModel;

namespace LabTestPlatform.UI.Models;

public class SystemNode
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NodeType { get; set; } = string.Empty;
    public object? Data { get; set; }
    public SystemNode? Parent { get; set; }
    public ObservableCollection<SystemNode> Children { get; set; } = new();
}
