using LCP.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LCP.Web.ViewComponents
{
    public class TrackingTimelineViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Pedido pedido)
        {
            return View(pedido);
        }
    }
}
