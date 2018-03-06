﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Coursework.Models;
using PagedList;

namespace Coursework.Controllers
{
    public class HomeController : Controller
    {
        // import database connection
        private CauseDBContext db = new CauseDBContext();

        public ActionResult Index()
        {
            // pass causes object to view
            return View(db.Causes.ToList());
        }

        //public ActionResult About()
        //{
        //    ViewBag.Message = "Your application description page.";

        //    return View();
        //}

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}

        // Password hashing from Stack Overflow, Kevin Nelson, 16/08/17, https://stackoverflow.com/questions/45723140/how-to-salt-and-compare-password-in-asp-net-mvc

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Login loginUser)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (CauseDBContext db = new CauseDBContext())
            {
                var matchedUsers = db.Members.FirstOrDefault(a => a.Email.Equals(loginUser.Email));
                if (matchedUsers != null && Crypto.VerifyHashedPassword(matchedUsers.Password, loginUser.Password))
                {
                    Session["UserID"] = matchedUsers.ID.ToString();
                    Session["UserName"] = matchedUsers.Name.ToString();
                    Session["Role"] = matchedUsers.Role.ToString();
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
            }
        }

        // GET: Destroy active session if any
        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
            Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-10);
            return RedirectToAction("Index");
        }

        // GET: Return a paged list of all cause objects
        // Paging courtesy ITworld, Matthew Mombrea, 05/08/15, https://www.itworld.com/article/2956575/development/how-to-sort-search-and-paginate-tables-in-asp-net-mvc-5.html
        public ActionResult Admin(int page = 1, int pageSize = 25)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "Admin")
            {
                page = page > 0 ? page : 1;
                pageSize = pageSize > 0 ? pageSize : 25;

                var causes = db.Causes.OrderBy(a => a.ID);
                return View(causes.ToPagedList(page, pageSize));
            }
            return RedirectToAction("Index");
        }
    }
}