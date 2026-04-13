using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_ReportingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Module/Module_Employee_Reporting
        #region Reporting Main View
        [HttpGet]
        public ActionResult Module_Employee_Reporting(string ReportingID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 415;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Mas_Employee_Reporting EmployeeReporting = new Mas_Employee_Reporting();

                //select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where  EmployeeName is not null and Deactivate=0 and CmpID= " + MasReport.CmpId + " and EmployeeBranchId= " + MasReport.BranchId + "and Mas_Employee.EmployeeLeft=0
                //param.Add("@query", "Select EmployeeId As Id,EmployeeName As Name From Mas_Employee where EmployeeName is not null and  Mas_Employee.Deactivate=0 and CmpId=" + Session["OnboardCmpId"] + " and EmployeeBranchId=" + Session["OnboardBranchId"] + " and EmployeeLeft=0 order by EmployeeName");
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Mas_Employee.Deactivate=0 and EmployeeLeft=0 and Mas_Employee.ContractorID=1 order by Name");
                var ManagerList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.ManagerLists = ManagerList;

                param.Add("@query", "Select Id As Id,ModuleName As Name from Tool_Module where Deactivate=0 order by ModuleName");
                var Module = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.ModuleList = Module;

                param.Add("@query", "select ApprovalLevelId AS Id,ApprovalLevelName As Name from Tool_ApprovalLevel where Deactivate=0 order by Name");
                var ApprovalList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetApprovalLevel = ApprovalList;


                DynamicParameters paramReport = new DynamicParameters();
                paramReport.Add("@p_ReportingID_Encrypted", "List");
                paramReport.Add("@p_ReportingEmployeeID", Session["OnboardEmployeeId"]);
                var dataReport = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_Employee_Reporting", paramReport).ToList();
                ViewBag.ReportingList = dataReport;


                ViewBag.SetReportintId = DapperORM.DynamicQueryList(@"Select ReportingModuleID,ReportingManager1,ReportingManager2,ReportingHR from Mas_Employee_Reporting where ReportingEmployeeID=" + Session["OnboardEmployeeId"] + " and Deactivate=0").ToList();

                if (ReportingID_Encrypted != null)
                {
                    DynamicParameters paramUpdate = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    paramUpdate.Add("@p_ReportingID_Encrypted", ReportingID_Encrypted);
                    paramUpdate.Add("@p_ReportingEmployeeID", Session["OnboardEmployeeId"]);
                    EmployeeReporting = DapperORM.ReturnList<Mas_Employee_Reporting>("sp_List_Mas_Employee_Reporting", paramUpdate).FirstOrDefault();
                    return View(EmployeeReporting);
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

        #region IsReportingExists
        [HttpPost]
        public ActionResult IsReportingExists(string[] ReportingModuleID, string ReportingID_Encrypted, int ReportingManager1, int? ApproverLevel)
        {
            try
            {
                //var message = "";
                //var Icon = "";
                for (var i = 0; i < ReportingModuleID.Count(); i++)
                {
                    int ReportingModule = Convert.ToInt32(ReportingModuleID[i]);
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_ReportingModuleID", ReportingModule);
                    // param.Add("@p_ReportingID_Encrypted", ReportingID_Encrypted);
                    param.Add("@p_ReportingManager1", ReportingManager1);
                    param.Add("@p_ReportingEmployeeID", Session["OnboardEmployeeId"]);
                    param.Add("@p_ApproverLevel", ApproverLevel);
                    //param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    //  param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Reporting", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    //var Message = param.Get<string>("@p_msg");
                    // var Icon = param.Get<string>("@p_Icon");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    if (TempData["Icon"] == "error")
                    {
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                }
                //var Message = param.Get<string>("@p_msg");
                //var Icon = param.Get<string>("@p_Icon");

                if (TempData["Message"] != "")
                {

                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        public static DataTable ConvertToDataTable<T>(IEnumerable<T> data)
        {
            DataTable table = new DataTable();
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                table.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyInfo property in properties)
                {
                    row[property.Name] = property.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }
            return table;
        }

        #region SaveUpdate 
        [HttpPost]
        public ActionResult SaveUpdate(List<RequestApproval> RequestApproval)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                string connectionString = Session["MyNewConnectionString"].ToString();
                string onboardEmpId = Session["OnboardEmployeeId"].ToString();
                string empName = Session["EmployeeName"].ToString();
                string machineName = Dns.GetHostName();

                var message = "";
                var Icon = "";

                using (var sqlconNew = new SqlConnection(connectionString))
                {
                    sqlconNew.Open();

                    // 1. Delete old entries for this employee
                    sqlconNew.Execute(
                        "DELETE FROM Mas_Employee_Reporting WHERE ReportingEmployeeID = @EmpId",
                        new { EmpId = onboardEmpId });

                    // 2. Convert list to DataTable
                    DataTable dt = ConvertToDataTable(RequestApproval);

                    // 3. Insert records securely using parameters
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        for (int j = 1; j < dt.Columns.Count; j++)
                        {
                            if (dt.Rows[i][j].ToString() == "0")
                                continue;

                            sqlconNew.Execute(@" INSERT INTO Mas_Employee_Reporting
                                (ReportingModuleID, ReportingEmployeeID, ReportingManager1, ApproverLevel, CreatedDate, CreatedBy, MachineName, IsCompulsory, Deactivate)
                                VALUES (@ModuleId, @EmpId, @ManagerId, @Level, GETDATE(), @CreatedBy, @MachineName, 1, 0)",
                                new
                                {
                                    ModuleId = dt.Rows[i][0],
                                    EmpId = onboardEmpId,
                                    ManagerId = dt.Rows[i][j],
                                    Level = j,
                                    CreatedBy = empName,
                                    MachineName = machineName
                                });
                        }
                    }

                    // 4. Check if Setup already exists and update or insert
                    var exists = sqlconNew.QueryFirstOrDefault<int>(
                        @"SELECT COUNT(1) FROM Mas_Employee_Setup WHERE SetupEmployeeId = @EmpId AND Deactivate = 0",
                        new { EmpId = onboardEmpId });

                    if (exists > 0)
                    {
                        sqlconNew.Execute(@"UPDATE Mas_Employee_Setup SET ModifiedBy = @ModifiedBy, ModifiedDate = GETDATE(), 
                        MachineName = @MachineName, SetupReporting = 1 WHERE SetupEmployeeId = @EmpId",
                            new
                            {
                                ModifiedBy = empName,
                                MachineName = machineName,
                                EmpId = onboardEmpId
                            });
                    }
                    else
                    {
                        sqlconNew.Execute(@"INSERT INTO Mas_Employee_Setup (Deactivate, CreatedBy, CreatedDate, MachineName, SetupEmployeeId, SetupReporting)
                            VALUES (0, @CreatedBy, GETDATE(), @MachineName, @EmpId, 1)",
                            new
                            {
                                CreatedBy = empName,
                                MachineName = machineName,
                                EmpId = onboardEmpId
                            });
                    }

                    // 5. Update TransferDocId if present
                    if (Session["TransferDocId"] != null)
                    {
                        sqlconNew.Execute(@"UPDATE Trans_BusinessUnit SET TransferStatus = 'Approved', ApprovedDate = GETDATE() 
                            WHERE TransferBusinessUnitId = @TransferId",
                            new { TransferId = Session["TransferDocId"] });
                    }

                    message = "Record saved successfully.";
                    Icon = "success";

                    return Json(new { message, Icon }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        //#region SaveUpdate
        //[HttpPost]
        //public ActionResult SaveUpdate(List<RequestApproval> RequestApproval)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { Area = "" });
        //        }

        //        var message = "";
        //        var Icon = "";
        //        string connectionString = Session["MyNewConnectionString"].ToString();

        //        using (var sqlconNew = new SqlConnection(connectionString))
        //        {
        //            sqlconNew.Open();

        //            // 1. Delete old entries for this employee
        //            string deleteQuery = $"DELETE FROM Mas_Employee_Reporting WHERE ReportingEmployeeID = {Session["OnboardEmployeeId"]}";
        //            sqlconNew.Execute(deleteQuery);

        //            // 2. Convert request to DataTable
        //            DataTable dt = ConvertToDataTable(RequestApproval);
        //            StringBuilder strBuilder = new StringBuilder();

        //            string onboardEmpId = Session["OnboardEmployeeId"].ToString();
        //            string empName = Session["EmployeeName"].ToString();
        //            string machineName = Dns.GetHostName();

        //            // 3. Loop through rows and columns to build insert statements
        //            for (int i = 0; i < dt.Rows.Count; i++)
        //            {
        //                for (int j = 1; j < dt.Columns.Count; j++)
        //                {
        //                    if (dt.Rows[i][j].ToString() == "0")
        //                        continue;

        //                    strBuilder.Append($@"
        //                INSERT INTO Mas_Employee_Reporting
        //                (ReportingModuleID, ReportingEmployeeID, ReportingManager1, ApproverLevel, CreatedDate, CreatedBy, MachineName, IsCompulsory, Deactivate)
        //                VALUES ('{dt.Rows[i][0]}', {onboardEmpId}, {dt.Rows[i][j]}, {j}, GETDATE(), '{empName}', '{machineName}', '1', '0');
        //            ");
        //                }
        //            }

        //            // 4. Check if Mas_Employee_Setup already exists
        //            var checkSetup = sqlconNew.Query(@"SELECT SetupReporting AS GetCount FROM Mas_Employee_Setup WHERE SetupEmployeeId = @EmpId AND Deactivate = 0",
        //                                             new { EmpId = onboardEmpId });

        //            if (checkSetup.Any())
        //            {
        //                strBuilder.Append($@"
        //            UPDATE Mas_Employee_Setup
        //            SET ModifiedBy = '{empName}', ModifiedDate = GETDATE(), MachineName = '{machineName}', SetupReporting = '1'
        //            WHERE SetupEmployeeId = '{onboardEmpId}';
        //        ");
        //            }
        //            else
        //            {
        //                strBuilder.Append($@"
        //            INSERT INTO Mas_Employee_Setup (Deactivate, CreatedBy, CreatedDate, MachineName, SetupEmployeeId, SetupReporting)
        //            VALUES ('0', '{empName}', GETDATE(), '{machineName}', '{onboardEmpId}', '1');
        //        ");
        //            }

        //            // 5. Execute the complete batch using your custom method
        //            string errorMsg = "";
        //            bool isSaved = objcon.SaveStringBuilder(strBuilder, out errorMsg);

        //            if (isSaved)
        //            {
        //                message = "Record saved successfully.";
        //                Icon = "success";
        //            }

        //            // 6. Optional TransferDocId update
        //            if (Session["TransferDocId"] != null)
        //            {
        //                sqlconNew.Execute("UPDATE Trans_BusinessUnit SET TransferStatus = 'Approved', ApprovedDate = GETDATE() WHERE TransferBusinessUnitId = @TransferId",
        //                                  new { TransferId = Session["TransferDocId"] });
        //            }

        //            var result = new { message, Icon };
        //            return Json(result, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}

        //#endregion

        //#region SaveUpdate New
        //[HttpPost]
        //public ActionResult SaveUpdate(List<RequestApproval> RequestApproval)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { Area = "" });
        //        }
        //        var message = "";
        //        var Icon = "";
        //        var connectionString = Session["MyNewConnectionString"].ToString();
        //        //SqlConnection sqlconNew = new SqlConnection(connectionString);
        //        using (var sqlconNew = new SqlConnection(connectionString))
        //        {
        //            sqlconNew.Open();
        //            var deleteQuery = "DELETE FROM Mas_Employee_Reporting WHERE ReportingEmployeeID = " + Session["OnboardEmployeeId"] + "";
        //            sqlconNew.Execute(deleteQuery);

        //            DataTable dt = ConvertToDataTable(RequestApproval);
        //            StringBuilder strBuilder = new StringBuilder();
        //            string abc = "";
        //            for (int i = 0; i < dt.Rows.Count; i++)
        //            {
        //                for (int j = 1; j < dt.Columns.Count; j++)
        //                {
        //                    if (dt.Rows[i][j].ToString() == "0")
        //                    {
        //                        //break;
        //                        continue;
        //                    }
        //                    abc = @"Insert Into Mas_Employee_Reporting (ReportingModuleID,ReportingEmployeeID,ReportingManager1,ApproverLevel,CreatedDate,CreatedBy,MachineName,IsCompulsory,Deactivate)
        //                                            values ('" + dt.Rows[i][0].ToString() + "'," + Session["OnboardEmployeeId"] + "," + dt.Rows[i][j].ToString() + "," + (j) + ",GetDate(),'" + Session["EmployeeName"] + "','" + Dns.GetHostName().ToString() + "','1','0') ";
        //                    strBuilder.Append(abc);
        //                }
        //            }
        //            var CheckEmployeeSetup = sqlconNew.Query(@"Select SetupReporting as GetCount from Mas_Employee_Setup where SetupEmployeeId='" + Session["OnboardEmployeeId"] + "' and deactivate=0");
        //            if (CheckEmployeeSetup.Count() != 0)
        //            {
        //                string UpdateEmployeeSetup = @"Update Mas_Employee_Setup Set ModifiedBy = '" + Session["EmployeeName"] + "',  ModifiedDate = Getdate(),    MachineName = '" + Dns.GetHostName().ToString() + "', SetupReporting = '1' Where SetupEmployeeId = '" + Session["OnboardEmployeeId"] + "'";
        //                strBuilder.Append(UpdateEmployeeSetup);
        //            }
        //            else
        //            {
        //                string SetEmployeeSetup = @"Insert Into Mas_Employee_Setup ( Deactivate,CreatedBy,CreatedDate,MachineName,SetupEmployeeId,SetupReporting) values ( '0','" + Session["EmployeeName"] + "', Getdate(),'" + Dns.GetHostName().ToString() + "','" + Session["OnboardEmployeeId"] + "','1')";
        //                strBuilder.Append(SetEmployeeSetup);
        //            }

        //            if (objcon.SaveStringBuilder(strBuilder, out abc))
        //            {
        //                message = "Record Save successfully";
        //                Icon = "success";

        //            }
        //            if (Session["TransferDocId"] != null)
        //            {
        //                var a = sqlconNew.Query("update  Trans_BusinessUnit set TransferStatus='Approved',ApprovedDate=GETDATE() where TransferBusinessUnitId=" + Session["TransferDocId"] + "");
        //            }
        //            var Message = new { message, Icon };

        //            return Json(Message, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}
        //#endregion

        //#region SaveUpdate
        //[HttpPost]
        //public ActionResult SaveUpdate(List<RequestApproval> RequestApproval)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { Area = "" });
        //        }
        //        DataTable dt = ConvertToDataTable(RequestApproval);
        //        var message = "";
        //        var Icon = "";
        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            for (int j = 1; j < dt.Columns.Count; j++)
        //            {
        //                var pending = DapperORM.DynamicQueryList("Select COUNT(*) as PendingCount from Tra_Approval where Status='Pending' and TraApproval_ApproverEmployeeId=" + dt.Rows[i][j].ToString() + " and TraApproval_EmployeeId=" + Session["OnboardEmployeeId"] + " AND TraApproval_ModuleId=" + dt.Rows[i][0].ToString() + " and Deactivate=0").FirstOrDefault();
        //                if (pending.PendingCount != 0)
        //                {
        //                    var a = pending.PendingCount;
        //                    if (a > 0)
        //                    {
        //                        var PendingemployeeName = DapperORM.DynamicQueryList("Select Concat(Mas_Employee.EmployeeName ,' - ', Mas_Employee.EmployeeNo ,' - ' ,Mas_Branch.BranchName) as Name from Mas_Employee,Mas_Branch  where Mas_Branch.BranchId=Mas_Employee.EmployeeBranchId and  employeeid=" + Session["OnboardEmployeeId"] + "").FirstOrDefault();
        //                        var EMPName = PendingemployeeName.Name;
        //                        message = "Manager with pending requests awaiting approval of employee " + EMPName + "";
        //                        Icon = "error";
        //                        var Messagea = new { message, Icon };
        //                        return Json(Messagea, JsonRequestBehavior.AllowGet);

        //                        //return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
        //                    }
        //                }
        //            }
        //        }
        //        //var deleteQuery = "DELETE FROM Mas_Employee_Reporting WHERE ReportingEmployeeID = " + Session["OnboardEmployeeId"] + "";
        //        //sqlcon.Execute(deleteQuery);
        //        DapperORM.Execute("DELETE FROM Mas_Employee_Reporting WHERE ReportingEmployeeID = " + Session["OnboardEmployeeId"] + "");
        //        // DataTable dt = ConvertToDataTable(RequestApproval);
        //        StringBuilder strBuilder = new StringBuilder();
        //        string abc = "";
        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            for (int j = 1; j < dt.Columns.Count; j++)
        //            {
        //                if (dt.Rows[i][j].ToString() == "0")
        //                {
        //                    break;
        //                }
        //                abc = @"Insert Into Mas_Employee_Reporting (ReportingModuleID,ReportingEmployeeID,ReportingManager1,ApproverLevel,CreatedDate,CreatedBy,MachineName,IsCompulsory,Deactivate)
        //                                            values (" + dt.Rows[i][0].ToString() + "," + Session["OnboardEmployeeId"] + "," + dt.Rows[i][j].ToString() + "," + (j) + ",GetDate(),'" + Session["EmployeeName"] + "','" + Dns.GetHostName().ToString() + "','1','0') ";
        //                strBuilder.Append(abc);
        //            }
        //        }
        //        var CheckEmployeeSetup = DapperORM.DynamicQueryList(@"Select SetupReporting as GetCount from Mas_Employee_Setup where SetupEmployeeId='" + Session["OnboardEmployeeId"] + "' and deactivate=0");
        //        if (CheckEmployeeSetup != null)
        //        {
        //            string UpdateEmployeeSetup = @"Update Mas_Employee_Setup Set ModifiedBy = '" + Session["EmployeeName"] + "',  ModifiedDate = Getdate(),    MachineName = '" + Dns.GetHostName().ToString() + "', SetupReporting = '1' Where SetupEmployeeId = '" + Session["OnboardEmployeeId"] + "'";
        //            strBuilder.Append(UpdateEmployeeSetup);
        //        }
        //        else
        //        {
        //            string SetEmployeeSetup = @"Insert Into Mas_Employee_Setup ( Deactivate,CreatedBy,CreatedDate,MachineName,SetupEmployeeId,SetupReporting) values ( '0','" + Session["EmployeeName"] + "', Getdate(),'" + Dns.GetHostName().ToString() + "','" + Session["OnboardEmployeeId"] + "','1')";
        //            strBuilder.Append(SetEmployeeSetup);
        //        }

        //        if (objcon.SaveStringBuilder(strBuilder, out abc))
        //        {
        //            message = "Record Save successfully";
        //            Icon = "success";

        //        }
        //        if (Session["TransferDocId"] != null)
        //        {
        //            var a = DapperORM.ExecuteQuery("update  Trans_BusinessUnit set TransferStatus='Approved',ApprovedDate=GETDATE() where TransferBusinessUnitId=" + Session["TransferDocId"] + "");
        //        }
        //        var Message = new { message, Icon };
        //        return Json(Message, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}
        //#endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(string ReportingID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_ReportingID_Encrypted", ReportingID_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parame
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Reporting", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_Reporting", "Module_Employee_Reporting");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsUserCopyExists
        [HttpGet]
        public ActionResult IsReportingCopyExists(string FromUser, string ToUser)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");

                    param.Add("@p_ReportingEmployeeID", Session["OnboardEmployeeId"]);
                    param.Add("@p_FromEmployeeid", FromUser);
                    param.Add("@p_ToEmployeeid", ToUser);
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_CopyReporting", param);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    if (Message != "")
                    {
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                }

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region ApprovalCopy
        [HttpPost]
        public ActionResult ApprovalCopy(int? FromUser, int? ToUser)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_process", "Save");
                // param.Add("@p_ReportingID_Encrypted", ReportingID_Encrypted);
                param.Add("@p_ReportingEmployeeID", Session["OnboardEmployeeId"]);
                // param.Add("@p_ReportingModuleID", ReportingModuleID);
                // param.Add("@p_ReportingManager1", Reporting.ReportingManager1);
                param.Add("@p_FromEmployeeid", FromUser);
                param.Add("@p_ToEmployeeid", ToUser);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_CopyReporting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Module_Employee_Reporting", "Module_Employee_Reporting");
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