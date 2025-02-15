using Microsoft.AspNetCore.Mvc.Rendering;

namespace Invoice_Generator.Infrastructures.Currencies
{
    public interface ICurrencyService
    {
        ICollection<SelectListItem> GetCurrencies();
    }
}
