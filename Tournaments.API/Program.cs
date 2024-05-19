[assembly: ApiController]
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
                .AddNewtonsoftJson();

builder.Services.RegisterApplicationServices(builder.Configuration)
                .AddEndpointsApiExplorer()
                .AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger().UseSwaggerUI();

    if (Environment.GetEnvironmentVariable("SEED_DATA") == "1")
    {
        await app.SeedDataAsync();
    }
}

app.UseHttpsRedirection()
   .UseAuthorization();

app.MapControllers();
app.Run();
