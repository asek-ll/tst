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
using TsT.Plugins.ClientProxy;
using TsT.Plugins.Jira;
using TsT.Plugins.Mvn;

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

            var plugins = new List<IPlugin>
            {
//                kernel.Get<ClientProxyPlugin>(),
                kernel.Get<PomReader>(),
                kernel.Get<JiraPlugin>(),
            };

            foreach (var plugin in plugins)
            {
                plugin.OnInit();
            }

            var mainForm = kernel.Get<MainForm>();
            Application.Run(mainForm);
        }
    }
}