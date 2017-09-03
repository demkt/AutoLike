using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoLike.TwitterSys;
using CoreTweet;
using Codeplex.Data;

namespace AutoLike
{
    public partial class Form1 : Form
    {
        public class User
        {
            public List<string> UserId { get; set; } = new List<string>();
        }

        static readonly string ck = "ON3QHIXfIOSqClghKfQvDVEMu";
        static readonly string cs = "mE0l0K7yGTRJdqh7jQxt41TUpB075mkcvPrdls736c9gqw9t6A";
        private Certif Api { get; set; }

        public Form1()
        {
            InitializeComponent();
            this.Api = new Certif(ck, cs);
            this.InitAddItems();
            this.GatherTweetFavo();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string pinData = this.textBox1.Text;

            if (System.Text.RegularExpressions.Regex.IsMatch(pinData, @"\d{7}"))
            {
                try
                {
                    this.Api.Token = OAuth.GetTokens(this.Api.Session, pinData);
                    this.label1.Text = "認証完了しました。";
                    (sender as Button).Enabled = false;
                    this.textBox1.Enabled = false;
                    this.Api.TokenFlag = true;
                }
                catch (TwitterException)
                {
                    this.label1.Text = "認証失敗しました。";
                    this.Api = new Certif(ck, cs);
                }
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            User readData;
            string data = this.textBox2.Text;

            if (System.Text.RegularExpressions.Regex.IsMatch(data, @"^[a-zA-Z0-9_]+$"))
            {
                using (var sr = new StreamReader("../../UserId.json"))
                {
                    readData = (User)DynamicJson.Parse(await sr.ReadToEndAsync());
                }
                using (var sw = new StreamWriter("../../UserId.json", false))
                {
                    if (!readData.UserId.Contains(data))
                    {
                        readData.UserId.Add(data);
                        this.listBox1.Items.Add(data);
                        string writeData = DynamicJson.Serialize(readData);
                        await sw.WriteLineAsync(writeData);
                    }
                    else
                    {
                        MessageBox.Show("既に追加されているIDです");
                    }
                }
            }
            else
            {
                MessageBox.Show("正しいIDではありません");
            }
            this.textBox2.Text = "";
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            User readData;
            string data = this.textBox2.Text;

            if (System.Text.RegularExpressions.Regex.IsMatch(data, @"^[a-zA-Z0-9_]+$"))
            {
                using (var sr = new StreamReader("../../UserId.json"))
                {
                    readData = (User)DynamicJson.Parse(await sr.ReadToEndAsync());
                }
                if (readData.UserId.Contains(data))
                {
                    using (var sw = new StreamWriter("../../UserId.json", false))
                    {
                        readData.UserId.Remove(data);
                        this.listBox1.Items.Remove(data);
                        string writeData = DynamicJson.Serialize(readData);
                        await sw.WriteLineAsync(writeData);
                    }
                }
                else
                {
                    MessageBox.Show("入力されたIDは追加されていません");
                }
            }
            else
            {
                MessageBox.Show("正しいIDではありません");
            }
            this.textBox2.Text = "";
        }

        private void InitAddItems()
        {
            User readData;
            using (var sr = new StreamReader("../../UserId.json"))
            {
                readData = (User)DynamicJson.Parse(sr.ReadToEnd());
                foreach (var id in readData.UserId)
                {
                    this.listBox1.Items.Add(id);
                }
            }
        }

        private void GatherTweetFavo()
        {
            Task.Run(() =>
            {
                ListBox.ObjectCollection idList;
                Action _GatherTweetFavo = null;
                _GatherTweetFavo = () =>
                {
                    User readData;
                    string userId = null;

                    if (this.Api.TokenFlag == true)
                    {
                        try
                        {
                            idList = this.listBox1.Items;
                            foreach (string id in idList)
                            {
                                userId = id;
                                this.Api.TweetFavo(3, id);
                            }
                        }
                        catch(TwitterException e)
                        {
                            if (e.Status == (System.Net.HttpStatusCode)429)
                            {
                                MessageBox.Show("API制限にかかりました");
                                Application.Exit();
                            }
                            else if (e.Status == System.Net.HttpStatusCode.NotFound)
                            {
                                using (var sr = new StreamReader("../../UserId.json"))
                                {
                                    readData = (User)DynamicJson.Parse(sr.ReadToEnd());
                                }
                                using (var sw = new StreamWriter("../../UserId.json", false))
                                {
                                    readData.UserId.Remove(userId);
                                    this.listBox1.Items.Remove(userId);
                                    string writeData = DynamicJson.Serialize(readData);
                                    sw.WriteLine(writeData);
                                }
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            _GatherTweetFavo();
                        }
                    }
                };

                while (true)
                {
                    _GatherTweetFavo();
                }
            });
        }
    }
}
