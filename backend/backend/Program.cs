using backend.Data;
using backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=comments.db"));

builder.Services.AddSingleton<ICaptchaService, CaptchaService>();
builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p =>
        p.WithOrigins(
            "http://localhost:4200",
            "http://localhost:80",
            "http://frontend"
          )
         .AllowAnyHeader()
         .AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

//app.UseSwagger();
//app.UseSwaggerUI();
app.UseCors();
app.UseStaticFiles();
app.MapControllers();
app.Run();