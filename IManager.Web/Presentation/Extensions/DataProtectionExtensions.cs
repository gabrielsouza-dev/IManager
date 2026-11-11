using Microsoft.AspNetCore.DataProtection;

namespace IManager.Web.Presentation.Extensions;

public static class DataProtectionExtensions
{
    public static void AddDataProtectionConfig(
    this WebApplicationBuilder builder)
    {
        builder.Services.AddDataProtection()
            // no container o content root é /app (volume ./keys:/app/keys); localmente, IManager.Web/keys
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")))
            .SetApplicationName("IManager");
    }
}