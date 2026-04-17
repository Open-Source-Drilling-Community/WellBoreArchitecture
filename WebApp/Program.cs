using MudBlazor;
using MudBlazor.Services;
using NORCE.Drilling.WellBoreArchitecture.WebApp;
using NORCE.Drilling.WellBoreArchitecture.WebPages;

var builder = WebApplication.CreateBuilder(args);

WebPagesHostConfiguration webPagesConfiguration = new()
{
    FieldHostURL = builder.Configuration["FieldHostURL"] ?? string.Empty,
    ClusterHostURL = builder.Configuration["ClusterHostURL"] ?? string.Empty,
    WellHostURL = builder.Configuration["WellHostURL"] ?? string.Empty,
    RigHostURL = builder.Configuration["RigHostURL"] ?? string.Empty,
    WellBoreHostURL = builder.Configuration["WellBoreHostURL"] ?? string.Empty,
    WellBoreArchitectureHostURL = builder.Configuration["WellBoreArchitectureHostURL"] ?? string.Empty,
    UnitConversionHostURL = builder.Configuration["UnitConversionHostURL"] ?? string.Empty,
};

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});
builder.Services.AddSingleton<IWellBoreArchitectureWebPagesConfiguration>(webPagesConfiguration);
builder.Services.AddSingleton<IWellBoreArchitectureAPIUtils, WellBoreArchitectureAPIUtils>();

var app = builder.Build();

app.UseForwardedHeaders();
app.UsePathBase("/WellBoreArchitecture/webapp");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
