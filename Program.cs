using Microsoft.EntityFrameworkCore;
using ExcelCompare.Infrastructure.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Register encoding provider for ExcelDataReader
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories and services
builder.Services.AddScoped<ExcelCompare.Application.Interfaces.IUploadBatchRepository, ExcelCompare.Infrastructure.Data.UploadBatchRepository>();
builder.Services.AddScoped<ExcelCompare.Application.Services.BulkInsertService>();
builder.Services.AddScoped<ExcelCompare.Application.Interfaces.IExcelUploadService, ExcelCompare.Application.Services.ExcelUploadService>();
builder.Services.AddScoped<ExcelCompare.Application.Interfaces.IComparisonService, ExcelCompare.Application.Services.ComparisonService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
