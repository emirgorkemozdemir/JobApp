using JobApp.App_Start;
using JobApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;


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
                db.User.Add(registering_user);

                registering_user.RegisterDate = DateTime.Now.Date;

                string hashed_password = SHAConverter.ComputeSha256Hash(registering_user.Password);
                registering_user.Password = hashed_password;

                db.SaveChanges();
                return RedirectToAction("Login");
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
                    return RedirectToAction("MainPage");
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

        // Profil Düzenleme Ekranı için bir action açtık. HttpGet oldugu için sayfa 
        // yüklenirken burası çalışacak. Bir id alyıoruz. Bunu daha sonra diger sayfalardan
        // çalıştıracagız.
        [HttpGet]
        public ActionResult EditProfile(int id)
        {
            // Gelen id degerine göre kullanıcıyı bulup sayfaya gönderiyoruz.
            // Bu sayede sayfada kullanıcının bilgileri dolu gelecek.
            User selected_user = db.User.Find(id);
            return View(selected_user);
        }

        // Sayfadan düzenle butonuna basıldıgında burası çalışacak (post)
        // Düzenlenecek olan kullanıcı buraya gönderilecek.
        [HttpPost]
        public ActionResult EditProfile(User posted_user)
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
                    return RedirectToAction("ListUsers");
                }
                else
                {
                    
                    return View();
                }
            }
            return RedirectToAction("ListUsers");
        }
    }
}