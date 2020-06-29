using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace SmallPdf_Converter
{
   public class Rates
    {
        File files = new File();

        /// <summary>
        /// Serialize Json
        /// </summary>
        /// <param name="Rates"></param>
        /// <param name="path"></param>
        public void Serialization(IDictionary<string, Dictionary<string, string>> Rates,string path)
        {
            try
            {
                List<Rates.Rate> lstRates = new List<Rates.Rate>();                
                foreach (var item in Rates)
                {
                    foreach (var item2 in item.Value)
                    {
                        Rates.Rate rates = new Rates.Rate();
                        rates.KeyBaseCurrency = item.Key.ToString();
                        rates.KeyCurrency = item2.Key.ToString();
                        rates.Values = item2.Value.ToString();
                        lstRates.Add(rates);
                    }                        
                }
                string json = JsonConvert.SerializeObject(lstRates);            
                files.CreateJson(json, path);        
            }
            catch(Exception ex)
            {
                Log.Error("Error: Rates.Serialization: " + ex.InnerException.ToString());
            }
           
        }
        /// <summary>
        /// Deserialize Json
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<Rates.Rate> Deserialize(string path)
        {            
            if (!System.IO.File.Exists(path))
            {
                return null;
            }
            else
            {
                string JsonString = System.IO.File.ReadAllText(path);
                List<Rates.Rate> objRates = JsonConvert.DeserializeObject<List<Rates.Rate>>(JsonString);
                return objRates;
            }       
        }

        public class Rate
        {
            public string KeyBaseCurrency { get; set; }
            public string KeyCurrency { get; set; }
            public string Values { get; set; }
        }
    }
}
