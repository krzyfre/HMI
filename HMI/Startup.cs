using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace HMI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            try
            {
                var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("signals.json").Build();

                var signals = config.GetSection("PLCSignals").GetChildren().ToArray();

                HMI.Controllers.HomeController.signals = new Models.Signal[signals.Count()];
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
                    HMI.Controllers.HomeController.signals[i] = signal;
                }

                HMI.Controllers.HomeController.signals = HMI.Controllers.HomeController.signals.OrderBy(h => h.Sequence).ToArray();

                var plcSettings = config.GetSection("PLCSettings");
                string path = "opc.tcp://" + (string)plcSettings.GetValue(typeof(string), "ip");
                path += ":" + (string)plcSettings.GetValue(typeof(string), "port");
                OPC.SetPath(path);
                Controllers.HomeController.node = (int)plcSettings.GetValue(typeof(int), "node");

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
            

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            OPC.StartOPC();
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
