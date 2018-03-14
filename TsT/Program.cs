/*
 * Created by SharpDevelop.
 * User: denblo
 * Date: 16.08.2016
 * Time: 19:10
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Ninject;
using TsT.Components;

namespace TsT
{
    /// <summary>
    /// Class with program entry point.
    /// </summary>
    internal sealed class Program
    {
        /// <summary>
        /// Program entry point.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var kernel = new StandardKernel(new Bindings());

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var moduleTypes = new List<Type>();
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetInterface("IModule") != null)
                    {
                        var module = kernel.Get(type) as IModule;
                        module.OnInit();
                    }
                }
            }

            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler((sender, eventArgs) => {
                var logger = kernel.Get<Logger>();
                logger.Error("Domain error " + eventArgs.ToString());
            });

            var mainForm = kernel.Get<MainForm>();
            Application.Run(mainForm);
        }
    }
}