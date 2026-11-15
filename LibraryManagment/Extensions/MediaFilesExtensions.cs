using LibraryManagement.Models.Options;
using LibraryManagement.Services.Media;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace LibraryManagement.API.Extensions
{
    public static class MediaFilesExtensions
    {
        // Serves uploaded images (Storage:Provider = Local) from Storage:Local:RootPath at Storage:Local:RequestPath
        public static WebApplication UseMediaFiles(this WebApplication app)
        {
            var options = app.Services.GetRequiredService<IOptions<StorageOptions>>().Value;
            if (!string.Equals(options.Provider, "Local", StringComparison.OrdinalIgnoreCase))
            {
                return app;
            }

            var root = LocalMediaStorage.ResolveRootPath(options.Local, app.Environment.ContentRootPath);
            Directory.CreateDirectory(root);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(root),
                RequestPath = "/" + options.Local.RequestPath.Trim('/'),
                OnPrepareResponse = context =>
                {
                    // File names are unique per upload, so browsers may cache them for long
                    context.Context.Response.Headers.CacheControl = "public, max-age=2592000";
                    context.Context.Response.Headers.XContentTypeOptions = "nosniff";
                }
            });

            return app;
        }
    }
}
