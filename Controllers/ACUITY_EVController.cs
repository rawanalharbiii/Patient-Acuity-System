using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using acuity_tooll.Models;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation;
using EntityState = System.Data.Entity.EntityState;
using PagedList;
using PagedList.Mvc;
namespace acuity_tooll.Controllers
{
    public class ACUITY_EVController : Controller
    {
        private Entities db = new Entities();

        // GET: ACUITY_EV
        private const int pageSize = 500;
        public ActionResult acuitylist(int? page, string search)
        {
            int pageNumber = (page ?? 1);
            List<ACUITY_DETAIL> DL = new List<ACUITY_DETAIL>();
            ViewBag.MASTER_ID = new SelectList(db.ACUITY_MASTER, "ID", "TITLE");
            ViewBag.DET_ID = new SelectList(db.ACUITY_DETAIL, "ID", "SUB_TITLE");
            ViewBag.MasterList = db.ACUITY_MASTER.ToList();
            ViewBag.DetailList = db.ACUITY_DETAIL.ToList();
            ACUITY_EV eV = new ACUITY_EV();
            List<ACUITY_EV> evList = db.ACUITY_EV.Where(x=>x.ACUITY_TOOL.STATUS=="I").OrderByDescending(x=>x.ADD_DATE).Where(x => x.ACUITY_TOOL.WARD_CODE.Contains(search.ToUpper()) || x.ACUITY_TOOL.P_NAME.Contains(search.ToUpper()) ||x.NURSE_NAME.Contains(search) || search == null).ToList();
            return View(evList.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Index(decimal id)
        {
            List<ACUITY_DETAIL> DL = new List<ACUITY_DETAIL>();
            ViewBag.MASTER_ID = new SelectList(db.ACUITY_MASTER, "ID", "TITLE");
            ViewBag.DET_ID = new SelectList(db.ACUITY_DETAIL, "ID", "SUB_TITLE");
            ViewBag.MasterList = db.ACUITY_MASTER.ToList();
            ViewBag.DetailList = db.ACUITY_DETAIL.ToList();
            ACUITY_EV eV = new ACUITY_EV();
            var aCUITY_EV = db.ACUITY_TOOL.Where(e => e.ID_ACUITY == id).Include(e => e.ACUITY_EV.Select(ev => ev.ACUITY_DETAIL)).Include(e => e.ACUITY_EV.Select(ev => ev.ACUITY_MASTER)).FirstOrDefault();
            return View(aCUITY_EV);
        }

        // GET: ACUITY_EV/Details/5
        public ActionResult Details(decimal? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ACUITY_EV aCUITY_EV = db.ACUITY_EV.Where(X => X.AC_ID == id).Include(a=>a.ACUITY_DETAIL).Include(e=>e.ACUITY_MASTER).FirstOrDefault();
            List<ACUITY_EV> AV = db.ACUITY_EV.Where(X => X.AC_ID == id).Include(a => a.ACUITY_DETAIL).Include(e => e.ACUITY_MASTER).ToList(); 

            if (aCUITY_EV == null)
            {
                return HttpNotFound();
            }
            ViewBag.SCORE = AV.Sum(e => e.ACUITY_DETAIL.VALUE);
            return View(aCUITY_EV);
        }

        // GET: ACUITY_EV/Create
        public ActionResult Create(decimal? id = null)
        {
            if (Session["USER_ID"] != null)
            {
                ViewBag.DET_ID = new SelectList(db.ACUITY_DETAIL, "ID", "SUB_TITLE");
                ViewBag.sv = new SelectList(db.ACUITY_DETAIL, "ID", "VALUE");
                ViewBag.MASTER_ID = new SelectList(db.ACUITY_MASTER, "ID", "TITLE");
                ViewBag.USER_ID = new SelectList(db.USER_PROFILE, "USER_ID", "USER_NAME");
                ViewBag.MRN = new SelectList(db.ACUITY_TOOL, "ID_ACUITY", "MRN", id);
                ViewBag.AC_ID = new SelectList(db.ACUITY_TOOL, "ID_ACUITY", "ID_ACUITY", id);
                ViewBag.P_NAME = new SelectList(db.ACUITY_TOOL, "ID_ACUITY", "P_NAME", id);
                ViewBag.ROOM = new SelectList(db.ACUITY_TOOL, "ID_ACUITY", "ROOM", id);
                List<SelectListItem> SHIFTL = new List<SelectListItem>() { new SelectListItem { Text = "AM", Value = "AM" }, new SelectListItem { Text = "PM", Value = "PM" } };
                ViewBag.SHIFT = SHIFTL;
                ViewBag.MasterList = db.ACUITY_MASTER.ToList();
                ViewBag.DetailList = db.ACUITY_DETAIL.ToList();
                List<ACUITY_EV> m = db.ACUITY_EV.Where(x => x.AC_ID == id).ToList();
            

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,AC_ID,MASTER_ID,DET_ID,SHIFT,ADD_DATE,SCORE,NURSE_NAME,REVIEW,REVIEWER_NAME,UPDATE_DATE,USER_ID")] ACUITY_EV aCUITY_EV, List<ACUITY_EV> details)
        {
            decimal sa= Convert.ToInt32(Session["USER_ID"]);
            try
            {
                    foreach (var detail in details)
                    {
                        detail.AC_ID = aCUITY_EV.AC_ID;
                        detail.SHIFT = aCUITY_EV.SHIFT;
                        detail.USER_ID = sa;
                        detail.REVIEW = aCUITY_EV.REVIEW;
                        detail.SHIFT = aCUITY_EV.SHIFT;
                        detail.NURSE_NAME = aCUITY_EV.NURSE_NAME;
                        detail.ADD_DATE = System.DateTime.Now.Date;
                        detail.TOTAL = aCUITY_EV.TOTAL;
                        detail.SCORE = aCUITY_EV.SCORE;
                    }
               
                ModelState.Clear();
                TryValidateModel(details);

                if (ModelState.IsValid)
                {  
                    db.ACUITY_EV.AddRange(details);
                    db.SaveChanges();
                    return RedirectToAction("acuitylist");
                }
                ModelState.Clear();
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
            List<SelectListItem> SHIFTL = new List<SelectListItem>(){new SelectListItem{ Text="AM", Value = "AM" },new SelectListItem{ Text="PM", Value = "PM" }};
            ViewBag.DET_ID = new SelectList(db.ACUITY_DETAIL, "ID", "SUB_TITLE", aCUITY_EV.DET_ID);
            ViewBag.MASTER_ID = new SelectList(db.ACUITY_MASTER, "ID", "TITLE", aCUITY_EV.MASTER_ID);
            ViewBag.USER_ID = new SelectList(db.USER_PROFILE, "USER_ID", "USER_NAME", aCUITY_EV.USER_ID);
            ViewBag.AC_ID = new SelectList(db.ACUITY_TOOL, "ID_ACUITY", "ID_ACUITY", aCUITY_EV.AC_ID);
            ViewBag.P_NAME = new SelectList(db.ACUITY_TOOL, "ID_ACUITY", "P_NAME", aCUITY_EV.AC_ID);
            ViewBag.ROOM = new SelectList(db.ACUITY_TOOL, "ID_ACUITY", "ROOM", aCUITY_EV.AC_ID);
            ViewBag.SHIFT = SHIFTL;
            ViewBag.MasterList = db.ACUITY_MASTER.ToList();
            ViewBag.DetailList = db.ACUITY_DETAIL.ToList();
            return RedirectToAction("acuitylist");
        }

        public ActionResult Report(decimal? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var main = db.ACUITY_TOOL.Where(e => e.ID_ACUITY == id).Include(e => e.ACUITY_EV.Select(ev => ev.ACUITY_DETAIL)).Include(e => e.ACUITY_EV.Select(ev => ev.ACUITY_MASTER)).FirstOrDefault();

            if (main == null)
            {
                return HttpNotFound();
            }
            ViewBag.MasterList = db.ACUITY_MASTER.ToList();
            ViewBag.DetailList = db.ACUITY_DETAIL.ToList();
            return View(main);
        }
        // GET: ACUITY_EV/Edit/1803
        public ActionResult Edit(decimal? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ACUITY_EV aCUITY_EV = db.ACUITY_EV.Where(X => X.AC_ID == id).FirstOrDefault();
            if (aCUITY_EV == null)
            {
                return HttpNotFound();
            }
            ViewBag.MRN = new SelectList(db.ACUITY_TOOL, "ID_ACUITY", "MRN", id);
            ViewBag.AC_ID = new SelectList(db.ACUITY_TOOL, "ID_ACUITY", "ID_ACUITY", id);
            ViewBag.P_NAME = new SelectList(db.ACUITY_TOOL, "ID_ACUITY", "P_NAME", id);
            ViewBag.ROOM = new SelectList(db.ACUITY_TOOL, "ID_ACUITY", "ROOM", id);
            ViewBag.SHIFT = new List<SelectListItem>(){ new SelectListItem { Text = "AM", Value = "AM" }, new SelectListItem { Text = "PM", Value = "PM" } }.Select(r => new SelectListItem {
               Value = r.Value,
               Text = r.Text,
               Selected = r.Value == aCUITY_EV.SHIFT }).ToList();
            return View(aCUITY_EV);
        }

        // POST: ACUITY_EV/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ACUITY_EV aCUITY_EV)
        {
            if (Session["USER_ID"] != null)
            {
                List<ACUITY_EV> details = db.ACUITY_EV.Where(x=>x.AC_ID==aCUITY_EV.AC_ID).ToList();
            try
            {
                foreach (var s in details)
                {
                    s.NURSE_NAME = aCUITY_EV.NURSE_NAME;
                    s.SHIFT = aCUITY_EV.SHIFT;
                    s.UPDATE_DATE = System.DateTime.Now.Date;
                    db.Entry(s).State = EntityState.Modified;
                }
                ModelState.Clear();
                TryValidateModel(details);

                if (ModelState.IsValid)
                {
                    db.SaveChanges();
                    return RedirectToAction("acuitylist");
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

            return View(aCUITY_EV);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: ACUITY_EV/Delete/5
        public ActionResult Delete(decimal? id)
        {
            if (Session["USER_ID"] != null)
            {
                if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ACUITY_EV aCUITY_EV = db.ACUITY_EV.Where(X => X.AC_ID == id).FirstOrDefault();
            if (aCUITY_EV == null)
            {
                return HttpNotFound();
            }
            return View(aCUITY_EV);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: ACUITY_EV/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(decimal id)
        {
            List<ACUITY_EV> aCUITY_EV = db.ACUITY_EV.Where(X => X.AC_ID == id).ToList();
            foreach(var s in aCUITY_EV)
            { db.ACUITY_EV.Remove(s); }
           
            db.SaveChanges();
            return RedirectToAction("acuitylist");
        }

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
