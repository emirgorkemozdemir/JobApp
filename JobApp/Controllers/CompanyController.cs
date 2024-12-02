﻿using JobApp.App_Start;
using JobApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


    namespace JobsApp.Controllers
    {
        public class CompanyController : Controller
        {
            JobsDBEntities db = new JobsDBEntities();

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Register(Company registeringCompany)
        {
            if (ModelState.IsValid)
            {

                if (db.Company.FirstOrDefault(c => c.Cusername == registeringCompany.Cusername) == null)
                {



                    string hashedPassword = SHAConverter.ComputeSha256Hash(registeringCompany.Cpassword);
                    registeringCompany.Cpassword = hashedPassword;

                    db.Company.Add(registeringCompany);
                    db.SaveChanges();

                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.ErrorMessage = "Bu isimde bir şirket kaydı bulunmaktadır";
                }
            }

            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Company loginCompany)
        {
            if (!string.IsNullOrEmpty(loginCompany.Cusername) && !string.IsNullOrEmpty(loginCompany.Cpassword))
            {

                string hashedPassword = SHAConverter.ComputeSha256Hash(loginCompany.Cpassword);


                Company selectedCompany = db.Company.FirstOrDefault(
                    c => c.Cusername == loginCompany.Cusername && c.Cpassword == hashedPassword);

                if (selectedCompany != null)
                {
                    Session["IsCompanyOnline"] = true;
                    Session["LoggedCompanyID"] = selectedCompany.CompanyID;

                    return RedirectToAction("CompanyMainPage");
                }
                else
                {
                    ViewBag.ErrorMessage = "Şirket kullanıcı adı veya şifre yanlış";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Şirket kullanıcı adı veya şifre boş bırakılamaz";
            }

            return View();
        }

        // 1. İş ilanı açma
        [HttpGet]
            public ActionResult CreateJobPosting()
            {
                if (Convert.ToBoolean(Session["IsCompanyOnline"]) == true)
                {
                    // Şirketin bağlı olduğu profesyonel meslekleri ve becerileri getirmek için yardımcı metot
                    ViewBag.proffesions = GetProfessions(); // Professions verilerini gönder
                    return View();
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }

            [HttpPost]
            public ActionResult CreateJobPosting(Posting newcPosting, string skills)
            {
                if (ModelState.IsValid)
                {

                // Giriş yapmış kullanıcının şirket ID'sini al
                int companyID = Convert.ToInt32(Session["LoggedCompanyID"]);

                    newcPosting.Company = companyID; // İş ilanına ait şirketi belirle
                newcPosting.Skills = skills.ToString();

                    db.Posting.Add(newcPosting); // Yeni iş ilanını veritabanına ekle
                    db.SaveChanges();

                    return RedirectToAction("ViewJobPostings");
                }
            ViewBag.proffesions = GetProfessions();
            return View(newcPosting);
            }

            // 2. İş ilanlarını görüntüleme
            [HttpGet]
            public ActionResult ViewJobPostings()
            {
                if (Convert.ToBoolean(Session["IsCompanyOnline"]) == true)
                {
                    int companyID = Convert.ToInt32(Session["LoggedCompanyID"]);

                    var jobPostings = db.Posting.Where(p => p.Company == companyID).ToList(); // Şirketin ilanlarını getir

                List<Posting> last_postings = new List<Posting>();

                foreach (Posting posting in jobPostings)
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
                return View(jobPostings);
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }

            // 3. İş ilanını silme
            [HttpPost]
            public ActionResult DeleteJobPosting(int cpostingID)
            {
                if (Convert.ToBoolean(Session["IsCompanyOnline"]) == true)
                {
                    var postingToDelete = db.Posting.Find(cpostingID);

                    if (postingToDelete != null)
                    {
                        db.Posting.Remove(postingToDelete); // İş ilanını sil
                        db.SaveChanges();
                    }

                    return RedirectToAction("ViewJobPostings");
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }

            // 4. İş ilanını güncelleme
            [HttpGet]
            public ActionResult EditJobPosting(int cpostingID)
            {
                if (Convert.ToBoolean(Session["IsCompanyOnline"]) == true)
                {
                ViewBag.proffesions = GetProfessions();
                var posting = db.Posting.Find(cpostingID);

                    if (posting != null)
                    {
                        ViewBag.proffesions = GetProfessions();
                        return View(posting);
                    }
                    else
                    {
                        return HttpNotFound();
                    }
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }

            [HttpPost]
            public ActionResult EditJobPosting(Posting updatedPosting)
            {
                if (ModelState.IsValid)
                {
                    var postingToUpdate = db.Posting.Find(updatedPosting.PostingID);

                    if (postingToUpdate != null)
                    {
                        postingToUpdate.Title = updatedPosting.Title;
                        postingToUpdate.Description = updatedPosting.Description;
                        postingToUpdate.Skills = updatedPosting.Skills;
                      
                        db.SaveChanges();
                    }

                    return RedirectToAction("ViewJobPostings");
                }
            ViewBag.proffesions = GetProfessions();
            return View(updatedPosting);
            }

            // Yardımcı metot: Profesyonel meslekler listesi
            public List<SelectListItem> GetProfessions()
            {
                List<SelectListItem> listItems = new List<SelectListItem>();
                var professions = db.Profession.ToList();
                foreach (var p in professions)
                {
                    SelectListItem item = new SelectListItem
                    {
                        Text = p.Name,
                        Value = p.ProfessionID.ToString()
                    };
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
    }
    }



