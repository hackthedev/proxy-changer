using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace proxy_changer
{
    class update
    {
        #region
        static public WebClient wc = new WebClient();
        static public Stream stream;
        static public StreamReader reader;
        static Form1 fm;

        static public string version;

        static public bool updates;

        static public string Gapp;
        #endregion

        public static void check(string app)
        {
            Gapp = app;
            fm = new Form1();

            try
            {
                stream = wc.OpenRead("https://adsoleware.com/updates/" + app + "/version.txt");
                reader = new StreamReader(stream);
                version = reader.ReadToEnd();

                wc.Dispose();
                stream.Dispose();
                reader.Dispose();
            }
            catch (Exception ex1)
            {
                MessageBox.Show("#65874" + ex1.Message);
            }

            if (Application.ProductVersion == version)
            {
                using (var client = new WebClient())
                {
                    try
                    {
                        if (File.Exists("packet.exe"))
                        {
                            File.Delete("packet.exe");
                        }

                        client.DownloadFile("https://adsoleware.com/updates/" + app + "/packet.exe", "packet.exe");
                        if (File.Exists("packet.exe"))
                        {
                            if (File.Exists(@"C:\Program Files\WinRAR\UnRAR.exe"))
                            {
                                Process.Start("packet.exe");
                                updates = true;
                            }
                            else
                            {
                                updates = false;
                                MessageBox.Show("Für die Installation wird WinRAR benötigt!\n\n" +
                                    "Installation fehlgeschlagen.", "AdSoleWare", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else
                        {
                            updates = false;
                            MessageBox.Show("Ein unerwartener Fehler ist passiert. Die setup-Datei wurde vermutlich nicht gefunden.", "AdSoleWare",
                                             MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        updates = false;
                        MessageBox.Show("Update Fehler!", Gapp, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else { updates = false; }
        }

        private void Debug(string message, string title = "Proxy Changer", MessageBoxButtons b = MessageBoxButtons.OK, MessageBoxIcon i = MessageBoxIcon.Information)
        {
            if (Debugger.IsAttached)
            {
                MessageBox.Show(message, title, b, i);
            }
        }
    }

}
