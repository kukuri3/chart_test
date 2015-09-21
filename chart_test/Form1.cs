using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
//        List<string> gList = new List<string> { Capacity = 40000000 };
        List<string> gList = new List<string>();
        Queue gQ;
        System.Net.Sockets.UdpClient udpClient;
        int gTimeCount = 0;

        public Form1()
        {
            InitializeComponent();
            gQ = new Queue();
            xInitUdp();
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            //UDP受信のコールバック
            //受信データをgQに入れる

            System.Net.Sockets.UdpClient udp =(System.Net.Sockets.UdpClient)ar.AsyncState;

            //非同期受信を終了する
            System.Net.IPEndPoint remoteEP = null;
            byte[] rcvBytes;
            try
            {
                rcvBytes = udp.EndReceive(ar, ref remoteEP);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Console.WriteLine("受信エラー({0}/{1})",
                    ex.Message, ex.ErrorCode);
                return;
            }
            catch (ObjectDisposedException ex)
            {
                //すでに閉じている時は終了
                Console.WriteLine("Socketは閉じられています。");
                return;
            }

            //データを文字列に変換し、キューに入れる
            string rcvMsg = System.Text.Encoding.UTF8.GetString(rcvBytes);
            rcvMsg.Replace("\n", " ").Replace("\r", "");
            //受信したデータをキューに入れる
            string displayMsg = string.Format("ip,{0},port,{1},msg,{2}", remoteEP.Address, remoteEP.Port, rcvMsg.Replace("\n", " ").Replace("\r", ""));
            //           textBox1.BeginInvoke(
            //               new Action<string>(AppendText), displayMsg);
            gQ.Enqueue(displayMsg);

            //再びデータ受信を開始する
            udp.BeginReceive(ReceiveCallback, udp);
        }
        private void xInitUdp()
        {
            int portno = 4126;
            if (udpClient != null)
            {
                return;
            }
            
            //UdpClientを作成し、指定したポート番号にバインドする
            System.Net.IPEndPoint localEP = new System.Net.IPEndPoint(System.Net.IPAddress.Any, portno);

            udpClient = new System.Net.Sockets.UdpClient(localEP);
            //非同期的なデータ受信を開始する
            udpClient.BeginReceive(ReceiveCallback, udpClient);
            xLog("udp open. port="+portno);

        }


        public string xGetPart(string src, string name)
        {
            //カンマ区切りされたsrcからnameの次の文字列を返す
            //なかったら"null"を返す
            string ret_string = "null";
            string[] part = src.Split(',');
            for (int i = 0; i < part.Length; i++)
            {
                if(part[i].Equals(name)){
                    ret_string = part[i + 1];
                }
            }
            return ret_string;

        }
        private void xWriteFile(String s)
        {
            //ファイルにデータ出力
            DateTime dt = DateTime.Now;
            string logtext = "HEAD," + dt.ToString("yyyyMMddhhmmss") + ",stime," + dt.ToOADate() + "," + s + ",TERM";
            textBox1.AppendText(logtext + "\n");
            System.IO.StreamWriter sw = new System.IO.StreamWriter("data.txt", true, System.Text.Encoding.GetEncoding("shift_jis"));
            sw.WriteLine(logtext);
            sw.Close();
        }
        private void xLog(String s)
        {
            //ログ出力
            DateTime dt = DateTime.Now;
            string logtext = dt.ToString("yyyy/MM/dd hh:mm:ss ") + s;
            textBox1.AppendText(logtext + "\n");
            System.IO.StreamWriter sw = new System.IO.StreamWriter("log.txt", true, System.Text.Encoding.GetEncoding("shift_jis"));
            sw.WriteLine(logtext);
            sw.Close();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            xLoadFile();


        }
        private void xLoadFile()
        {

            //ファイルから読み込んで表示テスト
            var fs = System.IO.File.OpenRead("data.txt");

            System.IO.StreamReader file = new System.IO.StreamReader(fs, System.Text.Encoding.GetEncoding("Shift_JIS"));
            
            string line;

            //後ろから10MBぐらいから読み始める。
            if (fs.Length > 10000000L)
            {
                long pos = fs.Length - 9000000;
                fs.Seek(pos, System.IO.SeekOrigin.Begin);
            }
            //ファイルから読んでキューに入れる
            while ((line = file.ReadLine()) != null)
            {
                gList.Add(line);
            }
            file.Close();
            xLog("count=" + gList.Count);
            xLog("last line=" + line);
            xLog("capacity=" + gList.Capacity);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            xDrawAllChart();
        }

        private void xDrawAllChart()
        {
            xDrawChart("sotominami", chart1);
            xDrawChart("LDK", chart2);
            xDrawChart("1F_HALL", chart3);
            xDrawChart("2F_HALL", chart4);
            chart1.SaveImage("chart1.jpg", ChartImageFormat.Jpeg);
            chart2.SaveImage("chart2.jpg", ChartImageFormat.Jpeg);
            chart3.SaveImage("chart3.jpg", ChartImageFormat.Jpeg);
            chart4.SaveImage("chart4.jpg", ChartImageFormat.Jpeg);
        }
        private void xDrawChart(string name, Chart cht)
        {
            Series s_temp = new Series();
            Series s_dew = new Series();
            Series s_hum = new Series();

            cht.Series.Clear();

            double time = 0;
            for (int i = 0; i < gList.Count; i++)
            {
                string s = (string)gList[i];
                if (s.Contains("HEAD") && s.Contains("TERM"))
                {
                    if (xGetPart(s, "name").Equals(name))
                    {
                        time = double.Parse(xGetPart(s, "stime"));
                        double tm = double.Parse(xGetPart(s, "temp")) / 10;
                        double dp = double.Parse(xGetPart(s, "dew")) / 10;
                        double hum = double.Parse(xGetPart(s, "hum")) / 10;
                        DateTime dt = DateTime.FromOADate(time);

                        s_temp.Points.AddXY(dt, tm);
                        s_dew.Points.AddXY(dt, dp);
                        s_hum.Points.AddXY(dt, hum);

                    }
                }
            }
            s_temp.Name = "temp";
            s_temp.ChartType = SeriesChartType.FastLine;
            s_temp.YAxisType = AxisType.Primary;
            s_temp.Color = Color.DarkRed;

            s_dew.Name = "dew"; 
            s_dew.ChartType = SeriesChartType.FastLine;
            s_dew.YAxisType = AxisType.Primary;
            s_dew.Color = Color.DarkGreen;

            s_hum.Name = "hum";
            s_hum.ChartType = SeriesChartType.FastLine;
            s_hum.YAxisType = AxisType.Secondary;
            s_hum.Color = Color.Blue;

            cht.Series.Add(s_temp);
            cht.Series.Add(s_dew);
            cht.Series.Add(s_hum);


            cht.ChartAreas[0].AxisX.Minimum = (int)time - 1;
            cht.ChartAreas[0].AxisX.Maximum = (int)time + 1;

            xLog("last time=" + DateTime.FromOADate(time).ToString());

            cht.ChartAreas[0].AxisX.Interval = 1 / 24;
            cht.ChartAreas[0].AxisX.LabelStyle.Format = "MM/dd HH:mm";
            cht.ChartAreas[0].AxisY.Minimum = -10;
            cht.ChartAreas[0].AxisY.Maximum = 40;
            cht.ChartAreas[0].AxisY.Interval = 10;
            cht.ChartAreas[0].AxisY2.Minimum = 0;
            cht.ChartAreas[0].AxisY2.Maximum = 100;
            cht.ChartAreas[0].AxisY2.Interval = 20;

        }

        private void xDrawChart2(string[] position,string yousoname, Chart cht)
        {
            int posnum = position.Count();
            double time = 0;
            Series[] ss = new Series[posnum];

            for (int p = 0; p < posnum; p++)
            {
                ss[p] = new Series();


                for (int i = 0; i < gList.Count; i++)
                {
                    string s = (string)gList[i];
                    if (s.Contains("HEAD") && s.Contains("TERM"))
                    {
                        string positionname = xGetPart(s, "name");
                        if (positionname.Equals(position[p]))
                        {
                            time = double.Parse(xGetPart(s, "stime"));
                            
                            double tm = double.Parse(xGetPart(s, yousoname)) / 10;
                            DateTime dt = DateTime.FromOADate(time);
                            ss[p].Points.AddXY(dt, tm);

                        }
                    }
                }
                ss[p].Name = position[p];
                ss[p].ChartType = SeriesChartType.FastLine;
                ss[p].YAxisType = AxisType.Primary;
                
                cht.Series.Add(ss[p]);

            }
            cht.ChartAreas[0].AxisX.Minimum = (int)time - 1;
            cht.ChartAreas[0].AxisX.Maximum = (int)time + 1;

            cht.ChartAreas[0].AxisX.Interval = 1 / 24;
            cht.ChartAreas[0].AxisX.LabelStyle.Format = "MM/dd HH:mm";
            cht.ChartAreas[0].AxisY.Minimum = 10;
            cht.ChartAreas[0].AxisY.Maximum = 40;
            cht.ChartAreas[0].AxisY.Interval = 10;
//            cht.ChartAreas[0].AxisY2.Minimum = 0;
//            cht.ChartAreas[0].AxisY2.Maximum = 100;
//            cht.ChartAreas[0].AxisY2.Interval = 20;

        }

        private void button5_Click(object sender, EventArgs e)
        {
            xDrawChart2(new string[]{"sotominami","1F_HALL","2F_HALL","josituki","LDK"},"temp",chart2);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //タイマイベント1sec
            //キューからファイルに書き込む
            while (gQ.Count > 0)
            {
                string text;
                text = (string)gQ.Dequeue();    //キューから取り出す

                xWriteFile(text);

                // カンマ区切りで分割して配列に格納する
                string[] stArrayData = text.Split(',');

                // データを確認する
                int i = 0;
                foreach (string stData in stArrayData)
                {
                    xLog(i.ToString()+","+stData);
                    i++;
                }
            }

            //時々グラフを更新する
            gTimeCount++;
            if (gTimeCount >= 100)
            {
                xDrawAllChart();
            }
        }

    }
}
