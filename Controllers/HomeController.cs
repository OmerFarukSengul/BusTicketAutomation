using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OtobusFinalProje.Data;
using OtobusFinalProje.ViewModels;

namespace OtobusFinalProje.Controllers
{
    public class HomeController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var model = new SeferAraViewModel
                {
                    Tarih = DateTime.Today,
                    Sehirler = await _context.Sehirlers
                        .OrderBy(x => x.SehirAdi)
                        .Select(x => new SelectListItem
                        {
                            Value = x.Id.ToString(),
                            Text = x.SehirAdi
                        })
                        .ToListAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ana sayfa yüklenirken hata oluþtu.");
                SetError("Ana sayfa yüklenirken bir hata oluþtu.");
                return View(new SeferAraViewModel());
            }
        }
    }
}