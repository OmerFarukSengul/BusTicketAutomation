using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtobusFinalProje.Data;
using OtobusFinalProje.Helpers;
using OtobusFinalProje.Models;
using OtobusFinalProje.ViewModels;
using System.Security.Claims;

namespace OtobusFinalProje.Controllers
{
    public class AuthController : BaseController
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // GİRİŞ SAYFASI
        [HttpGet]
        public IActionResult Giris()
        {
            return View(new GirisViewModel());
        }

        // GİRİŞ İŞLEMİ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Giris(GirisViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var kullanici = await _context.Kullanicilars
                .Include(x => x.Rol)
                .FirstOrDefaultAsync(x =>
                    x.Email == model.Email &&
                    x.AktifMi);

            // Kullanıcı yoksa
            if (kullanici == null)
            {
                SetError("E-posta veya şifre hatalı.");
                return View(model);
            }

            // Şifre yanlışsa
            bool sifreDogruMu = PasswordHelper.Dogrula(
                model.Sifre,
                kullanici.SifreHash);

            if (!sifreDogruMu)
            {
                SetError("E-posta veya şifre hatalı.");
                return View(model);
            }

            // CLAIMS
            var claims = new List<Claim>
            {
                // KULLANICI ID
                new Claim(
                    ClaimTypes.NameIdentifier,
                    kullanici.Id.ToString()
                ),

                // AD SOYAD
                new Claim(
                    ClaimTypes.Name,
                    kullanici.Ad + " " + kullanici.Soyad
                ),

                // EMAIL
                new Claim(
                    ClaimTypes.Email,
                    kullanici.Email
                ),

                // ROL
                new Claim(
                    ClaimTypes.Role,
                    kullanici.Rol?.RolAdi ?? "Kullanici"
                )
            };

            // COOKIE AUTH
            var identity = new ClaimsIdentity(
                claims,
                "UserCookie"
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                "UserCookie",
                principal
            );

            SetSuccess("Giriş başarılı.");

            return RedirectToAction("Index", "Home");
        }

        // KAYIT SAYFASI
        [HttpGet]
        public IActionResult Kayit()
        {
            return View(new KayitViewModel());
        }

        // KAYIT İŞLEMİ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kayit(KayitViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // EMAIL KONTROL
            bool emailVarMi = await _context.Kullanicilars
                .AnyAsync(x => x.Email == model.Email);

            if (emailVarMi)
            {
                SetError("Bu e-posta zaten kayıtlı.");
                return View(model);
            }

            // KULLANICI ROLÜ
            var kullaniciRol = await _context.Rollers
                .FirstOrDefaultAsync(x =>
                    x.RolAdi == "Kullanici");

            // ROL YOKSA OLUŞTUR
            if (kullaniciRol == null)
            {
                kullaniciRol = new Roller
                {
                    RolAdi = "Kullanici"
                };

                _context.Rollers.Add(kullaniciRol);

                await _context.SaveChangesAsync();
            }

            // YENİ KULLANICI
            var kullanici = new Kullanicilar
            {
                Ad = model.Ad,
                Soyad = model.Soyad,
                Email = model.Email,
                Telefon = model.Telefon,

                // HASHLENMİŞ ŞİFRE
                SifreHash = PasswordHelper.Hashle(model.Sifre),

                RolId = kullaniciRol.Id,

                AktifMi = true,

                KayitTarihi = DateTime.Now
            };

            _context.Kullanicilars.Add(kullanici);

            await _context.SaveChangesAsync();

            SetSuccess("Kayıt başarılı. Şimdi giriş yapabilirsin.");

            return RedirectToAction("Giris");
        }

        // ÇIKIŞ
        public async Task<IActionResult> Cikis()
        {
            await HttpContext.SignOutAsync("UserCookie");

            return RedirectToAction("Index", "Home");
        }
    }
}