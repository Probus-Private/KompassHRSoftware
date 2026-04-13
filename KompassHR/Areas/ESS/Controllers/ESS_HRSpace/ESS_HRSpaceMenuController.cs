using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_HRSpace
{
    public class ESS_HRSpaceMenuController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_HRSpaceMenu
        #region MainView
        public ActionResult ESS_HRSpaceMenu(int? id, int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (ScreenId != null)
                {
                    Session["ModuleId"] = id;
                    Session["ScreenId"] = ScreenId;
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), id, ScreenId, "Form", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                else
                {
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(Session["ModuleId"]), Convert.ToInt32(Session["ScreenId"]), "Form", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        public ActionResult EmployeeHRSearch(string SearchText)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select top(1) PlantHREmployeeID from Mas_PlantHR where PlantHREmployeeID=" + Session["EmployeeId"] + " and Deactivate=0");
                var PlantHREmployeeID = DapperORM.ExecuteSP<EmployeeGlobalSearch>("sp_QueryExcution", param).ToList();
                if (PlantHREmployeeID.Count != 0)
                {
                    DynamicParameters paramy = new DynamicParameters();
                    paramy.Add("@query", "Select top(500) * from View_Global_EmployeeSearch where EmployeeNo like '%" + SearchText + "%' or EmployeeCardNo like '%" + SearchText + "%' or EmployeeName like '%" + SearchText + "%' or PrimaryMobileNo like '%" + SearchText + "%' or AadharNo like '%" + SearchText + "%' or PanNo like '%" + SearchText + "%'");
                    var GetEmployeeGlobalSearch = DapperORM.ExecuteSP<EmployeeGlobalSearch>("sp_QueryExcution", paramy).ToList();
                    return Json(GetEmployeeGlobalSearch, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters paramy = new DynamicParameters();
                    paramy.Add("@query", "Select top(500) * from View_Global_EmployeeSearch where EmployeeNo like '%" + SearchText + "%' or EmployeeCardNo like '%" + SearchText + "%' or EmployeeName like '%" + SearchText + "%' or PrimaryMobileNo like '%" + SearchText + "%' or AadharNo like '%" + SearchText + "%' or PanNo like '%" + SearchText + "%'");
                    var GetEmployeeGlobalSearch = DapperORM.ExecuteSP<EmployeeGlobalSearch>("sp_QueryExcution", paramy).ToList();
                    return Json(GetEmployeeGlobalSearch, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Redirect to custom error page with error message
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        public ActionResult showEmployeeAllDetails(int EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
              
                DynamicParameters MulQuery = new DynamicParameters();
                MulQuery.Add("@p_EmployeeId", EmployeeId);            
                using (var multi = DapperORM.DynamicMultipleResultList("sp_HRSpace_Details", MulQuery))
                {
                    ViewBag.EmployeeAllDetails = multi.Read<dynamic>().ToList();
                    ViewBag.EmployeePersonalDetails = multi.Read<dynamic>().FirstOrDefault();
                }
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