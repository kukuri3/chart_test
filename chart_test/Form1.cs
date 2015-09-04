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
using System.Collections.Generic;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
//        List<string> gList = new List<string> { Capacity = 40000000 };
        List<string> gList = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Seriesの作成
            Series test = new Series();
            Series test2 = new Series();
            Series seri1 = new Series();
            Series seri2 = new Series();
            Series seri3 = new Series();

            //グラフのタイプを指定
            test.ChartType = SeriesChartType.StepLine;
 
 
            //グラフのデータを追加
            for (int i = 1; i < 24; i++)
            {
                DateTime dt;
                dt=new DateTime(2000, 8, 1, i, 30, 0);
                test.Points.AddXY(dt, Math.Sin(i * Math.PI / 180.0));
                seri1.Points.AddXY(dt, Math.Sin(i * Math.PI / 180.0));
                seri2.Points.AddXY(dt, Math.Cos(i * Math.PI / 180.0));
                seri3.Points.AddXY(dt, Math.Sinh(i * Math.PI / 180.0));

            }

            //作ったSeriesをchartコントロールに追加する
            chart1.Series.Add(test);
            chart2.Series.Add(seri1);
            chart2.Series.Add(seri2);
            chart2.Series.Add(seri3);
            chart2.Series[1].ChartArea = "Default";
            chart2.Series[2].ChartArea = "Default";


            //chart2.Series[0].Name = "t";
            //CreateYAxis(chart2, chart2.ChartAreas[0], chart2.Series[1], 13, 8);
            //CreateYAxis(chart2, chart2.ChartAreas[0], chart2.Series[2], 22, 8);


            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //軸ラベルの設定
            chart1.ChartAreas[0].AxisX.Title = "angle(rad)";
            chart1.ChartAreas[0].AxisY.Title = "sin";
            //※Axis.TitleFontでフォントも指定できるがこれはデザイナで変更したほうが楽

            //X軸最小値、最大値、目盛間隔の設定
            DateTime dt1 = new DateTime(2000, 8, 1, 13, 30, 0);
            DateTime dt2 = new DateTime(2000, 8, 31, 13, 30, 0);

            chart1.ChartAreas[0].AxisX.Minimum = dt1.ToOADate();
            chart1.ChartAreas[0].AxisX.Maximum = dt2.ToOADate();
            chart1.ChartAreas[0].AxisX.Interval = 60;

            //Y軸最小値、最大値、目盛間隔の設定
            chart1.ChartAreas[0].AxisY.Minimum = -1;
            chart1.ChartAreas[0].AxisY.Maximum = 1;
            chart1.ChartAreas[0].AxisY.Interval = 0.2;

            //目盛線の消去
            //chart1.ChartAreas["area1"].AxisX.MajorGrid.Enabled = false;
            //chart1.ChartAreas["area1"].AxisY.MajorGrid.Enabled = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

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
        private void xLog(String s)
        {
            //ログ出力
            DateTime dt = DateTime.Now;
            string logtext = "HEAD," + dt.ToString("yyyyMMddhhmmss") + ",stime," + dt.ToOADate() + "," + s + ",TERM";
            textBox1.AppendText(logtext + "\n");
            System.IO.StreamWriter sw = new System.IO.StreamWriter("log.txt", true, System.Text.Encoding.GetEncoding("shift_jis"));
            sw.WriteLine(logtext);
            sw.Close();
        }
        private void button3_Click(object sender, EventArgs e)
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
            xDrawChart("sotominami", chart1);
            xDrawChart("LDK", chart2);
            xDrawChart("1F_HALL", chart3);
            xDrawChart("2F_HALL", chart4);

        }

        private void xDrawChart(string name, Chart cht)
        {
            Series s_temp = new Series();
            Series s_dew = new Series();
            Series s_hum = new Series();
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

    }
}
