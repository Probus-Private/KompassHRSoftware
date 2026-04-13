using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using KompassHR.Models;

namespace KompassHR.Report
{
    public partial class Rdlc_Report_webForm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                System.Data.DataSet dsVisitorPass = new System.Data.DataSet();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    SqlCommand sqlComm = new SqlCommand("Select VisitorId,InDateTime,HostName,DepartmentName,VisitorName,MobileNo,             PurposeName,             CompanyName,             Address,             VechileNo			 from visitor_Tra_Master           ", sqlcon);
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(dsVisitorPass);
                }
                

                ReportDataSource rds = new ReportDataSource("dsVisitorPass", dsVisitorPass.Tables[0]);
                ReportViewer1.LocalReport.DataSources.Add(rds);
                string path = Server.MapPath("~\\Report");
                ReportViewer1.LocalReport.ReportPath = path + "\\rdlcVisitorPass.rdlc";
                //ReportViewer1.LocalReport.Refresh();
                ReportViewer1.Visible = true;

            }
           
        }
    }
}