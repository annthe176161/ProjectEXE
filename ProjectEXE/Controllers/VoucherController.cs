using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectEXE.Models;
using ProjectEXE.Services.Implementations;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.Voucher;

namespace ProjectEXE.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VoucherController : Controller
    {
        private readonly IVourcherService _voucherService;
        private readonly int ITEM_PER_PAGE = 10;
        public VoucherController(IVourcherService vourcherService)
        {
            _voucherService = vourcherService;
        }

        // GET: Voucher
        public async Task<IActionResult> Index(string? code, bool? status, int p = 1)
        {
            var vouchers = await _voucherService.GetAllVouchers();

            // Lọc theo mã
            if (!string.IsNullOrWhiteSpace(code))
            {
                vouchers = vouchers
                    .Where(v => v.Code != null && v.Code.Contains(code, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Lọc theo trạng thái
            if (status.HasValue)
            {
                vouchers = vouchers
                    .Where(v => v.IsActive == status.Value) 
                    .ToList();
            }

            var totalRecord = vouchers.Count();
            var totalPage = (int)Math.Ceiling((double)totalRecord / ITEM_PER_PAGE); 
            p = Math.Max(1, Math.Min(p, totalPage));

            ViewBag.CurrentPage = p;
            ViewBag.CountPage = totalPage;
            ViewBag.Code = code;
            ViewBag.Status = status;
            ViewBag.PageSize = ITEM_PER_PAGE;
            ViewBag.TotalRecord = totalRecord;

            var paggedVoucher = vouchers
                .Skip((p - 1) * ITEM_PER_PAGE)
                .Take(ITEM_PER_PAGE)
                .ToList();

            return View(paggedVoucher);
        }

        // GET: Voucher/Create
        [HttpGet]
        public IActionResult Create()
        {
            // Tạo một viewmodel mới với giá trị mặc định
            var model = new VoucherViewModel
            {
                VoucherId = Guid.NewGuid().ToString(), 
                CreateAt = DateOnly.FromDateTime(DateTime.Now),
                IsActive = true
            };
            return View(model);
        }

        // POST: Voucher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VoucherViewModel model)
        {
          
            
            // Kiểm tra mã voucher đã tồn tại chưa
            if (!await _voucherService.IsCodeUnique(model.Code))
            {
                ModelState.AddModelError("Code", "Mã voucher này đã tồn tại");
            }

            // Kiểm tra hạn sử dụng
            if (model.ExpiryDate.HasValue && model.ExpiryDate < model.CreateAt)
            {
                ModelState.AddModelError("ExpiryDate", "Ngày hết hạn phải lớn hơn hoặc bằng ngày tạo");
            }

            if (ModelState.IsValid)
            {
                if (await _voucherService.CreateVoucher(model))
                {
                    TempData["Success"] = "Thêm mới voucher thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi thêm mới voucher");
                }
            }

            return View(model);
        }

        // GET: Voucher/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var voucher = await _voucherService.GetVoucherById(id);
            if (voucher == null)
            {
                return NotFound();
            }
            return View(voucher);
        }

        // POST: Voucher/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VoucherViewModel model)
        {
            // Kiểm tra mã voucher đã tồn tại chưa (trừ voucher hiện tại)
            if (!await _voucherService.IsCodeUnique(model.Code, model.VoucherId))
            {
                ModelState.AddModelError("Code", "Mã voucher này đã tồn tại");
            }

            // Kiểm tra hạn sử dụng
            if (model.ExpiryDate.HasValue && model.ExpiryDate < model.CreateAt)
            {
                ModelState.AddModelError("ExpiryDate", "Ngày hết hạn phải lớn hơn hoặc bằng ngày tạo");
            }

            if (ModelState.IsValid)
            {
                if (await _voucherService.UpdateVoucher(model))
                {
                    TempData["Success"] = "Cập nhật voucher thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật voucher");
                }
            }

            return View(model);
        }

        // GET: Voucher/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var voucher = await _voucherService.GetVoucherById(id);
            if (voucher == null)
            {
                return NotFound();
            }
            return View(voucher);
        }

        // POST: Voucher/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (await _voucherService.DeleteVoucher(id))
            {
                TempData["Success"] = "Xóa voucher thành công!";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa voucher";
            }
            return RedirectToAction("Index");
        }
    }














}
