using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace KompassHR.Areas.ESS.Controllers.ESS_OrganisationChart
{
    public class ESS_OrganisationChart_MenuController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();

        // GET: ESS/ESS_OrganisationChart_Menu
        public ActionResult ESS_OrganisationChart_Menu(int? id, int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //param = new DynamicParameters();
                //param.Add("@query", "SELECT DISTINCT  dbo.Mas_PolicyGroup_Master.GroupName, dbo.Mas_Policy.PolicyName,Mas_PolicyLibrary.PolicyLibraryId, dbo.Mas_PolicyLibrary.Remark, dbo.Mas_PolicyLibrary.PolicyLibraryPolicyId, dbo.Mas_Policy.PolicyName, dbo.Mas_PolicyLibrary.FileType,case when Mas_PolicyLibrary.filetype='UploadFile' then (Select [DocInitialPath] from Tool_Documnet_DirectoryPath  where DocOrigin='PolicyLibrary')+Convert(varchar(50), PolicyLibraryId)+'\'+Mas_PolicyLibrary.DocumentPath else DocumentPath end  AS DocumentPath   FROM dbo.Mas_Employee INNER JOIN dbo.Mas_Employee_Policy ON dbo.Mas_Employee.EmployeeId = dbo.Mas_Employee_Policy.EmployeePolicyEmployeeID INNER JOIN dbo.Mas_PolicyGroup_Master ON dbo.Mas_Employee_Policy.PolicyId = dbo.Mas_PolicyGroup_Master.PolicyGroupMasterId INNER JOIN dbo.Mas_PolicyGroup_Detail ON dbo.Mas_PolicyGroup_Master.PolicyGroupMasterId = dbo.Mas_PolicyGroup_Detail.DetailPolicyGroupMasterId INNER JOIN dbo.Mas_PolicyLibrary ON dbo.Mas_PolicyGroup_Detail.PolicyGroupPolicyLibraryId = dbo.Mas_PolicyLibrary.PolicyLibraryId INNER JOIN dbo.Mas_Policy ON dbo.Mas_PolicyLibrary.PolicyLibraryPolicyId = dbo.Mas_Policy.PolicyId WHERE dbo.Mas_Employee.EmployeeId =" + Session["EmployeeId"] + "");
                //var MasPolicyLibrary = DapperORM.DynamicList("sp_QueryExcution", param);
                //ViewBag.GetEmployeeNamePolicy = MasPolicyLibrary;

                param = new DynamicParameters();
                param.Add("@p_CompanyId", Session["CompanyId"]);
                param.Add("@p_BranchId", Session["BranchId"]);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var Result = DapperORM.ExecuteSP<dynamic>("sp_Organisation_Type", param).ToList();
                ViewBag.GetOrganisationChartList = Result;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult ShowOrganisationChart(int? TypeId, string Name)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param = new DynamicParameters();
                param.Add("@p_TypeId", TypeId);
                param.Add("@p_CompanyId", Session["CompanyId"]);
                param.Add("@p_BranchId", Session["BranchId"]);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var Result = DapperORM.ExecuteSP<dynamic>("sp_Organisation_Chart", param).ToList();
                ViewBag.GetOrganisationChart = Result;
                Session["OrgTypeName"] = Name;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }



        public JsonResult GetOrgChartDetails(int? EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return Json(new { redirect = Url.Action("Login", "Login") }, JsonRequestBehavior.AllowGet);
                }

                param = new DynamicParameters();
                param.Add("@p_EmployeeId", EmployeeId);
                var result = DapperORM.ExecuteSP<dynamic>("sp_OrganisationChart_Details", param).ToList();

                // Process the result to convert the Employee Photo to Base64
                string Getbase64Image = null;
                foreach (var item in result)
                {
                    if (item.Question == "Employee Photo")
                    {
                        string photoPath = item.Answer; // Photo path from DB
                        if (System.IO.File.Exists(photoPath))
                        {
                            byte[] imageBytes = System.IO.File.ReadAllBytes(photoPath);
                            string base64String = Convert.ToBase64String(imageBytes);
                            string base64Image = $"data:image/png;base64,{base64String}"; // Assuming PNG, change if needed
                            Getbase64Image = base64Image;
                        }
                    }
                }

                return Json(new { Data = result, EmployeePhoto = Getbase64Image }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return Json(new { error = ex.Message, redirect = Url.Action("ErrorPage", "Login") }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}