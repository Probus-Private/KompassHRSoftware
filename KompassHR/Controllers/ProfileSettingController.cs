using Dapper;
using Google.Authenticator;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Controllers
{
    public class ProfileSettingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ProfileSetting
        #region ProfileSetting
        [HttpGet]
        public ActionResult ProfileSetting_ChangePassword()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                Mas_Employee_ESS Mas_employeeESS = new Mas_Employee_ESS();
                param = new DynamicParameters();
                param.Add("@p_ESSEmployeeId", Session["EmployeeId"]);
                Mas_employeeESS = DapperORM.ReturnList<Mas_Employee_ESS>("sp_List_Mas_Employee_ESS", param).FirstOrDefault();
                if (Mas_employeeESS != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    new DateTime(1, 1, 1); // 01/01/0001 00:00:00

                    if (Mas_employeeESS.ESSLastPasswordChange != DateTime.MinValue)
                    {
                        ViewBag.LastUpdate = Mas_employeeESS.ESSLastPasswordChange;
                    }
                    else
                    {
                        ViewBag.LastUpdate = Mas_employeeESS.CreatedDate;
                    }
                }

                return View(Mas_employeeESS);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region UpdateChangePassword
        public ActionResult UpdateChangePassword(Mas_Employee_ESS ESSChangePassword)
        {
            try
            {
                if (ESSChangePassword.OldPassword.ToLower() == ESSChangePassword.NewPassword.ToLower())
                {
                    TempData["Message"] = "Old password and New password cannot be same .";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ProfileSetting_ChangePassword", "ProfileSetting");
                }
                var ESSPassword = DapperORM.DynamicQueryList("Select Count(*) as Count from Mas_Employee_ESS where Deactivate=0 and ESSEmployeeId =" + Session["EmployeeId"] + " and ESSPassword='" + ESSChangePassword.OldPassword + "' COLLATE SQL_Latin1_General_CP1_CS_AS ");
                if (ESSPassword.Count != 0)
                {
                    var MachineName = Dns.GetHostName().ToString();
                    DapperORM.DynamicQuerySingle("update Mas_Employee_ESS  set ESSPassword='" + ESSChangePassword.NewPassword + "',ESSAnswer='" + ESSChangePassword.ESSAnswer + "',ESSSecurityQuestion='" + ESSChangePassword.ESSSecurityQuestion + "',ModifiedDate=GETDATE(),ModifiedBy='" + Session["EmployeeName"] + "',MachineName='" + Dns.GetHostName().ToString() + "', ESSLastPasswordChange=GETDATE() where ESSId_Encrypted='" + ESSChangePassword.ESSId_Encrypted + "'");
                    TempData["Message"] = "Password changed successfully";
                    TempData["Icon"] = "success";
                    return RedirectToAction("ProfileSetting_ChangePassword", "ProfileSetting");
                }
                else
                {
                    TempData["Message"] = "Old password is incorrect";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ProfileSetting_ChangePassword", "ProfileSetting");
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Authentication

        public ActionResult ProfileSetting_MFA()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return Json(new { success = false, message = "Session expired. Please log in again." });
                }
                string empCode = Session["EmployeeId"]?.ToString();
                Mas_Employee_ESS model = new Mas_Employee_ESS();
                model = DapperORM.ListOfDynamicQueryListWithParam<Mas_Employee_ESS>("SELECT * FROM mas_employee_ess WHERE ESSEmployeeId = " + empCode + " and Deactivate=0").FirstOrDefault();
                ViewBag.Action = model?.MFAToken != null ? "Reset" : "Generate";
                return View(model);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpPost]
        public JsonResult GenerateQRCode()
        {
            if (Session["EmployeeId"] == null)
                return Json(new { success = false, message = "Session expired. Please log in again." });

            string employeeId = Session["EmployeeNo"].ToString();

            var employee = DapperORM.DynamicQuerySingle("SELECT IsMFA, MFAToken, MFAEnabled FROM mas_employee_ess ess join Mas_Employee emp on ess.ESSEmployeeId=emp.EmployeeId WHERE emp.EmployeeNo = '" + employeeId + "' AND ess.Deactivate = 0 and emp.Deactivate=0 and ess.IsExit=0 and ESSIsActive=1");

            bool isMFASet = employee != null && Convert.ToBoolean(employee.IsMFA);
            bool isFirstTime = !isMFASet || string.IsNullOrEmpty(employee?.MFAToken);

            var tfa = new TwoFactorAuthenticator();

            if (isFirstTime || Request["forceReset"] == "true")
            {
                // Generate a random secret
                string originalSecret = Guid.NewGuid().ToString("N").Substring(0, 32);

                // Save original in session
                Session["GeneratedMFAToken"] = originalSecret;
                Session["UserUniqueKey"] = originalSecret;

                // Generate QR
                var setupInfo = tfa.GenerateSetupCode("KompassHR", $"Employee_{employeeId}", Encoding.UTF8.GetBytes(originalSecret));

                // Extract the manual entry key (this is what user should enter manually)
                string manualEntryKey = setupInfo.ManualEntryKey;

                // Get current codes for both secrets to compare
                string originalSecretCode = tfa.GetCurrentPIN(originalSecret);
                string manualKeyCode = tfa.GetCurrentPIN(manualEntryKey);

                // Debug: Check if they match
                bool codesMatch = originalSecretCode == manualKeyCode;

                return Json(new
                {
                    success = true,
                    showQRCode = true,
                    barcodeImageUrl = setupInfo.QrCodeSetupImageUrl,
                    manualEntryKey = manualEntryKey,
                    debugInfo = new
                    {
                        originalSecret = originalSecret,
                        manualEntryKey = manualEntryKey,
                        originalSecretCode = originalSecretCode,
                        manualKeyCode = manualKeyCode,
                        codesMatch = codesMatch,
                        secretLengthOriginal = originalSecret.Length,
                        secretLengthManual = manualEntryKey.Length,
                        time = DateTime.Now.ToString("HH:mm:ss")
                    }
                });
            }
            else
            {
                Session["UserUniqueKey"] = employee.MFAToken;
                return Json(new
                {
                    success = true,
                    showQRCode = false,
                    message = "MFA is already enabled."
                });
            }
        }

        [HttpPost]
        public JsonResult Verify2FA(Mas_Employee_ESS model)
        {
            try
            {
                string empCode = Session["EmployeeId"]?.ToString();

                if (model.MFAEnabled == false)
                {
                    DapperORM.ExecuteQuery($@"
                UPDATE mas_employee_ess 
                SET MFAEnabled = 0, MFAToken = NULL, IsMFA = 0
                WHERE ESSEmployeeId = {empCode} AND Deactivate = 0
            ");
                    Session["GeneratedMFAToken"] = null;
                    return Json(new { success = true, message = "MFA disabled successfully." });
                }

                string secret = Session["GeneratedMFAToken"]?.ToString();
                string enteredCode = model.MFAToken?.Trim();

                if (string.IsNullOrEmpty(secret))
                    return Json(new { success = false, message = "Please generate QR code before submitting." });

                var tfa = new TwoFactorAuthenticator();

                // Get ALL possible valid codes for debugging
                string currentCode = tfa.GetCurrentPIN(secret);
                string[] allValidCodes = tfa.GetCurrentPINs(secret, TimeSpan.FromSeconds(90));

                // Try multiple validation methods
                bool isValidDirect = tfa.ValidateTwoFactorPIN(secret, enteredCode);
                bool isValidWithTolerance = tfa.ValidateTwoFactorPIN(secret, enteredCode, TimeSpan.FromSeconds(30));
                bool isValidExtended = tfa.ValidateTwoFactorPIN(secret, enteredCode, TimeSpan.FromSeconds(60));

                bool isValid = isValidDirect || isValidWithTolerance || isValidExtended;

                if (isValid)
                {
                    //        string enableSql = @"
                    //    UPDATE mas_employee_ess 
                    //    SET MFAEnabled = 1, MFAToken = @MFAToken, IsMFA = 1 
                    //    WHERE ESSEmployeeId = @EmpCode AND Deactivate = 0
                    //";

                    DapperORM.ExecuteQuery($@"
UPDATE mas_employee_ess SET MFAEnabled = 1, MFAToken = '" + secret + "', IsMFA = 1  WHERE ESSEmployeeId = '" + empCode + "' AND Deactivate = 0");


                    Session["GeneratedMFAToken"] = null;
                    return Json(new { success = true, message = "2FA enabled successfully." });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "SECRET MISMATCH DETECTED",
                        debugInfo = new
                        {
                            sessionSecret = secret,
                            enteredCode = enteredCode,
                            currentExpectedCode = currentCode,
                            allValidCodes = allValidCodes,
                            validationResults = new
                            {
                                direct = isValidDirect,
                                with30sTolerance = isValidWithTolerance,
                                with60sTolerance = isValidExtended
                            },
                            serverTime = DateTime.Now.ToString("HH:mm:ss"),
                            timeTicks = DateTime.Now.Ticks
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        #endregion

    }
}
