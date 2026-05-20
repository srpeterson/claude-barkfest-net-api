using Barkfest.API.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddBarkfestServices();

var app = builder.Build();

await app.InitialiseDatabaseAsync();
app.ConfigurePipeline();

await app.RunAsync();

public partial class Program;
