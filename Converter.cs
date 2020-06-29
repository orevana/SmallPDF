using Newtonsoft.Json.Linq;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SmallPdf_Converter
{
    public class Converter
    {

        public async Task<Dictionary<string, string>> GetApiCall(string apiUrl)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            try
            {              
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        string[] items = JObject.Parse(data)["conversion_rates"].ToString().Split(',');
                        foreach (string item in items)
                        {
                            string[] keyValue = item.Replace('"', ' ').Replace("{", string.Empty).Replace("}", string.Empty).Split(':');
                            dictionary.Add(keyValue[0].Trim().Replace("\r\n", string.Empty), keyValue[1].Trim());
                        }
                
                        response.Dispose();
                    }
                    client.Dispose();
                }                
                return dictionary;
            }
            catch(Exception ex)
            {
                Log.Error("ERROR: API Call: " + ex.Message.ToString());
                return dictionary;
            }
        }


    }
}
