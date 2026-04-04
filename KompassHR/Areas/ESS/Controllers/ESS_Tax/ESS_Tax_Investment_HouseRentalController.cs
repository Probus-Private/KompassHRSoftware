using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Tax;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class ESS_Tax_Investment_HouseRentalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_Tax_Investment_HouseRental
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        #region ESS_Tax_Investment_HouseRental Main View
        public ActionResult ESS_Tax_Investment_HouseRental()
        {
            try
            {

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 177;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
                var GetEmpoyee = "Select * from View_IncomeTax_Employee Where EmployeeID = " + Session["EmployeeId"] + "";
                var IncomeTaxEmployee = DapperORM.ExecuteQuery(GetEmpoyee);
                if (IncomeTaxEmployee != null)
                {
                    ViewBag.GetHouseRentalEmployee = IncomeTaxEmployee;
                }
                else
                {
                    ViewBag.GetHouseRentalEmployee = "";
                }
                var Date = DapperORM.DynamicQueryList($@"Select  HRA_SubmitDate, HRA_SubmitCount from IncomeTax_InvestmentDeclaration where Deactivate = 0  and InvestmentDeclarationFyearId = {Session["TaxFyearId"]} and InvestmentDeclarationEmployeeId = {Session["EmployeeId"]}").FirstOrDefault();
                TempData["HRA_SubmitDate"] = null;
                TempData["HRA_SubmitCount"] = null;
                if (Date != null)
                {
                    TempData["HRA_SubmitDate"] = Date.HRA_SubmitDate;
                    TempData["HRA_SubmitCount"] = Date.HRA_SubmitCount;
                }

                var PanLimit = DapperORM.DynamicQueryList("Select HRA_PanLimit From IncomeTax_Rule where  Deactivate=0 and IncomeTaxRule_FyearId= " + Session["TaxFyearId"] + "").FirstOrDefault();
                if (PanLimit != null)
                {
                    TempData["HRA_PanLimit"] = PanLimit.HRA_PanLimit;
                }
                //Check Opening Closing Date Submit is valid or not 
                var CheckSubmit = DapperORM.ExecuteQuery(@"Select top 1 [Month],[FromDay],[ToDay] from[IncomeTax_DocumentOpenClose] where Deactivate=0 and  [Month]=Month(Getdate()) and OpenCloseTypeID =1 and day(Getdate()) Between [FromDay] and ([ToDay])");
                TempData["SubmitValid"] = CheckSubmit;

                InvestmentDeclaration_HRA HRA_InvestmentDeclaration = new InvestmentDeclaration_HRA();
                param = new DynamicParameters();
                param.Add("@p_InvestmentDeclarationFyearId", Session["TaxFyearId"]);
                param.Add("@p_InvestmentDeclarationEmployeeId", Session["EmployeeId"]);
                param.Add("@p_EmployeeNo", Session["EmployeeNo"]);
                HRA_InvestmentDeclaration = DapperORM.ReturnList<InvestmentDeclaration_HRA>("sp_List_IncomeTax_InvestmentDeclaration_HRA", param).FirstOrDefault();
                if (HRA_InvestmentDeclaration != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                }
                return View(HRA_InvestmentDeclaration);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region MetroNonMetroCity
        [HttpPost]
        public ActionResult CheckMetroNonMetro(string CityName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_CityName", CityName);
                param.Add("@query", "Select MetroCity from IncomeTax_MetroCity where Deactivate=0 and MetroCity='" + CityName + "'");
                var City = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).SingleOrDefault();

                return Json(City, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion MetroNonMetroCity

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(InvestmentDeclaration_HRA HRA, HttpPostedFileBase AttachFile)
        {
            try
            {
                //if (HRA.InvestmentDeclaration_Encrypted == null && HRA.InvestmentDeclarationFyearId == 0)
                //{
                //    param.Add("@p_process", "Save");

                //}
                //else
                //{
                //    param.Add("@p_process", "Update");

                //}
                param.Add("@p_process", string.IsNullOrEmpty(HRA.InvestmentDeclaration_Encrypted) ? "Save" : "Update");
                param.Add("@p_InvestmentDeclarationFyearId", Session["TaxFyearId"]);
                param.Add("@P_InvestmentDeclarationEmployeeId", Session["EmployeeId"]);
                //param.Add("@P_InvestmentDeclarationEmployeeNo", 1);
                //param.Add("@P_InvestmentDeclarationEmployeeName", "Dhirendra");
                //--------------------Metro-------------------------------------------
                param.Add("@P_HRA_Apr_Metro", HRA.HRA_Apr_Metro);
                param.Add("@P_HRA_May_Metro", HRA.HRA_May_Metro);
                param.Add("@P_HRA_Jun_Metro", HRA.HRA_Jun_Metro);
                param.Add("@P_HRA_Jul_Metro", HRA.HRA_Jul_Metro);
                param.Add("@P_HRA_Aug_Metro", HRA.HRA_Aug_Metro);
                param.Add("@P_HRA_Sep_Metro", HRA.HRA_Sep_Metro);
                param.Add("@P_HRA_Oct_Metro", HRA.HRA_Oct_Metro);
                param.Add("@P_HRA_Nov_Metro", HRA.HRA_Nov_Metro);
                param.Add("@P_HRA_Dec_Metro", HRA.HRA_Dec_Metro);
                param.Add("@P_HRA_Jan_Metro", HRA.HRA_Jan_Metro);
                param.Add("@P_HRA_Feb_Metro", HRA.HRA_Feb_Metro);
                param.Add("@P_HRA_Mar_Metro", HRA.HRA_Mar_Metro);
                param.Add("@P_HRA_Jan_Metro", HRA.HRA_Jan_Metro);
                //--------------------City Name-------------------------------------------

                param.Add("@P_HRA_Apr_CityName", HRA.HRA_Apr_CityName);
                param.Add("@P_HRA_May_CityName", HRA.HRA_May_CityName);
                param.Add("@P_HRA_Jun_CityName", HRA.HRA_Jun_CityName);
                param.Add("@P_HRA_Jul_CityName", HRA.HRA_Jul_CityName);
                param.Add("@P_HRA_Aug_CityName", HRA.HRA_Aug_CityName);
                param.Add("@P_HRA_Sep_CityName", HRA.HRA_Sep_CityName);
                param.Add("@P_HRA_Oct_CityName", HRA.HRA_Oct_CityName);
                param.Add("@P_HRA_Nov_CityName", HRA.HRA_Nov_CityName);
                param.Add("@P_HRA_Dec_CityName", HRA.HRA_Dec_CityName);
                param.Add("@P_HRA_Jan_CityName", HRA.HRA_Jan_CityName);
                param.Add("@P_HRA_Feb_CityName", HRA.HRA_Feb_CityName);
                param.Add("@P_HRA_Mar_CityName", HRA.HRA_Mar_CityName);

                //--------------------Rent Amount-------------------------------------------
                param.Add("@P_HRA_Apr_CurrentAmt", HRA.HRA_Apr_CurrentAmt);
                param.Add("@P_HRA_May_CurrentAmt", HRA.HRA_May_CurrentAmt);
                param.Add("@P_HRA_Jun_CurrentAmt", HRA.HRA_Jun_CurrentAmt);
                param.Add("@P_HRA_Jul_CurrentAmt", HRA.HRA_Jul_CurrentAmt);
                param.Add("@P_HRA_Aug_CurrentAmt", HRA.HRA_Aug_CurrentAmt);
                param.Add("@P_HRA_Sep_CurrentAmt", HRA.HRA_Sep_CurrentAmt);
                param.Add("@P_HRA_Oct_CurrentAmt", HRA.HRA_Oct_CurrentAmt);
                param.Add("@P_HRA_Nov_CurrentAmt", HRA.HRA_Nov_CurrentAmt);
                param.Add("@P_HRA_Dec_CurrentAmt", HRA.HRA_Dec_CurrentAmt);
                param.Add("@P_HRA_Jan_CurrentAmt", HRA.HRA_Jan_CurrentAmt);
                param.Add("@P_HRA_Feb_CurrentAmt", HRA.HRA_Feb_CurrentAmt);
                param.Add("@P_HRA_Mar_CurrentAmt", HRA.HRA_Mar_CurrentAmt);

                //--------------------LandLord Name-------------------------------------------
                param.Add("@P_HRA_Apr_LandlordName", HRA.HRA_Apr_LandlordName);
                param.Add("@P_HRA_May_LandlordName", HRA.HRA_May_LandlordName);
                param.Add("@P_HRA_Jun_LandlordName", HRA.HRA_Jun_LandlordName);
                param.Add("@P_HRA_Jul_LandlordName", HRA.HRA_Jul_LandlordName);
                param.Add("@P_HRA_Aug_LandlordName", HRA.HRA_Aug_LandlordName);
                param.Add("@P_HRA_Sep_LandlordName", HRA.HRA_Sep_LandlordName);
                param.Add("@P_HRA_Oct_LandlordName", HRA.HRA_Oct_LandlordName);
                param.Add("@P_HRA_Nov_LandlordName", HRA.HRA_Nov_LandlordName);
                param.Add("@P_HRA_Dec_LandlordName", HRA.HRA_Dec_LandlordName);
                param.Add("@P_HRA_Jan_LandlordName", HRA.HRA_Jan_LandlordName);
                param.Add("@P_HRA_Feb_LandlordName", HRA.HRA_Feb_LandlordName);
                param.Add("@P_HRA_Mar_LandlordName", HRA.HRA_Mar_LandlordName);

                //--------------------LandLord PAN-------------------------------------
                param.Add("@P_HRA_Apr_PanNo", HRA.HRA_Apr_PanNo);
                param.Add("@P_HRA_May_PanNo", HRA.HRA_May_PanNo);
                param.Add("@P_HRA_Jun_PanNo", HRA.HRA_Jun_PanNo);
                param.Add("@P_HRA_Jul_PanNo", HRA.HRA_Jul_PanNo);
                param.Add("@P_HRA_Aug_PanNo", HRA.HRA_Aug_PanNo);
                param.Add("@P_HRA_Sep_PanNo", HRA.HRA_Sep_PanNo);
                param.Add("@P_HRA_Oct_PanNo", HRA.HRA_Oct_PanNo);
                param.Add("@P_HRA_Nov_PanNo", HRA.HRA_Nov_PanNo);
                param.Add("@P_HRA_Dec_PanNo", HRA.HRA_Dec_PanNo);
                param.Add("@P_HRA_Jan_PanNo", HRA.HRA_Jan_PanNo);
                param.Add("@P_HRA_Feb_PanNo", HRA.HRA_Feb_PanNo);
                param.Add("@P_HRA_Mar_PanNo", HRA.HRA_Mar_PanNo);

                //--------------------Address-------------------------------------------
                param.Add("@P_HRA_Apr_Address", HRA.HRA_Apr_Address);
                param.Add("@P_HRA_May_Address", HRA.HRA_May_Address);
                param.Add("@P_HRA_Jun_Address", HRA.HRA_Jun_Address);
                param.Add("@P_HRA_Jul_Address", HRA.HRA_Jul_Address);
                param.Add("@p_HRA_Aug_Address", HRA.HRA_Aug_Address);
                param.Add("@P_HRA_Sep_Address", HRA.HRA_Sep_Address);
                param.Add("@P_HRA_Oct_Address", HRA.HRA_Oct_Address);
                param.Add("@P_HRA_Nov_Address", HRA.HRA_Nov_Address);
                param.Add("@P_HRA_Dec_Address", HRA.HRA_Dec_Address);
                param.Add("@P_HRA_Jan_Address", HRA.HRA_Jan_Address);
                param.Add("@P_HRA_Feb_Address", HRA.HRA_Feb_Address);
                param.Add("@P_HRA_Mar_Address", HRA.HRA_Mar_Address);

                param.Add("@p_HRA_Total", HRA.HRA_Total);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_InvestmentDeclaration_HRA", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;


                if (Icon == "success")
                {
                    if (AttachFile != null)
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET HRA_ProofUpload = '" + Session["EmployeeNo"] + " - " + "HRA" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

                        var GetDocPath = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'IncomeTax_Investment'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = Path.Combine(GetFirstPath, Session["EmployeeNo"].ToString()) + "\\";

                        if (!Directory.Exists(FirstPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }
                        else
                        {
                            // **Delete only the file matching "{EmployeeId} - HRA.*" pattern**
                            try
                            {
                                string searchPattern = Session["EmployeeNo"] + " - HRA.*"; // Pattern to match (any extension)
                                string[] files = Directory.GetFiles(FirstPath, searchPattern); // Get matching files

                                foreach (string file in files)
                                {
                                    if (System.IO.File.Exists(file))
                                    {
                                        System.IO.File.SetAttributes(file, FileAttributes.Normal);
                                        System.IO.File.Delete(file);
                                    }
                                }
                            }
                            catch (IOException ioEx)
                            {
                                TempData["Message"] = "File deletion error: " + ioEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_HouseRental", "ESS_Tax_Investment_HouseRental");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_HouseRental", "ESS_Tax_Investment_HouseRental");
                            }
                        }

                        // Extract file extension
                        string fileExtension = Path.GetExtension(AttachFile.FileName);
                        string newFileName = Session["EmployeeNo"] + " - " + "HRA" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);
                    }
                }

                return RedirectToAction("ESS_Tax_Investment_HouseRental", "ESS_Tax_Investment_HouseRental");
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