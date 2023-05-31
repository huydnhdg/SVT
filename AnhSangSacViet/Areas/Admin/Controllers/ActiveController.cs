using Microsoft.AspNet.Identity;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using WebBHDTCHUNG.Areas.Admin.Data;
using WebBHDTCHUNG.Models;
using WebBHDTCHUNG.Utils;

namespace WebBHDTCHUNG.Areas.Admin.Controllers
{
    [Authorize]
    public class ActiveController : Controller
    {
        private CMSBHDTCHUNGEntities db = new CMSBHDTCHUNGEntities();
        //public ActionResult Index(long? id)
        //{
        //    if (User.Identity.Name == "administrator")
        //    {
        //        var model1 = from a in db.ProductActives
        //                     join b in db.Products on a.ProductId equals b.Id
        //                     join c in db.Customers on a.CustomerId equals c.Id
        //                     select new ActiveViewModel()
        //                     {
        //                         Id = a.Id,
        //                         Activedate = a.Activedate,
        //                         Activeby = a.Activeby,
        //                         Type = a.Type,
        //                         Buydate = a.Buydate,
        //                         ProductName = b.Name,
        //                         Limited = b.Limited,
        //                         Serial = b.Serial,
        //                         CustomerName = c.Name,
        //                         CustomerPhone = c.Phone,
        //                         CustomerId = c.Id,
        //                         CustomerAddress = c.Address,
        //                         CustomerDistrict = c.District,
        //                         CustomerCity = c.City
        //                     };
        //        if (id != null)
        //        {
        //            model1 = model1.Where(a => a.CustomerId == id);
        //        }
        //        return View(model1);
        //    }
        //    string userId = User.Identity.GetUserId();
        //    var model = from a in db.ProductActives
        //                join b in db.Products on a.ProductId equals b.Id
        //                where b.Createby == Utility.IdPatner //Vincent
        //                join c in db.Customers on a.CustomerId equals c.Id
        //                select new ActiveViewModel()
        //                {
        //                    Id = a.Id,
        //                    Activedate = a.Activedate,
        //                    Activeby = a.Activeby,
        //                    Type = a.Type,
        //                    Buydate = a.Buydate,
        //                    ProductName = b.Name,
        //                    Limited = b.Limited,
        //                    Serial = b.Serial,
        //                    CustomerName = c.Name,
        //                    CustomerPhone = c.Phone,
        //                    CustomerId = c.Id,
        //                    CustomerAddress = c.Address,
        //                    CustomerDistrict = c.District,
        //                    CustomerCity = c.City
        //                };
        //    if (id != null)
        //    {
        //        model = model.Where(a => a.CustomerId == id);
        //    }

        //    return View(model);
        //}
        private static List<ActiveViewModel> listExc = new List<ActiveViewModel>();
        public ActionResult Index(string SearchString, int? page, string StartDate, string EndDate, string Status, long id = 0)
        {

            var model = from a in db.ProductActives
                        join b in db.Products on a.ProductId equals b.Id
                        join c in db.Customers on a.CustomerId equals c.Id
                        where b.Createby == Utility.IdPatner
                        select new ActiveViewModel()
                        {
                            Id = a.Id,
                            Activedate = a.Activedate,
                            Activeby = a.Activeby,
                            Type = a.Type,
                            Buydate = a.Buydate,
                            ProductName = b.Name,
                            Limited = b.Limited,
                            Serial = b.Serial,
                            CustomerName = c.Name,
                            CustomerPhone = c.Phone,
                            CustomerId = c.Id,
                            CustomerAddress = c.Address,
                            CustomerDistrict = c.District,
                            CustomerCity = c.City
                        };
            if (id > 0)
            {
                model = model.Where(a => a.CustomerId == id);
            }
            if (!string.IsNullOrEmpty(SearchString))
            {
                model = model.Where(a => a.CustomerName.Contains(SearchString) || a.CustomerPhone.Contains(SearchString) || a.Serial == SearchString);
            }
            if (!String.IsNullOrEmpty(StartDate))
            {
                DateTime d = DateTime.Parse(StartDate);
                model = model.OrderByDescending(a => a.Activedate).Where(s => s.Activedate >= d);
                ViewBag.startDate = StartDate;
            }
            if (!String.IsNullOrEmpty(EndDate))
            {
                DateTime d = DateTime.Parse(EndDate);
                d = d.AddDays(1);
                model = model.OrderByDescending(a => a.Activedate).Where(s => s.Activedate <= d);
                ViewBag.endDate = EndDate;
            }

            listExc = model.ToList();
            int pageSize = 100;
            int pageNumber = (page ?? 1);
            return View(listExc.OrderByDescending(a => a.Activedate).ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
        {
            string userId = User.Identity.GetUserId();
            ViewBag.ProductId = new SelectList(db.Products.Where(a => a.Createby == userId).ToList(), "Id", "Name", null);
            ViewBag.CustomerId = new SelectList(db.Customers.Where(a => a.Createby == userId).ToList(), "Id", "Name", null);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "")] ProductActive productActive)
        {
            if (ModelState.IsValid)
            {
                db.ProductActives.Add(productActive);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(productActive);

        }
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //ProductActive productActive = db.ProductActives.Find(id);

            var model = from a in db.ProductActives
                        join b in db.Products on a.ProductId equals b.Id
                        join c in db.Customers on a.CustomerId equals c.Id
                        where b.Createby == Utility.IdPatner
                        select new ActiveViewModel()
                        {
                            Id = a.Id,
                            Activedate = a.Activedate,
                            Activeby = a.Activeby,
                            Type = a.Type,
                            Buydate = a.Buydate,
                            ProductName = b.Name,
                            Limited = b.Limited,
                            Serial = b.Serial,
                            CustomerName = c.Name,
                            CustomerPhone = c.Phone,
                            CustomerId = c.Id,
                            CustomerAddress = c.Address,
                            CustomerDistrict = c.District,
                            CustomerCity = c.City,
                            ProductId = b.Id,

                        };

            ActiveViewModel model1 = model.FirstOrDefault(p => p.Id == id);
            DateTime mydate = (DateTime)model1.Activedate;
            model1.ExDate = mydate.AddMonths((int)model1.Limited);

            if (model1 == null)
            {
                return HttpNotFound();
            }

            //Vincent
            //string userId = User.Identity.GetUserId();
            string userId = Utility.IdPatner;
            ViewBag.ProductId = new SelectList(db.Products.Where(a => a.Createby == userId).ToList(), "Id", "Name", null);
            //ViewBag.ProductCode = new SelectList(Utility.openWith.ToList(), "Key", "Value");
            ViewBag.CustomerId = new SelectList(db.Customers.Where(a => a.Createby == userId).ToList(), "Id", "Name", null);
            //productActive.InstallationAgentAddress = customer.InstallationAgentAddress;
            //productActive.CarBrandname = customer.CarBrandname;
            return View(model1);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "")] ActiveViewModel productActive)
        {
            if (ModelState.IsValid)
            {
                var model = db.ProductActives.Find(productActive.Id);
                var customer = db.Customers.Find(productActive.CustomerId);
                var product = db.Products.Find(productActive.ProductId);
                customer.Name = productActive.CustomerName;
                db.Entry(customer).State = EntityState.Modified;
                product.Serial = productActive.Serial;
                product.Name = productActive.ProductName;
                db.Entry(product).State = EntityState.Modified;
                model.Activedate = productActive.Activedate;
                model.ProductId = product.Id;
                model.Buydate = productActive.Buydate;
                model.CustomerId = customer.Id;
                model.Serial = product.Serial;
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(productActive);
        }
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductActive productActive = db.ProductActives.Find(id);
            if (productActive == null)
            {
                return HttpNotFound();
            }
            return View(productActive);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            ProductActive productActive = db.ProductActives.Find(id);
            db.ProductActives.Remove(productActive);

            var prod = db.Products.Find(productActive.ProductId);
            prod.Status = null;
            db.Entry(prod).State = EntityState.Modified;

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
                        var activatedProducts = new List<UploadActiveProductModel>();
                        int count = 0;
                        int index = 0;
                        string productSerialNotFound = "";
                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;
                            for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                            {
                                var serial = "";
                                try { serial = workSheet.Cells[rowIterator, 1].Value.ToString(); } catch (Exception) { }
                                var product = db.Products.Where(a => a.Createby == Utility.IdPatner).Where(a => a.Serial == serial).SingleOrDefault();

                                if (product == null)
                                {
                                    count++;
                                    productSerialNotFound += serial + ", ";
                                }
                                else
                                {
                                    UploadActiveProductModel prodview = new UploadActiveProductModel();
                                    Customer customer = new Customer();

                                    try { customer.Phone = workSheet.Cells[rowIterator, 2].Value.ToString(); } catch (Exception) { }
                                    try { customer.Name = workSheet.Cells[rowIterator, 3].Value.ToString(); } catch (Exception) { }
                                    try { customer.Address = workSheet.Cells[rowIterator, 4].Value.ToString(); } catch (Exception) { }
                                    try { customer.District = workSheet.Cells[rowIterator, 5].Value.ToString(); } catch (Exception) { }
                                    try { customer.City = workSheet.Cells[rowIterator, 6].Value.ToString(); } catch (Exception) { }
                                    try { customer.Email = workSheet.Cells[rowIterator, 7].Value.ToString(); } catch (Exception) { }

                                    customer.Createby = product.Createby;

                                    long cusId = addCustomer(customer);

                                    addActive(product, cusId, customer.Phone);

                                    index++;
                                    prodview.Index = index;
                                    prodview.Customer = customer;
                                    prodview.Product = product;
                                    activatedProducts.Add(prodview);
                                }
                            }

                            ViewBag.mess = "Đã kích hoạt các sản phẩm thành công.";
                            db.SaveChanges();

                            if (count > 0)
                            {
                                ViewBag.mess = "Có " + count + " số serial không tìm thấy: " + productSerialNotFound.TrimEnd(',');
                            }
                        }
                        return View(activatedProducts);
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

        private void addActive(Product product, long customer, string phone)
        {
            //update status table product
            product.Status = 1;
            db.Entry(product).State = EntityState.Modified;

            //add thong tin table productactive
            if (User.Identity.GetUserId() != null)//trường hợp đại lý kích hoạt
            {
                var existedProductActive = db.ProductActives.Where(p => p.CustomerId == customer
                                                    && p.ProductId == product.Id).FirstOrDefault();
                if (existedProductActive != null)
                {
                    existedProductActive.ProductId = product.Id;
                    existedProductActive.CustomerId = customer;
                    existedProductActive.Type = 2;
                    //Activeby = User.Identity.GetUserId(),
                    existedProductActive.Activeby = User.Identity.GetUserName();//Vincent
                    db.Entry(existedProductActive).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    var prodActive = new ProductActive()
                    {
                        Activedate = DateTime.Now,
                        ProductId = product.Id,
                        CustomerId = customer,
                        Type = 2,
                        //Activeby = User.Identity.GetUserId(),
                        Activeby = User.Identity.GetUserName(),//Vincent                                                               
                    };
                    db.ProductActives.Add(prodActive);
                }
            }
            else
            {
                var existedProductActive = db.ProductActives.Where(p => p.CustomerId == customer
                                                    && p.ProductId == product.Id).FirstOrDefault();
                if (existedProductActive != null)
                {
                    existedProductActive.ProductId = product.Id;
                    existedProductActive.CustomerId = customer;
                    existedProductActive.Type = 1;
                    //Activeby = User.Identity.GetUserId(),
                    existedProductActive.Activeby = User.Identity.GetUserName();//Vincent
                    db.Entry(existedProductActive).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    var prodActive = new ProductActive()
                    {
                        Activedate = DateTime.Now,
                        ProductId = product.Id,
                        CustomerId = customer,
                        Type = 1,
                        //Activeby = User.Identity.GetUserId(),
                        Activeby = User.Identity.GetUserName(),//Vincent                                                               
                    };
                    db.ProductActives.Add(prodActive);
                }
            }

            db.SaveChanges();
        }
        private long addCustomer(Customer customer)
        {
            customer.Phone = customer.Phone.Trim();
            customer.Phone = Utils.FormatString.formatUserId(customer.Phone, 0);
            var existedCustomer = db.Customers.Where(a => a.Phone == customer.Phone);
            if (existedCustomer.Count() > 0)
            {
                var existedCustomer1 = existedCustomer.First();
                //Vincent
                if (String.IsNullOrEmpty(customer.Name))
                {
                    existedCustomer1.Name = customer.Name;
                    existedCustomer1.City = customer.City;
                    existedCustomer1.District = customer.District;
                    existedCustomer1.Address = customer.Address;
                    existedCustomer1.Phone = customer.Phone;
                    existedCustomer1.Email = customer.Email;
                    db.Entry(existedCustomer1).State = EntityState.Modified;
                    return db.SaveChanges();
                }
                else
                {
                    return existedCustomer1.Id;
                }

            }
            else
            {
                customer.Createdate = DateTime.Now;
                db.Customers.Add(customer);
                db.SaveChanges();
                db.Entry(customer).GetDatabaseValues();
                return customer.Id;
            }
        }
    }
}