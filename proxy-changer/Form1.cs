using CefSharp;
using CefSharp.WinForms;
using MetroFramework;
using MetroFramework.Forms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace proxy_changer

{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        public const int INTERNET_OPTION_REFRESH = 37;
        static bool settingsReturn, refreshReturn;

        int counter = 0;
        string file;
        string filebase = Directory.GetCurrentDirectory();
        private void Form1_Load(object sender, EventArgs e)
        {
            server_counter.Text = "";
            label2.Text = "Version " + Application.ProductVersion;

            CheckProxy();

            if (CheckForInternetConnection() == true)
            {
                update.check("proxy-changer");
            }

            if(update.updates == true)
            {
                Application.Exit();
            }

            // Cosmetics for UI
            metroTabControl1.SelectedIndex = 0;
            this.ActiveControl = label2;
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/generate_204"))
                    return true;
            }
            catch
            {
                return false;
            }
        }

        private void refresh_vpn()
        {
            // These lines implement the Interface in the beginning of program 
            // They cause the OS to refresh the settings, causing IP to realy update
            settingsReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            refreshReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }

        RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
        private void set_vpn(string ip)
        {
            registry.SetValue("ProxyEnable", 1);
            registry.SetValue("ProxyServer", ip);

            refresh_vpn();

            status.Text = "Connected to: " + ip;
            status.ForeColor = Color.Green;

            this.Icon = Properties.Resources.ip_green;
        }

        private void CheckProxy()
        {
            try
            {
                if (registry != null)
                {
                    Object o = registry.GetValue("ProxyServer");
                    Object o1 = registry.GetValue("ProxyEnable");

                    if (o1.ToString() == "0")
                    {
                        status.Text = "Disconnected from: " + o as String;
                        status.ForeColor = Color.Red;
                    }
                    else if (o1.ToString() == "1")
                    {
                        status.Text = "Connected to: " + o as String;
                        status.ForeColor = Color.Red;
                    }
                    else
                    {
                        Debug("Not found any shit\n" + o1 as string);
                    }
                }
            }
            catch (Exception ex)  //just for demonstration...it's always best to handle specific exceptions
            {
                Debug(ex.Message);
            }
        }
        
        private void unset_vpn()
        {
            registry.SetValue("ProxyEnable", 0);
            refresh_vpn();

            setStatusTextFromThread("Disconnected", Color.Red, Color.Transparent);

            this.Icon = Properties.Resources.ip_red;
        }

        private void setStatusTextFromThread(string text, Color font, Color back)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(final_add), new object[] { text });
                return;
            }
            else
            {
                status.Text = text;
                status.ForeColor = font;
                status.BackColor = back;
            }
        }

        int label_counter = 0;
        public void final_add(string value)
        {

            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(final_add), new object[] { value });
                return;
            }

            try
            {
                string counter = value.Split(':')[0];
                string ip = value.Split(':')[1];
                string port = value.Split(':')[2];
                string country = value.Split(':')[3];
                string ms = value.Split(':')[4];
                
                this.dt.Rows.Insert(Int32.Parse(counter), ip + ":" + port, country, ms);

                label_counter++;
                server_counter.Text = "( " + label_counter.ToString() + " )";
            }
            catch(Exception ex2)
            {
                Debug("Error while trying to list Server:\n" + ex2.Message);
            }
        }

        public void final_log(string value)
        {
            try
            {
                if (InvokeRequired)
                {
                    this.Invoke(new Action<string>(final_log), new object[] { value });
                    return;
                }

                textBox1.AppendText(value + Environment.NewLine);
            }
            catch
            {

            }
        }

        public void disable_radio(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(disable_radio), new object[] { value });
                return;
            }

            disable_radio2();
        }

        public void enable_radio(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(enable_radio), new object[] { value });
                return;
            }

            enable_radio2();
        }

        public void clear_dt(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(clear_dt), new object[] { value });
                return;
            }

            server_counter.Text = "( " +  label_counter.ToString() + " )";
            dt.Refresh();
        }

        private void enable_radio2()
        {
            radioButton1.Enabled = true;
            radioButton2.Enabled = true;
            radioButton3.Enabled = true;
            radioButton4.Enabled = true;
        }

        private void disable_radio2()
        {
            radioButton1.Enabled = false;
            radioButton2.Enabled = false;
            radioButton3.Enabled = false;
            radioButton4.Enabled = false;
        }

        string[] lines;
        private void getServers()
        {
            label_counter = 0;

            clear_dt("");
            bool fallback = false;

            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile("https://adsoleware.com/apps/updates/proxy-changer/lists/" + file, file);
                    final_log(Environment.NewLine + Environment.NewLine + "Downloaded list '" + file + "'");
                }

                fallback = false;
            }
            catch (Exception ex)
            {
                final_log(Environment.NewLine + "Error on downloading IP List");

                if (File.Exists(file))
                {
                    fallback = true;
                    final_log("Falling back to local list :)");
                }
            }

            

            try
            {
                lines = File.ReadAllLines(file);
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                {
                    Application.Exit();
                }
                else
                {
                    Application.Restart();
                }
            }

            string term;
            if(fallback == true) { term = "local fallback"; } else { term = ""; }

            final_log("Loaded " + term + " list '" + file + "'" + Environment.NewLine);

            disable_radio("");

            foreach (string s in lines)
            {
                string formated = s.Split(':')[0];

                Ping pingSender = new Ping();
                PingReply reply = pingSender.Send(formated);

                if (reply.Status == IPStatus.Success)
                {
                    string tmp = counter.ToString() + ":" + s.ToString() + ":" + GetCountryByIP(formated) + ":" + reply.RoundtripTime + "ms";
                    final_add(tmp);
                    counter++;
                    
                }
                else
                {
                    final_log(formated + " : " + reply.Status.ToString());
                }
            }

            enable_radio("");
        }

        public string GetCountryByIP(string ipAddress)
        {
            try
            {
                string strReturnVal;
                string ipResponse = IPRequestHelper("http://ip-api.com/xml/" + ipAddress);

                //return ipResponse;
                XmlDocument ipInfoXML = new XmlDocument();
                ipInfoXML.LoadXml(ipResponse);
                XmlNodeList responseXML = ipInfoXML.GetElementsByTagName("query");

                NameValueCollection dataXML = new NameValueCollection();

                dataXML.Add(responseXML.Item(0).ChildNodes[2].InnerText, responseXML.Item(0).ChildNodes[2].Value);

                strReturnVal = responseXML.Item(0).ChildNodes[1].InnerText.ToString(); // Contry
                strReturnVal += "(" +

                responseXML.Item(0).ChildNodes[2].InnerText.ToString() + ")";  // Contry Code 
                return strReturnVal;
            }
            catch
            {
                final_log("Unknown Location for: " + ipAddress);
                return "Unknown";
            }
        }

        public string IPRequestHelper(string url)
        {

            try
            {
                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();

                StreamReader responseStream = new StreamReader(objResponse.GetResponseStream());
                string responseRead = responseStream.ReadToEnd();

                responseStream.Close();
                responseStream.Dispose();

                return responseRead;
            }
            catch
            {
                return "Unknown";
            }
        }

        private void dt_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn &&
                e.RowIndex >= 0)
            {
                try
                {
                    DataGridViewRow row = this.dt.Rows[e.RowIndex];
                    string ip = row.Cells["Server"].Value.ToString();

                    textBox1.AppendText(Environment.NewLine + "Connecting to " + ip + Environment.NewLine);
                    set_vpn(ip);

                    MetroMessageBox.Show(this, "Connected to " + ip + "\n\nYou might need to wait for a minute for the connection to work", "Proxy Changer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("index"))
                    {
                        MetroMessageBox.Show(this, "ERR:\n" + ex.Message, "Proxy Changer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Developed by Marcel Schalk, published on AdSoleWare.com!\n\n" +
                "" +
                "Any Questions:\n" +
                "privat@adsoleware.com", "Marcel Schalk, Copyright", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            unset_vpn();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if(textBox1.Text.Length >= 32760)
            {
                textBox1.Clear();
            }
        }

        private void metroTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void disableSpinner(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(disableSpinner), new object[] { value });
                return;
            }
        }

        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs args)
        {
            if (!args.IsLoading)
            {
                disableSpinner("");
                
            }
        }

        private void reportServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                int selectedrowindex = dt.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = dt.Rows[selectedrowindex];
                string a = Convert.ToString(selectedRow.Cells["Server"].Value);

                Clipboard.SetText(a);

                MetroMessageBox.Show(this, "Server " + a + " was copied to your clipboard", "Proxy Changer", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("NULL"))
                {
                    MetroMessageBox.Show(this, "Please select a Server to report", "Proxy Changer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MetroMessageBox.Show(this, "Unexpected Error:\n" + ex.Message, "Proxy Changer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void refreshListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dt.Rows.Clear();
            Task.Run(() => getServers());
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            dt.Rows.Clear();
            file = "http.txt";
            Task.Run(() => getServers());
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            dt.Rows.Clear();
            file = "https.txt";
            Task.Run(() => getServers());
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            dt.Rows.Clear();
            file = "sock4.txt";
            Task.Run(() => getServers());
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            dt.Rows.Clear();
            file = "sock5.txt";
            Task.Run(() => getServers());
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            MetroMessageBox.Show(this, "Protocol used for Web Client like Chrome with HTTP", "Proxy Changer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            MetroMessageBox.Show(this, "Protocol used for Web Client like Chrome but with HTTPS", "Proxy Changer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            MetroMessageBox.Show(this, "Allows Client-Server applications (Like Windows Programs)", "Proxy Changer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            MetroMessageBox.Show(this, "lightweight, general-purpose proxy that sits at layer 5 of the OSI model and uses a tunneling method. It supports various types of traffic generated by protocols, such as HTTP, SMTP and FTP. SOCKs5 is faster than a VPN and easy to use. Since the proxy uses a tunneling method, public cloud users can access resources behind the firewall using SOCKs5 over a secured tunnel such as SSH", "Proxy Changer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            string message = "If you have selected a Protocol once, you have to restart in order to clear the list";
            MetroMessageBox.Show(this, message, "Proxy Changer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void clearListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dt.Rows.Clear();
            dt.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            unset_vpn();
            enable_radio2();
            MetroMessageBox.Show(this, "Disconnected from Server.\nYou might need to wait for a minute.", "Proxy Changer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void Show(string message, string title = "Proxy Changer", MessageBoxButtons b = MessageBoxButtons.OK, MessageBoxIcon i = MessageBoxIcon.Information)
        {
            MetroMessageBox.Show(this, message, title, b, i);
        }

        private void label4_Click(object sender, EventArgs e)
        {
            Process.Start(label4.Text);
        }

        private void label5_Click(object sender, EventArgs e)
        {
            Process.Start(label5.Text);
        }

        private void label7_Click(object sender, EventArgs e)
        {
            Process.Start(label7.Text);
        }

        private void label9_Click(object sender, EventArgs e)
        {
            Process.Start(label9.Text);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void support_Click(object sender, EventArgs e)
        {

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
