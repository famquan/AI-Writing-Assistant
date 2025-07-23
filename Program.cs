using AI_Writing_Assistant;
using AI_Writing_Assistant.Forms;
using AI_Writing_Assistant.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Forms;

public static class Program
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        Application.Run(ServiceProvider.GetRequiredService<MainForm>());
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddSingleton<SettingsService>();
        services.AddSingleton<OpenAiService>();
        services.AddSingleton<GeminiService>();

        services.AddSingleton<IAiServiceFactory, AiServiceFactory>();
        services.AddTransient<IAiService>(provider =>
        {
            var factory = provider.GetRequiredService<IAiServiceFactory>();
            return factory.CreateService();
        });

        services.AddSingleton<MainForm>();
    }
}
