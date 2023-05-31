using Microsoft.AspNet.Identity;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebBHDTCHUNG.Models;
using WebBHDTCHUNG.Utils;

namespace WebBHDTCHUNG.Controllers
{
    [RoutePrefix("kich-hoat")]
    public class ActiveController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private CMSBHDTCHUNGEntities db = new CMSBHDTCHUNGEntities();
        [Route]
        public ActionResult Index()
        {
            TempData["province"] = getProvince();
            return View();
        }
        [HttpPost]
        public ActionResult Send(string serial, string prodname, string name, string province, string district, string address, string phone, string email)
        {
            phone = Utils.FormatString.formatUserId(phone, 0);
            logger.Info(string.Format("{0} {1} {2} {3} {4} {5} {6}", serial, prodname, name, province, district, address, phone));
            var product = db.Products.Where(a => a.Createby == Utility.IdPatner).Where(a => a.Serial == serial).SingleOrDefault();

            if (product == null)
            {
                logger.Info("not valid");
                return Json(ResResult("Không tìm thấy thông tin sản phẩm", null), JsonRequestBehavior.AllowGet);//san pham khong ton tai
            }
            else
            {
                var checkactive = db.TempBrandnames.Find(product.Createby);//check co kich hoat qua web khong
                if (checkactive.Activeweb != 1)
                {
                    return Json(ResResult("Không tìm thấy thông tin sản phẩm", null), JsonRequestBehavior.AllowGet);
                }
                if (product.Status != 1)
                {
                    //add thong tin khach hàng
                    long cusId = addCustomer(name, province, district, address, phone, email, product.Createby);
                    //add thong tin kich hoat san pham
                    //string productCode = Utility.getProductCode(product_code);
                    //string productCode = product_code;
                    addActive(product, cusId, phone);
                    //gui brandname neu partner dang ki
                    if (checkactive.Status == 1)
                    {
                        checkSendBrandname(product.Createby, phone, serial, DateTime.Now.ToString("dd/MM/yyyy"), DateTime.Now.AddMonths(product.Limited ?? default(int)).ToString("dd/MM/yyyy"));
                    }

                    //show thong tin kich hoat ra website
                    logger.Info("active ok");                    
                    var model = new ProductActiveModel()
                    {
                        Name = product.Name,
                        Serial = product.Serial,
                        Model = product.Model,
                        MadeIn = product.MadeIn,
                        Limited = product.Limited,
                        Activedate = DateTime.Now
                    };
                    return Json(ResResult("Sản phẩm này đã được kích hoạt thành công", model), JsonRequestBehavior.AllowGet);//san pham khong ton tai
                }
                else
                {
                    logger.Info("actived");
                    return Json(ResResult("Sản phẩm này đã được kích hoạt trước đó", null), JsonRequestBehavior.AllowGet);//san pham khong ton tai
                }
            }
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
        private void addActive(Product product, long customer, string phone)
        {
            //update status table product
            product.Status = 1;
            db.Entry(product).State = EntityState.Modified;

            //add thong tin table productactive
            if (User.Identity.GetUserId() != null)//trường hợp đại lý kích hoạt
            {
                var prodActive = new ProductActive()
                {
                    Activedate = DateTime.Now,
                    ProductId = product.Id,
                    CustomerId = customer,
                    Type = 2,
                    //Activeby = User.Identity.GetUserId(),
                    Activeby = User.Identity.GetUserName(),//Vincent
                    //ProductCode = productCode
                };
                db.ProductActives.Add(prodActive);
            } 
            else
            {
                var prodActive = new ProductActive()
                {
                    Activedate = DateTime.Now,
                    ProductId = product.Id,
                    CustomerId = customer,
                    Type = 1,
                    Activeby = phone,//Vincent

                };
                db.ProductActives.Add(prodActive);
            }

            db.SaveChanges();
        }
        private long addCustomer(string name, string province, string district, string address, string phone, string email, string createby)
        {
            var customer = db.Customers.Where(a => a.Phone == phone).SingleOrDefault();
            if (customer != null)
            {
                //Vincent
                if (String.IsNullOrEmpty(customer.Name))
                {
                    customer.Name = name;
                    customer.City = province;
                    customer.District = district;
                    customer.Address = address;
                    customer.Phone = phone;
                    customer.Email = email;
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
                    Name = name,
                    City = province,
                    District = district,
                    Address = address,
                    Phone = phone,
                    Email = email,
                    Createdate = DateTime.Now,
                    Createby = createby
                };
                db.Customers.Add(newcus);
                db.SaveChanges();
                return newcus.Id;
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

        public string ResResult(string message, ProductActiveModel model)
        {
            Ress ress = new Ress()
            {
                message = message,
                prodActive = model
            };
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(ress);//convert to json
            return result;
        }
        public class Ress
        {
            public string message { get; set; }
            public ProductActiveModel prodActive { get; set; }
        }

        [HttpPost]
        public ActionResult GetProduct(string serial)
        {
            var prod = db.Products.Where(a => a.Serial == serial).SingleOrDefault();
            string result = "";
            if (prod != null)
            {
                result = prod.Name;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult GetCustomer(string phone)
        {
            phone = Utils.FormatString.formatUserId(phone, 0);
            var cus = db.Customers.Where(a => a.Phone == phone).SingleOrDefault();
            string result = null;
            if (phone == null)
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            if (cus != null)
            {
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                result = javaScriptSerializer.Serialize(cus);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }       
    }
}