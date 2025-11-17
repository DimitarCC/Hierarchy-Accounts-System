using Asp.Versioning;
using HierarchyAccountsSystem.BusinessLogic.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HASDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), optionsBuilder => optionsBuilder.MigrationsAssembly("HierarchyAccountsSystem.Api")));

builder.Services.AddControllers().AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(o => {
  o.ReportApiVersions = true;
  o.AssumeDefaultVersionWhenUnspecified = true;
  o.DefaultApiVersion = new ApiVersion(1, 0);
}).AddApiExplorer(options => {
  options.GroupNameFormat = "'v'VVV";
  options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(options => {
  options.SwaggerDoc("v1", new() { Title = "HierarchyAccountsSystem API", Version = "v1" });
  var filePath = Path.Combine(AppContext.BaseDirectory, "HierarchyAccountsSystem.API.xml");
  options.IncludeXmlComments(filePath, true);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
