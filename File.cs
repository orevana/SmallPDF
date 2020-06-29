using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmallPdf_Converter
{
    public class File
    {
        /// <summary>
        /// Create json
        /// </summary>
        /// <param name="json"></param>
        /// <param name="path"></param>
        public void CreateJson(string json,string path)
        {
            try
            {
                System.IO.File.WriteAllText(path, json);

            }
            catch (Exception ex)
            {
                Log.Error("Error: File.CreateJson: " + ex.InnerException.ToString());
            }
        }
        /// <summary>
        /// Read Json
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string ReadJson (string path)
        {
            string JsonString = "";
            try
            {
                JsonString = System.IO.File.ReadAllText(path);
                return JsonString;
            }
            catch(Exception ex)
            {
                Log.Fatal("Fatal Error: File.ReadJson: " + ex.InnerException.ToString());
                return JsonString;
            }
        }
    }
}
