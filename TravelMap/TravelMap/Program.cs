using TravelMap.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

//基础设置：创建应用构建器，用于配置服务和中间件
var builder = WebApplication.CreateBuilder(args);

//数据库配置
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// JWT 身份验证
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]))
        };
    });

//启用权限验证
builder.Services.AddAuthorization();

//配置跨域资源共享：允许所有来源、所有请求方法和请求头
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddControllers();// 注册 MVC 控制器
builder.Services.AddEndpointsApiExplorer();// 为 API 探索提供元数据
builder.Services.AddSwaggerGen();// 生成 Swagger/OpenAPI 文档

//中间管道配置
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();// 启用 Swagger JSON 端点
    app.UseSwaggerUI();// 启用 Swagger UI 界面
}

app.UseHttpsRedirection();// 强制 HTTPS

app.UseAuthentication();// 启用身份验证

app.UseAuthorization();// 启用授权

app.UseCors("AllowAll");// 应用 CORS 策略

app.MapControllers();// 映射控制器路由

app.Run();// 启动应用

