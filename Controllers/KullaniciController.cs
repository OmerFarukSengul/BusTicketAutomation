using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtobusFinalProje.Data;
using System.Security.Claims;

namespace OtobusFinalProje.Controllers
{
    [Authorize]
    public class KullaniciController : Controller
    {
        private readonly AppDbContext _context;

        public KullaniciController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Rezervasyonlar()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdStr, out int userId))
                return RedirectToAction("Giris", "Auth");

            var rezervasyonlar = await _context.Rezervasyonlars
                .Include(x => x.Sefer)
                    .ThenInclude(s => s.Firma)
                .Include(x => x.Sefer)
                    .ThenInclude(s => s.Rota)
                        .ThenInclude(r => r.NeredenSehir)
                .Include(x => x.Sefer)
                    .ThenInclude(s => s.Rota)
                        .ThenInclude(r => r.NereyeSehir)
                .Include(x => x.RezervasyonKoltuklaris)
                    .ThenInclude(k => k.BinisDurak)
                        .ThenInclude(d => d.Sehir)
                .Include(x => x.RezervasyonKoltuklaris)
                    .ThenInclude(k => k.InisDurak)
                        .ThenInclude(d => d.Sehir)
                .Where(x => x.KullaniciId == userId)
                .OrderByDescending(x => x.RezervasyonTarihi)
                .ToListAsync();

            return View(rezervasyonlar);
        }
    }
}