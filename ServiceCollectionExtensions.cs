using Invoice_Generator.Applications;
using Invoice_Generator.Infrastructures.Currencies;
using Invoice_Generator.Infrastructures.Emails;
using Invoice_Generator.Infrastructures.Pdfs;
using Invoice_Generator.Models;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Invoice_Generator
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAllCustomServices(this IServiceCollection services)
        {
            services.AddScoped<IEmailSender, SMTPEmailService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IPdfService, PdfService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<LoggingService>();

            return services;
        }
    }
}
