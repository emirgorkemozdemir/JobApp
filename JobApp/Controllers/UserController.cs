using JobApp.App_Start;
using JobApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;


namespace JobsApp.Controllers
{
    public class UserController : Controller
    {
        JobsDBEntities db = new JobsDBEntities();

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Register(User registering_user)
        {
            if (ModelState.IsValid)
            {
                if (db.User.Where(user=>user.Username == registering_user.Username).FirstOrDefault() == null && db.User.Where(user => user.Mail == registering_user.Mail).FirstOrDefault() == null)
                {
                    db.User.Add(registering_user);

                    registering_user.RegisterDate = DateTime.Now.Date;

                    string hashed_password = SHAConverter.ComputeSha256Hash(registering_user.Password);
                    registering_user.Password = hashed_password;

                    db.SaveChanges();
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.errormessage = "Kullanıcı adı veya mail adresi kullanılmakta";
                    return View();
                }
                
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(User posted_user)
        {
            if (posted_user.Username !=null && posted_user.Password !=null)
            {
                string hashed_pass = SHAConverter.ComputeSha256Hash(posted_user.Password);
                User selected_user = db.User.Where(user => user.Username == posted_user.Username && user.Password == hashed_pass).FirstOrDefault();

                if (selected_user!=null)
                {
                    Session["IsUserOnline"] = true;
                    Session["LoggedUserID"] = selected_user.UserID;
                    return RedirectToAction("UserMainPage");
                }
                else
                {
                    ViewBag.errmsg = "Kullanıcı adı veya şifre yanlış";
                    return View();
                }
             
            }
            else
            {
                ViewBag.errmsg = "Kullanıcı adı veya şifre boş girilemez";
                return View();
            }
           
        }

        public List<SelectListItem> GetProfessions()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            var professions = db.Profession.ToList();
            foreach (var p in professions)
            {
                SelectListItem item = new SelectListItem();
                item.Text = p.Name;
                item.Value = p.ProfessionID.ToString();
                listItems.Add(item);
            }
            return listItems;
        }

        [HttpGet]
        public JsonResult GetSkills(int professionId)
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            var skills = db.Skill.Where(s=>s.Profession==professionId).ToList();
            foreach (var s in skills)
            {
                SelectListItem item = new SelectListItem();
                item.Text = s.Name;
                item.Value =s.SkillID.ToString();
                listItems.Add(item);
            }
            return Json(listItems,JsonRequestBehavior.AllowGet);
        }

    

        // Profil Düzenleme Ekranı için bir action açtık. HttpGet oldugu için sayfa 
        // yüklenirken burası çalışacak. Bir id alyıoruz. Bunu daha sonra diger sayfalardan
        // çalıştıracagız.
        [HttpGet]
        public ActionResult EditProfile(int id)
        {
            if (Convert.ToBoolean(Session["IsUserOnline"])==true)
            {
                // Gelen id degerine göre kullanıcıyı bulup sayfaya gönderiyoruz.
                // Bu sayede sayfada kullanıcının bilgileri dolu gelecek.
                ViewBag.proffesions = GetProfessions();
                User selected_user = db.User.Find(id);
                return View(selected_user);
            }
            else
            {
                return RedirectToAction("Login");
            }
         
        }

        // Sayfadan düzenle butonuna basıldıgında burası çalışacak (post)
        // Düzenlenecek olan kullanıcı buraya gönderilecek.
        [HttpPost]
        public ActionResult EditProfile(User posted_user)
        {
            if (Convert.ToBoolean(Session["IsUserOnline"]) == true)
            {
                // Giriş yapılı mı kontorlü
                if (Convert.ToBoolean(Session["IsUserOnline"]) == true)
                {
                    // Model valid mi kontrolü
                    if (ModelState.IsValid)
                    {
                        // Burada solda kalanı veritabanındaki kullanıcı gibi düşünün.
                        // Sagdaki de posttan gelen kullanıcı.
                        // Soldaki degerleri sagdakiler ile degiştirip kaydediyoruz.
                        User db_user = db.User.Find(posted_user.UserID);
                        db_user.Name = posted_user.Name;
                        db_user.Surname = posted_user.Surname;
                        db_user.Phone = posted_user.Phone;
                        db_user.Profession = posted_user.Profession;
                        db_user.Skills = posted_user.Skills;
                        db.SaveChanges();
                        return RedirectToAction("MyProfile");
                    }
                    else
                    {
                        ViewBag.proffesions = GetProfessions();
                        return View();
                    }
                }
                return RedirectToAction("MyProfile");
            }
            else
            {
                return RedirectToAction("Login");
            }

        }


        [HttpGet]
        public ActionResult MyProfile()
        {
            // Kullanıcının oturumda olup olmadığını kontrol et
            if (Convert.ToBoolean(Session["IsUserOnline"]) == true)
            {
                int userId = Convert.ToInt32(Session["LoggedUserID"]); // Oturumdaki kullanıcı ID'si

                // Veritabanından kullanıcının bilgilerini al
                User user = db.User.Find(userId);

                if (user != null)
                {
                    // Kullanıcı bilgilerini view'a gönder
                    return View(user);
                }
                else
                {
                    // Kullanıcı bulunamadı
                    
                    return RedirectToAction("Login");
                }
            }
            else
            {
                // Oturum açılmamışsa login sayfasına yönlendir
                return RedirectToAction("Login");

            }
        }



        [HttpGet]
        public ActionResult UserMainPage()
        {
            return View();
        }
       
     
            // İlan detay sayfasına yönlendiren action
       [HttpGet]
       public ActionResult JobDetails(int id)
       {
           // İlanı ve ilan sahibini veritabanından al
           var job = db.Posting.Find(id);

           if (job == null)
           {
               // İlan bulunamazsa hata sayfasına yönlendir
               return HttpNotFound();
           }

           // Detayları gösteren view'a job modelini gönder
           return View(job);
       }

        [HttpGet]
        public ActionResult ChangeMyPassword()
        {
            if (Convert.ToBoolean(Session["IsUserOnline"]) == true)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public ActionResult ChangeMyPassword(string oldpass, string newpass1, string newpass2)
        {
            if (Convert.ToBoolean(Session["IsUserOnline"]) == true)
            {
                int selected_id = Convert.ToInt32(Session["LoggedUserID"]);
                var selected_user = db.User.Find(selected_id);
                if (SHAConverter.ComputeSha256Hash(oldpass) == selected_user.Password)
                {
                    if (newpass1==newpass2)
                    {
                        selected_user.Password = SHAConverter.ComputeSha256Hash(newpass1);
                        db.SaveChanges();
                    }
                }
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

    }

}
