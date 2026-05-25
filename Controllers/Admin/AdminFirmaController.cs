using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtobusFinalProje.Data;
using OtobusFinalProje.Models;

namespace OtobusFinalProje.Controllers.Admin
{
    [Route("Admin/Firma")]
    public class AdminFirmaController : Controller
    {
        private readonly AppDbContext _context;

        public AdminFirmaController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var firmalar = await _context.Firmalars
                .OrderBy(x => x.FirmaAdi)
                .ToListAsync();

            return View(firmalar);
        }

        [HttpPost("Ekle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(string firmaAdi, string? telefon, string? email)
        {
            if (string.IsNullOrWhiteSpace(firmaAdi))
            {
                TempData["Hata"] = "Firma adı boş olamaz.";
                return RedirectToAction(nameof(Index));
            }

            var varMi = await _context.Firmalars
                .AnyAsync(x => x.FirmaAdi.ToLower() == firmaAdi.Trim().ToLower());

            if (varMi)
            {
                TempData["Hata"] = "Bu firma zaten kayıtlı.";
                return RedirectToAction(nameof(Index));
            }

            _context.Firmalars.Add(new Firmalar
            {
                FirmaAdi = firmaAdi.Trim(),
                Telefon = telefon,
                Email = email,
                AktifMi = true
            });

            await _context.SaveChangesAsync();

            TempData["Basari"] = "Firma eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("DurumDegistir/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DurumDegistir(int id)
        {
            var firma = await _context.Firmalars.FindAsync(id);

            if (firma == null)
            {
                TempData["Hata"] = "Firma bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            firma.AktifMi = !firma.AktifMi;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}