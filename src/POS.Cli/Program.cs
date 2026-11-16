using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Pos.Cli;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
var demoDirectory = Path.Combine(AppContext.BaseDirectory, "data-demo");
var demoOnly = args.Contains("--demo");

try
{
    using var provider = CompositionRoot.Build(dataDirectory);
    var io = provider.GetRequiredService<ConsoleIo>();

    if (demoOnly || io.Confirm("Запустить демо-сценарий? Он работает на отдельных данных, ваши данные не затрагиваются"))
    {
        RunDemo(demoDirectory);
    }

    if (!demoOnly)
    {
        provider.GetRequiredService<ConsoleApp>().Run();
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Ввод завершён, выход.");
}

static void RunDemo(string directory)
{
    if (Directory.Exists(directory))
    {
        Directory.Delete(directory, recursive: true);
    }

    using var provider = CompositionRoot.Build(directory);
    provider.GetRequiredService<DemoScenario>().Run(directory);
}