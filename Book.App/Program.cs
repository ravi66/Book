using Book;
using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using SqliteWasmHelper;
using MudBlazor.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSqliteWasmDbContextFactory<BookDbContext>(
  opts => opts.UseSqlite("Data Source=book.sqlite3"));

builder.Services.AddMudServices();

builder.Services.AddBookServices();

await builder.Build().RunAsync();
