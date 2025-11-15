using Avalonia;
using System;

namespace LabTestPlatform
{
    internal class Program
    {
        // Avalonia配置，不要删除；由Avalonia设计器使用
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia配置，不要删除；由Avalonia设计器使用
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
