using System.Text.Json;
using LCP.Web.Models.ViewModels.Pedidos;
using Microsoft.AspNetCore.Mvc;

namespace LCP.Web.ViewComponents
{
    public class CarritoBadgeViewComponent : ViewComponent
    {
        private const string SessionCartKey = "LCP_CART_SESSION";

        public IViewComponentResult Invoke()
        {
            var cartJson = HttpContext.Session.GetString(SessionCartKey);
            int count = 0;

            if (!string.IsNullOrEmpty(cartJson))
            {
                var items = JsonSerializer.Deserialize<List<CarritoItemViewModel>>(cartJson);
                count = items?.Sum(i => i.Cantidad) ?? 0;
            }

            return View(count);
        }
    }
}
