using Dapper;
using KompassHR.Areas.Reports.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
namespace KompassHR.Models
{
    public class BulkAccessClass
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        // Method to get the Company Name
        public List<AllDropDownBind> GetCompanyName()
        {
            var EmpId = HttpContext.Current.Session["EmployeeId"];
            DynamicParameters paramCompany = new DynamicParameters();
            paramCompany.Add("@p_employeeid", EmpId);
            return DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
        }

        // Method to get the Business Unit (Branch)
        public List<AllDropDownBind> GetBusinessUnit(int? cmpId, int? employeeId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@p_employeeid", employeeId);
            param.Add("@p_CmpId", cmpId);
            return DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
        }

        // Method to get Employee Name
        public List<AllDropDownBind> GetEmployeeNames(int employeeId, int cmpId, DateTime getFromDate)
        {
            string query = @"
            SELECT EmployeeId as Id, CONCAT(EmployeeName, ' - ', EmployeeNo) as Name
            FROM Mas_Employee 
            WHERE Deactivate = 0 
              AND EmployeeId IN 
                  (SELECT DISTINCT Inoutemployeeid 
                   FROM Atten_InOut 
                   WHERE Deactivate = 0 
                     AND InOutBranchId IN 
                         (SELECT BranchID 
                          FROM UserBranchMapping 
                          WHERE EmployeeID = @employeeId 
                          AND IsActive = 1 
                          AND CmpID = @CmpId) 
                     AND MONTH(InOutDate) = @month 
                     AND YEAR(InOutDate) = @year 
                     AND EmployeeId <> 1)
            UNION
            SELECT EmployeeId as Id, CONCAT(EmployeeName, ' - ', EmployeeNo) as Name
            FROM Mas_Employee 
            WHERE Deactivate = 0 
              AND EmployeeBranchId IN 
                  (SELECT BranchID 
                   FROM UserBranchMapping 
                   WHERE EmployeeID = @employeeId 
                     AND IsActive = 1 
                     AND CmpID = @CmpId)
              AND (MONTH(JoiningDate) <= @month AND YEAR(JoiningDate) <= @year)
              AND (MONTH(LeavingDate) = @month AND YEAR(LeavingDate) = @year OR LeavingDate IS NULL)
            ORDER BY Name";

            DynamicParameters param = new DynamicParameters();
            param.Add("@employeeId", employeeId);
            param.Add("@CmpId", cmpId);
            param.Add("@month", getFromDate.Month);
            param.Add("@year", getFromDate.Year);

            return DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
        }

        // Method to get Shift Sort Name
        public List<ShiftDropDown> GetShiftSortName(int employeeId, int cmpId)
        {
            string query = @"
            SELECT DISTINCT ShiftName + ' ( ' + FORMAT(CONVERT(datetime, BeginTime), 'HH:mm') + ' - ' + FORMAT(CONVERT(datetime, EndTime), 'HH:mm') + ' )' AS Name, 
                            SName
            FROM Atten_Shifts 
            WHERE Deactivate = 0 
              AND ShiftBranchId IN 
                  (SELECT BranchID 
                   FROM UserBranchMapping 
                   WHERE EmployeeID = @employeeId 
                     AND IsActive = 1 
                     AND CmpID = @CmpId)";

            DynamicParameters param = new DynamicParameters();
            param.Add("@employeeId", employeeId);
            param.Add("@CmpId", cmpId);

            return DapperORM.ReturnList<ShiftDropDown>("sp_QueryExcution", param).ToList();
        }

        // Method to get Contractor Dropdown
        public List<AllDropDownBind> GetContractorDropdown(int employeeId, int cmpId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@p_qry", " AND Mas_ContractorMapping.BranchID IN (SELECT BranchID FROM UserBranchMapping WHERE EmployeeID = @employeeId AND IsActive = 1 AND CmpID = @CmpId)");
            param.Add("@employeeId", employeeId);
            param.Add("@CmpId", cmpId);

            return DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param).ToList();
        }

        //THIS CODE FOR CHECK USER HAVE RIGHTS OR NOT FOR THIS PAGE
        public bool CheckAccess(int ScreenId, int UserAccessPolicyId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@query", $@"SELECT COUNT(1) as GetCount
                                   FROM Tool_UserAccessPolicyMaster,Tool_UserAccessPolicyDetails 
                                   Where Tool_UserAccessPolicyMaster.UserGroupId=Tool_UserAccessPolicyDetails.UserGroupDetails_UserGroupID
                                   and Tool_UserAccessPolicyMaster.Deactivate=0 
                                   and Tool_UserAccessPolicyMaster.UserGroupId={UserAccessPolicyId} 
                                   and Tool_UserAccessPolicyDetails.UserGroupDetails_ScreenID= {ScreenId}");
            var result = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).FirstOrDefault();
            int checkValid = result?.GetCount ?? 0;
            return checkValid == 1;
        }

        public List<AllDropDownBind> GetEmployeeName(int? BranchId)
        {

            DynamicParameters param = new DynamicParameters();
            param.Add("@query", "select employeeid as id,CONCAT(EmployeeName, ' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeeBranchId='" + BranchId + "' and Employeeleft=0 and ContractorID=1 order by Name");
            //var Employee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
            return DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

        }

        public List<AllDropDownBind> GetAllEmployeeName()
        {

            DynamicParameters param = new DynamicParameters();
            param.Add("@query", "select employeeid as id,CONCAT(EmployeeName, ' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and Employeeleft=0 and ContractorID<>1 order by Name");
            //var Employee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
            return DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

        }
        public List<AllDropDownBind> AllEmployeeName()
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@query", "select employeeid as id,CONCAT(EmployeeName, ' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and Employeeleft=0 and ContractorID=1 order by Name");
            //var Employee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
            return DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
        }

        public List<AllDropDownBind> AllEmployeeName_WithoutContractor()
        {
            var EmployeeId =  HttpContext.Current.Session["EmployeeId"];
            var CmpId = HttpContext.Current.Session["CompanyId"];
            DynamicParameters param = new DynamicParameters();
            param.Add("@query", $@"select employeeid as id,CONCAT(EmployeeName, ' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and Employeeleft=0 
            AND mas_employee.EmployeeBranchId IN(SELECT ub.BranchID FROM UserBranchMapping ub WHERE ub.EmployeeID = {EmployeeId} AND ub.CmpID = {CmpId}) order by Name");
            //var Employee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
            return DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
        }
    }
}