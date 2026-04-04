using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Web;

namespace KompassHR.Models
{
    public class NotificationService
    {
   //     private readonly string _baseUrl = "http://115.124.123.180:8094/SendNotification";
        //private readonly string _baseUrl = "http://115.124.123.77:8091/SendNotification";

        public string SendNotification(string customerCode, object requestData)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // ✅ Header
                    client.DefaultRequestHeaders.Remove("CustomerCode");
                    client.DefaultRequestHeaders.Add("CustomerCode", customerCode);

                    // ✅ JSON Serialize
                    string json = JsonConvert.SerializeObject(requestData);
                    Console.WriteLine("📤 Sending JSON: " + json); // For debug
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var SetbaserUrl = HttpContext.Current.Session["GetBaseURLForNotification"].ToString();
                    // ✅ POST
                    HttpResponseMessage response = client.PostAsync(SetbaserUrl, content).GetAwaiter().GetResult();

                    string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                        return $"✅ Success: {responseContent}";
                    else
                        return $"❌ Bad Request ({(int)response.StatusCode}): {responseContent}";
                }
            }
            catch (Exception ex)
            {
                return $"❌ Exception: {ex.Message}";
            }
        }

    }

    public class NotificationModel
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public int NotifyEmpId { get; set; }
        public string CreatedBy { get; set; }
        public string RequestType { get; set; }
    }
}