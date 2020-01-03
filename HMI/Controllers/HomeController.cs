using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HMI.Models;
using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;

namespace HMI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public static Signal[] signals;
        public static int node;

        public static bool suctionCup1;
        public static bool suctionCup2;
        public static bool suctionCup3;
        public static bool suctionCup4;
        public static string orderPartType;
        public static string preparationPartType;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            await ReadValues();
            
            return View();
        }

        public async Task<IActionResult> RestartOPC()
        {
            await OPC.StartOPC();
            await ReadValues();

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Toggle(string variableName)
        {
            await ToggleValue(variableName);
            await ReadValues();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> SetStringValue(string variableName, string value)
        {
            await OPC.WriteVar(node, new KeyValuePair<string, object>[] { new KeyValuePair<string, object>(variableName, value) });
            await ReadValues();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> SetValues(string[] variableNames, string[] values, string[] types)
        {
            int submitCount = values.Count(h => h == "submit");
            int submitsEncountered = 0;
            KeyValuePair<string, object>[] submitSignals = null;
            KeyValuePair<string, object>[] valuesToSet = new KeyValuePair<string, object>[variableNames.Length - submitCount];
            for (int i =0; i< variableNames.Length; i++)
            {
                switch (types[i])
                {
                    case "bool":
                        valuesToSet[i - submitsEncountered] = new KeyValuePair<string, object>(variableNames[i], bool.Parse(values[i]));
                        break;
                    case "string":
                        valuesToSet[i - submitsEncountered] = new KeyValuePair<string, object>(variableNames[i], (string)values[i]);
                        break;
                    case "ushort":
                        valuesToSet[i - submitsEncountered] = new KeyValuePair<string, object>(variableNames[i], ushort.Parse(values[i]));
                        break;
                    case "int":
                        valuesToSet[i - submitsEncountered] = new KeyValuePair<string, object>(variableNames[i], int.Parse(values[i]));
                        break;
                    case "submit":
                        if (submitSignals == null)
                        {
                            submitSignals = new KeyValuePair<string, object>[1];
                            submitSignals[0] = new KeyValuePair<string, object>(variableNames[i], true);
                        }
                        else
                        {
                            submitSignals = submitSignals.Append(new KeyValuePair<string, object>(variableNames[i], true)).ToArray();
                        }
                        
                        submitsEncountered++;
                        break;
                    default:
                        valuesToSet[i - submitsEncountered] = new KeyValuePair<string, object>(variableNames[i], values[i]);
                        break;
                }

                
            }
            await OPC.WriteVar(node, valuesToSet );
            if(submitSignals!=null) await OPC.WriteVar(node, submitSignals);
            await ReadValues();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> SetNumValue(string variableName, ushort value)
        {
            await OPC.WriteVar(node, new KeyValuePair<string, object>[] { new KeyValuePair<string, object>(variableName, value) });
            await ReadValues();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> SetIntValue(string variableName, int value)
        {
            await OPC.WriteVar(node, new KeyValuePair<string, object>[] { new KeyValuePair<string, object>(variableName, value) });
            await ReadValues();
            return RedirectToAction("Index", "Home");
        }

        public async Task<bool> ReadValues()
        {
            string[] valuesToRead = new string[signals.Length];
            for(int i=0; i< signals.Length; i++)
            {
                valuesToRead[i] = signals[i].Key;
            }

            DataValue[] values = (await OPC.ReadVar(node, valuesToRead));
            if (values == null) return false;
            for(int i =0; i< values.Length; i++)
            {
                if (signals[i].Type == "bool")
                {
                    signals[i].boolValue = values[i].GetValueOrDefault<bool>();
                }
                else if (signals[i].Type == "string")
                {
                    signals[i].stringValue = values[i].GetValueOrDefault<string>();
                }
                else if (signals[i].Type == "ushort")
                {
                    signals[i].numValue = values[i].GetValueOrDefault<ushort>();
                }
                else if (signals[i].Type == "int")
                {
                    signals[i].intValue = values[i].GetValueOrDefault<int>();
                }

            }

            return true;
        }



        public async Task<bool> ToggleValue(string variableName)
        {
            DataValue value = (await OPC.ReadVar(node, new string[] { variableName })).FirstOrDefault();
            bool variable = value.GetValueOrDefault<bool>();
            if (variable == false)
            {
                OPC.WriteVar(node, new KeyValuePair<string, object>[] { new KeyValuePair<string, object>(variableName, true) });
            }
            else
            {
                OPC.WriteVar(node, new KeyValuePair<string, object>[] { new KeyValuePair<string, object>(variableName, false) });
            }
            return true;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
