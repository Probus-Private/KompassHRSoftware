using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Recruitment
{
    public class ESS_Recruitment_CandidateSelectionController : Controller
    {
        // GET: ESS/ESS_Recruitment_CandidateSelection
        public ActionResult ESS_Recruitment_CandidateSelection()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
    }
}