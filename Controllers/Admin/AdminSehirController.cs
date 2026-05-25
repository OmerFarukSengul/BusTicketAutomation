using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtobusFinalProje.Data;
using OtobusFinalProje.Models;
using OtobusFinalProje.ViewModels;

namespace OtobusFinalProje.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Sehir")]
    public class AdminSehirController : Controller
    {
        private readonly AppDbContext _context;

        public AdminSehirController(AppDbContext context)
        {
            _context = context;
        }

        // =======================
        // LİSTELEME
        // =======================
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var sehirler = await _context.Sehirlers
                .OrderBy(x => x.SehirAdi)
                .ToListAsync();

            return View(sehirler);
        }

        // =======================
        // EKLE
        // =======================
        [HttpPost("Ekle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(string sehirAdi)
        {
            if (string.IsNullOrWhiteSpace(sehirAdi))
            {
                TempData["Hata"] = "Şehir adı boş bırakılamaz.";
                return RedirectToAction("Index");
            }

            string normalized = sehirAdi.Trim().ToLower();

            bool varMi = await _context.Sehirlers
                .AnyAsync(x => x.SehirAdi.ToLower() == normalized);

            if (varMi)
            {
                TempData["Hata"] = "Bu şehir zaten mevcut.";
                return RedirectToAction("Index");
            }

            var yeniSehir = new Sehirler
            {
                SehirAdi = sehirAdi.Trim(),
                AktifMi = true
            };

            _context.Sehirlers.Add(yeniSehir);
            await _context.SaveChangesAsync();

            TempData["Basari"] = "Şehir başarıyla eklendi.";
            return RedirectToAction("Index");
        }

        // =======================
        // SİL (SOFT DELETE)
        // =======================
        [HttpPost("Sil/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var sehir = await _context.Sehirlers.FindAsync(id);

            if (sehir == null)
            {
                TempData["Hata"] = "Şehir bulunamadı.";
                return RedirectToAction("Index");
            }

            // HARD DELETE YOK → SOFT DELETE
            sehir.AktifMi = false;

            _context.Sehirlers.Update(sehir);
            await _context.SaveChangesAsync();

            TempData["Basari"] = "Şehir pasife alındı.";
            return RedirectToAction("Index");
        }

        // =======================
        // DÜZENLE SAYFASI
        // =======================
        [HttpGet("Duzenle/{id}")]
        public async Task<IActionResult> Duzenle(int id)
        {
            var sehir = await _context.Sehirlers.FindAsync(id);

            if (sehir == null)
                return NotFound();

            var model = new SehirViewModel
            {
                Id = sehir.Id,
                SehirAdi = sehir.SehirAdi,
                AktifMi = sehir.AktifMi
            };

            return View(model);
        }

        // =======================
        // GÜNCELLE
        // =======================
        [HttpPost("Duzenle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(SehirViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var sehir = await _context.Sehirlers.FindAsync(model.Id);

            if (sehir == null)
                return NotFound();

            string normalized = model.SehirAdi.Trim().ToLower();

            bool varMi = await _context.Sehirlers
                .AnyAsync(x => x.Id != model.Id &&
                               x.SehirAdi.ToLower() == normalized);

            if (varMi)
            {
                TempData["Hata"] = "Bu şehir adı zaten mevcut.";
                return View(model);
            }

            sehir.SehirAdi = model.SehirAdi.Trim();
            sehir.AktifMi = model.AktifMi;

            _context.Sehirlers.Update(sehir);
            await _context.SaveChangesAsync();

            TempData["Basari"] = "Şehir güncellendi.";
            return RedirectToAction("Index");
        }
    }
}