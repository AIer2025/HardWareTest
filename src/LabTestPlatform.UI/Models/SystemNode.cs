using System.Collections.ObjectModel;

namespace LabTestPlatform.UI.Models
{
    public class SystemNode
    {
        // 修正：添加默认值以修复 CS8618 警告
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public string? Type { get; set; } // "System", "Platform", "Module"

        public ObservableCollection<SystemNode> Children { get; set; } = new();
    }
}