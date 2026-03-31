using Personenvervoer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

// Register application services
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddScoped<MemberService>();
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<RidepatternService>();
builder.Services.AddScoped<GoogleApiService>();
builder.Services.AddScoped<RideService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();