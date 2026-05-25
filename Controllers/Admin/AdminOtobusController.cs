using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OtobusFinalProje.Data;
using OtobusFinalProje.Models;

namespace OtobusFinalProje.Controllers.Admin
{
    [Route("Admin/Otobus")]
    public class AdminOtobusController : Controller
    {
        private readonly AppDbContext _context;

        public AdminOtobusController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var otobusler = await _context.Otobuslers
                .Include(x => x.Firma)
                .ToListAsync();

            ViewBag.Firmalar = new SelectList(_context.Firmalars, "Id", "FirmaAdi");

            return View(otobusler);
        }

        [HttpPost("Ekle")]
        public async Task<IActionResult> Ekle(Otobusler otobus)
        {
            if (string.IsNullOrWhiteSpace(otobus.Plaka))
            {
                TempData["Hata"] = "Plaka boş olamaz.";
                return RedirectToAction("Index");
            }

            _context.Otobuslers.Add(new Otobusler
            {
                FirmaId = otobus.FirmaId,
                Plaka = otobus.Plaka,
                KoltukSayisi = otobus.KoltukSayisi,
                Model = otobus.Model,
                AktifMi = true
            });

            await _context.SaveChangesAsync();

            TempData["Basari"] = "Otobüs eklendi.";
            return RedirectToAction("Index");
        }
    }
}