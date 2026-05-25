using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OtobusFinalProje.Data;
using OtobusFinalProje.Models;

namespace OtobusFinalProje.Controllers.Admin
{
    [Route("Admin/Rota")]
    public class AdminRotaController : Controller
    {
        private readonly AppDbContext _context;

        public AdminRotaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var rotalar = await _context.Rotalars
                .Include(x => x.NeredenSehir)
                .Include(x => x.NereyeSehir)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return View("~/Views/AdminRota/Index.cshtml", rotalar);
        }

        [HttpGet("Ekle")]
        public async Task<IActionResult> Ekle()
        {
            await SehirleriDoldur();
            return View("~/Views/AdminRota/Ekle.cshtml");
        }

        [HttpPost("Ekle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(List<int> durakSehirIdleri, int? mesafeKm, int? tahminiSureDakika)
        {
            durakSehirIdleri = durakSehirIdleri
                .Where(x => x > 0)
                .ToList();

            if (durakSehirIdleri.Count < 2)
            {
                TempData["Hata"] = "En az 2 şehir seçmelisiniz.";
                return RedirectToAction(nameof(Ekle));
            }

            if (durakSehirIdleri.Distinct().Count() != durakSehirIdleri.Count)
            {
                TempData["Hata"] = "Aynı şehir rotada birden fazla seçilemez.";
                return RedirectToAction(nameof(Ekle));
            }

            bool rotaVarMi = await _context.Rotalars.AnyAsync(x =>
                x.NeredenSehirId == durakSehirIdleri.First() &&
                x.NereyeSehirId == durakSehirIdleri.Last());

            if (rotaVarMi)
            {
                TempData["Hata"] = "Bu başlangıç ve bitişe sahip rota zaten var.";
                return RedirectToAction(nameof(Ekle));
            }

            var rota = new Rotalar
            {
                NeredenSehirId = durakSehirIdleri.First(),
                NereyeSehirId = durakSehirIdleri.Last(),
                MesafeKm = mesafeKm,
                TahminiSureDakika = tahminiSureDakika,
                AktifMi = true
            };

            _context.Rotalars.Add(rota);
            await _context.SaveChangesAsync();

            for (int i = 0; i < durakSehirIdleri.Count; i++)
            {
                _context.RotaDuraklaris.Add(new RotaDuraklari
                {
                    RotaId = rota.Id,
                    SehirId = durakSehirIdleri[i],
                    SiraNo = i + 1,
                    AktifMi = true
                });
            }

            await _context.SaveChangesAsync();

            TempData["Basari"] = "Rota başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Detay/{id}")]
        public async Task<IActionResult> Detay(int id)
        {
            var rota = await _context.Rotalars
                .Include(x => x.NeredenSehir)
                .Include(x => x.NereyeSehir)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (rota == null)
                return NotFound();

            ViewBag.Duraklar = await _context.RotaDuraklaris
                .Include(x => x.Sehir)
                .Where(x => x.RotaId == id)
                .OrderBy(x => x.SiraNo)
                .ToListAsync();

            return View("~/Views/AdminRota/Detay.cshtml", rota);
        }

        private async Task SehirleriDoldur()
        {
            var sehirler = await _context.Sehirlers
                .Where(x => x.AktifMi)
                .OrderBy(x => x.SehirAdi)
                .ToListAsync();

            ViewBag.Sehirler = new SelectList(sehirler, "Id", "SehirAdi");
        }
    }
}