using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace Discord_Timed_Message_Sender
{
	public partial class Discord : Form
	{
        private NotifyIcon notifyIcon;
        private TimeSpan selectedTimeSpan;
        private Timer timer;
        private NOTIFYICONDATA notifyIconData;
        private string selectedToken;
        private string channelId;
        private string message;
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int NIM_ADD = 0x00000000;
        private const int NIM_DELETE = 0x00000002;
        private const int NIF_MESSAGE = 0x00000001;
        private const int NIF_ICON = 0x00000002;
        private const int NIF_TIP = 0x00000004;
        private const int WM_LBUTTONDOWN = 0x0201;

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("user32.dll")]
        private static extern IntPtr ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr Shell_NotifyIcon(int dwMessage, NOTIFYICONDATA pnid);

        private struct NOTIFYICONDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uID;
            public int uFlags;
            public int uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
        }



        public static List<string> GetDiscordTokens()
		{
			List<string> discordTokens = new List<string>();
			DirectoryInfo rootFolder = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\Roaming\\Discord\\Local Storage\\leveldb");
			bool exists = rootFolder.Exists;
			if (exists)
			{
				foreach (FileInfo file in rootFolder.GetFiles("*.ldb"))
				{
					string fileContent = file.OpenText().ReadToEnd();
					foreach (object obj in Regex.Matches(fileContent, "[\\w-]{24}\\.[\\w-]{6}\\.[\\w-]{27}"))
					{
						Match match = (Match)obj;
						string token = match.Value;
						bool isValidToken = Discord.ValidateToken(token);
						bool flag = isValidToken;
						if (flag)
						{
							discordTokens.Add(token);
						}
						else
						{
							MessageBox.Show("Invalid token found: " + token, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						}
					}
				}
			}
			else
			{
				MessageBox.Show("Token not found!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			return discordTokens;
		}
		private static bool ValidateToken(string token)
		{
			bool result;
			try
			{
				using (WebClient client = new WebClient())
				{
					client.Headers.Add("Authorization", token);
					string response = client.DownloadString("https://discord.com/api/v10/users/@me");
					result = true;
				}
			}
			catch (WebException)
			{
				result = false;
			}
			return result;
		}
		private void SendMessageToServer(string token, string channelId, string message)
		{
			try
			{
				using (WebClient client = new WebClient())
				{
					client.Headers.Add("Authorization", token);
					client.Headers.Add("Content-Type", "application/json");
					string payload = "{ \"content\": \"" + message + "\" }";
					byte[] data = Encoding.UTF8.GetBytes(payload);
					string url = "https://discord.com/api/v10/channels/" + channelId + "/messages";
					byte[] response = client.UploadData(url, "POST", data);
					MessageBox.Show("Message sent successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
			}
			catch (WebException ex)
			{
				MessageBox.Show("An error occurred while sending the message: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
		private void SendMessage(string token, string channelId, string message)
		{
			try
			{
				using (WebClient client = new WebClient())
				{
					client.Headers.Add("Authorization", token);
					client.Headers.Add("Content-Type", "application/json");
					string payload = "{ \"content\": \"" + message + "\" }";
					byte[] data = Encoding.UTF8.GetBytes(payload);
					string url = "https://discord.com/api/v10/channels/" + channelId + "/messages";
					byte[] response = client.UploadData(url, "POST", data);
					MessageBox.Show("Message sent successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
			}
			catch (WebException ex)
			{
				MessageBox.Show("An error occurred while sending the message: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
		public Discord()
		{
			this.InitializeComponent();

            notifyIconData = new NOTIFYICONDATA();
            notifyIconData.cbSize = Marshal.SizeOf(notifyIconData);
            notifyIconData.uFlags = NIF_ICON | NIF_TIP | NIF_MESSAGE;
            notifyIconData.uCallbackMessage = WM_LBUTTONDOWN;
            notifyIconData.hWnd = Handle;
            notifyIconData.hIcon = Properties.Resources.discord.Handle;
            notifyIconData.szTip = "Discord Timed Message Sender";
        }

		private void guna2TextBox1_TextChanged(object sender, EventArgs e)
		{
		}
		private void guna2GradientButton1_Click(object sender, EventArgs e)
		{
		}
		private void AutoToken_Click(object sender, EventArgs e)
		{
			List<string> tokens = Discord.GetDiscordTokens();
			bool flag = tokens.Count > 0;
			if (flag)
			{
				ComboBox.ObjectCollection items = this.guna2ComboBox1.Items;
				object[] items2 = tokens.ToArray();
				items.AddRange(items2);
				this.guna2ComboBox1.SelectedIndex = 0;
			}
			else
			{
				MessageBox.Show("Token not found!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		private void Discord_Load(object sender, EventArgs e)
		{
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = Properties.Resources.discord;
            notifyIcon.MouseClick += notifyIcon_MouseClick;
            notifyIcon.Visible = true;
        }
        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
        }
        private void guna2Button1_Click(object sender, EventArgs e)
		{
			string newToken = this.guna2TextBox1.Text.Trim();
			bool flag = !string.IsNullOrEmpty(newToken);
			if (flag)
			{
				bool isValidToken = Discord.ValidateToken(newToken);
				bool flag2 = isValidToken;
				if (flag2)
				{
					this.guna2ComboBox1.Items.Add(newToken);
					this.guna2TextBox1.Text = string.Empty;
					MessageBox.Show("Token Successfully Added!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
				else
				{
					MessageBox.Show("Invalid Token Entered!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
			else
			{
				MessageBox.Show("Please Enter a Valid Token!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.selectedToken = this.guna2ComboBox1.SelectedItem.ToString();
		}
		private void guna2Button2_Click(object sender, EventArgs e)
		{
			bool flag = string.IsNullOrEmpty(this.selectedToken);
			if (flag)
			{
				MessageBox.Show("Please select a token!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			else
			{
				this.SendMessageToServer(this.selectedToken, this.channelId, this.message);
			}
		}
		private void guna2TextBox2_TextChanged(object sender, EventArgs e)
		{
			this.channelId = this.guna2TextBox2.Text.Trim();
		}
        private void guna2Button3_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(selectedToken))
			{
				MessageBox.Show("Please select a token!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			int seconds = (int)guna2NumericUpDown1.Value;
            int minutes = (int)guna2NumericUpDown2.Value;
            int hours = (int)guna2NumericUpDown3.Value;
            int days = (int)guna2NumericUpDown4.Value;
            TimeSpan selectedTimeSpan = new TimeSpan(days, hours, minutes, seconds);
            Timer timer = new Timer();
            timer.Interval = (int)selectedTimeSpan.TotalMilliseconds;
            timer.Tick += (s, _) =>
            {
                SendMessageToServer(selectedToken, channelId, message);

                // Zamanlayıcıyı durdur
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }
        private void label4_Click(object sender, EventArgs e)
		{
		}
		private void guna2TextBox3_TextChanged(object sender, EventArgs e)
		{
			this.message = this.guna2TextBox3.Text.Trim();
		}
		private void label3_Click(object sender, EventArgs e)
		{
		}
        private void guna2Button2_Click_1(object sender, EventArgs e)
        {
                Application.Exit();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            SendMessageToServer(selectedToken, channelId, message);
            timer.Stop();
            timer.Dispose();
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
			Hide();
        }
    }
}
