using LanguageExt.Common;
using Nancy.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Fluent;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmallPdf_Converter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Converter converter = new Converter();
        Rates Rates = new Rates();
        Dictionary<string, string> dtCurrency = new Dictionary<string, string>();
        Dictionary<string, string> dtBaseCurrency = new Dictionary<string, string>();
        IDictionary<string, Dictionary<string, string>> dtConverter = new Dictionary<string, Dictionary<string, string>>();
        Dictionary<string, string> dict = new Dictionary<string, string>();
        string path = AppDomain.CurrentDomain.BaseDirectory + @"\ConverterConfiguration.json";
        bool Update = false;
        public MainWindow()
        {
            InitializeComponent();
            //Inizialize Log
            InizializeLog();
            InicializeLoad();
        }


        private void InicializeLoad()
        {
            try
            {                
                //Load Combo Currency Base
                dtBaseCurrency.Add("Select Currency", "");
                dtBaseCurrency.Add("EUR", "https://v6.exchangerate-api.com/v6/f2c364cb2b79dd330e6ff886/latest/EUR");
                dtBaseCurrency.Add("USD", "https://v6.exchangerate-api.com/v6/f2c364cb2b79dd330e6ff886/latest/USD");
                foreach (KeyValuePair<string, string> item in dtBaseCurrency)
                {
                    cmbBaseCurrency.Items.Add(item.Key);
                    cmbBaseCurrency.SelectedIndex = 0;
                }
                LoadSetting(path);
            }
            catch (Exception ex)
            {
                Log.Error("Error: MainWindow.Load: " + ex.InnerException.ToString());
            }          
        }

    private void LoadSetting(string path)
        {
            try
            {
                if (Rates.Deserialize(path)!=null)
                {
                    List<Rates.Rate> lstRates = Rates.Deserialize(path);
                    IDictionary<string, Dictionary<string, string>> IdtSettings = new Dictionary<string, Dictionary<string, string>>();
                    Dictionary<string, string> dtSetting = new Dictionary<string, string>();
                    foreach (var items in lstRates)
                    {
                        var data = new Rates.Rate { KeyBaseCurrency = items.KeyBaseCurrency, KeyCurrency = items.KeyCurrency, Values = items.Values };

                        DGRates.Items.Add(data);
                        if (!IdtSettings.ContainsKey(items.KeyBaseCurrency))
                        {
                            dtSetting = new Dictionary<string, string>();
                            dtSetting.Add(items.KeyCurrency, items.Values);
                            IdtSettings.Add(items.KeyBaseCurrency, dtSetting);
                        }
                        else
                        {
                            if (!dtSetting.ContainsKey(items.KeyCurrency))
                            {
                                dtSetting.Add(items.KeyCurrency, items.Values);
                                IdtSettings[items.KeyBaseCurrency] = dtSetting;
                            }
                            else
                            {
                                dtSetting[items.KeyCurrency] = items.Values;
                                IdtSettings[items.KeyBaseCurrency] = dtSetting;
                            }
                        }
                    }
                    dtConverter = IdtSettings;
                }
            }
            catch(Exception ex)
            {
                Log.Error("Error: MainWindow.LoadSetting: " + ex.InnerException.ToString());
            }
        }
        /// <summary>
        /// Load Currency values
        /// </summary>
        /// <param name="apiUrl"></param>
        /// <returns></returns>
        public async Task<string> LoadCurrency(string apiUrl)
        {
            string Exchange = "";
            try
            {
               
                dtCurrency = await converter.GetApiCall(apiUrl);
                foreach (KeyValuePair<string, string> item in dtCurrency)
                {
                    cmbCurrency.Items.Add(item.Key.Trim());
                  
                }
                if (Update==false)
                {
                    cmbCurrency.SelectedIndex = 0;
                    txtValue.Text = dtCurrency[cmbCurrency.SelectedValue.ToString().Trim()];
                }
                    
                return Exchange;
            } 
            catch (Exception ex)
            {
                Log.Fatal("Fatal Error: MainWindow.LoadCurrency: " + ex.InnerException.ToString());
                return Exchange;
            }
        }

        private void cmbBaseCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
               
                    if (cmbBaseCurrency.SelectedIndex != 0)
                    {
                        if (dtBaseCurrency.ContainsKey(cmbBaseCurrency.SelectedItem.ToString()) == true)
                        {
                            LoadCurrency(dtBaseCurrency[cmbBaseCurrency.SelectedItem.ToString()].ToString());

                        }

                    }
            }
            catch (Exception ex)
            {
                Log.Fatal("Fatal Error: MainWindow.cmbBaseCurrency_SelectionChanged: " + ex.InnerException.ToString());
            }

        }

        private void cmbCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {               
                    txtValue.Text = dtCurrency[cmbCurrency.SelectedValue.ToString().Trim()];

            }
            catch(Exception ex)
            {
                Log.Error("Error: MainWindow.cmbCurrency_SelectionChanged: " + ex.InnerException.ToString());
            }

        }

        /// <summary>
        /// Save/Add Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                if ((cmbBaseCurrency.SelectedIndex == 0) || (txtValue.Text == ""))
                {

                    MessageBox.Show("All fields are required!!!", "INFORMATION", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    if (Update == true)
                        RemoveValue();
                   
                    AddValue();
                    
                }
            }
            catch (Exception ex)
            {
                Log.Fatal("Fatal Error: MainWindow.Button_Click: " + ex.InnerException.ToString());
            }
        }
        /// <summary>
        /// Remove/Update Datagrid
        /// </summary>
        private void RemoveValue()
        {
            try
            {
                Update = false;
                int SelectedIndex = DGRates.SelectedIndex;
                Rates.Rate GridRates = ((SmallPdf_Converter.Rates.Rate)DGRates.Items[SelectedIndex]);

                if (dtConverter.ContainsKey(GridRates.KeyBaseCurrency) == true)
                {
                    dict = dtConverter[GridRates.KeyBaseCurrency.ToString().Trim()];

                    if (dict.ContainsKey(GridRates.KeyCurrency.ToString().Trim())==true)
                    {
                        
                        dict.Remove(GridRates.KeyCurrency.ToString().Trim());
                        dtConverter[GridRates.KeyBaseCurrency.ToString().Trim()] = dict;
                    
                        DGRates.Items.RemoveAt(DGRates.SelectedIndex);
                        Rates.Serialization(dtConverter, path);
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Fatal("Fatal Error: MainWindow.RemoveValue: " + ex.InnerException.ToString());

            }

        }
        /// <summary>
        /// Add new row in Datagrid
        /// </summary>
        private void AddValue()
        {
            try
            {
                if (dtConverter.ContainsKey(cmbBaseCurrency.SelectedValue.ToString()) == true)
                {
                    dict = dtConverter[cmbBaseCurrency.SelectedValue.ToString().Trim()];

                    if (!dict.ContainsKey(cmbCurrency.SelectedValue.ToString().Trim()))
                    {
                        dict.Add(cmbCurrency.SelectedValue.ToString().Trim(), txtValue.Text.ToString().Trim());
                        dtConverter[cmbBaseCurrency.SelectedValue.ToString().Trim()] = dict;
                        var data = new Rates.Rate { KeyBaseCurrency = cmbBaseCurrency.SelectedValue.ToString().Trim(), KeyCurrency = cmbCurrency.SelectedValue.ToString().Trim(), Values = txtValue.Text.ToString().Trim() };
                        DGRates.Items.Add(data);
                    }
                    else
                    {
                        MessageBox.Show("Duplicate Value!!!", "INFORMATION", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    dict = new Dictionary<string, string>();
                    dict.Add(cmbCurrency.SelectedValue.ToString().Trim(), txtValue.Text.ToString().Trim());
                    dtConverter.Add(cmbBaseCurrency.SelectedValue.ToString(), dict);
                    var data = new Rates.Rate { KeyBaseCurrency = cmbBaseCurrency.SelectedValue.ToString().Trim(), KeyCurrency = cmbCurrency.SelectedValue.ToString().Trim(), Values = txtValue.Text.ToString().Trim() };
                    DGRates.Items.Add(data);

                }
                Rates.Serialization(dtConverter, path);
                InicializeControlls();
            }
            catch(Exception ex)
            {
                Log.Error("Error: MainWindow.ADD: " + ex.InnerException.ToString());

            }
        }
        /// <summary>
        /// Delete  Grid Information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int SelectedIndex = DGRates.SelectedIndex;
                Rates.Rate GridRates = ((SmallPdf_Converter.Rates.Rate)DGRates.Items[SelectedIndex]);

                MessageBoxResult result= MessageBox.Show("Are you sure?"+"\n\nBase Currency: " + GridRates.KeyBaseCurrency+"\nCurrency: " + GridRates.KeyCurrency + "\nExcahange Rate: "+ GridRates.Values,"DELETE",MessageBoxButton.YesNo,MessageBoxImage.Information);
               
                if (result==MessageBoxResult.Yes)
                {
                    RemoveValue();
                   
                }
                DataRatesFill();
            }
            catch (Exception ex)
            {
                Log.Error("Error: MainWindow.ButtonDelete_Click: " + ex.InnerException.ToString());

            }
        }
        /// <summary>
        /// Update Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Update = true;
                int SelectedIndex = DGRates.SelectedIndex;
                Rates.Rate GridRates = ((SmallPdf_Converter.Rates.Rate)DGRates.Items[SelectedIndex]);
                txtValue.Text = GridRates.Values.Trim();
                cmbBaseCurrency.Text = GridRates.KeyBaseCurrency.Trim();
                cmbCurrency.Text = GridRates.KeyCurrency.Trim();
                cmbBaseCurrency.IsReadOnly = true;
            }
            catch (Exception ex)
            {
                Log.Fatal("Fatal Error: MainWindow.ButtonUpdate_Click: " + ex.InnerException.ToString());

            }
        }
        /// <summary>
        /// Inicialize Controllers
        /// </summary>
        private void InicializeControlls()
        {
            txtValue.Text = "";
            cmbCurrency.SelectedIndex = 0;
            cmbBaseCurrency.SelectedIndex = 0;

        }
        private void DataRatesFill()
        {
            try
            {
                DGRates.Items.Clear();
                foreach (var item in dtConverter)
                {
                    foreach(var item2 in item.Value)
                    {
                        var data = new Rates.Rate { KeyBaseCurrency = item.Key.Trim(), KeyCurrency = item2.Key.Trim(), Values = item2.Value.Trim() };

                        DGRates.Items.Add(data);
                    }
                }
                DGRates.Items.Refresh();
            }
            catch(Exception ex)
            {
                Log.Error("Error: MainWindow.ButtonUpdate_Click: " + ex.InnerException.ToString());

            }
        }
        /// <summary>
        /// Inizialize log
        /// </summary>
        private void InizializeLog()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "Log.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            // Apply config           
            NLog.LogManager.Configuration = config;
        }
    }
}
