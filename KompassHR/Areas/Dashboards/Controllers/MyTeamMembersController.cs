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
    public class MyTeamMembersController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Dashboards/MyTeamMembers
        #region MyTeamMembers Main View
        [HttpGet]
        public ActionResult MyTeamMembers()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var EmployeeId = Session["EmployeeId"];
                //Here pass the Session Id I Just pass HaedCoded Value for the Data 
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.ExecuteSP<ESSDashboard>("sp_ESSMyTeam", param).ToList();                
                ViewBag.GetMyTeamList = data;                   
             
            return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        //#region Get PhotoBase64Convert
        //[HttpGet]
        //public ActionResult GetPhotoBase64Convert()
        //{
        //    try
        //    {
        //        var EmployeeId = Session["EmployeeId"];
        //        //Here pass the Session Id I Just pass HaedCoded Value for the Data 
        //        param.Add("@p_EmployeeId", Session["EmployeeId"]);
        //        var data = DapperORM.ExecuteSP<dynamic>("sp_ESSMyTeam", param).ToList();
        //        ViewBag.GetMyTeamList = data;


        //        var list = new List<string>();                
        //        for (int i = 0; i < data.Count; i++)
        //        {
        //            var PhotoPath = "";
        //            string fullPath = data[i].PhotoPath;
        //            string PhotoName = data[i].PhotoName;
        //            if (fullPath != null)
        //            {
        //                if (!Directory.Exists(fullPath))
        //                {
        //                    if(PhotoName !=null)
        //                    {
        //                        using (Image image = Image.FromFile(fullPath))
        //                        {
        //                            using (MemoryStream m = new MemoryStream())
        //                            {
        //                                image.Save(m, image.RawFormat);
        //                                byte[] imageBytes = m.ToArray();

        //                                // Convert byte[] to Base64 String
        //                                string base64String = Convert.ToBase64String(imageBytes);
        //                                PhotoPath = "data:image; base64," + base64String;
        //                                list.Add(PhotoPath);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                list.Add("");
        //            }
        //        }
        //        return Json(new { list = list}, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}
        //#endregion
    }
}