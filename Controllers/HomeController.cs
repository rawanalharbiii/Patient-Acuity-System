using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using acuity_tooll.Models;
using Oracle.ManagedDataAccess.Client;
using System.ComponentModel.DataAnnotations;
using System.Data;
using PagedList;
using System.Data.Entity;
using EntityState = System.Data.Entity.EntityState;

namespace acuity_tooll.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(USER_PROFILE objUser)
        {
            if (ModelState.IsValid)
            {
                using (Entities db = new Entities())
                {
                    var obj = db.USER_PROFILE.Where(a => a.USER_NAME.Equals(objUser.USER_NAME) && a.USER_PASSWORD.Equals(objUser.USER_PASSWORD)).FirstOrDefault();
                    if (obj != null)
                    {
                        Session["USER_ID"] = obj.USER_ID.ToString();
                        Session["USER_NAME"] = obj.USER_NAME.ToString();
                        return RedirectToAction("welcom");
                    }
                    else { ViewBag.Message = "  Invalid LogIn , please try again !!"; }
                }
            }
           
            
            return View(objUser);
        }
        public ActionResult welcom() 
        {
            Entities db = new Entities();
            List<RCM_OMAR> RCMList = db.RCM_OMAR.ToList();
            ViewBag.A1 = RCMList.Where(x => x.WARD_CODE == "1A").ToList().Count();
            ViewBag.A2 = RCMList.Where(x => x.WARD_CODE == "2A").ToList().Count();
            ViewBag.B1 = RCMList.Where(x => x.WARD_CODE == "1B").ToList().Count();
            ViewBag.B2 = RCMList.Where(x => x.WARD_CODE == "2B").ToList().Count();
            ViewBag.B3 = RCMList.Where(x => x.WARD_CODE == "3B").ToList().Count();
            ViewBag.B4 = RCMList.Where(x => x.WARD_CODE == "4B").ToList().Count();
            ViewBag.C1 = RCMList.Where(x => x.WARD_CODE == "1C").ToList().Count();
            ViewBag.D1 = RCMList.Where(x => x.WARD_CODE == "1D").ToList().Count();
            ViewBag.E1 = RCMList.Where(x => x.WARD_CODE == "1E").ToList().Count();
            ViewBag.I1 = RCMList.Where(x => x.WARD_CODE == "1I").ToList().Count();
            ViewBag.K1 = RCMList.Where(x => x.WARD_CODE == "1K").ToList().Count();
            ViewBag.M1 = RCMList.Where(x => x.WARD_CODE == "1M").ToList().Count();
            ViewBag.N1 = RCMList.Where(x => x.WARD_CODE == "1N").ToList().Count();

            return View(); 
        }


        public ActionResult UserDashBoard (decimal? id, string ns, DateTime? d )
        {
            Entities db = new Entities();
            return View(db.ACUITY_EV.Include(a => a.ACUITY_DETAIL).Include(a => a.ACUITY_MASTER).Include(a => a.USER_PROFILE).Include(a => a.ACUITY_TOOL).Where(e => e.ACUITY_TOOL.ADD_DATE==d || e.ACUITY_TOOL.WARD_CODE.Contains(ns.ToUpper()) || e.ACUITY_TOOL.WARD_CODE == ns || e.ACUITY_TOOL.MRN==id).ToList());
        }


        public ActionResult page1(string search)
        {
            Entities db = new Entities();

            return View(db.RCM_OMAR.Where(x => x.WARD_CODE.Contains(search.ToUpper()) || x.PATIENT_NAME.Contains(search.ToUpper()) || /*x.MRN.Equals(decimal.Parse(search)) ||*/search == null).ToList());

        }

        public ActionResult ac_page()
        {
            Entities db = new Entities();
            List<ACUITY_TOOL> acList = db.ACUITY_TOOL.ToList();


            return View(acList);

        }
        public ActionResult Contact()
        { return View(); }
            [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateTool(decimal id)
        {
            Entities db = new Entities();
            ACUITY_TOOL obj = db.ACUITY_TOOL.Where(x => x.MRN == id).FirstOrDefault();
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult save(ACUITY_TOOL up)
        {
            Entities db = new Entities();
            ACUITY_TOOL obj = db.ACUITY_TOOL.Where(x => x.MRN == up.MRN).FirstOrDefault();
            obj.ACC_N = up.ACC_N;
            obj.P_NAME = up.P_NAME;
            obj.ADD_DATE = up.ADD_DATE;
            obj.ROOM = up.ROOM;
            obj.BED = up.BED;
            obj.DURATION = up.DURATION;
            obj.WARD_CODE = up.WARD_CODE;
            obj.WARD_DIS = up.WARD_DIS;
            obj.DOCTOR = up.DOCTOR;

            db.SaveChanges();
            return RedirectToAction("ac_page");

        }
        [HttpGet]
        public ActionResult ReviewTool(string search)
        {
            Entities db = new Entities();
            List<ACUITY_EV> rcmList = db.ACUITY_EV.Where(x => x.ACUITY_TOOL.WARD_CODE.Contains(search.ToUpper()) || search == null && x.REVIEW!="T").ToList();
            return View(rcmList);
        }


        [HttpPost]
        public ActionResult ReviewTool(IEnumerable<decimal?> ch1)
        {
            if (Session["USER_ID"] != null)
            {

                List<ACUITY_EV> getch1 = new List<ACUITY_EV>();
                Entities db = new Entities();
                getch1 = db.ACUITY_EV.Where(x => ch1.Contains(x.AC_ID)).ToList();
                try
                {
                    foreach (var s in getch1)
                    {
                        s.REVIEW = "T";
                        s.REVIEWER_NAME = Session["USER_NAME"].ToString();
                        db.Entry(s).State = EntityState.Modified;
                    }
                    ModelState.Clear();
                    TryValidateModel(getch1);

                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();
                    }
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    Exception raise = dbEx;
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            string message = string.Format("{0}:{1}",
                                validationErrors.Entry.Entity.ToString(),
                                validationError.ErrorMessage);
                            raise = new InvalidOperationException(message, raise);
                        }
                    }
                    throw raise;
                }
                return View(db.ACUITY_EV.Where(x => x.REVIEW != "T").ToList());
            }

            else
            {
                return RedirectToAction("Index");
            }
        }

    public ActionResult LogOff()
        {

            Session["USER_NAME"] = null;
            Session.Abandon();
            return RedirectToAction("welcom");
        }


        public ActionResult addacuity(string search)
        {
            Entities db = new Entities();
            db.Database.ExecuteSqlCommand("INSERT INTO ACUITY_TOOL (ACUITY_TOOL.ACC_N, ACUITY_TOOL.ADD_DATE, ACUITY_TOOL.BED, ACUITY_TOOL.DOCTOR, ACUITY_TOOL.DURATION, ACUITY_TOOL.MRN, ACUITY_TOOL.P_NAME, ACUITY_TOOL.ROOM, ACUITY_TOOL.WARD_CODE, ACUITY_TOOL.WARD_DIS,ACUITY_TOOL.STATUS) SELECT RCM_OMAR.ACCOUNT_, RCM_OMAR.ADMISSION_DATE_, RCM_OMAR.BED, RCM_OMAR.NAME_, RCM_OMAR.LOS, RCM_OMAR.MRN, RCM_OMAR.PATIENT_NAME, RCM_OMAR.ROOM, RCM_OMAR.WARD_CODE, RCM_OMAR.WARD_DIS,'I' FROM RCM_OMAR WHERE NOT EXISTS(SELECT * FROM ACUITY_TOOL WHERE MRN = RCM_OMAR.MRN AND ADD_DATE = RCM_OMAR.ADMISSION_DATE_ AND ACC_N = RCM_OMAR.ACCOUNT_)");
            db.Database.ExecuteSqlCommand("UPDATE ACUITY_TOOL SET STATUS ='D'WHERE MRN not in  (SELECT MRN FROM RCM_OMAR)");

            return View(db.ACUITY_TOOL.Where(x => x.STATUS == "I").Where(x => x.WARD_CODE.Contains(search.ToUpper()) || x.P_NAME.Contains(search.ToUpper()) || search == null).ToList());

        }

    }

}