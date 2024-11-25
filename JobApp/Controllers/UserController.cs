using JobApp.App_Start;
using JobApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.IO;


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
                if (db.User.Where(user => user.Username == registering_user.Username).FirstOrDefault() == null && db.User.Where(user => user.Mail == registering_user.Mail).FirstOrDefault() == null)
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
            if (posted_user.Username != null && posted_user.Password != null)
            {
                string hashed_pass = SHAConverter.ComputeSha256Hash(posted_user.Password);
                User selected_user = db.User.Where(user => user.Username == posted_user.Username && user.Password == hashed_pass).FirstOrDefault();

                if (selected_user != null)
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
            var skills = db.Skill.Where(s => s.Profession == professionId).ToList();
            foreach (var s in skills)
            {
                SelectListItem item = new SelectListItem();
                item.Text = s.Name;
                item.Value = s.SkillID.ToString();
                listItems.Add(item);
            }
            return Json(listItems, JsonRequestBehavior.AllowGet);
        }



        // Profil Düzenleme Ekranı için bir action açtık. HttpGet oldugu için sayfa 
        // yüklenirken burası çalışacak. Bir id alyıoruz. Bunu daha sonra diger sayfalardan
        // çalıştıracagız.
        [HttpGet]
        public ActionResult EditProfile(int id)
        {
            if (Convert.ToBoolean(Session["IsUserOnline"]) == true)
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
            List<Posting> postings = db.Posting.ToList();

            List<Posting> last_postings = new List<Posting>();

            foreach (Posting posting in postings)
            {
                var skills = posting.Skills.Split(',');
                posting.SkillsList = new List<Skill>();
                foreach (string skill_id in skills)
                {
                    Skill selected = db.Skill.Find(Convert.ToInt32(skill_id));

                    posting.SkillsList.Add(selected);
                }
                last_postings.Add(posting);
            }
            return View(last_postings);
        }

        public string CheckSkills(int user_id, int job_id)
        {
            var selected_user = db.User.Find(user_id);
            var selected_job = db.Posting.Find(job_id);

            if (selected_user.Skills==null)
            {
                return "Yetenek eşleşmesi için profilinizi doldurunuz";
            }

            var user_skills = selected_user.Skills.Split(',');
            var job_skills = selected_job.Skills.Split(',');

            int matched_skill = 0;
            foreach (var yetenek in job_skills)
            {
                if (user_skills.Contains(yetenek))
                {
                    matched_skill++;
                }
            }

            return $"Bu iş ilanı için {job_skills.Count()} aranan yetenek içerisinden {matched_skill} tanesine sahipsiniz";
        }

        // İlan detay sayfasına yönlendiren action
        [HttpGet]
        public ActionResult JobDetails(int job_id)
        {
            if (Convert.ToBoolean(Session["IsUserOnline"]) == true)
            {
                // İlanı ve ilan sahibini veritabanından al
                var job = db.Posting.Find(job_id);

                if (job == null)
                {
                    // İlan bulunamazsa hata sayfasına yönlendir
                    return HttpNotFound();
                }


                var skills = job.Skills.Split(',');
                job.SkillsList = new List<Skill>();
                foreach (string skill_id in skills)
                {
                    Skill selected = db.Skill.Find(Convert.ToInt32(skill_id));

                    job.SkillsList.Add(selected);
                }

                ViewBag.matched = CheckSkills(Convert.ToInt32(Session["LoggedUserID"]), job_id);
                // Detayları gösteren view'a job modelini gönder
                return View(job);
            }
            else
            {
                return RedirectToAction("Login");
            }
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
                    if (newpass1 == newpass2)
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

        [HttpPost]
        public ActionResult UploadCv(int companyid)
        {
            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase postedFile = Request.Files["myfile"];
                string path = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                int selected_user = Convert.ToInt32(Session["LoggedUserID"]);
                postedFile.SaveAs(path + Path.GetFileName(postedFile.FileName));

                // Cv'yi kullanıcıyı da atayarak veritabanına kaydettim.
                CV saving_cv = new CV();
                saving_cv.Link = (path + Path.GetFileName(postedFile.FileName));
                saving_cv.SelectedUser = selected_user;
                db.CV.Add(saving_cv);
                db.SaveChanges();

                // Kullanıcının sahibi oldugu cv'nin idsini seçmem lazım.
                var selected_cv = db.CV.Where(cv => cv.SelectedUser == selected_user).FirstOrDefault();

                Application application = new Application();
                application.CV =Convert.ToInt32(selected_cv.CvID);
                application.Companyy = companyid;
                application.Userr = selected_user;
                db.Application.Add(application);
                db.SaveChanges();
            }

            return RedirectToAction("UserMainPage");
        }
    }

}
