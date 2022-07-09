using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Razmetka_baza
{
    public partial class Form1 : Form
    {
        //координаты разметки
        private readonly List<Point> razmetka = new List<Point>();

        //количество серверов с камерами
        private readonly List<Server_Kol> ser_kol = new List<Server_Kol>();

        //координаты изображения
        private int cam_coord_x;

        private int cam_coord_y;

        //координаты мыши
        private int coord_x;

        private int coord_y;

        // айди поисковика
        private int i_poisk;
        private int j_poisk;

        private int k_poisk;

        // сохранение всех данных по камерам
        private Server_JSON servers = new Server_JSON();

        //кол-во камер
        private int yacheika;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            obnovlenie_dannbIh();
            zapolnenie_combobox();
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }

        private void obnovlenie_dannbIh()
        {
            
            var URLdatabase = "http://.../server";
            using (var webClient = new WebClient {Encoding = Encoding.UTF8})
            {
                var json = webClient.DownloadString(URLdatabase);
                var abc = JObject.Parse(json);
                servers = JsonConvert.DeserializeObject<Server_JSON>(json);
            }

            yacheika = 0;
            for (var i = 0; i < servers.data.Count; i++)
            for (var j = 0; j < servers.data[i].pdks.Count; j++)
            for (var k = 0; k < servers.data[i].pdks[j].cams.Count; k++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[yacheika].Cells[0].Value = servers.data[i].name;
                dataGridView1.Rows[yacheika].Cells[1].Value = servers.data[i].pdks[j].name;
                dataGridView1.Rows[yacheika].Cells[2].Value = servers.data[i].pdks[j].cams[k].route;
                dataGridView1.Rows[yacheika].Cells[3].Value = servers.data[i].pdks[j].cams[k].url;
                dataGridView1.Rows[yacheika].Cells[4].Value = servers.data[i].pdks[j].cams[k].lines.ToString();
                dataGridView1.Rows[yacheika].Cells[5].Value = servers.data[i].pdks[j].cams[k].bus.ToString();
                dataGridView1.Rows[yacheika].Cells[6].Value = servers.data[i].pdks[j].cams[k].truck.ToString();
                yacheika = yacheika + 1;
            }

            ser_kol.Clear();
            foreach (var t in servers.data)
            {
                var kolichestvo = t.pdks.Aggregate(0, (current, t1) => current + t1.cams.Count);
                ser_kol.Add(new Server_Kol {id_serv = t.id, server = t.name, kol = kolichestvo});
            }
            comboBox4.Items.Clear();
            foreach (var t in ser_kol) comboBox4.Items.Add(t.server);
        }

        private void zapolnenie_combobox()
        {
            for (var i = 0; i < servers.data.Count; i++) comboBox1.Items.Add(servers.data[i].name);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            i_poisk = comboBox1.SelectedIndex;
            if (comboBox1.SelectedIndex >= 0)
            {
                comboBox2.Items.Clear();
                comboBox3.Items.Clear();
                for (var j = 0; j < servers.data[i_poisk].pdks.Count; j++)
                    comboBox2.Items.Add(servers.data[i_poisk].pdks[j].name);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            obnovlenie_dannbIh();
            comboBox1_SelectedIndexChanged(sender, e);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            j_poisk = comboBox2.SelectedIndex;
            comboBox3.Items.Clear();
            try
            {
                for (var k = 0; k < servers.data[i_poisk].pdks[j_poisk].cams.Count; k++)
                    comboBox3.Items.Add(servers.data[i_poisk].pdks[j_poisk].cams[k].route);
            }
            catch
            {
                // ignored
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            razmetka.Clear();
            var camera_url = "http://.../big/" + servers.data[i_poisk].pdks[j_poisk].cams[k_poisk].id;
            using (var client = new WebClient())
            {
                client.DownloadFile(camera_url, AppDomain.CurrentDomain.BaseDirectory + "test.jpg");
            }

            var myImg = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "test.jpg");
            label8.Text = myImg.Width.ToString();
            cam_coord_x = myImg.Width;
            label7.Text = myImg.Height.ToString();
            cam_coord_y = myImg.Height;
            myImg.Dispose();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Load(camera_url);
            Parser_Point(servers.data[i_poisk].pdks[j_poisk].cams[k_poisk].layout);
            label11.Text = (razmetka.Count / 2 - 1).ToString();
            Risovanie(servers.data[i_poisk].pdks[j_poisk].cams[k_poisk].layout);
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            k_poisk = comboBox3.SelectedIndex;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            coord_x = e.X;
            coord_y = e.Y;
            label4.Text = Convert.ToString(coord_x);
            label5.Text = Convert.ToString(coord_y);
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var g = pictureBox1.CreateGraphics();
                var mySolidBrush = new SolidBrush(Color.Red);
                g.FillEllipse(mySolidBrush, coord_x - 2, coord_y - 2, 7, 7);
                razmetka.Add(new Point
                    {X = cam_coord_x * coord_x / pictureBox1.Width, Y = cam_coord_y * coord_y / pictureBox1.Height});
                var stroka = "[";
                for (var i = 0; i < razmetka.Count; i++)
                {
                    if (i != 0) stroka = stroka + ",";
                    stroka = stroka + "[" + razmetka[i].X + "," + razmetka[i].Y + "]";
                }

                stroka = stroka + "]";
                richTextBox1.Text = stroka;
                Risovanie(stroka);
            }

            if (e.Button == MouseButtons.Right && razmetka.Count != 0)
            {
                razmetka.RemoveAt(razmetka.Count - 1);
                var stroka = "[";
                for (var i = 0; i < razmetka.Count; i++)
                {
                    if (i != 0) stroka = stroka + ",";
                    stroka = stroka + "[" + razmetka[i].X + "," + razmetka[i].Y + "]";
                }

                stroka = stroka + "]";
                using (var fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "test.jpg", FileMode.Open))
                {
                    var bmp = new Bitmap(fs);
                    pictureBox1.Image = (Bitmap) bmp.Clone();
                }

                Risovanie(stroka);
            }

            label11.Text = (razmetka.Count / 2 - 1).ToString();
        }

        private void Risovanie(string str)
        {
            Task.Run(() =>
            {
                Thread.Sleep(100);
                pictureBox1.Invoke(new Action(() =>
                {
                    if (razmetka.Count != 0)
                    {
                        var g = pictureBox1.CreateGraphics();
                        var mySolidBrush = new SolidBrush(Color.Red);
                        try
                        {
                            for (var i = 0; i < razmetka.Count; i++)
                                g.FillEllipse(mySolidBrush, pictureBox1.Width * razmetka[i].X / cam_coord_x - 2,
                                    pictureBox1.Height * razmetka[i].Y / cam_coord_y - 2, 7, 7);
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    richTextBox1.Text = str;
                }));
            });
        }

        private void Parser_Point(string str)
        {
            var chisla = new string[]{};
            if (str == "") return;
            try
            {
                chisla = str.Split(',', '[', ']');
            }
            catch
            {
                return;
            }
            richTextBox1.Text = chisla[1];
            for (var i = 0; i < chisla.Length; i++)
                if ((i + 1) % 4 == 0)
                    razmetka.Add(new Point {X = Convert.ToInt32(chisla[i - 1]), Y = Convert.ToInt32(chisla[i])});
            Risovanie(str);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (Proverka_Lines())
            {
                var url = "http://.../cam";
                using (var webClient = new WebClient())
                {
                    webClient.Headers["Content-Type"] = "application/json";
                    var izmenenie = new cams
                    {
                        id = servers.data[i_poisk].pdks[j_poisk].cams[k_poisk].id,
                        route = servers.data[i_poisk].pdks[j_poisk].cams[k_poisk].route,
                        url = servers.data[i_poisk].pdks[j_poisk].cams[k_poisk].url,
                        bus = servers.data[i_poisk].pdks[j_poisk].cams[k_poisk].bus,
                        truck = servers.data[i_poisk].pdks[j_poisk].cams[k_poisk].truck,
                        layout = richTextBox1.Text,
                        lines = razmetka.Count / 2 - 1,
                        pdk_id = servers.data[i_poisk].pdks[j_poisk].cams[k_poisk].pdk_id
                    };
                    var json = System.Text.Json.JsonSerializer.Serialize(izmenenie);
                    Console.WriteLine(json);
                    webClient.UploadString(url, json);
                }
            }
        }

        private bool Proverka_Lines()
        {
            if (razmetka.Count >= 4 && razmetka.Count % 2 == 0)
            {
                servers.data[i_poisk].pdks[j_poisk].cams[k_poisk].lines = razmetka.Count / 2 - 1;
                return true;
            }

            MessageBox.Show("Проблема разметки", "Размечено нечетное кол-во полос");
            return false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var redaktor = new Form2();
            redaktor.Show();
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridView2.Rows.Clear();
            label13.Text = ser_kol[comboBox4.SelectedIndex].kol.ToString();
            for (var i = 0; i < servers.data[comboBox4.SelectedIndex].pdks.Count; i++)
            {
                dataGridView2.Rows.Add();
                dataGridView2.Rows[i].Cells[0].Value = servers.data[comboBox4.SelectedIndex].pdks[i].name;
                dataGridView2.Rows[i].Cells[1].Value =
                    servers.data[comboBox4.SelectedIndex].pdks[i].cams.Count.ToString();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (comboBox4.Text != "" && textBox1.Text != "" && button6.Text == "Добавить")
            {
                var url = "http://.../pdk";
                using (var webClient = new WebClient())
                {
                    var izmenenie = new PDKs
                    {
                        name = textBox1.Text,
                        server_id = ser_kol[comboBox4.SelectedIndex].id_serv
                    };
                    var json = System.Text.Json.JsonSerializer.Serialize(izmenenie);
                    Console.WriteLine(json);
                    webClient.UploadString(url, json);
                }
            }
        }

        private void dataGridView2_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dataGridView3.Rows.Clear();
            var street = dataGridView2.Rows[dataGridView2.CurrentCellAddress.Y].Cells[0].Value.ToString();
            textBox1.Text = street;
            for (var i = 0; i < servers.data.Count; i++)
            for (var j = 0; j < servers.data[i].pdks.Count; j++)
                if (servers.data[i].pdks[j].name == street)
                {
                    for (var k = 0; k < servers.data[i].pdks[j].cams.Count; k++)
                    {
                        dataGridView3.Rows.Add();
                        dataGridView3.Rows[k].Cells[0].Value = servers.data[i].pdks[j].cams[k].route;
                        dataGridView3.Rows[k].Cells[1].Value = servers.data[i].pdks[j].cams[k].url;
                        dataGridView3.Rows[k].Cells[2].Value = servers.data[i].pdks[j].cams[k].layout;
                    }

                    break;
                }
        }


        private void dataGridView3_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var route = dataGridView3.Rows[dataGridView3.CurrentCellAddress.Y].Cells[0].Value.ToString();
            for (var i = 0; i < servers.data.Count; i++)
            for (var j = 0; j < servers.data[i].pdks.Count; j++)
            for (var k = 0; k < servers.data[i].pdks[j].cams.Count; k++)
                if (servers.data[i].pdks[j].cams[k].route == route)
                {
                    textBox2.Text = servers.data[i].pdks[j].cams[k].route;
                    textBox3.Text = servers.data[i].pdks[j].cams[k].url;
                    textBox4.Text = servers.data[i].pdks[j].cams[k].layout;
                    textBox5.Text = servers.data[i].pdks[j].cams[k].lines.ToString();
                    textBox6.Text = servers.data[i].pdks[j].cams[k].truck.ToString();
                    textBox7.Text = servers.data[i].pdks[j].cams[k].bus.ToString();
                    break;
                }
        }

        private class Server_Kol
        {
            public int id_serv { get; set; }
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

        private class Point
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
    }
}