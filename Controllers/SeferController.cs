using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtobusFinalProje.Data;
using OtobusFinalProje.ViewModels;

namespace OtobusFinalProje.Controllers
{
    public class SeferController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SeferController> _logger;

        public SeferController(AppDbContext context, ILogger<SeferController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Listele(SeferAraViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SetError("Lütfen arama bilgilerini doğru giriniz.");
                return RedirectToAction("Index", "Home");
            }

            if (model.NeredenSehirId == model.NereyeSehirId)
            {
                SetError("Nereden ve nereye şehir aynı olamaz.");
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var seferler = await _context.Seferlers
                    .Include(x => x.Firma)
                    .Include(x => x.Otobus)
                    .Include(x => x.SeferDuraklaris)
                        .ThenInclude(x => x.Sehir)
                    .Where(x =>
                        x.AktifMi &&
                        x.KalkisTarihiSaat.Date == model.Tarih.Date)
                    .OrderBy(x => x.KalkisTarihiSaat)
                    .ToListAsync();

                var liste = new List<SeferListeViewModel>();

                foreach (var sefer in seferler)
                {
                    var duraklar = sefer.SeferDuraklaris
                        .Where(x => x.AktifMi)
                        .OrderBy(x => x.SiraNo)
                        .ToList();

                    var binis = duraklar.FirstOrDefault(x => x.SehirId == model.NeredenSehirId);
                    var inis = duraklar.FirstOrDefault(x => x.SehirId == model.NereyeSehirId);

                    if (binis == null || inis == null || binis.SiraNo >= inis.SiraNo)
                        continue;

                    var doluKoltukSayisi = await _context.RezervasyonKoltuklaris
                        .Include(x => x.Rezervasyon)
                        .Include(x => x.BinisDurak)
                        .Include(x => x.InisDurak)
                        .Where(x =>
                            x.Rezervasyon.SeferId == sefer.Id &&
                            x.BinisDurakId != null &&
                            x.InisDurakId != null)
                        .ToListAsync();

                    var doluSayisi = doluKoltukSayisi
                        .Where(x =>
                            x.BinisDurak != null &&
                            x.InisDurak != null &&
                            binis.SiraNo < x.InisDurak.SiraNo &&
                            inis.SiraNo > x.BinisDurak.SiraNo)
                        .Select(x => x.KoltukNo)
                        .Distinct()
                        .Count();

                    liste.Add(new SeferListeViewModel
                    {
                        SeferId = sefer.Id,
                        BinisDurakId = binis.Id,
                        InisDurakId = inis.Id,
                        FirmaAdi = sefer.Firma?.FirmaAdi ?? "Bilinmiyor",
                        Nereden = binis.Sehir?.SehirAdi ?? "",
                        Nereye = inis.Sehir?.SehirAdi ?? "",
                        KalkisTarihiSaat = sefer.KalkisTarihiSaat,
                        VarisTarihiSaat = sefer.VarisTarihiSaat,
                        Fiyat = sefer.Fiyat,
                        KoltukSayisi = sefer.Otobus?.KoltukSayisi ?? 0,
                        DoluKoltukSayisi = doluSayisi
                    });
                }

                if (!liste.Any())
                    SetWarning("Seçtiğiniz kriterlere uygun sefer bulunamadı.");

                ViewBag.AramaTarih = model.Tarih.ToString("dd.MM.yyyy");

                return View(liste);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Seferler listelenirken hata oluştu.");
                SetError("Seferler listelenirken bir hata oluştu.");
                return RedirectToAction("Index", "Home");
            }
        }
    }
}