using DataCollectionPlatform.Data;
using DataCollectionPlatform.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")
        ?? "Data Source=data.db"));

builder.Services.AddHttpClient<IUrlFetchService, UrlFetchService>();
builder.Services.AddScoped<IImportExportService, ImportExportService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Initialize(db);
}

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Items}/{action=Index}/{id?}");

// 啟動後自動開啟瀏覽器
var urls = builder.Configuration["Urls"] ?? "http://localhost:5000";
var launchUrl = urls.Split(';').First().Trim();
_ = Task.Run(async () =>
{
    await Task.Delay(1500);
    try
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(launchUrl)
        {
            UseShellExecute = true
        });
    }
    catch { /* 無法開啟瀏覽器時靜默忽略 */ }
});

app.Run();
