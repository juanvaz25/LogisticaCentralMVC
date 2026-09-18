using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace LCP.Web.Services
{
    public interface IStorageService
    {
        Task<string?> GuardarArchivoAsync(IFormFile? archivo, string subCarpeta);
        Task EliminarArchivoAsync(string? rutaRelativa);
    }

    public class StorageService : IStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string[] _extensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };

        public StorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string?> GuardarArchivoAsync(IFormFile? archivo, string subCarpeta)
        {
            if (archivo == null || archivo.Length == 0)
                return null;

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!_extensionesPermitidas.Contains(extension))
                throw new InvalidOperationException("Formato de archivo no permitido. Solo se aceptan imágenes (.jpg, .jpeg, .png, .webp).");

            var carpetaDestino = Path.Combine(_env.WebRootPath, "uploads", subCarpeta);
            if (!Directory.Exists(carpetaDestino))
            {
                Directory.CreateDirectory(carpetaDestino);
            }

            var nombreArchivoUnico = $"{Guid.NewGuid():N}{extension}";
            var rutaCompleta = Path.Combine(carpetaDestino, nombreArchivoUnico);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return $"/uploads/{subCarpeta}/{nombreArchivoUnico}";
        }

        public Task EliminarArchivoAsync(string? rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa))
                return Task.CompletedTask;

            try
            {
                var rutaLimpia = rutaRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var rutaCompleta = Path.Combine(_env.WebRootPath, rutaLimpia);

                if (File.Exists(rutaCompleta))
                {
                    File.Delete(rutaCompleta);
                }
            }
            catch
            {
                // Ignorar error al borrar archivo si no existe
            }

            return Task.CompletedTask;
        }
    }
}
