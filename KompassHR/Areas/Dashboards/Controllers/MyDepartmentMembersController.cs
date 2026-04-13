using Dapper;
using KompassHR.Areas.Dashboards.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace KompassHR.Areas.Dashboards.Controllers
{
    public class MyDepartmentMembersController : Controller
    {
        // GET: Dashboards/MyDepartmentMembers
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region MyDepartmentMembers Main View
        [HttpGet]
        public ActionResult MyDepartmentMembers()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters Mydepartment = new DynamicParameters();
                Mydepartment.Add("@query", @"Select  EmployeeNo ,EmployeeName ,Mas_Designation.DesignationName,JoiningDate,PrimaryMobile,CompanyMobileNo,PersonalEmailId,Gender
                                            ,dbo.funGetDateDifference(MAs_employee.JoiningDate, getdate()) as Experience
                                            ,(Select[DocInitialPath] from Tool_Documnet_DirectoryPath  where DocOrigin = 'Onboarding') + Convert(varchar(50), EmployeeId) + '\'+  'Photo' +  '\'+ [PhotoPath]  AS PhotoPath 
                                             from  dbo.Mas_Employee INNER JOIN
                                             dbo.Mas_Designation ON dbo.Mas_Employee.EmployeeDesignationID = dbo.Mas_Designation.DesignationId LEFT OUTER JOIN
                                             dbo.Mas_Employee_Personal ON dbo.Mas_Employee.EmployeeId = dbo.Mas_Employee_Personal.PersonalEmployeeId LEFT OUTER JOIN
                                             dbo.Mas_Employee_Photo ON dbo.Mas_Employee.EmployeeId = dbo.Mas_Employee_Photo.PhotoEmployeeId
                                             where Mas_employee.Deactivate = 0 and Mas_employee.EmployeeLeft = 0 and Mas_employee.ContractorID=1  
                                             and EmployeeDepartmentid = (select e.Employeedepartmentid from mas_employee e where e.employeeid = " + Session["EmployeeId"] + ") ");
                var GetMyDepartmentMembers = DapperORM.ExecuteSP<ESSDashboard>("sp_QueryExcution", Mydepartment).ToList(); // SP_getReportingManager
                ViewBag.MyDepartmentMembers = GetMyDepartmentMembers;

                //GetPhotoBase64Convert();
                //ViewBag.GetImages = ViewBag.GetData;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Get PhotoBase64Convert
        [HttpGet]
        public ActionResult GetPhotoBase64Convert()
        {
            try
            {
                DynamicParameters Mydepartment = new DynamicParameters();
                Mydepartment.Add("@query", @"Select  EmployeeNo ,EmployeeName ,Mas_Designation.DesignationName,JoiningDate,PrimaryMobile,CompanyMobileNo,PersonalEmailId,Gender
                                            ,dbo.funGetDateDifference(MAs_employee.JoiningDate, getdate()) as Experience
                                            ,(Select[DocInitialPath] from Tool_Documnet_DirectoryPath  where DocOrigin = 'Onboarding') + Convert(varchar(50), EmployeeId) + '\'+ 'Photo'+ '\'+[PhotoPath]  AS PhotoPath 
                                             ,Mas_Employee_Photo.PhotoPath as PhotoName
                                            from  dbo.Mas_Employee INNER JOIN
                                             dbo.Mas_Designation ON dbo.Mas_Employee.EmployeeDesignationID = dbo.Mas_Designation.DesignationId LEFT OUTER JOIN
                                             dbo.Mas_Employee_Personal ON dbo.Mas_Employee.EmployeeId = dbo.Mas_Employee_Personal.PersonalEmployeeId LEFT OUTER JOIN
                                             dbo.Mas_Employee_Photo ON dbo.Mas_Employee.EmployeeId = dbo.Mas_Employee_Photo.PhotoEmployeeId
                                             where Mas_employee.Deactivate = 0 and Mas_employee.EmployeeLeft = 0
                                             and EmployeeDepartmentid = (select e.Employeedepartmentid from mas_employee e where e.employeeid = " + Session["EmployeeId"] + ") ");
                var GetMyDepartmentMembers = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", Mydepartment).ToList(); // SP_getReportingManager
                ViewBag.MyDepartmentMembers = GetMyDepartmentMembers;


                var list = new List<string>();
                for (int i = 0; i < GetMyDepartmentMembers.Count; i++)
                {
                    var PhotoPath = "";
                    string fullPath = GetMyDepartmentMembers[i].PhotoPath;
                    string PhotoName = GetMyDepartmentMembers[i].PhotoName;
                    //if (fullPath == null)
                    //{
                    //    fullPath = Server.MapPath("/assets/img/Allimages/Male.png");
                    //}
                    if (fullPath != null)
                    {

                        if (!Directory.Exists(fullPath))
                        {

                            if (PhotoName != null)
                            {
                                using (Image image = Image.FromFile(fullPath))
                                {
                                    using (MemoryStream m = new MemoryStream())
                                    {
                                        image.Save(m, image.RawFormat);
                                        byte[] imageBytes = m.ToArray();

                                        // Convert byte[] to Base64 String
                                        string base64String = Convert.ToBase64String(imageBytes);
                                        PhotoPath = "data:image; base64," + base64String;
                                        list.Add(PhotoPath);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        list.Add("");
                    }

                }
                return Json(new { list = list }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
    }
}