using TravelMap.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

//�������ã�����Ӧ�ù��������������÷�����м��
var builder = WebApplication.CreateBuilder(args);

//���ݿ�����
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// JWT �����֤
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

//����Ȩ����֤
builder.Services.AddAuthorization();

//���ÿ�����Դ��������������Դ���������󷽷�������ͷ
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddControllers();// ע�� MVC ������
builder.Services.AddEndpointsApiExplorer();// Ϊ API ̽���ṩԪ����
builder.Services.AddSwaggerGen();// ���� Swagger/OpenAPI �ĵ�

//�м�ܵ�����
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();// ���� Swagger JSON �˵�
    app.UseSwaggerUI();// ���� Swagger UI ����
}

app.UseHttpsRedirection();// ǿ�� HTTPS

app.UseAuthentication();// ���������֤

app.UseAuthorization();// ������Ȩ

app.UseCors("AllowAll");// Ӧ�� CORS ����

app.MapControllers();// ӳ�������·��

app.Run();// ����Ӧ��

