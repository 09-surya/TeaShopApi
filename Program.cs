using WebApiTeaShopManageMent.DAL;  // ✅ Add this for DAL
using WebApiTeaShopManageMent.Models;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin());
});

// ✅ Register DAL (Dependency Injection)
builder.Services.AddScoped<TeaShopDal>();
builder.Services.AddScoped<MenuDal>();

var app = builder.Build();

// Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();

//var builder = WebApplication.CreateBuilder(args);

//// ✅ Add services
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// ✅ Allow CORS (Netlify frontend → API)
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowNetlify", policy =>
//    {
//        policy.WithOrigins("https://teacoffeemanagement.netlify.app") // your Netlify URL
//              .AllowAnyHeader()
//              .AllowAnyMethod();
//    });
//});
//builder.Services.AddScoped<TeaShopDal>();
//builder.Services.AddScoped<MenuDal>();
//var app = builder.Build();

//// ✅ Enable Swagger only in Development (or everywhere for now)
//if (app.Environment.IsDevelopment() || true) // <-- force true for testing
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseCors("AllowNetlify");
//app.UseAuthorization();

//app.MapControllers();
//app.Run();
