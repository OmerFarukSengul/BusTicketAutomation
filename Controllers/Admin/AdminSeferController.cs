using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OtobusFinalProje.Data;
using OtobusFinalProje.Models;

namespace OtobusFinalProje.Controllers.Admin
{
    [Route("Admin/Sefer")]
    public class AdminSeferController : Controller
    {
        private readonly AppDbContext _context;

        public AdminSeferController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var seferler = await _context.Seferlers
                .Include(x => x.Rota)
                    .ThenInclude(x => x.NeredenSehir)
                .Include(x => x.Rota)
                    .ThenInclude(x => x.NereyeSehir)
                .Include(x => x.Firma)
                .Include(x => x.Otobus)
                .OrderByDescending(x => x.KalkisTarihiSaat)
                .ToListAsync();

            return View("~/Views/AdminSefer/Index.cshtml", seferler);
        }

        [HttpGet("Ekle")]
        public async Task<IActionResult> Ekle()
        {
            await DropdownlariDoldur();
            return View("~/Views/AdminSefer/Ekle.cshtml");
        }

        [HttpPost("Ekle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(Seferler model)
        {
            if (model.RotaId <= 0 || model.FirmaId <= 0 || model.OtobusId <= 0)
            {
                TempData["Hata"] = "Rota, firma ve otobüs seçilmelidir.";
                return RedirectToAction(nameof(Ekle));
            }

            if (model.VarisTarihiSaat <= model.KalkisTarihiSaat)
            {
                TempData["Hata"] = "Varış tarihi kalkıştan sonra olmalıdır.";
                return RedirectToAction(nameof(Ekle));
            }

            var otobus = await _context.Otobuslers.FirstOrDefaultAsync(x => x.Id == model.OtobusId);

            if (otobus == null || otobus.FirmaId != model.FirmaId)
            {
                TempData["Hata"] = "Seçilen otobüs bu firmaya ait değil.";
                return RedirectToAction(nameof(Ekle));
            }

            var rotaDuraklari = await _context.RotaDuraklaris
                .Where(x => x.RotaId == model.RotaId && x.AktifMi)
                .OrderBy(x => x.SiraNo)
                .ToListAsync();

            if (rotaDuraklari.Count < 2)
            {
                TempData["Hata"] = "Seçilen rotanın en az 2 durağı olmalıdır.";
                return RedirectToAction(nameof(Ekle));
            }

            var sefer = new Seferler
            {
                RotaId = model.RotaId,
                FirmaId = model.FirmaId,
                OtobusId = model.OtobusId,
                KalkisTarihiSaat = model.KalkisTarihiSaat,
                VarisTarihiSaat = model.VarisTarihiSaat,
                Fiyat = model.Fiyat,
                Aciklama = model.Aciklama,
                AktifMi = true,
                OlusturmaTarihi = DateTime.Now
            };

            _context.Seferlers.Add(sefer);
            await _context.SaveChangesAsync();

            foreach (var durak in rotaDuraklari)
            {
                _context.SeferDuraklaris.Add(new SeferDuraklari
                {
                    SeferId = sefer.Id,
                    SehirId = durak.SehirId,
                    SiraNo = durak.SiraNo,
                    VarisSaati = null,
                    KalkisSaati = null,
                    AktifMi = true
                });
            }

            await _context.SaveChangesAsync();

            TempData["Basari"] = "Sefer ve sefer durakları başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        private async Task DropdownlariDoldur()
        {
            var rotalar = await _context.Rotalars
                .Include(x => x.NeredenSehir)
                .Include(x => x.NereyeSehir)
                .Where(x => x.AktifMi)
                .ToListAsync();

            var firmalar = await _context.Firmalars
                .Where(x => x.AktifMi)
                .OrderBy(x => x.FirmaAdi)
                .ToListAsync();

            var otobusler = await _context.Otobuslers
                .Include(x => x.Firma)
                .Where(x => x.AktifMi)
                .OrderBy(x => x.Plaka)
                .ToListAsync();

            ViewBag.Rotalar = new SelectList(
                rotalar.Select(x => new
                {
                    x.Id,
                    Ad = x.NeredenSehir.SehirAdi + " → " + x.NereyeSehir.SehirAdi
                }),
                "Id",
                "Ad"
            );

            ViewBag.Firmalar = new SelectList(firmalar, "Id", "FirmaAdi");

            ViewBag.Otobusler = new SelectList(
                otobusler.Select(x => new
                {
                    x.Id,
                    Ad = x.Plaka + " - " + x.Firma.FirmaAdi
                }),
                "Id",
                "Ad"
            );
        }
    }
}