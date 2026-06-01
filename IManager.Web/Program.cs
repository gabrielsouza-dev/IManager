using IManager.Web.Presentation.Extensions;
using IManager.Web.Presentation.Mappings;
using IManager.Web.Presentation.Middlewares;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Serilog;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
}).AddRazorRuntimeCompilation();

builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Clear();
    options.ViewLocationFormats.Add("/Presentation/Views/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("/Presentation/Views/Shared/{0}.cshtml");
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var culture = new CultureInfo("pt-BR");
    options.DefaultRequestCulture = new RequestCulture(culture);
    options.SupportedCultures = new[] { culture };
    options.SupportedUICultures = new[] { culture };
});

builder.AddLogging();
Log.Information("Iniciando Aplicação...");

builder.AddConfigurations();
builder.AddContext();
builder.AddAuth();
builder.AddServices();
builder.AddRepositories();
builder.AddFluentValidation();
builder.AddDataProtectionConfig();
builder.AddRateLimitConfig();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseRequestLocalization();

await app.Initialize();

app.UseCors("AllowAll");

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
    app.UseHsts();
else
    app.UseDeveloperExceptionPage();

app.UseStatusCodePages(async context =>
{
    var request = context.HttpContext.Request;
    var response = context.HttpContext.Response;

    if (request.Path.StartsWithSegments("/api"))
    {
        response.ContentType = "application/json";
        await response.WriteAsync(
            System.Text.Json.JsonSerializer.Serialize(new
            {
                status = response.StatusCode,
                message = response.StatusCode switch
                {
                    401 => "Não autorizado.",
                    403 => "Acesso negado.",
                    404 => "Recurso não encontrado.",
                    _ => "Erro inesperado."
                }
            })
        );
    }
    else
    {
        context.HttpContext.Response.Redirect($"/Home/Error?statusCode={response.StatusCode}");
    }
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();