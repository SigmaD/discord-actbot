// Add special assembly references to be parsed by ACT here.  ACT will always add itself as a reference.
// Example:  (Keep the // commenting in actual use)  
// reference:System.dll
using System;
using System.Reflection;
using Advanced_Combat_Tracker;
using System.Windows.Forms;
using System.Net;
using System.Text;
using System.IO;
// ACT will parse these assembly attributes to show plugin info in the same way it would if it were a DLL
[assembly: AssemblyTitle("Discord Bot Script")]
[assembly: AssemblyDescription("Sends current parse data to discord bot every 3 seconds")]
[assembly: AssemblyVersion("0.0.0.2")]

namespace Some_ACT_Plugin
{
    public class Clipboard_Sharer_Plugin : IActPluginV1 // To be loaded by ACT, plugins must implement this interface
    {
        Label statusLabel;	// Handle for the status label passed by InitPlugin()
        Timer tmr = new Timer();

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            statusLabel = pluginStatusText;
            statusLabel.Text = "Plugin started";
            Label lbl = new Label();
            lbl.Location = new System.Drawing.Point(8, 8);
            lbl.AutoSize = true;
            lbl.Text = "Updating the clipboard every second.";
            pluginScreenSpace.Controls.Add(lbl);
            tmr.Interval = 3000;
            tmr.Tick += tmr_Tick;
            tmr.Enabled = true;
        }

        void tmr_Tick(object sender, EventArgs e)
        {
			//Create format for log text
			TextExportFormatOptions tfoCustom = new TextExportFormatOptions("{n}{NAME15} | {Job} | {encdps-*}", "ENCDPS", true, true, "({duration}) {title}{n}(Max Hit: {maxhit-*})");
			
			var logdata = ActGlobals.oFormActMain.GetTextExport(ActGlobals.oFormActMain.ActiveZone.ActiveEncounter, tfoCustom);

			//Check logdata isn't empty - return if so
			if (logdata == ""){
				return;
			}
			
			var request = (HttpWebRequest)WebRequest.Create("http://localhost:3000");

			var postData = "guid=" + Uri.EscapeDataString("08c40205-9a73-460e-b992-a55a9e6aa8ca");
				postData += "&log=" + logdata;
			var data = Encoding.ASCII.GetBytes(postData);

			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = data.Length;
			request.ReadWriteTimeout = 500;
			
			//Oh no - This isn't async and makes ACT super laggy until the timeout or a response (TODO)
            try
            {
				using (var stream = request.GetRequestStream())
				{
					stream.Write(data, 0, data.Length);
				}

				var response = (HttpWebResponse)request.GetResponse();

				var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
				
				statusLabel.Text = responseString;
            }
            catch
            {
				statusLabel.Text = "not connected";
            }
        }

        public void DeInitPlugin()  // You must unsubscribe to any events you use
        {
            tmr.Enabled = false;
            tmr.Tick -= tmr_Tick;
            statusLabel.Text = "Plugin exited";
        }
    }
}
