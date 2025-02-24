using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Azure.Identity;
using FarmaU.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure the SQL connection with Azure AD authentication
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var credential = new DefaultAzureCredential();
var token = credential.GetToken(
    new Azure.Core.TokenRequestContext(
        new[] { "https://database.windows.net/.default" }
    )
);

var sqlConnection = new SqlConnection(connectionString)
{
    AccessToken = token.Token
};

// Add DbContext service
builder.Services.AddDbContext<FarmaUContext>(options =>
    options.UseSqlServer(sqlConnection));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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
