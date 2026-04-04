using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Marketplace;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_Marketplace
{
    public class ESS_Marketplace_MarketplaceGalleryController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Marketplace_MarketplaceGallery
        public ActionResult ESS_Marketplace_MarketplaceGallery()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 204;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_Category", 0);
                var data = DapperORM.ExecuteSP<dynamic>("Sp_GetMarketPlaceGallary", param).ToList();
                ViewBag.MarketplaceGallery = data;

                DynamicParameters param1 = new DynamicParameters();

                param1.Add("@query", "select MarketPlaceCategoryID,MarketPlaceCategory from MarketPlace_Category where Deactivate=0 order by MarketPlaceCategory asc");
                var MarketCategory = DapperORM.ReturnList<MarketPlace_Category>("sp_QueryExcution", param1);
                ViewBag.GetMarketCategory = MarketCategory;
                //System.Data.DataSet dataset = new System.Data.DataSet();
                //dataset = new System.Data.DataSet();
                //SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
                //if (sqlcon != null && sqlcon.State == ConnectionState.Closed)
                //{
                //    sqlcon.Open();
                //}
                //SqlCommand cmd = new SqlCommand("dbo.[Sp_GetMarketPlaceGallary]", sqlcon);
                //cmd.CommandTimeout = 0;
                //cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.AddWithValue("@p_Category", 0);
                //SqlDataAdapter dtadp = new SqlDataAdapter(cmd);
                //dtadp.Fill(dataset);
                //DataTable dtGallary = dataset.Tables[0];


                //for (int i = 0; i < dtGallary.Rows.Count; i++)
                //{
                //    Image image = Image.FromFile(dtGallary.Rows[i]["Photo1"].ToString());
                //    MemoryStream m = new MemoryStream();
                //    image.Save(m, image.RawFormat);
                //    byte[] imageBytes = m.ToArray();
                //    Convert byte[] to Base64 String
                //    string base64String = Convert.ToBase64String(imageBytes);
                //    list.Add("data:image; base64," + base64String);
                //    var GetPhoto1 = "data:image; base64," + base64String;
                //    dtGallary.Rows[i]["PhotoF1"] = GetPhoto1.ToString();



                //    Image image2 = Image.FromFile(dtGallary.Rows[i]["Photo2"].ToString());
                //    MemoryStream m2 = new MemoryStream();
                //    image2.Save(m2, image2.RawFormat);
                //    byte[] imageBytes2 = m2.ToArray();
                //    Convert byte[] to Base64 String
                //    string base64String2 = Convert.ToBase64String(imageBytes2);
                //    list.Add("data:image; base64," + base64String);
                //    var GetPhoto2 = "data:image; base64," + base64String2;
                //    dtGallary.Rows[i]["PhotoF2"] = GetPhoto2.ToString();

                //    Image image3 = Image.FromFile(dtGallary.Rows[i]["Photo3"].ToString());
                //    MemoryStream m3 = new MemoryStream();
                //    image3.Save(m3, image3.RawFormat);
                //    byte[] imageBytes3 = m3.ToArray();
                //    Convert byte[] to Base64 String

                //    string base64String3 = Convert.ToBase64String(imageBytes3);
                //    list.Add("data:image; base64," + base64String);
                //    var GetPhoto3 = "data:image; base64," + base64String3;
                //    dtGallary.Rows[i]["PhotoF3"] = GetPhoto3.ToString();

                //}

                //var Photos1 = new List<string>();
                //var Photos2 = new List<string>();
                //var Photos3 = new List<string>();
                //for (var i = 0; i <= data.Count - 1; i++)
                //{
                //    using (Image image = Image.FromFile(data[i].Photo1))
                //    {
                //        using (MemoryStream m = new MemoryStream())
                //        {
                //            image.Save(m, image.RawFormat);
                //            byte[] imageBytes = m.ToArray();
                //            // Convert byte[] to Base64 String
                //            string base64String = Convert.ToBase64String(imageBytes);
                //            //list.Add("data:image; base64," + base64String);  
                //            var GetPhoto1 = "data:image; base64," + base64String;
                //            Photos1.Add(GetPhoto1);
                //        }
                //    }
                //    using (Image image = Image.FromFile(data[i].Photo2))
                //    {
                //        using (MemoryStream m = new MemoryStream())
                //        {
                //            image.Save(m, image.RawFormat);
                //            byte[] imageBytes = m.ToArray();
                //            // Convert byte[] to Base64 String
                //            string base64String = Convert.ToBase64String(imageBytes);
                //            //list.Add("data:image; base64," + base64String);  
                //            var GetPhoto2 = "data:image; base64," + base64String;
                //            Photos1.Add(GetPhoto2);
                //        }
                //    }
                //    using (Image image = Image.FromFile(data[i].Photo3))
                //    {
                //        using (MemoryStream m = new MemoryStream())
                //        {
                //            image.Save(m, image.RawFormat);
                //            byte[] imageBytes = m.ToArray();
                //            // Convert byte[] to Base64 String
                //            string base64String = Convert.ToBase64String(imageBytes);
                //            //list.Add("data:image; base64," + base64String);  
                //            var GetPhoto3 = "data:image; base64," + base64String;
                //            Photos1.Add(GetPhoto3);
                //        }
                //    }
                //    ViewBag.GetAllPhotos1 = Photos1;
                //}


                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }


        public ActionResult GetMarketplacePostsByCategory(int categoryId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Category", categoryId);
                var posts = DapperORM.ExecuteSP<dynamic>("Sp_GetMarketPlaceGallary", param).ToList();

                if (posts == null || posts.Count == 0)
                {
                    // Return a custom message if no posts are found
                    return Json(new { message = "No posts found for the selected category." }, JsonRequestBehavior.AllowGet);
                }

                // Return the posts as JSON if found
                return Json(new { posts = posts }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Return an error message if an exception occurs
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        #region Get PhotoBase64Convert
        public JsonResult GetBase64Image(string photoPath)
        {
            try
            {
                if (!System.IO.File.Exists(photoPath))
                {
                    return Json(new { success = false, message = "Image not available." }, JsonRequestBehavior.AllowGet);
                }

                byte[] imageBytes = System.IO.File.ReadAllBytes(photoPath);
                string base64Image = "data:image/jpeg;base64," + Convert.ToBase64String(imageBytes);

                // Use a custom JsonResult that allows large payload
                return new JsonResult
                {
                    Data = new { success = true, base64Image },
                    MaxJsonLength = Int32.MaxValue,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        #endregion
    }
}