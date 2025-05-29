using Microsoft.EntityFrameworkCore;
using eCHU.Models;
using eCHU.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Add Entity Framework
builder.Services.AddDbContext<WebAPIContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add CORS - 修改為更寬鬆的設定
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });

    // 如果需要支援 credentials，使用這個設定
    options.AddPolicy("AllowCredentials", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Email Service
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "eCHU API",
        Version = "v1"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "eCHU API V1");
        c.RoutePrefix = "swagger";
    });
}

// 移除 HTTPS 重定向以避免協議問題
// app.UseHttpsRedirection();

// 使用 CORS - 放在 UseAuthorization 之前
app.UseCors("AllowAll"); // 或者使用 "AllowCredentials" 如果需要

app.UseAuthorization();

app.MapControllers();

// 添加預設路由處理
app.MapGet("/", () => "eCHU API is running");

app.Run();