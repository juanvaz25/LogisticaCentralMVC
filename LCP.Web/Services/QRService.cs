using QRCoder;

namespace LCP.Web.Services
{
    public interface IQRService
    {
        string GenerarQrBase64(string texto);
    }

    public class QRService : IQRService
    {
        public string GenerarQrBase64(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(texto, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(20);
            return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
        }
    }
}
