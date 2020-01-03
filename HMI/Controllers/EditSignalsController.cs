using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace HMI.Controllers
{
    public class EditSignalsController : Controller
    {
        public IActionResult Index()
        {
           
            return View();
        }

        public IActionResult SaveSignals(string fileContent)
        {
            System.IO.File.WriteAllText("signals.json", fileContent);

            try
            {
                var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("signals.json").Build();

                var signals = config.GetSection("PLCSignals").GetChildren().ToArray();

                HomeController.signals = new Models.Signal[signals.Count()];
                for (int i = 0; i < signals.Count(); i++)
                {
                    Models.Signal signal = new Models.Signal();
                    signal.Key = signals[i].Key;
                    signal.Name = (string)signals[i].GetValue(typeof(string), "Name");
                    signal.Type = (string)signals[i].GetValue(typeof(string), "Type");
                    signal.Controll = (string)signals[i].GetValue(typeof(string), "Controll");
                    signal.Sequence = (int)signals[i].GetValue(typeof(int), "Sequence");
                    try { signal.Devider = (bool)signals[i].GetValue(typeof(bool), "Devider"); }
                    catch { signal.Devider = false; }
                    try { signal.Label = (string)signals[i].GetValue(typeof(string), "Label"); }
                    catch { signal.Label = null; }
                    try { signal.signalGroup = (int)signals[i].GetValue(typeof(int), "SignalGroup"); }
                    catch { signal.signalGroup = -1; }
                    try { signal.Size = (int)signals[i].GetValue(typeof(int), "Size"); }
                    catch { signal.Size = -1; }
                    try
                    {
                        var allowedValues = signals[i].GetSection("AllowedValues").GetChildren();
                        if (allowedValues == null || allowedValues.Count() <= 0)
                        {
                            signal.AllowedValues = null;
                        }
                        else
                        {
                            signal.AllowedValues = new List<KeyValuePair<string, string>>();
                            signal.AllowedValues.Add(new KeyValuePair<string, string>("", ""));
                            foreach (var allowedValue in allowedValues)
                            {
                                signal.AllowedValues.Add(new KeyValuePair<string, string>(allowedValue.Key, allowedValue.Value));
                            }
                        }
                    }
                    catch { signal.AllowedValues = null; }


                    HomeController.signals[i] = signal;

                    
                }

                HomeController.signals = HomeController.signals.OrderBy(h => h.Sequence).ToArray();

                var plcSettings = config.GetSection("PLCSettings");
                string path = "opc.tcp://" + (string)plcSettings.GetValue(typeof(string), "ip");
                path += ":" + (string)plcSettings.GetValue(typeof(string), "port");
                OPC.SetPath(path);
                HomeController.node = (int)plcSettings.GetValue(typeof(int), "node");


            }
            catch (Exception e)
            {
                Models.Signal signal = new Models.Signal();
                signal.Label = "Invalid signal configuration !!!!!";
                HMI.Controllers.HomeController.signals = new Models.Signal[2];
                HMI.Controllers.HomeController.signals[0] = signal;
                signal = new Models.Signal();
                signal.Label = e.Message;
                HMI.Controllers.HomeController.signals[1] = signal;
            }

            return RedirectToAction("Index", "Home");
        }

    }
}