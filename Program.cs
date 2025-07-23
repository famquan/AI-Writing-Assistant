using AI_Writing_Assistant;
using AI_Writing_Assistant.Services;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddSingleton<AIService>();
        services.AddSingleton<MainForm>();
    }
}
