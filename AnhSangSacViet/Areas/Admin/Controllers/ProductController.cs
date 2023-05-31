﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using System.Net;
//using System.Data.Entity;
//using WebBHDTCHUNG.Areas.Admin.Data;
//using WebBHDTCHUNG.Models;
//using Microsoft.AspNet.Identity;
//using OfficeOpenXml;
//using System.Web.Script.Serialization;
//using WebBHDTCHUNG.Utils;

//namespace WebBHDTCHUNG.Areas.Admin.Controllers
//{
//    [Authorize]
//    public class ProductController : Controller
//    {
//        private CMSBHDTCHUNGEntities db = new CMSBHDTCHUNGEntities();
//        private string userId = "";
//        public ActionResult Index()
//        {
//            if (User.Identity.Name == "administrator")
//            {
//                var model1 = db.Products.OrderByDescending(m => m.Createdate);
//                return View(model1);
//            }
//            //Vincent
//            userId = User.Identity.GetUserId();
//            var model = db.Products.Where(a => a.Createby == Utility.IdPatner).OrderByDescending(m => m.Createdate);

//            return View(model);
//        }
//        public ActionResult Create()
//        {
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Create([Bind(Include = "")] Product product)
//        {
//            if (ModelState.IsValid)
//            {
//                ViewBag.SerialError = "";
//                var cp = db.Products.Where(a => a.Serial == product.Serial).SingleOrDefault();
//                if (cp != null)
//                {
//                    ViewBag.SerialError = "Serial đã bị trùng.";
//                    return View(product);
//                }

//                product.Createby = Utility.IdPatner; //Vincent
//                product.Createdate = DateTime.Now;
//                db.Products.Add(product);
//                db.SaveChanges();
//                return RedirectToAction("Index");
//            }

//            return View(product);

//        }
//        public ActionResult Edit(long? id)
//        {
//            if (id == null)
//            {
//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//            }
//            Product product = db.Products.Find(id);
//            if (product == null)
//            {
//                return HttpNotFound();
//            }
//            return View(product);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Edit([Bind(Include = "")] Product product)
//        {
//            if (ModelState.IsValid)
//            {
//                db.Entry(product).State = EntityState.Modified;
//                db.SaveChanges();
//                return RedirectToAction("Index");
//            }
//            return View(product);
//        }

//        public ActionResult Active(long? id)
//        {
//            if (id == null)
//            {
//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//            }
//            Product product = db.Products.Find(id);
//            if (product == null)
//            {
//                return HttpNotFound();
//            }
//            ActiveViewModel active = new ActiveViewModel()
//            {
//                ProductId = product.Id
//            };
//            TempData["province"] = getProvince();
//            return View(active);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Active([Bind(Include = "")] ActiveViewModel active)
//        {
//            if (ModelState.IsValid)
//            {
//                //update product
//                var prod = db.Products.Find(active.ProductId);
//                //add customer
//                long cusId = addCustomer(active.CustomerPhone, active.CustomerName, active.City, active.District, active.Address, active.Email, userId);
//                //add productactive
//                addActive(prod, cusId);
//                //gui brandname neu partner dang ki
//                checkSendBrandname(prod.Createby, active.CustomerPhone, prod.Serial, active.Activedate.Value.ToString("dd/MM/yyyy"), active.Activedate.Value.AddMonths(prod.Limited ?? default(int)).ToString("dd/MM/yyyy"));
//                return RedirectToAction("Index");
//            }
//            return View(active);
//        }

//        private long addCustomer(string phone, string name, string province, string district, string address, string email, string create)
//        {
//            var customer = db.Customers.Where(a => a.Phone == phone).SingleOrDefault();
//            if (customer != null)
//            {
//                //Vincent
//                if (String.IsNullOrEmpty(customer.Name))
//                {
//                    customer.Name = name;
//                    customer.City = province;
//                    customer.District = district;
//                    customer.Address = address;
//                    customer.Email = email;
//                    db.Entry(customer).State = EntityState.Modified;
//                    return db.SaveChanges();
//                }
//                else
//                {
//                    return customer.Id;
//                }

//            }
//            else
//            {
//                var newcus = new Customer()
//                {
//                    Phone = phone,
//                    Name = name,
//                    City = province,
//                    District = district,
//                    Address = address,
//                    Email = email,
//                    Createdate = DateTime.Now,
//                    Createby = create
//                };
//                db.Customers.Add(newcus);
//                db.SaveChanges();
//                return newcus.Id;
//            }
//        }
//        private void addActive(Product product, long customer)
//        {
//            //update status table product
//            product.Status = 1;
//            db.Entry(product).State = EntityState.Modified;

//            //add thong tin table productactive
//            if (userId.Length > 0)
//            {
//                var prodActive = new ProductActive()
//                {
//                    Activedate = DateTime.Now,
//                    ProductId = product.Id,
//                    CustomerId = customer,
//                    Type = 2,
//                    Activeby = userId,

//                };
//                db.ProductActives.Add(prodActive);
//            }
//            else
//            {
//                var prodActive = new ProductActive()
//                {
//                    Activedate = DateTime.Now,
//                    ProductId = product.Id,
//                    CustomerId = customer,
//                    Type = 1,

//                };
//                db.ProductActives.Add(prodActive);
//            }

//            db.SaveChanges();
//        }
//        private void checkSendBrandname(string partner, string phone, string s1, string s2, string s3)
//        {
//            var brand = db.TempBrandnames.Find(partner);
//            if (brand != null)
//            {
//                string mess = string.Format(brand.TempActive, brand.ShowName, s1, s2, s3);
//                int send = Utils.SendMTBrandname.SentMTMessage(mess, phone, brand.ShowName, brand.ShowName, "0");
//                Brandname brandName = new Brandname()
//                {
//                    Status = send,
//                    Message = mess,
//                    Createdate = DateTime.Now,
//                    PhoneSend = phone
//                };
//                db.Brandnames.Add(brandName);
//                db.SaveChanges();
//            }
//        }
//        public List<Province> getProvince()
//        {
//            var province = db.Provinces.OrderBy(a => a.Name).ToList();
//            return province;
//        }

//        [HttpPost]
//        public ActionResult GetCity(string name)
//        {
//            District city = new District();
//            var id = db.Provinces.Where(s => s.Name == name).SingleOrDefault();//get id theo ten
//            var provi = db.Districts.Where(x => x.ProvinceId == id.Id).ToList();//get ds quan huyen
//            var ress = new List<String>();//add data vao response
//            foreach (var i in provi)
//            {
//                ress.Add(i.Name);
//            }
//            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
//            string result = javaScriptSerializer.Serialize(ress);//convert to json
//            return Json(result, JsonRequestBehavior.AllowGet);
//        }

//        public ActionResult Delete(long? id)
//        {
//            if (id == null)
//            {
//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//            }
//            Product product = db.Products.Find(id);
//            if (product == null)
//            {
//                return HttpNotFound();
//            }
//            return View(product);
//        }

//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public ActionResult DeleteConfirmed(long id)
//        {
//            Product product = db.Products.Find(id);
//            db.Products.Remove(product);
//            db.SaveChanges();
//            return RedirectToAction("Index");
//        }

//        public ActionResult Uploadfile()
//        {
//            return View();
//        }
//        [HttpPost]
//        public ActionResult Uploadfile(FormCollection formCollection)
//        {
//            ViewBag.mess = "Đã có lỗi xảy ra.";
//            if (Request != null)
//            {
//                try
//                {
//                    HttpPostedFileBase file = Request.Files["UploadedFile"];
//                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
//                    {
//                        //int count = 0;
//                        string fileName = file.FileName;
//                        string fileContentType = file.ContentType;
//                        byte[] fileBytes = new byte[file.ContentLength];
//                        var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
//                        var products = new List<UploadProductModel>();
//                        int count = 0;
//                        using (var package = new ExcelPackage(file.InputStream))
//                        {
//                            var currentSheet = package.Workbook.Worksheets;
//                            var workSheet = currentSheet.First();
//                            var noOfCol = workSheet.Dimension.End.Column;
//                            var noOfRow = workSheet.Dimension.End.Row;
//                            for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
//                            {
//                                UploadProductModel prodview = new UploadProductModel();
//                                Product product = new Product();
//                                product.Createdate = DateTime.Now;
//                                //Vincent
//                                product.Createby = Utility.IdPatner;
//                                product.Serial = workSheet.Cells[rowIterator, 1].Value.ToString();
//                                product.Name = workSheet.Cells[rowIterator, 2].Value.ToString();
//                                product.Code = workSheet.Cells[rowIterator, 3].Value.ToString();

//                                //product.Model = workSheet.Cells[rowIterator, 4].Value.ToString();
//                                //Vincent - add try catch
//                                try { product.Model = workSheet.Cells[rowIterator, 4].Value.ToString(); } catch (Exception) { }

//                                try { product.MadeIn = workSheet.Cells[rowIterator, 5].Value.ToString(); } catch (Exception) { }

//                                try
//                                {
//                                    product.Exportdate = DateTime.ParseExact(workSheet.Cells[rowIterator, 6].Value.ToString(), "dd/MM/yyyy", null);
//                                }
//                                catch (Exception) { }
//                                try
//                                {
//                                    product.Arisingdate = DateTime.ParseExact(workSheet.Cells[rowIterator, 7].Value.ToString(), "dd/MM/yyyy", null);
//                                }
//                                catch (Exception) { }
//                                product.Limited = int.Parse(workSheet.Cells[rowIterator, 8].Value.ToString());
//                                try
//                                {
//                                    product.Category = workSheet.Cells[rowIterator, 9].Value.ToString();
//                                }
//                                catch (Exception) { }
//                                try
//                                {
//                                    product.Importdate = DateTime.ParseExact(workSheet.Cells[rowIterator, 10].Value.ToString(), "dd/MM/yyyy", null);
//                                }
//                                catch (Exception) { }

//                                prodview.Product = product;
//                                products.Add(prodview);
//                                db.Products.Add(product);
//                                var cp = db.Products.Where(a => a.Serial == product.Serial).SingleOrDefault();
//                                if (cp != null)
//                                {
//                                    count++;
//                                    prodview.Error = "Serial đã bị trùng.";
//                                }

//                            }
//                            if (count == 0)
//                            {
//                                ViewBag.mess = "Đã lưu sản phẩm thành công.";
//                                db.SaveChanges();
//                            }
//                        }
//                        return View(products);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    ViewBag.mess = ex;
//                    return View();
//                }
//            }
//            return View();
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;
using WebBHDTCHUNG.Areas.Admin.Data;
using WebBHDTCHUNG.Models;
using Microsoft.AspNet.Identity;
using OfficeOpenXml;
using System.Web.Script.Serialization;
using System.Web.Security;
using PagedList;
using System.IO;
using OfficeOpenXml.Style;
using System.Drawing;
using WebBHDTCHUNG.Utils;

namespace WebBHDTCHUNG.Areas.Admin.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private CMSBHDTCHUNGEntities db = new CMSBHDTCHUNGEntities();
        private string userId = "";
        private static List<Product> listExc = new List<Product>();
        public ActionResult Index(string SearchString, int? page, string StartDate, string EndDate, string Status)
        {
            if (User.Identity.Name == "administrator")
            {
                var model1 = db.Products.OrderByDescending(m => m.Createdate);
                return View(model1);
            }

            //check xem co phai mod k
            //neu la mod thi phai lay theo id partner createby
            string partner = db.AspNetUsers.Find(User.Identity.GetUserId()).Createby;
            if (User.IsInRole("Mod"))
            {
                userId = partner;
            }
            else
            {
                userId = User.Identity.GetUserId();
            }
            //userId = User.Identity.GetUserId();
            var model = db.Products.Where(a => a.Createby == userId); //.OrderByDescending(m => m.Createdate);
            if (!String.IsNullOrEmpty(SearchString))
            {
                model = model.Where(s => s.Name.Contains(SearchString)
                                       || s.Serial.Contains(SearchString)
                                       || s.Model.Contains(SearchString)
                                       || s.Code.Contains(SearchString)
                                       );
                ViewBag.SearchString = SearchString;
            }

            if (!String.IsNullOrEmpty(Status))
            {
                int i = Int32.Parse(Status);
                if (i == 1)
                {
                    model = model.Where(s => s.Status == i);
                }
                else if (i == 0)
                {
                    model = model.Where(s => s.Status == 0 || s.Status == null);
                }
                ViewBag.Status = Status;
            }

            if (!String.IsNullOrEmpty(StartDate))
            {
                DateTime d = DateTime.Parse(StartDate);
                model = model.OrderByDescending(a => a.Exportdate).Where(s => s.Exportdate >= d);
                ViewBag.startDate = StartDate;
            }
            if (!String.IsNullOrEmpty(EndDate))
            {
                DateTime d = DateTime.Parse(EndDate);
                d = d.AddDays(1);
                model = model.OrderByDescending(a => a.Exportdate).Where(s => s.Exportdate <= d);
                ViewBag.endDate = EndDate;
            }

            model = model.OrderByDescending(a => a.Createdate);

            listExc = model.ToList();
            int pageSize = 100;
            int pageNumber = (page ?? 1);
            return View(listExc.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
        {
            return View();
        }

        public ActionResult ActiveFile()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ActiveFile(FormCollection formCollection)
        {
            ViewBag.mess = "Đã có lỗi. Hãy kiểm tra dữ liệu đầu vào.";
            if (Request != null)
            {
                try
                {
                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        List<ActiveViewModel> product_view = new List<ActiveViewModel>();
                        var item_product_view = new ActiveViewModel();
                        //int count = 0;
                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;
                            var user = db.AspNetUsers.Where(a => a.UserName == "svtadmin").SingleOrDefault();
                            int count = 0;
                            for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                            {
                                string serial = "", buydate = "", phonenumber = "", cusname = "", address = "", district = "", province = "", uagent = "";
                                DateTime? buydatetime = null;
                                try
                                {
                                    serial = workSheet.Cells[rowIterator, 1].Value.ToString();
                                }
                                catch (Exception ex) { }
                                //try
                                //{
                                //    buydate = workSheet.Cells[rowIterator, 2].Value.ToString();
                                //    buydatetime = DateTime.ParseExact(buydate, "dd/MM/yyyy", null);
                                //}
                                //catch (Exception ex) { }
                                try
                                {
                                    phonenumber = workSheet.Cells[rowIterator, 2].Value.ToString();
                                    phonenumber = FormatString.formatUserId(phonenumber.Trim(), 0);
                                }
                                catch (Exception ex) { }
                                try
                                {
                                    cusname = workSheet.Cells[rowIterator, 3].Value.ToString();
                                }
                                catch (Exception ex) { }
                                try
                                {
                                    address = workSheet.Cells[rowIterator, 4].Value.ToString();
                                }
                                catch (Exception ex) { }
                                try
                                {
                                    district = workSheet.Cells[rowIterator, 5].Value.ToString();
                                }
                                catch (Exception ex) { }
                                try
                                {
                                    province = workSheet.Cells[rowIterator, 6].Value.ToString();
                                }
                                catch (Exception ex) { }
                                try
                                {
                                    uagent = workSheet.Cells[rowIterator, 7].Value.ToString();
                                }
                                catch (Exception ex) { }
                                item_product_view = new ActiveViewModel()
                                {
                                    Serial = serial,
                                    Buydate = buydatetime,
                                    CustomerPhone = phonenumber,
                                    CustomerName = cusname,
                                    Address = address,
                                    District = district,
                                    City = province,
                                    Activeby = uagent
                                };
                                if (serial != null && !"".Equals(serial))
                                {
                                    var product = db.Products.Where(a => a.Serial == serial.Trim() && a.Status == null && a.Createby == user.Id).FirstOrDefault();
                                    if (product == null)
                                    {
                                        item_product_view.ProductName = "Sản phẩm không tồn tại";
                                        ViewBag.mess = "Không thể kích hoạt sản phẩm từ dòng " + rowIterator;

                                    }
                                    //if (!"#".Equals(uagent) && uagent != null && !"".Equals(uagent))
                                    //{
                                    //    var agent = db.AspNetUsers.Where(a => a.UserName == uagent).SingleOrDefault();
                                    //    if (agent == null)
                                    //    {
                                    //        item_product_view.ProductName = "Đại lý không hợp lệ";
                                    //        ViewBag.mess = "Đại lý không thể kích hoạt sản phẩm từ dòng " + rowIterator;

                                    //    }
                                    //}
                                    //kich hoat san pham
                                    //
                                    if (string.IsNullOrEmpty(item_product_view.ProductName) && product.Status == null)
                                    {
                                        product.Status = 1;
                                        db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                                        // Customer c = new Customer();
                                        Customer cus = db.Customers.Where(a => a.Phone == phonenumber && a.Createby == user.Id).SingleOrDefault();
                                        if (cus == null)
                                        {
                                            cus = addCustomerByFile(phonenumber, cusname, address, province, province, uagent, user.Id);
                                        }

                                        //add thông tin prodactive
                                        addProdActive(product, cus, buydatetime, uagent);
                                        db.SaveChanges();
                                        count++;
                                        item_product_view.ProductName = "thành công";
                                    }
                                    else
                                    {
                                        item_product_view.ProductName = "trạng thái không hợp lệ";
                                    }
                                }
                                else
                                {
                                    item_product_view.ProductName = "Serial null";

                                }
                                product_view.Add(item_product_view);
                            }
                            ViewBag.mess = "Đã kích hoạt thành công " + count + " sản phẩm trong danh sách";

                            return View(product_view);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.mess = ex;
                    return View();
                }
            }
            return View();
        }

        public Customer addCustomerByFile(string phone, string cusname, string address, string district, string province, string agent, string id)
        {
            if (agent == "#")
            {
                agent = null;
            }
            Customer c = new Customer();
            var customer = db.Customers.Where(a => a.Phone == phone && a.Createby == id).SingleOrDefault();
            if (customer != null)
            {
                // Cập nhật lại thông tin khách hàng
                customer.Name = cusname;
                customer.Address = address;
                customer.District = district;
                customer.City = province;
                db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                c = customer;
            }
            else
            {
                Customer cus = new Customer()
                {
                    Phone = phone,
                    Name = cusname,
                    Address = address,
                    District = district,
                    City = province,
                    Createdate = DateTime.Now,
                    Createby = id
                };
                db.Customers.Add(cus);
                c = cus;
            }
            db.SaveChanges();
            return c;
        }

        public void addProdActive(Product product, Customer customer, DateTime? buyDate, string agent)
        {
            if (!"".Equals(agent) || agent == null)
            {
                agent = null;
            }
            ProductActive active = new ProductActive()
            {
                ProductId = product.Id,
                CustomerId = customer.Id,
                Activedate = DateTime.Now,
                Activeby = agent,
                Type = 2,
                Buydate = buyDate
            };
            db.ProductActives.Add(active);
            // db.SaveChanges();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "")] Product product)
        {
            if (ModelState.IsValid)
            {
                //check xem co phai mod k
                //neu la mod thi phai lay theo id partner createby
                string partner = db.AspNetUsers.Find(User.Identity.GetUserId()).Createby;
                if (User.IsInRole("Mod"))
                {
                    product.Createby = partner;
                }
                else
                {
                    product.Createby = User.Identity.GetUserId();
                }


                product.Createdate = DateTime.Now;
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(product);

        }
        public ActionResult Edit(long? id)
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
        public ActionResult Edit([Bind(Include = "")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        public ActionResult Active(long? id)
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
            ActiveViewModel active = new ActiveViewModel()
            {
                ProductId = product.Id
            };
            TempData["province"] = getProvince();
            return View(active);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Active([Bind(Include = "")] ActiveViewModel active)
        {
            if (ModelState.IsValid)
            {
                //update product
                var prod = db.Products.Find(active.ProductId);
                //add customer
                long cusId = addCustomer(active.CustomerPhone, active.CustomerName, active.City, active.District, active.Address, active.Email, userId);
                //add productactive, bổ sung ngày kích hoạt theo form chọn
                addActive(prod, cusId, active);
                //gui brandname neu partner dang ki
                if (active.Activedate == null)
                {
                    active.Activedate = DateTime.Now;
                }
                checkSendBrandname(prod.Createby, active.CustomerPhone, prod.Serial, active.Activedate.Value.ToString("dd/MM/yyyy"), active.Activedate.Value.AddMonths(prod.Limited ?? default(int)).ToString("dd/MM/yyyy"));
                return RedirectToAction("Index");
            }
            return View(active);
        }

        private long addCustomer(string phone, string name, string province, string district, string address, string email, string create)
        {
            string Createby = "";
            phone = Utils.FormatString.formatUserId(phone, 0);
            string partner = db.AspNetUsers.Find(User.Identity.GetUserId()).Createby;
            if (User.IsInRole("Mod"))
            {
                Createby = partner;
            }
            else
            {
                Createby = User.Identity.GetUserId();
            }

            var customer = db.Customers.Where(a => a.Phone == phone && a.Createby == Createby).SingleOrDefault();
            if (customer != null)
            {

                if (customer.Name.Length == 0)
                {
                    customer.Name = name;
                    customer.City = province;
                    customer.District = district;
                    customer.Address = address;
                    customer.Email = email;
                    // ADD BY TRUNGVD 2021-06-24
                    customer.Createby = Createby;
                    db.Entry(customer).State = EntityState.Modified;
                    return db.SaveChanges();
                }
                else
                {
                    return customer.Id;
                }
            }
            else
            {
                var newcus = new Customer()
                {
                    Phone = phone,
                    Name = name,
                    City = province,
                    District = district,
                    Address = address,
                    Email = email,
                    Createdate = DateTime.Now,
                    Createby = Createby
                };
                db.Customers.Add(newcus);
                db.SaveChanges();
                return newcus.Id;
            }
        }
        private void addActive(Product product, long customer, ActiveViewModel active)
        {
            //update status table product
            product.Status = 1;
            db.Entry(product).State = EntityState.Modified;
            db.SaveChanges();

            string ActiveBy = "";
            string partner = db.AspNetUsers.Find(User.Identity.GetUserId()).Createby;
            if (User.IsInRole("Mod"))
            {
                ActiveBy = partner;
            }
            else
            {
                ActiveBy = User.Identity.GetUserName();
            }
            DateTime? activeDate = active.Activedate;
            if (activeDate == null)
            {
                activeDate = DateTime.Now;
            }

            // Add vào bảng Active
            var prodActive = new ProductActive()
            {
                Activedate = activeDate,
                ProductId = product.Id,
                CustomerId = customer,
                Type = 2,
                Activeby = ActiveBy,
            };
            db.ProductActives.Add(prodActive);
            //}
            //else
            //{
            //    var prodActive = new ProductActive()
            //    {
            //        Activedate = DateTime.Now,
            //        ProductId = product.Id,
            //        CustomerId = customer,
            //        Type = 1,
            //    };
            //    db.ProductActivess.Add(prodActive);
            //}

            db.SaveChanges();
        }
        private void checkSendBrandname(string partner, string phone, string s1, string s2, string s3)
        {
            var brand = db.TempBrandnames.Find(partner);
            if (brand != null)
            {
                string mess = string.Format(brand.TempActive, brand.ShowName, s1, s2, s3);
                int send = Utils.SendMTBrandname.SentMTMessage(mess, phone, brand.ShowName, brand.ShowName, "0");
                Brandname brandName = new Brandname()
                {
                    Status = send,
                    Message = mess,
                    Createdate = DateTime.Now,
                    PhoneSend = phone
                };
                db.Brandnames.Add(brandName);
                db.SaveChanges();
            }
        }
        public List<Province> getProvince()
        {
            var province = db.Provinces.OrderBy(a => a.Name).ToList();
            return province;
        }

        [HttpPost]
        public ActionResult GetCity(string name)
        {
            District city = new District();
            var id = db.Provinces.Where(s => s.Name == name).SingleOrDefault();//get id theo ten
            var provi = db.Districts.Where(x => x.ProvinceId == id.Id).ToList();//get ds quan huyen
            var ress = new List<String>();//add data vao response
            foreach (var i in provi)
            {
                ress.Add(i.Name);
            }
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(ress);//convert to json
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(long? id)
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Uploadfile()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Uploadfile(FormCollection formCollection)
        {
            ViewBag.mess = "Đã có lỗi xảy ra.";
            if (Request != null)
            {
                try
                {
                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        //int count = 0;
                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                        var products = new List<UploadProductModel>();
                        int count = 0;
                        // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;
                            for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                            {
                                UploadProductModel prodview = new UploadProductModel();
                                Product product = new Product();
                                product.Createdate = DateTime.Now;

                                //check xem co phai mod k
                                //neu la mod thi phai lay theo id partner createby
                                string partner = db.AspNetUsers.Find(User.Identity.GetUserId()).Createby;
                                if (User.IsInRole("Mod"))
                                {
                                    product.Createby = partner;
                                }
                                else
                                {
                                    product.Createby = User.Identity.GetUserId();
                                }
                                //product.Createby = User.Identity.GetUserId();
                                product.Serial = workSheet.Cells[rowIterator, 1].Value.ToString();
                                product.Name = workSheet.Cells[rowIterator, 2].Value.ToString();
                                product.Code = workSheet.Cells[rowIterator, 3].Value.ToString();
                                product.Model = workSheet.Cells[rowIterator, 4].Value.ToString();
                                try { product.MadeIn = workSheet.Cells[rowIterator, 5].Value.ToString(); } catch (Exception) { }

                                try
                                {
                                    product.Exportdate = DateTime.ParseExact(workSheet.Cells[rowIterator, 6].Value.ToString(), "dd/MM/yyyy", null);
                                }
                                catch (Exception) { }
                                try
                                {
                                    product.Arisingdate = DateTime.ParseExact(workSheet.Cells[rowIterator, 7].Value.ToString(), "dd/MM/yyyy", null);
                                }
                                catch (Exception) { }
                                product.Limited = int.Parse(workSheet.Cells[rowIterator, 8].Value.ToString());
                                try
                                {
                                    product.Category = workSheet.Cells[rowIterator, 9].Value.ToString();
                                }
                                catch (Exception) { }
                                try
                                {
                                    product.Importdate = DateTime.ParseExact(workSheet.Cells[rowIterator, 10].Value.ToString(), "dd/MM/yyyy", null);
                                }
                                catch (Exception) { }

                                prodview.Product = product;
                                products.Add(prodview);
                                db.Products.Add(product);
                                var cp = db.Products.Where(a => a.Serial == product.Serial).SingleOrDefault();
                                if (cp != null)
                                {
                                    count++;
                                    prodview.Error = "Serial đã bị trùng.";
                                }

                            }
                            if (count == 0)
                            {
                                ViewBag.mess = "Đã lưu sản phẩm thành công.";
                                db.SaveChanges();
                            }
                        }
                        return View(products);
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.mess = ex;
                    return View();
                }
            }
            return View();
        }
        public ActionResult DeleteAll(long[] productIDs)
        {
            foreach (long productID in productIDs)
            {
                Product product = db.Products.Find(productID);
                db.Products.Remove(product);
            }
            db.SaveChanges();
            return Json("All the product deleted successfully!");
        }

        public FileResult ExportExc()
        {
            var stream = CreateExcelFileDetail(listExc);
            var buffer = stream as MemoryStream;
            return File(buffer.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "product_" + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + ".xlsx");
        }
        private Stream CreateExcelFileDetail(List<Product> list, Stream stream = null)
        {
            // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
            {
                excelPackage.Workbook.Properties.Author = "SVT";
                excelPackage.Workbook.Properties.Company = "SVT";
                excelPackage.Workbook.Properties.Title = "Báo cáo";
                excelPackage.Workbook.Worksheets.Add("Product");
                var workSheet = excelPackage.Workbook.Worksheets[1];
                //workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
                BindingFormatForExcelDetail(workSheet, list);
                excelPackage.Save();
                return excelPackage.Stream;
            }
        }
        private void BindingFormatForExcelDetail(ExcelWorksheet worksheet, List<Product> listItems)
        {
            // Set default width cho tất cả column
            worksheet.DefaultColWidth = 25;
            // Tự động xuống hàng khi text quá dài
            worksheet.Cells.Style.WrapText = false;
            // Tạo header
            worksheet.Cells[1, 1].Value = "STT";
            worksheet.Cells[1, 1].AutoFitColumns(6);
            worksheet.Cells[1, 2].Value = "Tên sản phẩm";
            worksheet.Cells[1, 2].AutoFitColumns();
            worksheet.Cells[1, 3].Value = "Mã cào";
            worksheet.Cells[1, 3].AutoFitColumns();
            worksheet.Cells[1, 4].Value = "Serial";
            worksheet.Cells[1, 4].AutoFitColumns(17);
            worksheet.Cells[1, 5].Value = "Model";
            worksheet.Cells[1, 5].AutoFitColumns(17);
            worksheet.Cells[1, 6].Value = "Ngày nhập";
            worksheet.Cells[1, 6].AutoFitColumns(11);
            worksheet.Cells[1, 7].Value = "Ngày xuất kho";
            worksheet.Cells[1, 7].AutoFitColumns(11);
            worksheet.Cells[1, 8].Value = "Ngày sản xuất";
            worksheet.Cells[1, 8].AutoFitColumns(11);
            worksheet.Cells[1, 9].Value = "Bảo hành(tháng)";
            worksheet.Cells[1, 9].AutoFitColumns(11);
            worksheet.Cells[1, 10].Value = "Trạng thái";
            worksheet.Cells[1, 10].AutoFitColumns(11);

            // Lấy range vào tạo format cho range đó ở đây là từ A1 tới I1
            using (var range = worksheet.Cells["A1:I1"])
            {
                // Set PatternType
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                // Set Màu cho Background
                // range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0xc6d9f1));
                range.Style.Fill.BackgroundColor.SetColor(Color.Black);
                // Canh giữa cho các text
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //// Set Font cho text  trong Range hiện tại
                //range.Style.Font.SetFromFont(new Font("Arial", 10));
                //// Set Border
                //range.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                //// Set màu cho Border
                //range.Style.Border.Bottom.Color.SetColor(Color.DarkBlue);

                //range.AutoFilter = true;
                range.Style.Font.Bold = true;
                range.Style.Font.Size = 10;
                range.Style.Font.Color.SetColor(Color.White);
            }

            // Đỗ dữ liệu từ list vào 
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                var rowCur = i + 2;
                string headerRange = "A" + rowCur + ":I" + rowCur;
                worksheet.Cells[headerRange].Style.Font.Bold = false;
                worksheet.Cells[headerRange].Style.Font.Size = 11;
                worksheet.Cells[headerRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["B" + rowCur].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells["C" + rowCur].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells["D" + rowCur].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


                worksheet.Cells[rowCur, 1].Value = i + 1;
                worksheet.Cells[rowCur, 2].Value = item.Name;
                worksheet.Cells[rowCur, 3].Value = item.Serial;
                worksheet.Cells[rowCur, 4].Value = item.Code;
                worksheet.Cells[rowCur, 5].Value = item.Model;
                if (item.Importdate != null)
                {
                    worksheet.Cells[rowCur, 6].Style.Numberformat.Format = "dd/MM/yyyy";
                    worksheet.Cells[rowCur, 6].Value = item.Importdate.Value.ToString("dd/MM/yyyy");
                }
                if (item.Exportdate != null)
                {
                    worksheet.Cells[rowCur, 7].Style.Numberformat.Format = "dd/MM/yyyy";
                    worksheet.Cells[rowCur, 7].Value = item.Exportdate.Value.ToString("dd/MM/yyyy");
                }
                if (item.Arisingdate != null)
                {
                    worksheet.Cells[rowCur, 8].Style.Numberformat.Format = "dd/MM/yyyy";
                    worksheet.Cells[rowCur, 8].Value = item.Arisingdate.Value.ToString("dd/MM/yyyy");
                }

                worksheet.Cells[rowCur, 9].Value = item.Limited;

                if (item.Status == 1)
                {

                    worksheet.Cells[rowCur, 10].Value = "Kích hoạt";
                }
                else
                {
                    worksheet.Cells[rowCur, 10].Value = "";
                }
            }
        }
    }
}