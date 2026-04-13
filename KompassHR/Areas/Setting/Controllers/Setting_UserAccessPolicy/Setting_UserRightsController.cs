using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_UserAccessPolicy;
using KompassHR.Models;
using Newtonsoft.Json;
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
using Z.Dapper.Plus;

namespace KompassHR.Areas.Setting.Controllers.Setting_UserAccessPolicy
{
    public class Setting_UserRightsController : Controller
    {

        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        // GET: Setting/Setting_UserRights
        public ActionResult Setting_UserRights(Tool_UserRights obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                if (GetComapnyName.Count>0)
                {
                    ViewBag.CompanyName = GetComapnyName;
                    var CmpId = GetComapnyName[0].Id;
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_employeeid", Session["EmployeeId"]);
                    param1.Add("@p_CmpId", CmpId);
                    var GetBranchId = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param1).ToList();
                    if(GetBranchId.Count>0)
                    {
                        ViewBag.BranchName = GetBranchId;
                        var BranchId = GetBranchId[0].Id;
                        DynamicParameters paramEmpName = new DynamicParameters();
                        paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee ,Mas_Employee_ESS where Mas_Employee.Deactivate=0 and Mas_Employee_ESS.Deactivate=0 and Mas_Employee_ESS.ESSEmployeeId=Mas_Employee.EmployeeId and CmpID= "+CmpId + " and Mas_Employee.EmployeeBranchId="+ BranchId + " and Mas_Employee.EmployeeLeft=0 ");
                        ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                    }
                    else
                    {
                        ViewBag.BranchName = "";
                        ViewBag.EmployeeName = "";
                    }
                   
                }
                else
                {
                    ViewBag.CompanyName = "";
                }

                var Query = "";
                if (obj.BranchId != null)
                {

                    DynamicParameters paramBranchList = new DynamicParameters();
                    paramBranchList.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranchList.Add("@p_CmpId", obj.CmpId);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranchList).ToList();
                    ViewBag.BranchName = data;

                    DynamicParameters paramEmpName1 = new DynamicParameters();
                    paramEmpName1.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee ,Mas_Employee_ESS where Mas_Employee.Deactivate=0 and Mas_Employee_ESS.Deactivate=0 and Mas_Employee_ESS.ESSEmployeeId=Mas_Employee.EmployeeId and CmpID= " + obj.CmpId + " and Mas_Employee.EmployeeBranchId=" + obj.BranchId + " and Mas_Employee.EmployeeLeft=0 ");
                    ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName1).ToList();
                }
                else
                {
                    Query = "and mas_branch.BranchId in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + obj.CmpId + " and UserBranchMapping.IsActive = 1)";
                    //ViewBag.GetEmpolyeeName = "";
                    //ViewBag.BranchName = "";
                }

                DynamicParameters param3 = new DynamicParameters();
                ViewBag.AddUpdateTitle = "Add";
                param3.Add("@query", "select UserGroupId as Id , UserGroupName as Name  from Tool_UserAccessPolicyMaster  where Deactivate=0 order by UserGroupName");
                var UserGroupPolicy = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();
                ViewBag.GroupName = UserGroupPolicy;

                if (obj.UserRightsEmployeeId != null)
                {
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_EmployeeId", obj.UserRightsEmployeeId);
                    param2.Add("@p_GroupId", obj.UserRightsGroupId);
                    var data = DapperORM.ExecuteSP<dynamic>("sp_GetUserRightsScreenList", param2).ToList();
                    ViewBag.GetMenus = data;
                }
                else
                {
                    ViewBag.GetMenus = null;
                }
                return View(obj);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult GetscreenName(int EmployeeId, int GroupId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_EmployeeId", EmployeeId);
                param1.Add("@p_GroupId", GroupId);
                var data = DapperORM.ExecuteSP<dynamic>("sp_GetUserRightsScreenList", param1).ToList();
                ViewBag.GetMenus = data;
                return View("Setting_UserRights");
                //return Json(new { data = data }, JsonRequestBehavior.AllowGet);

                //var GetModuleId = DapperORM.DynamicQuerySingle(@"Select Distinct(ScreenModuleId) As ScreenModuleId from Tool_UserAccessPolicyDetails , Tool_ScreenMaster where UserGroupDetails_UserGroupID = '" + GroupId + "' and ScreenId = UserGroupDetails_ScreenID").ToList();
                //var ScreenModuleId = GetModuleId[0].ScreenModuleId;

                //param.Add("@query", @"select userrightsemployeeid from tool_userrights where UserRightsEmployeeId = '" + EmployeeId + "' and UserRightsModuleId in ('" + ScreenModuleId + "')");
                //var CheckExist = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();

                ////var CheckExist = DapperORM.DynamicQuerySingle(@"select userrightsemployeeid from tool_userrights where UserRightsEmployeeId = '" + EmployeeId + "' and UserRightsModuleId in ('" + GetModuleId + "')").FirstOrDefault();
                ////var CheckExist = DapperORM.DynamicQuerySingle(@"select userrightsemployeeid from tool_userrights where UserRightsEmployeeId = '" + EmployeeId + "' and UserRightsModuleId = '"+ ScreenModuleId + "'");

                ////if (CheckExist.Count!=0)
                ////{
                ////    param.Add("@query", @"select Tool_screenMaster.ScreenID,Tool_screenMasteR.ScreenModuleId , Tool_screenMaster.ScreenDisplayMenuName,Tool_screenMaster.ScreenMenuType, ISNULL(Tool_UserRights.IsMenu, 0) AS IsMenu,
                ////    ISNULL(Tool_UserRights.IsSave, 0)  AS IsSave, ISNULL(Tool_UserRights.IsUpdate, 0) AS IsUpdate, ISNULL(Tool_UserRights.IsDelete, 0) AS IsDelete, ISNULL(Tool_UserRights.IsList, 0) AS IsList from Tool_UserAccessPolicyMaster
                ////    inner join Tool_UserAccessPolicyDetails on Tool_UserAccessPolicyDetails.UserGroupDetails_UserGroupID = Tool_UserAccessPolicyMaster.UserGroupId and  Tool_UserAccessPolicyDetails.UserGroupDetails_UserGroupID=" + GroupId + "join Tool_screenMaster on Tool_screenMaster.ScreenId = Tool_UserAccessPolicyDetails.UserGroupDetails_ScreenID left join Tool_UserRights on Tool_UserRights.UserRightsScreenId = Tool_UserAccessPolicyDetails.UserGroupDetails_ScreenID and Tool_UserRights.UserRightsEmployeeId = " + EmployeeId + "");
                ////    var data = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
                ////    return Json(new { data = data }, JsonRequestBehavior.AllowGet);
                ////}

                //if (CheckExist.Count != 0)
                //{
                //    var GetUserRightsModuleId = DapperORM.DynamicQuerySingle(@"Select Distinct(UserRightsModuleId) as ModuleId from Tool_UserRights where UserRightsEmployeeId="+ EmployeeId + "").ToList();
                //    List<decimal> dataList = new List<decimal>();
                //    var numbers = GetUserRightsModuleId.Count;
                //    for (int i = 0; i < numbers; i++)
                //    {
                //        dataList.Add(GetUserRightsModuleId[i].ModuleId);
                //    }
                //    string ScreenModuleIdNew = string.Join(",", dataList);
                //    param.Add("@query", @"select Tool_screenMaster.ScreenID,Tool_screenMasteR.ScreenModuleId , Tool_screenMaster.ScreenDisplayMenuName,Tool_screenMaster.ScreenMenuType, ISNULL(Tool_UserRights.IsMenu, 0) AS IsMenu, ISNULL(Tool_UserRights.IsSave, 0)  AS IsSave, ISNULL(Tool_UserRights.IsUpdate, 0) AS IsUpdate,ISNULL(Tool_UserRights.IsDelete, 0) AS IsDelete, ISNULL(Tool_UserRights.IsList, 0) AS IsList  ,Tool_screenMaster.DescriptionForUser from Tool_UserRights join Tool_ScreenMaster on Tool_ScreenMaster.ScreenId=Tool_UserRights.UserRightsScreenId	 where UserRightsEmployeeId=" + EmployeeId + "  and UserRightsModuleId in(" + ScreenModuleIdNew + ") and Tool_ScreenMaster.IsActive=1  and Tool_ScreenMaster.Deactivate = 0 union all select Tool_screenMaster.ScreenID, Tool_screenMasteR.ScreenModuleId, Tool_screenMaster.ScreenDisplayMenuName, Tool_screenMaster.ScreenMenuType, '0' AS IsMenu, '0' AS IsSave, '0' AS IsUpdate, '0' AS IsDelete, '0' AS IsList, Tool_screenMaster.DescriptionForUser  from Tool_ScreenMaster  where Tool_ScreenMaster.IsActive = 1 and ScreenModuleId in(" + ScreenModuleIdNew + ")  and Tool_ScreenMaster.Deactivate = 0 and  Tool_screenMaster.ScreenId not in (select UserRightsScreenId from Tool_UserRights where Tool_UserRights.UserRightsEmployeeId = " + EmployeeId + "   ) order by ScreenModuleId, ScreenMenuType");
                //    var data = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();


                //    //DynamicParameters param1 = new DynamicParameters();
                //    //param1.Add("@p_EmployeeId", EmployeeId);
                //    //param1.Add("@p_GroupId", GroupId);
                //    //var data = DapperORM.ExecuteSP<dynamic>("sp_GetUserRights_Access", param1).ToList();
                //    return Json(new { data = data }, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    param.Add("@query", @"select Tool_screenMaster.ScreenID, Tool_screenMasteR.ScreenModuleId , Tool_screenMaster.ScreenDisplayMenuName,Tool_screenMaster.ScreenMenuType, ISNULL(Tool_UserAccessPolicyDetails.IsMenu, 0) AS IsMenu, ISNULL(Tool_UserAccessPolicyDetails.IsSave, 0)  AS IsSave, ISNULL(Tool_UserAccessPolicyDetails.IsUpdate, 0) AS IsUpdate, ISNULL(Tool_UserAccessPolicyDetails.IsDelete, 0) AS IsDelete, ISNULL(Tool_UserAccessPolicyDetails.IsList, 0) AS IsList ,Tool_screenMaster.DescriptionForUser from Tool_UserAccessPolicyMaster inner join Tool_UserAccessPolicyDetails on Tool_UserAccessPolicyDetails.UserGroupDetails_UserGroupID = Tool_UserAccessPolicyMaster.UserGroupId and  Tool_UserAccessPolicyDetails.UserGroupDetails_UserGroupID =" + GroupId + " join Tool_screenMaster on Tool_screenMaster.ScreenId = Tool_UserAccessPolicyDetails.UserGroupDetails_ScreenID left join Tool_UserRights on Tool_UserRights.UserRightsScreenId = Tool_UserAccessPolicyDetails.UserGroupDetails_ScreenID and Tool_UserRights.UserRightsEmployeeId = " + EmployeeId + " union all select  Tool_screenMaster.ScreenID, Tool_screenMasteR.ScreenModuleId , Tool_screenMaster.ScreenDisplayMenuName,Tool_screenMaster.ScreenMenuType,'0'AS IsMenu,'0' AS IsSave,'0'AS IsUpdate,'0'AS IsDelete,'0'AS IsList , Tool_screenMaster.DescriptionForUser from Tool_ScreenMaster where ScreenId not in (select UserGroupDetails_ScreenID from Tool_UserAccessPolicyDetails where UserGroupDetails_UserGroupID=" + GroupId + " ) and Deactivate=0 and Tool_screenMasteR.ScreenModuleId in (select UserGroupDetails_ModuleID from Tool_UserAccessPolicyDetails where UserGroupDetails_ModuleID=" + ScreenModuleId + ")");

                //    var data = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
                //    return Json(new { data = data }, JsonRequestBehavior.AllowGet);
                //}


                //if exists(select userrightsemployeeid from tool_userrights where UserRightsEmployeeId = 1)
                //begin
                //Print('not null')
                //select Tool_screenMaster.ScreenID,Tool_screenMasteR.ScreenModuleId , Tool_screenMaster.ScreenDisplayMenuName, ISNULL(Tool_UserRights.IsMenu, 0) AS IsMenu,
                // ISNULL(Tool_UserRights.IsSave, 0)  AS IsSave, ISNULL(Tool_UserRights.IsUpdate, 0) AS IsUpdate, ISNULL(Tool_UserRights.IsDelete, 0) AS IsDelete, ISNULL(Tool_UserRights.IsList, 0) AS IsList from Tool_UserAccessPolicyMaster
                //      inner join Tool_UserAccessPolicyDetails on Tool_UserAccessPolicyDetails.UserGroupDetails_UserGroupID = Tool_UserAccessPolicyMaster.UserGroupId and  Tool_UserAccessPolicyDetails.UserGroupDetails_UserGroupID = 12 join
                //          Tool_screenMaster on Tool_screenMaster.ScreenId = Tool_UserAccessPolicyDetails.UserGroupDetails_ScreenID left join Tool_UserRights on Tool_UserRights.UserRightsScreenId = Tool_UserAccessPolicyDetails.UserGroupDetails_ScreenID and
                //Tool_UserRights.UserRightsEmployeeId = 1
                //end
                //else
                //begin
                //Print(' null')
                //select Tool_screenMaster.ScreenID,Tool_screenMasteR.ScreenModuleId , Tool_screenMaster.ScreenDisplayMenuName, ISNULL(Tool_UserAccessPolicyDetails.IsMenu, 0) AS IsMenu,
                // ISNULL(Tool_UserAccessPolicyDetails.IsSave, 0)  AS IsSave, ISNULL(Tool_UserAccessPolicyDetails.IsUpdate, 0) AS IsUpdate, ISNULL(Tool_UserAccessPolicyDetails.IsDelete, 0) AS IsDelete, ISNULL(Tool_UserAccessPolicyDetails.IsList, 0) AS IsList from Tool_UserAccessPolicyMaster
                //     inner join Tool_UserAccessPolicyDetails on Tool_UserAccessPolicyDetails.UserGroupDetails_UserGroupID = Tool_UserAccessPolicyMaster.UserGroupId and  Tool_UserAccessPolicyDetails.UserGroupDetails_UserGroupID = 12 join
                //         Tool_screenMaster on Tool_screenMaster.ScreenId = Tool_UserAccessPolicyDetails.UserGroupDetails_ScreenID left join Tool_UserRights on Tool_UserRights.UserRightsScreenId = Tool_UserAccessPolicyDetails.UserGroupDetails_ScreenID and
                //Tool_UserRights.UserRightsEmployeeId = 1
                //end


                //           param.Add("@query", @"select Tool_screenMaster.ScreenID,Tool_screenMasteR.ScreenModuleId , Tool_screenMaster.ScreenDisplayMenuName, ISNULL(Tool_UserRights.IsMenu,0) AS IsMenu,			
                //ISNULL(Tool_UserRights.IsSave,0)  AS IsSave,ISNULL(Tool_UserRights.IsUpdate,0) AS IsUpdate,ISNULL(Tool_UserRights.IsDelete,0) AS IsDelete,	ISNULL(Tool_UserRights.IsList,0) AS IsList from Tool_UserAccessPolicyMaster
                //inner join Tool_UserAccessPolicyDetails on Tool_UserAccessPolicyDetails.UserGroupDetails_UserGroupID=Tool_UserAccessPolicyMaster.UserGroupId and  Tool_UserAccessPolicyDetails.UserGroupDetails_UserGroupID=" + GroupId + "join Tool_screenMaster on Tool_screenMaster.ScreenId=Tool_UserAccessPolicyDetails.UserGroupDetails_ScreenID left join Tool_UserRights on Tool_UserRights.UserRightsScreenId=Tool_UserAccessPolicyDetails.UserGroupDetails_ScreenID and Tool_UserRights.UserRightsEmployeeId=" + EmployeeId + "");
                //           var data = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
                //           return Json(new { data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                var BUID = Branch[0].Id;
                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee ,Mas_Employee_ESS where Mas_Employee.Deactivate=0 and Mas_Employee_ESS.Deactivate=0 and Mas_Employee_ESS.ESSEmployeeId=Mas_Employee.EmployeeId and CmpID= " + CmpId + " and Mas_Employee.EmployeeBranchId=" + BUID + " and Mas_Employee.EmployeeLeft=0 ");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                return Json(new { EmployeeName = EmployeeName, Branch = Branch }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetUnit
        [HttpGet]
        public ActionResult GetUnit(int? CmpId, int? UnitBranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (UnitBranchId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee ,Mas_Employee_ESS where Mas_Employee.Deactivate=0 and Mas_Employee_ESS.Deactivate=0 and Mas_Employee_ESS.ESSEmployeeId=Mas_Employee.EmployeeId and CmpID= " + CmpId + " and Mas_Employee.EmployeeBranchId=" + UnitBranchId + " and Mas_Employee.EmployeeLeft=0 ");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public ActionResult SaveUpdate(List<Tool_UserRightsInsertUpdate> UseRigths)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                //string json = JsonConvert.SerializeObject(UseRigths);
                //    param.Add("@p_process", "Save");
                //    //param.Add("@p_UserRightsScreenId", UseRigths[i].UserRightsScreenId);
                //    //param.Add("@p_UserRightsModuleId", UseRigths[i].UserRightsModuleId);
                //    //param.Add("@p_IsMenu", UseRigths[i].IsMenu);
                //    //param.Add("@p_IsSave", UseRigths[i].IsSave);
                //    //param.Add("@p_IsUpdate", UseRigths[i].IsUpdate);
                //    //param.Add("@p_IsDelete", UseRigths[i].IsDelete);
                //    //param.Add("@p_IsList", UseRigths[i].IsList);
                //    param.Add("@p_UserRightsEmployeeId", EmployeeId);
                //    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                //    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);

                //    param.Add("@UserRightsTable", dt, DbType.Object, ParameterDirection.Input);
                //    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                //    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                //    var data = DapperORM.ExecuteReturn("sp_SUD_Tool_UserRights", param);
                //    TempData["Message"] = param.Get<string>("@p_msg");
                //    TempData["Icon"] = param.Get<string>("@p_Icon");
                //}

                //DataTable dt = ConvertToDataTable(UseRigths);
                //param.Add("@UserRightsTable", dt, DbType.Object, ParameterDirection.Input);
                //param.Add("@p_process", "Save");
                //param.Add("@p_jsonData", json);
                //param.Add("@p_UserRightsEmployeeId", EmployeeId);
                //param.Add("@p_MachineName", Dns.GetHostName().ToString());
                //param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                //param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                //param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                //var data = DapperORM.ExecuteReturn("sp_SUD_Tool_UserRights", param);
                //TempData["Message"] = param.Get<string>("@p_msg");
                //TempData["Icon"] = param.Get<string>("@p_Icon");
                //return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);

                StringBuilder strBuilder = new StringBuilder();
                string abc = "";
                string DeleteUserRights = " Delete from Tool_UserRights " +
                                                         " Where  UserRightsEmployeeId=" + UseRigths[0].EmployeeId + "";
                                                         //  " and UserRightsScreenId = " + UseRigths[0].UserRightsScreenId + " "+                                                   
                                                         //" and UserRightsModuleId = " + UseRigths[0].UserRightsModuleId + "";
                strBuilder.Append(DeleteUserRights);
                
              
               
                foreach (var Data in UseRigths)
                {
                    string SetUserRights = " Insert Into Tool_UserRights ( UserRightsEmployeeId,UserRightsScreenId,UserRightsModuleId,IsMenu,Deactivate,CreatedBy,CreatedDate,MachineName) values ( " + Data.EmployeeId + "," + Data.UserRightsScreenId + "," + Data.UserRightsModuleId + ",'" + Data.IsMenu + "','0','" + Session["EmployeeName"] + "', Getdate(),'" + Dns.GetHostName().ToString() + "')";
                    strBuilder.Append(SetUserRights);
                }

                
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record Save successfully";
                    TempData["Icon"] = "success";
                }
                if (abc != "")
                {
                    TempData["Message"] = abc;
                    TempData["Icon"] = "error";
                }

                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_UserRightsID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Tool_UserRights", param);
                ViewBag.GetUserRights = data;
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