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
using System.Timers;
using System.Xml;
// ACT will parse these assembly attributes to show plugin info in the same way it would if it were a DLL
[assembly: AssemblyTitle("Discord Bot Script")]
[assembly: AssemblyDescription("Sends current parse data to discord bot every 3 seconds")]
[assembly: AssemblyVersion("1.0.0.0")]

namespace Some_ACT_Plugin
{
    public class Discord_bot_Plugin : IActPluginV1 // To be loaded by ACT, plugins must implement this interface
    {
        Label statusLabel;	// Handle for the status label passed by InitPlugin()
		string settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\DISCORDACTBOT.config.xml");
		SettingsSerializer xmlSettings;
        System.Timers.Timer tmr = new System.Timers.Timer();
		
		//components
		TextBox textBox1 = new System.Windows.Forms.TextBox();
		TextBox textBox2 = new System.Windows.Forms.TextBox();

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            statusLabel = pluginStatusText;
            statusLabel.Text = "Plugin started";
			
			//Label
            Label lbl = new Label();
            lbl.Location = new System.Drawing.Point(8, 8);
            lbl.AutoSize = true;
            lbl.Text = "Discord Bot address:";
            pluginScreenSpace.Controls.Add(lbl);
			
			//textBox1
			textBox1.Location = new System.Drawing.Point(8, 30);
			textBox1.Name = "textBox1";
			textBox1.Size = new System.Drawing.Size(431, 20);
			textBox1.TabIndex = 1;
			textBox1.Text = "http://localhost:3000";
			pluginScreenSpace.Controls.Add(textBox1);
			
			//Label
            Label lbl2 = new Label();
            lbl2.Location = new System.Drawing.Point(8, 55);
            lbl2.AutoSize = true;
            lbl2.Text = "ACT Key (sent by discord bot upon authentication):";
            pluginScreenSpace.Controls.Add(lbl2);
			
			//textBox2
			textBox2.Location = new System.Drawing.Point(8, 77);
			textBox2.Name = "textBox2";
			textBox2.Size = new System.Drawing.Size(431, 20);
			textBox2.TabIndex = 1;
			textBox2.Text = "";
			pluginScreenSpace.Controls.Add(textBox2);
			
			//Timer
            tmr.Interval = 3000;
			tmr.Elapsed += new ElapsedEventHandler(tmr_Tick);
			tmr.Start();
            tmr.Enabled = true;
			
			//Load settings?
			xmlSettings = new SettingsSerializer(this); // Create a new settings serializer and pass it this instance
			LoadSettings();
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
			
			var request = (HttpWebRequest)WebRequest.Create(textBox1.Text);

			var postData = "guid=" + Uri.EscapeDataString(textBox2.Text);
				postData += "&log=" + logdata;
			var data = Encoding.ASCII.GetBytes(postData);

			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = data.Length;
			request.ReadWriteTimeout = 500;
			
			//Make the request
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
			tmr.Stop();
			SaveSettings();
            statusLabel.Text = "Plugin exited";
        }
		
		void LoadSettings()
		{
			// Add any controls you want to save the state of
			xmlSettings.AddControlSetting(textBox1.Name, textBox1);
			xmlSettings.AddControlSetting(textBox2.Name, textBox2);

			if (File.Exists(settingsFile))
			{
				FileStream fs = new FileStream(settingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				XmlTextReader xReader = new XmlTextReader(fs);

				try
				{
					while (xReader.Read())
					{
						if (xReader.NodeType == XmlNodeType.Element)
						{
							if (xReader.LocalName == "SettingsSerializer")
							{
								xmlSettings.ImportFromXml(xReader);
							}
						}
					}
				}
				catch (Exception ex)
				{
					statusLabel.Text = "Error loading settings: " + ex.Message;
				}
				xReader.Close();
			}
		}
		void SaveSettings()
		{
			FileStream fs = new FileStream(settingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			XmlTextWriter xWriter = new XmlTextWriter(fs, Encoding.UTF8);
			xWriter.Formatting = Formatting.Indented;
			xWriter.Indentation = 1;
			xWriter.IndentChar = '\t';
			xWriter.WriteStartDocument(true);
			xWriter.WriteStartElement("Config");    // <Config>
			xWriter.WriteStartElement("SettingsSerializer");    // <Config><SettingsSerializer>
			xmlSettings.ExportToXml(xWriter);   // Fill the SettingsSerializer XML
			xWriter.WriteEndElement();  // </SettingsSerializer>
			xWriter.WriteEndElement();  // </Config>
			xWriter.WriteEndDocument(); // Tie up loose ends (shouldn't be any)
			xWriter.Flush();    // Flush the file buffer to disk
			xWriter.Close();
		}
    }
}
