using Avalonia.Controls;
using Avalonia.Platform.Storage;
using LabTestPlatform.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LabTestPlatform.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 打开Excel文件选择对话框
        /// </summary>
        public async Task<string?> OpenExcelFileDialogAsync()
        {
            var filePickerOptions = new FilePickerOpenOptions
            {
                Title = "选择Excel文件",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Excel文件")
                    {
                        Patterns = new[] { "*.xlsx", "*.xls" }
                    }
                }
            };

            var result = await StorageProvider.OpenFilePickerAsync(filePickerOptions);
            return result?.FirstOrDefault()?.Path.LocalPath;
        }

        /// <summary>
        /// 保存文件对话框
        /// </summary>
        public async Task<string?> SaveFileDialogAsync(string defaultFileName, string fileExtension)
        {
            var filePickerOptions = new FilePickerSaveOptions
            {
                Title = "保存文件",
                SuggestedFileName = defaultFileName,
                FileTypeChoices = new[]
                {
                    new FilePickerFileType(fileExtension.ToUpper() + "文件")
                    {
                        Patterns = new[] { "*." + fileExtension }
                    }
                }
            };

            var result = await StorageProvider.SaveFilePickerAsync(filePickerOptions);
            return result?.Path.LocalPath;
        }
    }
}
