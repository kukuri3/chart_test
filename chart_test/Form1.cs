﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
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
            if (checkBox1.Checked)
            {
                // Set custom chart area position
                chart2.ChartAreas["Default"].Position = new ElementPosition(25, 10, 68, 85);
                chart2.ChartAreas["Default"].InnerPlotPosition = new ElementPosition(10, 0, 90, 90);

                // Create extra Y axis for second and third series
                CreateYAxis(chart2, chart2.ChartAreas["Default"], chart2.Series["Series2"], 13, 8);
                CreateYAxis(chart2, chart2.ChartAreas["Default"], chart2.Series["Series3"], 22, 8);
            }
            else
            {
                // Set default chart areas
                chart2.Series["Series2"].ChartArea = "Default";
                chart2.Series["Series3"].ChartArea = "Default";

                // Remove newly created series and chart areas
                while (chart2.Series.Count > 3)
                {
                    chart2.Series.RemoveAt(3);
                }
                while (chart2.ChartAreas.Count > 1)
                {
                    chart2.ChartAreas.RemoveAt(1);
                }

                // Set default chart are position to Auto
                chart2.ChartAreas["Default"].Position.Auto = true;
                chart2.ChartAreas["Default"].InnerPlotPosition.Auto = true;

            }
        }

        public void CreateYAxis(Chart chart, ChartArea area, Series series, float axisOffset, float labelsSize)
        {
            // オリジナルのseriesのため、新しいチャートエリアを作る
            ChartArea areaSeries = chart.ChartAreas.Add("ChartArea_" + series.Name);
            areaSeries.BackColor = Color.Transparent;
            areaSeries.BorderColor = Color.Transparent;
            areaSeries.Position.FromRectangleF(area.Position.ToRectangleF());
            areaSeries.InnerPlotPosition.FromRectangleF(area.InnerPlotPosition.ToRectangleF());
            areaSeries.AxisX.MajorGrid.Enabled = false;
            areaSeries.AxisX.MajorTickMark.Enabled = false;
            areaSeries.AxisX.LabelStyle.Enabled = false;
            areaSeries.AxisY.MajorGrid.Enabled = false;
            areaSeries.AxisY.MajorTickMark.Enabled = false;
            areaSeries.AxisY.LabelStyle.Enabled = false;
            areaSeries.AxisY.IsStartedFromZero = area.AxisY.IsStartedFromZero;

             
            series.ChartArea = areaSeries.Name;

            // Create new chart area for axis
            ChartArea areaAxis = chart.ChartAreas.Add("AxisY_" + series.ChartArea);
            areaAxis.BackColor = Color.Transparent;
            areaAxis.BorderColor = Color.Transparent;
            areaAxis.Position.FromRectangleF(chart.ChartAreas[series.ChartArea].Position.ToRectangleF());
            areaAxis.InnerPlotPosition.FromRectangleF(chart.ChartAreas[series.ChartArea].InnerPlotPosition.ToRectangleF());

            // Create a copy of specified series
            Series seriesCopy = chart.Series.Add(series.Name + "_Copy");
            seriesCopy.ChartType = series.ChartType;
            foreach (DataPoint point in series.Points)
            {
                seriesCopy.Points.AddXY(point.XValue, point.YValues[0]);
            }

            // Hide copied series
            seriesCopy.IsVisibleInLegend = false;
            seriesCopy.Color = Color.Transparent;
            seriesCopy.BorderColor = Color.Transparent;
            seriesCopy.ChartArea = areaAxis.Name;

            // Disable drid lines & tickmarks
            areaAxis.AxisX.LineWidth = 0;
            areaAxis.AxisX.MajorGrid.Enabled = false;
            areaAxis.AxisX.MajorTickMark.Enabled = false;
            areaAxis.AxisX.LabelStyle.Enabled = false;
            areaAxis.AxisY.MajorGrid.Enabled = false;
            areaAxis.AxisY.IsStartedFromZero = area.AxisY.IsStartedFromZero;
            areaAxis.AxisY.LabelStyle.Font = area.AxisY.LabelStyle.Font;

            // Adjust area position
            areaAxis.Position.X -= axisOffset;
            areaAxis.InnerPlotPosition.X += labelsSize;

        }
    }
}