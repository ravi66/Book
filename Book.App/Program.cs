using Blazored.LocalStorage;
using Book;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    string[] supportedCultures = new[] { "en-GB", "fr-FR" };
    options
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
        //.SetDefaultCulture("en-GB");
});

builder.Services.AddLocalization(options =>
    options.ResourcesPath = "Resources");

builder.Services.AddBesqlDbContextFactory<BookDbContext>(options => options.UseSqlite("Data Source=book.sqlite3"));

builder.Services.AddMudServices();

builder.Services.AddBookServices();

//await builder.Services.BuildServiceProvider().SetDefaultCultureAsync();

await builder.Build().RunAsync();
