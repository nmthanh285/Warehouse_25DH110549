using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Warehouse_25DH110549.Models;

namespace Warehouse_25DH110549.Controllers
{
    public class ProductsController : Controller
    {
        private WarehouseEntities db = new WarehouseEntities();

        // GET: Products
        public ActionResult Index(string searchString, string statusFilter, string sortOrder)
        {
            // 1. Lấy toàn bộ sản phẩm
            var products = db.Products.AsQueryable();

            // 2. Tìm theo tên (không phân biệt hoa thường)
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.ProductName.ToLower().Contains(searchString.ToLower()));
            }

            // 3. Lọc theo trạng thái tồn kho
            if (!string.IsNullOrEmpty(statusFilter))
            {
                switch (statusFilter)
                {
                    case "OutOfStock": // Hết hàng
                        products = products.Where(p => p.Quantity == 0);
                        break;
                    case "LowStock":   // Sắp hết (1-4)
                        products = products.Where(p => p.Quantity >= 1 && p.Quantity <= 4);
                        break;
                    case "InStock":    // Đủ hàng (>=5)
                        products = products.Where(p => p.Quantity >= 5);
                        break;
                }
            }

            // 4. Sắp xếp theo Giá
            ViewBag.CurrentSort = sortOrder;
            ViewBag.PriceSortParm = sortOrder == "price_asc" ? "price_desc" : "price_asc";

            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.ProductID); // Mặc định sắp xếp theo ID
                    break;
            }

            // 5. Truyền thông báo nếu không có kết quả
            if (!products.Any())
            {
                ViewBag.Message = "Không tìm thấy sản phẩm nào phù hợp với yêu cầu của bạn.";
            }

            return View(products.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductName,Category,Price,Quantity")] Product product)
        {
            // 1. Xử lý chuỗi: Cắt khoảng trắng 2 đầu (Trim)
            if (product.ProductName != null)
            {
                product.ProductName = product.ProductName.Trim();
            }
            if (product.Category != null)
            {
                product.Category = product.Category.Trim();
            }

            // 2. Kiểm tra trùng tên (Không phân biệt hoa thường, đã Trim)
            if (!string.IsNullOrEmpty(product.ProductName))
            {
                // Đưa tên nhập vào về chữ thường để so sánh
                string tenNhapVao = product.ProductName.ToLower();

                // Tìm trong DB xem có sản phẩm nào trùng tên không
                var checkTrung = db.Products.FirstOrDefault(p => p.ProductName.ToLower() == tenNhapVao);

                if (checkTrung != null)
                {
                    // Gắn lỗi vào trường ProductName để hiển thị lên giao diện
                    ModelState.AddModelError("ProductName", "Tên sản phẩm này đã tồn tại trong hệ thống (không phân biệt hoa thường)!");
                }
            }

            // 3. Lưu vào CSDL nếu mọi thứ hợp lệ (bao gồm các validate ở Bước 2 và Bước 3)
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Nếu có lỗi, trả về giao diện kèm thông báo
            return View(product);
        }

        // POST: Products/Create
        public ActionResult Create()
        {
            return View();
        }
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Creates([Bind(Include = "ProductName,Category,Price,Quantity")] Product product)
        {
            // 1. Xử lý chuỗi: Cắt khoảng trắng 2 đầu
            if (product.ProductName != null) product.ProductName = product.ProductName.Trim();
            if (product.Category != null) product.Category = product.Category.Trim();

            // 2. Kiểm tra trùng tên (Câu 6)
            if (!string.IsNullOrEmpty(product.ProductName))
            {
                string tenNhapVao = product.ProductName.ToLower();
                var checkTrung = db.Products.FirstOrDefault(p => p.ProductName.ToLower() == tenNhapVao);
                if (checkTrung != null)
                {
                    ModelState.AddModelError("ProductName", "Tên sản phẩm này đã tồn tại trong hệ thống!");
                }
            }

            // 3. Chặn lỗi số lượng âm hoặc giá âm (Đề phòng ModelState bị lọt)
            if (product.Price <= 0)
            {
                ModelState.AddModelError("Price", "Đơn giá phải lớn hơn 0.");
            }
            if (product.Quantity < 0)
            {
                ModelState.AddModelError("Quantity", "Số lượng phải là số nguyên không âm.");
            }

            // 4. Nếu dữ liệu hợp lệ thì mới lưu
            if (ModelState.IsValid)
            {
                try
                {
                    db.Products.Add(product);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    // Nếu Database vẫn báo lỗi, ta bắt lỗi và hiển thị ra màn hình
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            ModelState.AddModelError(validationError.PropertyName, "Lỗi CSDL: " + validationError.ErrorMessage);
                        }
                    }
                }
            }

            // Nếu có bất kỳ lỗi nào, trả về View kèm thông báo
            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,ProductName,Category,Price")] Product product)
        // Lưu ý: Đã bỏ "Quantity" ra khỏi Bind để không cho phép cập nhật
        {
            if (ModelState.IsValid)
            {
                // 1. Tìm sản phẩm gốc trong database lên
                var existingProduct = db.Products.Find(product.ProductID);

                if (existingProduct != null)
                {
                    // 2. Chỉ cập nhật các trường được phép sửa
                    existingProduct.ProductName = product.ProductName;
                    existingProduct.Category = product.Category;
                    existingProduct.Price = product.Price;

                    // KHÔNG cập nhật existingProduct.Quantity

                    // 3. Lưu thay đổi
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        // --- BẮT ĐẦU CODE CÂU 4: KIỂM KÊ ---

        // 1. Hàm GET: Hiển thị giao diện nhập liệu
        public ActionResult KiemKe(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // 2. Hàm POST: Xử lý khi bấm nút Cập nhật
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult KiemKe(int id, int? quantityThucTe)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Bắt lỗi: Phải là số nguyên không âm
            if (quantityThucTe == null || quantityThucTe < 0)
            {
                ViewBag.Error = "Số lượng thực tế phải là số nguyên không âm (>= 0)!";
                return View(product); // Trả về trang nhập liệu kèm thông báo lỗi
            }

            // Tính toán chênh lệch
            int chenhLech = quantityThucTe.Value - product.Quantity;
            string thongBao = "";

            if (chenhLech > 0)
            {
                thongBao = $"Chênh lệch tăng: +{chenhLech}";
            }
            else if (chenhLech < 0)
            {
                thongBao = $"Chênh lệch giảm: {chenhLech}";
            }
            else
            {
                thongBao = "Không chênh lệch";
            }

            // Cập nhật vào Database
            product.Quantity = quantityThucTe.Value;
            db.SaveChanges();

            // Lưu thông báo vào TempData để hiển thị ở trang Danh sách
            TempData["ThongBaoKiemKe"] = $"Đã cập nhật '{product.ProductName}'. {thongBao}";

            return RedirectToAction("Index");
        }
        // --- KẾT THÚC CODE CÂU 4 ---

        // --- BẮT ĐẦU CODE CÂU 5 ---

        // Hàm GET: Hiển thị giao diện
        public ActionResult KiemKeHai()
        {
            // Lấy danh sách sản phẩm để hiển thị vào Dropdown
            ViewBag.DanhSachSP = new SelectList(db.Products, "ProductID", "ProductName");
            return View();
        }

        // Hàm POST: Xử lý khi bấm nút Cập nhật
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult KiemKeHai(int? id1, int? id2, int? qty1, int? qty2)
        {
            // Nạp lại danh sách cho Dropdown phòng trường hợp bị lỗi trả về View
            ViewBag.DanhSachSP = new SelectList(db.Products, "ProductID", "ProductName");

            // 1. Kiểm tra dữ liệu đầu vào (Validate)
            if (id1 == null || id2 == null || qty1 == null || qty2 == null)
            {
                ViewBag.ThongBao = "Lỗi: Vui lòng nhập đầy đủ thông tin cho cả 2 sản phẩm.";
                return View();
            }

            if (id1 == id2)
            {
                ViewBag.ThongBao = "Lỗi: Hai sản phẩm được chọn phải khác nhau.";
                return View();
            }

            if (qty1 < 0 || qty2 < 0)
            {
                ViewBag.ThongBao = "Lỗi: Số lượng thực tế phải là số nguyên không âm.";
                return View();
            }

            // 2. Tìm sản phẩm trong DB
            Product p1 = db.Products.Find(id1);
            Product p2 = db.Products.Find(id2);

            if (p1 == null || p2 == null)
            {
                ViewBag.ThongBao = "Lỗi: Một hoặc cả hai mã sản phẩm không tồn tại trong hệ thống.";
                return View();
            }

            // 3. Xử lý Transaction (Giao dịch)
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Cập nhật số lượng
                    p1.Quantity = qty1.Value;
                    p2.Quantity = qty2.Value;

                    db.SaveChanges(); // Lưu tạm vào bộ nhớ

                    transaction.Commit(); // Xác nhận lưu vào CSDL (Chốt giao dịch)

                    ViewBag.ThongBao = $"Thành công: Đã cập nhật '{p1.ProductName}' thành {qty1} và '{p2.ProductName}' thành {qty2}.";
                }
                catch (Exception ex)
                {
                    // Nếu có bất kỳ lỗi hệ thống nào, hủy bỏ toàn bộ thay đổi
                    transaction.Rollback();
                    ViewBag.ThongBao = "Lỗi hệ thống: Không thể cập nhật. Chi tiết: " + ex.Message;
                }
            }

            return View();
        }
        // --- KẾT THÚC CODE CÂU 5 ---
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
