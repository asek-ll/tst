/*
 * Created by SharpDevelop.
 * User: denblo
 * Date: 16.08.2016
 * Time: 19:10
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using TsT.Components;

using Ninject;

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
			var mainForm = kernel.Get<MainForm>();
			Application.Run(mainForm);
		}

		
	}
}
