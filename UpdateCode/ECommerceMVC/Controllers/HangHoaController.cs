using ECommerceMVC.Data;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace ECommerceMVC.Controllers
{
    public class HangHoaController : Controller
    {
        private readonly Hshop2023Context db;
        private const int PageSize = 9; // Number of items per page

        public HangHoaController(Hshop2023Context context)
        {
            db = context;
        }

        public IActionResult Index(int? loai, int page = 1)
        {
            var hangHoas = db.HangHoas.AsQueryable();

            if (loai.HasValue)
            {
                hangHoas = hangHoas.Where(p => p.MaLoai == loai.Value);
            }

            var pagedList = hangHoas.Select(p => new HangHoaVM
            {
                MaHh = p.MaHh,
                TenHh = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                MoTaDonVi = p.MoTaDonVi ?? "",
                TenAlias = p.MaLoaiNavigation.TenLoai
            }).ToPagedList(page, PageSize);

            var viewModel = new HangHoaPagedVM
            {
                HangHoas = pagedList,
                CurrentPage = page,
                TotalPages = pagedList.PageCount,
                CurrentLoai = loai // Lưu trữ loại hiện tại
            };

            return View(viewModel);
        }

        public IActionResult Search(string? query, int page = 1)
        {
            var hangHoas = db.HangHoas.AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                hangHoas = hangHoas.Where(p => p.TenHh.Contains(query));
            }

            var pagedList = hangHoas.Select(p => new HangHoaVM
            {
                MaHh = p.MaHh,
                TenHh = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                MoTaDonVi = p.MoTaDonVi ?? "",
                TenAlias = p.MaLoaiNavigation.TenLoai
            }).ToPagedList(page, PageSize);

            var viewModel = new HangHoaPagedVM
            {
                HangHoas = pagedList,
                CurrentPage = page,
                TotalPages = pagedList.PageCount,
                CurrentQuery = query // Thêm dòng này để lưu trữ giá trị tìm kiếm hiện tại
            };

            return View(viewModel);
        }


        public IActionResult Detail()
        {
            var randomProduct = db.HangHoas
                .Include(p => p.MaLoaiNavigation)
                .OrderBy(r => Guid.NewGuid())
                .FirstOrDefault();

            if (randomProduct == null)
            {
                TempData["Message"] = "Không có sản phẩm nào trong cơ sở dữ liệu";
                return Redirect("/404");
            }

            var result = new ChiTietHangHoaVM
            {
                MaHh = randomProduct.MaHh,
                TenHH = randomProduct.TenHh,
                DonGia = randomProduct.DonGia ?? 0,
                ChiTiet = randomProduct.MoTa ?? string.Empty,
                Hinh = randomProduct.Hinh ?? string.Empty,
                MoTaNgan = randomProduct.MoTaDonVi ?? string.Empty,
                TenLoai = randomProduct.MaLoaiNavigation.TenLoai,
                SoLuongTon = 10, // tính sau
                DiemDanhGia = 5, // check sau
            };

            return View(result);
        }



    }
}
