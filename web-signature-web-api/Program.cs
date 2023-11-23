using Microsoft.EntityFrameworkCore;
using web_signature_web_api.Interfaces;
using web_signature_web_api.Migrations;
using web_signature_web_api.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Data Source=.\\SQLEXPRESS;Initial " +
                                                                   "Catalog=Signature;Integrated " +
                                                                   "Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True"));
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddScoped<IAESencryptionService, AESencryptionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder =>
{
    builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();