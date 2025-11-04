using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using ShopBTW.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers()
 .AddJsonOptions(o =>
  {
      o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
      // o.JsonSerializerOptions.WriteIndented = true; // по желанию
  });
builder.Services.AddEndpointsApiExplorer();    // для Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ShopBTW API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();   // ← теперь будет работать
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
