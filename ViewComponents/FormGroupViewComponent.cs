using Microsoft.AspNetCore.Mvc;
using MuhasebeStokWebApp.ViewModels.Shared;

namespace MuhasebeStokWebApp.ViewComponents
{
    public class FormGroupViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(FormGroupViewModel model)
        {
            return View(model);
        }
    }
} 