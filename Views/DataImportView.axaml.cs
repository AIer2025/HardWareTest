using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ReactiveUI;
using LabTestPlatform.UI.ViewModels;

namespace LabTestPlatform.UI.Views;

public partial class DataImportView : ReactiveUserControl<DataImportViewModel>
{
    public DataImportView()
    {
        InitializeComponent();
        
        // 订阅文件选择交互
        this.WhenActivated(disposables =>
        {
            if (ViewModel != null)
            {
                ViewModel.ShowOpenFileDialog.RegisterHandler(async interaction =>
                {
                    var filePath = await ShowOpenFileDialogAsync();
                    interaction.SetOutput(filePath);
                });
            }
        });
    }

    private async Task<string?> ShowOpenFileDialogAsync()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null)
            return null;

        try
        {
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择Excel文件",
                AllowMultiple = false,
                FileTypeFilter = new[] 
                { 
                    new FilePickerFileType("Excel Files") 
                    { 
                        Patterns = new[] { "*.xlsx", "*.xls" }
                    },
                    FilePickerFileTypes.All 
                }
            });

            if (files.Count > 0)
            {
                return files[0].Path.LocalPath;
            }
        }
        catch (Exception)
        {
            // 文件选择失败
        }

        return null;
    }
}
