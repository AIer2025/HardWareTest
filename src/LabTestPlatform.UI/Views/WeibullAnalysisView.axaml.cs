using Avalonia.Controls;
using LabTestPlatform.UI.ViewModels;
using System;

namespace LabTestPlatform.UI.Views;

public partial class WeibullAnalysisView : UserControl
{
    public WeibullAnalysisView()
    {
        InitializeComponent();
        
        // 方法1：立即检查DataContext
        if (DataContext is WeibullAnalysisViewModel viewModel)
        {
            ConnectPlot(viewModel);
        }
        
        // 方法2：监听DataContext变化
        this.DataContextChanged += OnDataContextChanged;
        
        // 方法3：在控件加载后再次尝试
        this.Loaded += OnLoaded;
    }
    
    private void OnDataContextChanged(object? sender, EventArgs args)
    {
        if (DataContext is WeibullAnalysisViewModel viewModel)
        {
            ConnectPlot(viewModel);
        }
    }
    
    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is WeibullAnalysisViewModel viewModel)
        {
            ConnectPlot(viewModel);
        }
    }
    
    private void ConnectPlot(WeibullAnalysisViewModel viewModel)
    {
        if (viewModel.AvaPlot != WeibullPlot)
        {
            viewModel.AvaPlot = WeibullPlot;
            Console.WriteLine($"✓ AvaPlot已连接: {WeibullPlot != null}");
            
            // 强制刷新一次
            if (WeibullPlot != null)
            {
                WeibullPlot.Refresh();
            }
        }
    }
}
