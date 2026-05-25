using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OtobusFinalProje.Data;
using OtobusFinalProje.Models;
using OtobusFinalProje.ViewModels;

namespace OtobusFinalProje.Controllers
{
    public class RezervasyonController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RezervasyonController> _logger;

        public RezervasyonController(AppDbContext context, ILogger<RezervasyonController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Olustur(int seferId, int? binisDurakId = null, int? inisDurakId = null)
        {
            try
            {
                var sefer = await _context.Seferlers
                    .Include(x => x.Firma)
                    .Include(x => x.Otobus)
                    .Include(x => x.Rota).ThenInclude(r => r.NeredenSehir)
                    .Include(x => x.Rota).ThenInclude(r => r.NereyeSehir)
                    .FirstOrDefaultAsync(x => x.Id == seferId);

                if (sefer == null)
                {
                    SetError("Sefer bulunamadı.");
                    return RedirectToAction("Index", "Home");
                }

                var duraklar = await _context.SeferDuraklaris
                    .Include(x => x.Sehir)
                    .Where(x => x.SeferId == seferId && x.AktifMi)
                    .OrderBy(x => x.SiraNo)
                    .ToListAsync();

                if (duraklar.Count < 2)
                {
                    SetError("Bu sefer için durak bilgisi bulunamadı.");
                    return RedirectToAction("Index", "Home");
                }

                int seciliBinis = binisDurakId ?? duraklar.First().Id;
                int seciliInis = inisDurakId ?? duraklar.Last().Id;

                var model = new RezervasyonOlusturViewModel
                {
                    SeferId = sefer.Id,
                    FirmaAdi = sefer.Firma?.FirmaAdi ?? "",
                    Nereden = sefer.Rota?.NeredenSehir?.SehirAdi ?? "",
                    Nereye = sefer.Rota?.NereyeSehir?.SehirAdi ?? "",
                    KalkisTarihiSaat = sefer.KalkisTarihiSaat,
                    VarisTarihiSaat = sefer.VarisTarihiSaat,
                    Fiyat = sefer.Fiyat,
                    KoltukSayisi = sefer.Otobus?.KoltukSayisi ?? 0,
                    BinisDurakId = seciliBinis,
                    InisDurakId = seciliInis,
                    DoluKoltuklar = await DoluKoltuklariGetir(seferId, seciliBinis, seciliInis),
                    Duraklar = duraklar.Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = $"{x.SiraNo}. {x.Sehir.SehirAdi}"
                    }).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rezervasyon sayfası açılırken hata oluştu.");
                SetError("Rezervasyon sayfası açılırken hata oluştu.");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Olustur(RezervasyonOlusturViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SetError("Lütfen biniş durağı, iniş durağı ve koltuk seçiniz.");
                return RedirectToAction("Olustur", new
                {
                    seferId = model.SeferId,
                    binisDurakId = model.BinisDurakId,
                    inisDurakId = model.InisDurakId
                });
            }

            try
            {
                var kullaniciIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(kullaniciIdStr, out int kullaniciId))
                {
                    SetError("Rezervasyon yapmak için giriş yapmalısınız.");
                    return RedirectToAction("Giris", "Auth");
                }

                var kullanici = await _context.Kullanicilars.FirstOrDefaultAsync(x => x.Id == kullaniciId);

                if (kullanici == null)
                {
                    SetError("Kullanıcı bulunamadı.");
                    return RedirectToAction("Giris", "Auth");
                }

                var sefer = await _context.Seferlers
                    .Include(x => x.Otobus)
                    .FirstOrDefaultAsync(x => x.Id == model.SeferId);

                if (sefer == null)
                {
                    SetError("Sefer bulunamadı.");
                    return RedirectToAction("Index", "Home");
                }

                var binisDurak = await _context.SeferDuraklaris
                    .FirstOrDefaultAsync(x => x.Id == model.BinisDurakId && x.SeferId == model.SeferId);

                var inisDurak = await _context.SeferDuraklaris
                    .FirstOrDefaultAsync(x => x.Id == model.InisDurakId && x.SeferId == model.SeferId);

                if (binisDurak == null || inisDurak == null || binisDurak.SiraNo >= inisDurak.SiraNo)
                {
                    SetError("Geçersiz durak seçimi.");
                    return RedirectToAction("Olustur", new { seferId = model.SeferId });
                }

                bool doluMu = await KoltukCakisiyorMu(
                    model.SeferId,
                    model.SecilenKoltukNo,
                    binisDurak.SiraNo,
                    inisDurak.SiraNo
                );

                if (doluMu)
                {
                    SetError("Bu koltuk seçtiğiniz durak aralığında dolu.");
                    return RedirectToAction("Olustur", new
                    {
                        seferId = model.SeferId,
                        binisDurakId = model.BinisDurakId,
                        inisDurakId = model.InisDurakId
                    });
                }

                HttpContext.Session.SetString("RezModel", JsonConvert.SerializeObject(model));
                return RedirectToAction("Baslat", "Odeme", new { tutar = sefer.Fiyat });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rezervasyon ödeme öncesi hazırlanırken hata oluştu.");
                SetError("Rezervasyon hazırlanırken bir hata oluştu.");
                return RedirectToAction("Olustur", new { seferId = model.SeferId });
            }
        }

        [HttpGet]
        public IActionResult Sorgula()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sorgula(string pnrNo)
        {
            if (string.IsNullOrWhiteSpace(pnrNo))
            {
                SetError("PNR numarası boş olamaz.");
                return View();
            }

            pnrNo = pnrNo.Trim().ToUpper();

            var rezervasyon = await _context.Rezervasyonlars
                .FirstOrDefaultAsync(x => x.PnrNo == pnrNo);

            if (rezervasyon == null)
            {
                SetError("Bu PNR numarasıyla kayıtlı rezervasyon bulunamadı.");
                return View();
            }

            return RedirectToAction("Detay", new { id = rezervasyon.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Detay(int id)
        {
            var rezervasyon = await _context.Rezervasyonlars
                .Include(x => x.Sefer).ThenInclude(x => x.Firma)
                .Include(x => x.Sefer).ThenInclude(x => x.Rota).ThenInclude(x => x.NeredenSehir)
                .Include(x => x.Sefer).ThenInclude(x => x.Rota).ThenInclude(x => x.NereyeSehir)
                .Include(x => x.RezervasyonKoltuklaris).ThenInclude(x => x.BinisDurak).ThenInclude(x => x.Sehir)
                .Include(x => x.RezervasyonKoltuklaris).ThenInclude(x => x.InisDurak).ThenInclude(x => x.Sehir)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (rezervasyon == null)
            {
                SetError("Rezervasyon bulunamadı.");
                return RedirectToAction("Index", "Home");
            }

            return View(rezervasyon);
        }

        private async Task<List<int>> DoluKoltuklariGetir(int seferId, int binisDurakId, int inisDurakId)
        {
            var binis = await _context.SeferDuraklaris.FirstOrDefaultAsync(x => x.Id == binisDurakId);
            var inis = await _context.SeferDuraklaris.FirstOrDefaultAsync(x => x.Id == inisDurakId);

            if (binis == null || inis == null || binis.SiraNo >= inis.SiraNo)
                return new List<int>();

            var koltuklar = await _context.RezervasyonKoltuklaris
                .Include(x => x.Rezervasyon)
                .Include(x => x.BinisDurak)
                .Include(x => x.InisDurak)
                .Where(x =>
                    x.Rezervasyon.SeferId == seferId &&
                    x.BinisDurakId != null &&
                    x.InisDurakId != null)
                .ToListAsync();

            return koltuklar
                .Where(x =>
                    x.BinisDurak != null &&
                    x.InisDurak != null &&
                    binis.SiraNo < x.InisDurak.SiraNo &&
                    inis.SiraNo > x.BinisDurak.SiraNo)
                .Select(x => x.KoltukNo)
                .Distinct()
                .ToList();
        }

        private async Task<bool> KoltukCakisiyorMu(int seferId, int koltukNo, int yeniBinisSira, int yeniInisSira)
        {
            var mevcutlar = await _context.RezervasyonKoltuklaris
                .Include(x => x.Rezervasyon)
                .Include(x => x.BinisDurak)
                .Include(x => x.InisDurak)
                .Where(x =>
                    x.Rezervasyon.SeferId == seferId &&
                    x.KoltukNo == koltukNo &&
                    x.BinisDurakId != null &&
                    x.InisDurakId != null)
                .ToListAsync();

            return mevcutlar.Any(x =>
                x.BinisDurak != null &&
                x.InisDurak != null &&
                yeniBinisSira < x.InisDurak.SiraNo &&
                yeniInisSira > x.BinisDurak.SiraNo);
        }
    }
}