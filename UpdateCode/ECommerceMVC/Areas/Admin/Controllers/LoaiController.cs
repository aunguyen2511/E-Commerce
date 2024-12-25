using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceMVC.Data;
using ECommerceMVC.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace ECommerceMVC.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]   
    public class LoaiController : Controller
    {
        private readonly Hshop2023Context _context;

        public LoaiController(Hshop2023Context context)
        {
            _context = context;
        }

        // GET: Admin/Loai
        public async Task<IActionResult> Index()
        {
            var loais = await _context.Loais.Select(l => new LoaiVM
            {
                MaLoai = l.MaLoai,
                TenLoai = l.TenLoai,
                TenLoaiAlias = l.TenLoaiAlias,
                MoTa = l.MoTa,
                Hinh = l.Hinh
            }).ToListAsync();

            return View(loais);
        }

        // GET: Admin/Loai/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Loai/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoaiVM loaiVM, IFormFile Hinh)
        {
            if (ModelState.IsValid)
            {
                var loai = new Loai
                {
                    TenLoai = loaiVM.TenLoai,
                    TenLoaiAlias = loaiVM.TenLoaiAlias,
                    MoTa = loaiVM.MoTa
                };

                // Handle file upload
                if (Hinh != null && Hinh.Length > 0)
                {
                    var fileName = Path.GetFileName(Hinh.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Hinh/Loai", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Hinh.CopyToAsync(stream);
                    }
                    loai.Hinh = fileName; // Update the file name in the database
                }

                _context.Add(loai);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(loaiVM);
        }

        // GET: Admin/Loai/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loai = await _context.Loais.FindAsync(id);
            if (loai == null)
            {
                return NotFound();
            }

            var loaiVM = new LoaiVM
            {
                MaLoai = loai.MaLoai,
                TenLoai = loai.TenLoai,
                TenLoaiAlias = loai.TenLoaiAlias,
                MoTa = loai.MoTa,
                Hinh = loai.Hinh
            };

            return View(loaiVM);
        }

        // POST: Admin/Loai/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LoaiVM loaiVM, IFormFile Hinh)
        {
            if (id != loaiVM.MaLoai)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var loai = await _context.Loais.FindAsync(id);
                    loai.TenLoai = loaiVM.TenLoai;
                    loai.TenLoaiAlias = loaiVM.TenLoaiAlias;
                    loai.MoTa = loaiVM.MoTa;

                    // Handle file upload
                    if (Hinh != null && Hinh.Length > 0)
                    {
                        var fileName = Path.GetFileName(Hinh.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Hinh/Loai", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Hinh.CopyToAsync(stream);
                        }
                        loai.Hinh = fileName; // Update the file name in the database
                    }

                    _context.Update(loai);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoaiExists(loaiVM.MaLoai))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(loaiVM);
        }

        // GET: Admin/Loai/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loai = await _context.Loais
                .FirstOrDefaultAsync(m => m.MaLoai == id);
            if (loai == null)
            {
                return NotFound();
            }

            var loaiVM = new LoaiVM
            {
                MaLoai = loai.MaLoai,
                TenLoai = loai.TenLoai,
                TenLoaiAlias = loai.TenLoaiAlias,
                MoTa = loai.MoTa,
                Hinh = loai.Hinh
            };

            return View(loaiVM);
        }

        // POST: Admin/Loai/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loai = await _context.Loais.FindAsync(id);
            _context.Loais.Remove(loai);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LoaiExists(int id)
        {
            return _context.Loais.Any(e => e.MaLoai == id);
        }
    }
}
