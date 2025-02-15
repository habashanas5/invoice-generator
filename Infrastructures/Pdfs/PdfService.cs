using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace Invoice_Generator.Infrastructures.Pdfs
{
    public class PdfService : IPdfService
    {
        private readonly IConverter _converter;

        public PdfService(IConverter converter)
        {
            _converter = converter;
        }

        public byte[] CreatePdfFromHtml(string htmlContent, string documentTitle)
        {
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                DocumentTitle = documentTitle,
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8" },
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            var file = _converter.Convert(pdf); // Ensure _converter is not null
            return file;
        }

        public byte[] CreatePdfFromPage(string pageUrl, string documentTitle)
        {
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                DocumentTitle = documentTitle,
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                Page = pageUrl,
                WebSettings = { DefaultEncoding = "utf-8" },
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            var file = _converter.Convert(pdf); // Ensure _converter is not null
            return file;
        }
    }
}
