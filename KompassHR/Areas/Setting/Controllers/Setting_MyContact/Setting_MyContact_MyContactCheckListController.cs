using Dapper;
using KompassHR.Areas.Setting.Models.MyContact;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_MyContact
{
    public class Setting_MyContact_MyContactCheckListController : Controller
    {
        // GET: MyContactSetting/MyContactCheckList
        public ActionResult Setting_MyContact_MyContactCheckList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramDropDown = new DynamicParameters();
                paramDropDown.Add("@query", "Select ContactCategoryId AS Id,ContactCategoryName As [Name] from Mas_ContactCategory Where Deactivate=0 order by Name");
                var listMas_ContactCategory = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", paramDropDown).ToList();
                ViewBag.GetContactCategoryName = listMas_ContactCategory;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        [HttpGet]
        public ActionResult GetMyContactList(int ContactCategoryID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ContactCategoryID", ContactCategoryID);
                var data = DapperORM.ReturnList<Mas_MyContact>("sp_List_Mas_MyContactCheckList", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
           
        }
    }
}