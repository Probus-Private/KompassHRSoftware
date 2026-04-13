using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Dapper;
using System.Net.Mail;

namespace KompassHR.Models
{
    public class clsEmail
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        public bool SendMail(int DocId, int CmpID, string origin, string EmailOrigin)
        {
            string SMTPServerName = "";
            int PortNo = 0;
            string FromEmailId = "";
            string Password = "";
            bool SSL = false;


            if (origin.ToString() == "1".ToString())
            {
                var ServerDetail = DapperORM.DynamicQuerySingle("Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where  Deactivate=0 and CmpId='" + CmpID + "'  and  Origin='" + 1 + "' ");
                if (ServerDetail != null)
                {
                    SMTPServerName = ServerDetail.SMTPServerName;
                    PortNo = ServerDetail.PortNo;
                    FromEmailId = ServerDetail.FromEmailId;
                    Password = ServerDetail.Password;
                    SSL = ServerDetail.SSL;


                    param.Add("@p_Origin", origin);
                    param.Add("@p_EmailOrigin", EmailOrigin);
                    param.Add("@p_DocId", DocId);
                    var Email = DapperORM.ExecuteSP<dynamic>("sp_GetEmail", param).FirstOrDefault();
                    if (Email != null)
                    {


                        SmtpClient smtp = new SmtpClient(SMTPServerName, PortNo);
                        smtp.EnableSsl = SSL;
                        smtp.UseDefaultCredentials = true;
                        smtp.Timeout = 100000;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                        smtp.Credentials = new System.Net.NetworkCredential(FromEmailId, Password);

                        MailMessage mail = new MailMessage(FromEmailId, Email.ToEmail);
                        mail.Subject = Email.Subject;
                        mail.Body = Email.Body;
                        //  mail.Attachments.Add(new Attachment(Fbank.ExportToStream(ExportFormatType.PortableDocFormat), "PaymentSlip.pdf"));
                        mail.IsBodyHtml = true;
                        smtp.Send(mail);
                    }
                }


            }
            else
            {
                var ServerDetail = DapperORM.DynamicQuerySingle("Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where  Deactivate=0 and CmpId='" + CmpID + "'  and  Origin='" + 1 + "' ");
                if (ServerDetail != null)
                {
                    SMTPServerName = ServerDetail.SMTPServerName;
                    PortNo = ServerDetail.PortNo;
                    FromEmailId = ServerDetail.FromEmailId;
                    Password = ServerDetail.Password;
                    SSL = ServerDetail.SSL;


                    param.Add("@p_Origin", origin);
                    param.Add("@p_EmailOrigin", EmailOrigin);
                    param.Add("@p_EmployeeId", DocId);
                    var Email = DapperORM.ExecuteSP<dynamic>("sp_GetResignationEmail", param).FirstOrDefault();
                    if (Email != null)
                    {


                        SmtpClient smtp = new SmtpClient(SMTPServerName, PortNo);
                        smtp.EnableSsl = SSL;
                        smtp.UseDefaultCredentials = true;
                        smtp.Timeout = 100000;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                        smtp.Credentials = new System.Net.NetworkCredential(FromEmailId, Password);

                        MailMessage mail = new MailMessage(FromEmailId, Email.ToEmail);
                        mail.Subject = Email.Subject;
                        mail.Body = Email.Body;
                        //  mail.Attachments.Add(new Attachment(Fbank.ExportToStream(ExportFormatType.PortableDocFormat), "PaymentSlip.pdf"));
                        mail.IsBodyHtml = true;
                        smtp.Send(mail);
                    }
                }

            }
            return false;

        }
    }
}