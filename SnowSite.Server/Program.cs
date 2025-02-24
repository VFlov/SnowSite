using SnowSite.Server.Data;
using SnowSite.Server.Hubs;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.WithOrigins("https://45.130.214.139:65311", "https://45.130.214.139:5020") // VERY IMPORTANT: SPECIFY YOUR FRONTEND ORIGIN
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });

});
builder.Services.AddSignalR();
builder.Services.AddControllers();

var app = builder.Build();
app.UseCors("AllowAll");
app.UseRouting(); // Добавьте UseRouting перед UseEndpoints
app.MapHub<CallHub>("/chatHub");
/*
app.UseEndpoints(endpoints =>
{
    // Настройка SignalR хаба
    endpoints.MapHub<CallHub>("/callHub");
});
*/

app.UseDefaultFiles();
app.UseStaticFiles();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.MapControllers();

app.MapFallbackToFile("/index.html");


app.Run();
