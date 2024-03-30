using Book;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddLocalization();

builder.Services.AddBesqlDbContextFactory<BookDbContext>(options => options.UseSqlite("Data Source=book.sqlite3"));

builder.Services.AddMudServices();

builder.Services.AddBookServices();

await builder.Build().RunAsync();
