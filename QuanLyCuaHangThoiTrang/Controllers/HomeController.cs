﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using QuanLyCuaHangThoiTrang.Extension;
using QuanLyCuaHangThoiTrang.Model;

namespace QuanLyCuaHangThoiTrang.Controllers
{
    public class HomeController : Controller
    {
        QuanLyCuaHangThoiTrangDbContext db = new QuanLyCuaHangThoiTrangDbContext();
        public string HoTen = "";
        protected void SetAlert(string message, string type)
        {
            TempData["AlertMessage"] = message;
            if (type == "success")
                TempData["AlertType"] = "alert-success";
            else if (type == "warning")
                TempData["AlertType"] = "alert-warning";
            else if (type == "error")
                TempData["AlertType"] = "alert-danger";
        }

        public ActionResult Index()
        {
            Session["Cart"] = new List<ChiTietPhieuDatHang>();
            //
            ViewBag.MenWears = db.HangHoas.Where(hh => hh.LoaiHangHoa.GioiTinh == "Nam" && hh.SoLuong >= 1).ToList();
            ViewBag.WomenWears = db.HangHoas.Where(hh => hh.LoaiHangHoa.GioiTinh == "Nữ" && hh.SoLuong >= 1).ToList();
            ViewBag.Bags = db.HangHoas.Where(hh => hh.LoaiHangHoa.TenLoaiHangHoa == "Túi xách" && hh.SoLuong >= 1).ToList();
            ViewBag.FootWears = db.HangHoas.Where(hh => (hh.LoaiHangHoa.TenLoaiHangHoa == "Giày Nam" && hh.SoLuong >= 1) || (hh.LoaiHangHoa.TenLoaiHangHoa == "Giày Nữ" && hh.SoLuong >= 1)
            || (hh.LoaiHangHoa.TenLoaiHangHoa == "Dép" && hh.SoLuong >= 1)).ToList();
            //Load hang hoa
            ViewBag.MenWears_Sale = db.HangHoas.Where(hh => hh.LoaiHangHoa.GioiTinh == "Nam" && hh.GiamGia > 0 && hh.SoLuong >= 1).ToList();
            ViewBag.WomenWears_Sale = db.HangHoas.Where(hh => hh.LoaiHangHoa.GioiTinh == "Nữ" && hh.GiamGia > 0 && hh.SoLuong >= 1).ToList();
            ViewBag.Bags_Sale = db.HangHoas.Where(hh => hh.LoaiHangHoa.TenLoaiHangHoa == "Túi xách" && hh.GiamGia > 0 && hh.SoLuong >= 1).ToList();
            ViewBag.FootWears_Sale = db.HangHoas.Where(hh => (hh.LoaiHangHoa.TenLoaiHangHoa == "Giày Nam" && hh.GiamGia != 0 && hh.SoLuong >= 1) || (hh.LoaiHangHoa.TenLoaiHangHoa == "Giày Nữ" && hh.GiamGia != 0 && hh.SoLuong >= 1)
            || (hh.LoaiHangHoa.TenLoaiHangHoa == "Dép" && hh.GiamGia != 0 && hh.SoLuong >= 1)).ToList();
            //load hang hoa sale
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [Authorize(Roles = "KhachHang")]
        public ActionResult UserProfile()
        {
            if(Session["Account"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {

                TempData["avatar"] = "/images/avatar/18.jpg";
                ViewBag.UserProfile = (NguoiDung)Session["Account"];
            }
            return View();
        }

        public ActionResult NavTopBar()
        {
            ViewBag.MenWears = db.LoaiHangHoas.Where(lhh => lhh.GioiTinh == "Nam").ToList();
            ViewBag.WomenWears = db.LoaiHangHoas.Where(lhh => lhh.GioiTinh == "Nữ").ToList();
            ViewBag.Other = db.LoaiHangHoas.Where(lhh => lhh.GioiTinh != "Nữ" && lhh.GioiTinh != "Nam").ToList();
            return PartialView();
        }

        [HttpPost]
        public void AddToCart(int MAHANGHOA, float GIA, float GIAMGIA)
        {
            var currentCart = Session["Cart"] as List<ChiTietPhieuDatHang>; //to check current cart
            if (!currentCart.Exists(o => o.MaHangHoa == MAHANGHOA))
            {
                ChiTietPhieuDatHang ctpdh = new ChiTietPhieuDatHang
                {
                    MaHangHoa = MAHANGHOA,
                    SoLuong = 1,
                    Gia = (decimal)(GIA * (1 - GIAMGIA)),
                    ThanhTien = (decimal)(GIA * (1 - GIAMGIA))
                };
                currentCart.Add(ctpdh);
                Session["Cart"] = currentCart;
            }
            else
            {
                ChiTietPhieuDatHang ctpdh = currentCart.Find(o => o.MaHangHoa == MAHANGHOA);
                ctpdh.SoLuong += 1;
                ctpdh.ThanhTien += (decimal)(GIA * (1 - GIAMGIA));
            }
        }
        [HttpPost]
        public void MinusCart(int MAHANGHOA, float GIA, float GIAMGIA)
        {
            var currentCart = Session["Cart"] as List<ChiTietPhieuDatHang>; //to check current cart
            ChiTietPhieuDatHang ctpdh = currentCart.Find(o => o.MaHangHoa == MAHANGHOA);
            ctpdh.SoLuong -= 1;
            ctpdh.ThanhTien -= (decimal)(GIA * (1 - GIAMGIA));
        }

        [HttpPost]
        public void DeleteFromCart(int MAHANGHOA)
        {
            var currentCart = Session["Cart"] as List<ChiTietPhieuDatHang>; //to check current cart
            var removedItem = currentCart.Find(i => i.MaHangHoa == MAHANGHOA);
            currentCart.Remove(removedItem);
            Session["Cart"] = currentCart;
        }

        [HttpPost]
        public ActionResult Order(string TENKHACHHANG, string SDT, string DIACHI, string EMAIL, float TONGTIEN)
        {
            //them phieu dat hang
            DateTime ngaydat = DateTime.Now;
            string tenkhachhang = TENKHACHHANG;
            string sdt = SDT;
            string diachi = DIACHI;
            string email = EMAIL;
            float tongtien = TONGTIEN*1000; //fixed bug
            string hinhthucthanhtoan = "Thanh toán khi nhận hàng";
            bool dathanhtoan = false;
            bool daxacnhan = false;
            bool isdeleted = false;
            PhieuDatHang pdh = new PhieuDatHang
            {
                NgayDat = ngaydat,
                TenKhachHang = tenkhachhang,
                SoDienThoai = sdt,
                Diachi = diachi,
                Email = email,
                TongTien = (decimal)tongtien,
                HinhThucThanhToan = hinhthucthanhtoan,
                DaThanhToan = dathanhtoan,
                DaXacNhan = daxacnhan,
                IsDeleted = isdeleted
            };
            try {
                db.PhieuDatHangs.Add(pdh);
                db.SaveChanges();
            }
            catch { throw; }
            //them chi tiet phieu dat hang
            int sophieudathang = pdh.SoPhieuDatHang;
            if (Session["Cart"] != null)
            {
                List<ChiTietPhieuDatHang> cart = Session["Cart"] as List<ChiTietPhieuDatHang>;
                for (int i = 0; i < cart.Count; i++)
                {
                    try {
                        db.ChiTietPhieuDatHangs.Add(new ChiTietPhieuDatHang
                        {
                            SoPhieuDatHang = sophieudathang,
                            MaHangHoa = cart[i].MaHangHoa,
                            SoLuong = cart[i].SoLuong,
                            Gia = cart[i].Gia,
                            ThanhTien = cart[i].ThanhTien
                        });
                        db.SaveChanges();
                        ////update so luong hang hoa
                        //HangHoa hh = db.HangHoas.AsEnumerable().SingleOrDefault(o => o.MaHangHoa == cart[i].MaHangHoa);
                        //if(hh!= null)
                        //hh.SoLuong -= cart[i].SoLuong;
                        //db.SaveChanges(); loai bo
                    }
                    catch { throw; }
                }
            }
            //reset gio hang
            Session["Cart"] = null;
            SetAlert("Đặt hàng thành công!", "success");

            return Redirect("Home/Index");
        }

        [HttpPost]
        public ActionResult DangNhap(FormCollection form)
        {
            string username = form["username"].ToString();
            string password = MD5Encode.CreateMD5(form["password"].ToString()); 

            var nguoiDung = db.NguoiDungs.SingleOrDefault(n => n.UserName == username && n.PassWord == password);
            if(nguoiDung != null)
            {
                IEnumerable<ChucVu> listQuyen = db.ChucVus.Where(n => n.MaChucVu == nguoiDung.MaChucVu);
                string quyen = "";
                foreach (var item in listQuyen)
                {
                    quyen += item.TenChucVu + ",";
                }
                quyen = quyen.Substring(0, quyen.Length - 1);
                PhanQuyen(nguoiDung.UserName.ToString(), quyen);
                Session["Account"] = nguoiDung;
                HoTen = nguoiDung.TenNguoiDung;
                if (nguoiDung.ChucVu.TenChucVu != "KhachHang")
                    return RedirectToAction("Index", "Manager/Home");
                return RedirectToAction("Index", "Home");
            }
            SetAlert("Sai tài khoản hoặc mật khẩu!", "error");
            return RedirectToAction("Index", "Home"); // Need add notification login not success
        }

        public void PhanQuyen(string username, string quyen)
        {
            FormsAuthentication.Initialize();

            var ticket = new FormsAuthenticationTicket(1, username,
                DateTime.Now, DateTime.Now.AddHours(5),
                false, quyen, FormsAuthentication.FormsCookiePath);

            var cookies = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            if (ticket.IsPersistent)
            {
                cookies.Expires = ticket.Expiration;
            }
            Response.Cookies.Add(cookies);
        }

        //[Authorize(Roles = "Admin")]
        [AllowAnonymous]
        public ActionResult DangXuat()
        {
            Session["Account"] = null;
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public ActionResult DangKy(FormCollection form)
        {
            string username = form["username"].ToString();
            string password = MD5Encode.CreateMD5(form["password"].ToString());
            string phone = form["phone"].ToString();
            string name = form["name"].ToString();
            var nguoiDung = db.NguoiDungs.SingleOrDefault(n => n.UserName == username);
            if (nguoiDung == null)
            {
                db.NguoiDungs.Add(new NguoiDung
                {
                    TenNguoiDung = name,
                    DiaChi = "",
                    SoDienThoai = phone,
                    Email = "",
                    CMND = "",
                    UserName = username,
                    PassWord = password,
                    IsDeleted = false,
                    MaChucVu = 6, // Customer
                    Avatar = ""
                }) ;
                db.SaveChanges();
                SetAlert("Tạo tài khoản thành công!", "success");
            }
            else
            {
                SetAlert("Tài khoản này đã có người sử dụng!", "error");
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "KhachHang")]
        public ActionResult CapNhat(FormCollection form)
        {
            var user = (NguoiDung)Session["Account"];
            string name = form["name"].ToString();
            string phone = form["phone"].ToString();
            string email = form["email"].ToString();
            string address = form["address"].ToString();
            string password = form["password"].ToString();

            var nguoidung = db.NguoiDungs.SingleOrDefault(n => n.UserName == user.UserName);
            if (nguoidung != null)
            {
                nguoidung.TenNguoiDung = name;
                nguoidung.SoDienThoai = phone;
                nguoidung.Email = email;
                nguoidung.DiaChi = address;
                if (!String.IsNullOrEmpty(password))
                {               
                    nguoidung.PassWord = MD5Encode.CreateMD5(password);
                }
                db.SaveChanges();
                SetAlert("Cập nhật thông tin thành công!", "success");
                Session["Account"] = nguoidung;
            }
            else
            {
                SetAlert("Không thể cập nhật thông tin", "error");
            }
            return RedirectToAction("UserProfile");
        }
    }
}