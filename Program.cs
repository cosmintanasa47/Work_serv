
using Work_serv;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // services.AddHostedService<Worker>();
        services.AddHostedService<do_it>();
    }).UseWindowsService()
    .Build();

var configsetting = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json").Build();

host.Run();
