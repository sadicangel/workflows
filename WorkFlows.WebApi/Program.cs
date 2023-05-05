using Elsa;
using Elsa.Persistence.EntityFramework.Core.Extensions;
using Elsa.Persistence.EntityFramework.Sqlite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddElsa(elsa => elsa
    .UseEntityFrameworkPersistence(ef => ef.UseSqlite())
    .AddConsoleActivities()
    .AddHttpActivities(http => http.BaseUrl = new("https://localhost:5001"))
    .AddQuartzTemporalActivities()
    .AddActivitiesFrom<Program>()
    .AddWorkflowsFrom<Program>());

builder.Services.AddElsaApiEndpoints();

builder.Services.AddRazorPages();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseHttpActivities();
app.UseRouting();

app.MapControllers();
app.MapFallbackToPage("/_Host");

app.Run();
