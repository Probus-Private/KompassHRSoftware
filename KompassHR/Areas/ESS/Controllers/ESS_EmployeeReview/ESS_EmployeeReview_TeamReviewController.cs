using Dapper;
using KompassHR.Areas.ESS.Models.ESS_EmployeeReview;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;

namespace KompassHR.Areas.ESS.Controllers.ESS_EmployeeReview
{
    public class ESS_EmployeeReview_TeamReviewController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_EmployeeReview_TeamReview
        #region TeamReview Main View
        [HttpGet]
        public ActionResult ESS_EmployeeReview_TeamReview(string EmployeeReviewId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Review_Employee ReviewEmployee = new Review_Employee();

                var Manager1 = Session["ManagerId1"];
                var Manger2 = Session["ManagerId2"];
                var HRId = Session["HRId"];
                var EmployeeId = Session["EmployeeId"];

                DynamicParameters EmployeeList = new DynamicParameters();
                EmployeeList.Add("@p_EmployeeID", Session["EmployeeId"]);
                EmployeeList.Add("@p_Origin", "ESS");
                var listMasEmployee = DapperORM.ReturnList<AllDropDownBind>("sp_DropDown_Employee", EmployeeList);
                ViewBag.GetEmployeeName = listMasEmployee;

                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Review_Employee where deactivate =0";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;
                }

                param = new DynamicParameters();
                var EmployeerReviweLastRecord = "select max(CreatedDate) As CreatedDate from Review_Employee where ReviewerEmployeeId=" + EmployeeId + "  and Deactivate = 0";
                var LastRecored = DapperORM.DynamicQuerySingle(EmployeerReviweLastRecord);
                ViewBag.LastRecored = LastRecored.CreatedDate;

                if (EmployeeReviewId_Encrypted != null)
                {
                    param = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@P_EmployeeReviewId_Encrypted", EmployeeReviewId_Encrypted);
                    ReviewEmployee = DapperORM.ReturnList<Review_Employee>("sp_List_Review_Employee", param).FirstOrDefault();
                    SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
                    {
                        var GetDocNo = "Select  DocNo As DocNo  from Review_Employee where EmployeeReviewId_Encrypted= '" + EmployeeReviewId_Encrypted + "' and deactivate =0";
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        ViewBag.DocNo = DocNo;
                    }
                }
                TempData["DocDate"] = ReviewEmployee.DocDate;
                TempData["NextReviewDate"] = ReviewEmployee.NextReviewDate;
                return View(ReviewEmployee);
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_EmployeeReview_TeamReview");
            }
        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Review_Employee ReviewEmployee)
        {
            try
            {
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(ReviewEmployee.EmployeeReviewId_Encrypted) ? "Save" : "Update");
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_EmployeeReviewId", ReviewEmployee.EmployeeReviewId);
                param.Add("@p_EmployeeReviewId_Encrypted", ReviewEmployee.EmployeeReviewId_Encrypted);
                param.Add("@p_ReviewEmployeeId", ReviewEmployee.ReviewEmployeeId);
                param.Add("@p_ReviewerEmployeeId", EmployeeId);
                param.Add("@p_DocNo", ReviewEmployee.DocNo);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_Rating", ReviewEmployee.Rating);
                param.Add("@p_DocDate", ReviewEmployee.DocDate);
                param.Add("@p_ReviewTittle", ReviewEmployee.ReviewTittle);
                param.Add("@p_Description", ReviewEmployee.Description);
                param.Add("@p_Suggestion", ReviewEmployee.Suggestion);
                param.Add("@p_ReviewPanel", ReviewEmployee.ReviewPanel);
                param.Add("@p_NextReviewDate", ReviewEmployee.NextReviewDate);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Review_Employee", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_EmployeeReview_TeamReview", "ESS_EmployeeReview_TeamReview");
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_EmployeeReview_TeamReview");
            }
        }
        #endregion

        #region Getlist
        [HttpGet]
        public ActionResult Getlist()
        {
            try
            {
                var EmployeeId = Session["EmployeeId"];
                param.Add("@P_EmployeeReviewId_Encrypted", "List");
                param.Add("@P_ReviewerEmployeeId", EmployeeId);
                var data = DapperORM.DynamicList("sp_List_Review_Employee", param);
                ViewBag.GetReviwearList = data;
                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_EmployeeReview_TeamReview");
            }
        }

        #endregion

        #region Delete
        public ActionResult Delete(string EmployeeReviewId_Encrypted)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_EmployeeReviewId_Encrypted", EmployeeReviewId_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Review_Employee", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Getlist", "ESS_EmployeeReview_TeamReview");
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_EmployeeReview_TeamReview");
            }
        }
        #endregion
    }
}