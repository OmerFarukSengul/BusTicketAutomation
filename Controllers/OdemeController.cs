using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OtobusFinalProje.Data;
using OtobusFinalProje.Models;
using OtobusFinalProje.ViewModels;
using System.Globalization;
using System.Security.Claims;

namespace OtobusFinalProje.Controllers
{
    public class OdemeController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<OdemeController> _logger;
        bool fakeMode = false;

        public OdemeController(AppDbContext context, IConfiguration config, ILogger<OdemeController> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> Baslat(decimal tutar)
        {
            if (fakeMode)
            {
                return RedirectToAction("Basarili");
            }


            var modelJson = HttpContext.Session.GetString("RezModel");

            if (string.IsNullOrWhiteSpace(modelJson))
            {
                SetError("Ödeme başlatmak için rezervasyon bilgisi bulunamadı.");
                return RedirectToAction("Index", "Home");
            }

            var kullaniciIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(kullaniciIdStr, out int kullaniciId))
            {
                SetError("Ödeme yapmak için giriş yapmalısınız.");
                return RedirectToAction("Giris", "Auth");
            }

            var kullanici = await _context.Kullanicilars.FirstOrDefaultAsync(x => x.Id == kullaniciId);

            if (kullanici == null)
            {
                SetError("Kullanıcı bulunamadı.");
                return RedirectToAction("Giris", "Auth");
            }

            var options = GetIyzicoOptions();

            var request = new CreateCheckoutFormInitializeRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = Guid.NewGuid().ToString("N"),
                Price = tutar.ToString("0.00", CultureInfo.InvariantCulture),
                PaidPrice = tutar.ToString("0.00", CultureInfo.InvariantCulture),
                Currency = Currency.TRY.ToString(),
                BasketId = Guid.NewGuid().ToString("N"),
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = $"{Request.Scheme}://{Request.Host}/Odeme/Basarili"
            };

            request.Buyer = new Buyer
            {
                Id = kullanici.Id.ToString(),
                Name = kullanici.Ad,
                Surname = kullanici.Soyad,
                Email = kullanici.Email,
                GsmNumber = kullanici.Telefon ?? "5000000000",
                IdentityNumber = "11111111111",
                RegistrationAddress = "Test Adres",
                Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                City = "Istanbul",
                Country = "Turkey"
            };
            request.BillingAddress = new Address
            {
                ContactName = kullanici.Ad + " " + kullanici.Soyad,
                City = "Istanbul",
                Country = "Turkey",
                Description = "Test adres",
                ZipCode = "34000"
            };

            request.ShippingAddress = new Address
            {
                ContactName = kullanici.Ad + " " + kullanici.Soyad,
                City = "Istanbul",
                Country = "Turkey",
                Description = "Test adres",
                ZipCode = "34000"
            };

            request.BasketItems = new List<BasketItem>
            {
                new BasketItem
                {
                    Id = "BILET-" + Guid.NewGuid().ToString("N")[..8],
                    Name = "Otobüs Bileti",
                    Category1 = "Bilet",
                    ItemType = BasketItemType.VIRTUAL.ToString(),
                    Price = tutar.ToString("0.00", CultureInfo.InvariantCulture)
                }
            };

            var result = await CheckoutFormInitialize.Create(request, options);

            if (result == null || result.Status != "success")
            {
                var hata = result?.ErrorMessage ?? "iyzico boş cevap döndü.";
                SetError("Ödeme ekranı başlatılamadı: " + hata);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Html = result.CheckoutFormContent;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Basarili(string token)
        {
            try
            {
                var modelJson = HttpContext.Session.GetString("RezModel");

                if (string.IsNullOrEmpty(modelJson))
                {
                    SetError("Rezervasyon bilgisi bulunamadı.");
                    return RedirectToAction("Index", "Home");
                }


                if (string.IsNullOrEmpty(modelJson))
                {
                    SetError("Rezervasyon bilgisi bulunamadı.");
                    return RedirectToAction("Index", "Home");
                }

                var model = JsonConvert.DeserializeObject<RezervasyonOlusturViewModel>(modelJson);

                HttpContext.Session.Remove("RezModel");
                if (model == null)
                {
                    SetError("Rezervasyon bilgisi okunamadı.");
                    return RedirectToAction("Index", "Home");
                }

                if (!string.IsNullOrWhiteSpace(token))
                {
                    var retrieveRequest = new RetrieveCheckoutFormRequest
                    {
                        Locale = Locale.TR.ToString(),
                        ConversationId = Guid.NewGuid().ToString("N"),
                        Token = token
                    };

                    var paymentResult = await CheckoutForm.Retrieve(retrieveRequest, GetIyzicoOptions());

                    if (paymentResult == null || paymentResult.Status != "success")
                    {
                        SetError("Ödeme doğrulanamadı.");
                        return RedirectToAction("Olustur", "Rezervasyon", new
                        {
                            seferId = model.SeferId,
                            binisDurakId = model.BinisDurakId,
                            inisDurakId = model.InisDurakId
                        });
                    }
                }

                var kullaniciIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(kullaniciIdStr, out int kullaniciId))
                {
                    SetError("Rezervasyon oluşturmak için giriş yapmalısınız.");
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
                    return RedirectToAction("Olustur", "Rezervasyon", new { seferId = model.SeferId });
                }

                bool doluMu = await KoltukCakisiyorMu(
                    model.SeferId,
                    model.SecilenKoltukNo,
                    binisDurak.SiraNo,
                    inisDurak.SiraNo
                );

                if (doluMu)
                {
                    SetError("Ödeme sırasında bu koltuk başka biri tarafından alınmış.");
                    return RedirectToAction("Olustur", "Rezervasyon", new
                    {
                        seferId = model.SeferId,
                        binisDurakId = model.BinisDurakId,
                        inisDurakId = model.InisDurakId
                    });
                }

                var pnr = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

                var rezervasyon = new Rezervasyonlar
                {
                    KullaniciId = kullaniciId,
                    SeferId = model.SeferId,
                    PnrNo = pnr,
                    YolcuAd = kullanici.Ad,
                    YolcuSoyad = kullanici.Soyad,
                    YolcuTelefon = kullanici.Telefon,
                    YolcuEmail = kullanici.Email,
                    ToplamTutar = sefer.Fiyat,
                    Durum = "Onaylandı",
                    RezervasyonTarihi = DateTime.Now
                };

                _context.Rezervasyonlars.Add(rezervasyon);
                await _context.SaveChangesAsync();

                var koltuk = new RezervasyonKoltuklari
                {
                    RezervasyonId = rezervasyon.Id,
                    KoltukNo = model.SecilenKoltukNo,
                    BinisDurakId = model.BinisDurakId,
                    InisDurakId = model.InisDurakId
                };

                _context.RezervasyonKoltuklaris.Add(koltuk);
                await _context.SaveChangesAsync();

                SetSuccess($"Ödeme başarılı. Rezervasyon oluşturuldu. PNR: {pnr}");
                return RedirectToAction("Detay", "Rezervasyon", new { id = rezervasyon.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödeme sonrası rezervasyon oluşturulurken hata oluştu.");
                SetError("Ödeme sonrası rezervasyon oluşturulurken hata oluştu.");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult Basarili()
        {
            return RedirectToAction("Index", "Home");
        }

        private Options GetIyzicoOptions()
        {
            var apiKey = _config["iyzico:ApiKey"];
            var secretKey = _config["iyzico:SecretKey"];
            var baseUrl = _config["iyzico:BaseUrl"];

            if (string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(secretKey) ||
                string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new Exception("iyzico ayarları eksik. appsettings.json içinde ApiKey, SecretKey ve BaseUrl kontrol et.");
            }

            return new Options
            {
                ApiKey = apiKey,
                SecretKey = secretKey,
                BaseUrl = baseUrl
            };
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