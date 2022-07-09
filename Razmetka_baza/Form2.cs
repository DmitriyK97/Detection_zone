using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Razmetka_baza
{
    public partial class Form2 : Form
    {
        //количество серверов с камерами
        private readonly List<Server_Kol> ser_kol = new List<Server_Kol>();

        // сохранение всех данных по камерам
        private readonly Server_JSON servers = new Server_JSON();

        public Form2()
        {
            InitializeComponent();
            var URLdatabase = "http://192.168.88.69:1010/server";
            var serializer = new JsonSerializer();
            using (var webClient = new WebClient {Encoding = Encoding.UTF8})
            {
                var json = webClient.DownloadString(URLdatabase);
                var abc = JObject.Parse(json);
                servers = JsonConvert.DeserializeObject<Server_JSON>(json);
            }

            for (var i = 0; i < servers.data.Count; i++)
            {
                var kolichestvo = 0;
                for (var j = 0; j < servers.data[i].pdks.Count; j++)
                    kolichestvo = kolichestvo + servers.data[i].pdks[j].cams.Count;
                ser_kol.Add(new Server_Kol {server = servers.data[i].name, kol = kolichestvo});
            }

            for (var i = 0; i < ser_kol.Count; i++) comboBox1.Items.Add(ser_kol[i].server);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "")
            {
                /* string url = "http://192.168.88.69:1010/cam";
                 using (var webClient = new WebClient())
                 {
                     cams izmenenie = new cams
                     {
                         id = servers.data[i_poisk].pdks[j_poisk].cams[k_poisk].id,
                         layout = richTextBox1.Text,
                         lines = razmetka.Count / 2 - 1,
                         pdk_id = servers.data[i_poisk].pdks[j_poisk].cams[k_poisk].pdk_id
                     };
                     string json = System.Text.Json.JsonSerializer.Serialize<cams>(izmenenie);
                     Console.WriteLine(json);
                     webClient.UploadString(url, json);
                 }
                 Form2.*/
            }
            else
            {
                MessageBox.Show("Не все параметры заполнены", "Ошибка!");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label8.Text = ser_kol[comboBox1.SelectedIndex].kol.ToString();
        }

        private class Server_Kol
        {
            public string server { get; set; }
            public int kol { get; set; }
        }

        private class Server_JSON
        {
            public List<Server> data { get; set; }
            public string ok { get; set; }
        }

        private class Server
        {
            public int id { get; set; }
            public string name { get; set; }
            public List<PDKs> pdks { get; set; }
        }

        private class PDKs
        {
            public int id { get; set; }
            public string name { get; set; }
            public List<cams> cams { get; set; }
            public int server_id { get; set; }
        }

        private class cams
        {
            public int id { get; set; }
            public string route { get; set; }
            public string url { get; set; }
            public string layout { get; set; }
            public int bus { get; set; }
            public int truck { get; set; }
            public int lines { get; set; }
            public int pdk_id { get; set; }
        }
    }
}