using Microsoft.AspNetCore.Mvc;

namespace OtobusFinalProje.Controllers
{
    public class BaseController : Controller
    {
        protected void SetSuccess(string message)
        {
            TempData["Basari"] = message;
        }

        protected void SetError(string message)
        {
            TempData["Hata"] = message;
        }

        protected void SetWarning(string message)
        {
            TempData["Uyari"] = message;
        }
    }
}