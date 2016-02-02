using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.UI.DataVisualization.Charting;

namespace IntegrationWebSiteMvc.Models
{
    /// <summary>
    /// http://www.codecapers.com/post/Build-a-Dashboard-With-Microsoft-Chart-Controls.aspx
    /// </summary>
    public class DashboardModel
    {
        //DBAInventoryDataContext db = new DBAInventoryDataContext();
        //List<DBASummary> pieData;
        //List<DBASupportMetric> metrics;

        private int CHART_WIDTH = 300;
        private int CHART_HEIGHT = 300;
        private string POINT_URL_HEAD = @"/Dashboard/Drilldown/";
        private string POINT_URL_TAIL = "?parameterName={0}&parameterValue=";

        public DashboardModel()
        {
            //pieData = db.usp_DBADashboardMetrics().ToList<DBASummary>();
            //metrics = (from x in db.DBASupportMetrics
            //            orderby x.SampleDate ascending
            //            select x).ToList<DBASupportMetric>();
        }

        public List<Chart> Charts
        {
            get
            {
                List<Chart> charts = new List<Chart>();

                charts.Add(BuildChart1());
                charts.Add(BuildChart2());
                charts.Add(BuildChart3());
                charts.Add(BuildChart4());
                charts.Add(BuildChart5());
                charts.Add(BuildChart6());

                return charts;
            }
        }

        internal class RawChartData
        {
            public string Title { get; set; }
            public string[] Data { get; set; }
            public string[] xValues { get; set; }
            public decimal[] yValues { get; set; }
            public string pointUrl { get; set; }
            public string legendUrl { get; set; }
            public System.Web.UI.DataVisualization.Charting.SeriesChartType ChartType { get; set; }
        }
        internal class BoundChartData
        {
            public string Title { get; set; }
            public System.Data.DataSet DataSource { get; set; }
            public string xValueMember { get; set; }
            public string[] yValueMembers { get; set; }
            public string pointUrl { get; set; }
            public string legendUrl { get; set; }
            public System.Web.UI.DataVisualization.Charting.SeriesChartType ChartType { get; set; }
        }

        private Chart BuildChart1()
        {
            /*
            var data = new RawChartData
            {
                ChartType = SeriesChartType.Column,
                Title = "Column",
                Data = new string[] { "A", "B", "C", "D" },
                xValues = new string[] { "A", "B", "C", "D" },
                yValues = new decimal[] { 1, 2, 3, 4 }
            };
            */

            string reportName = "rptDueDateOutcomes";
            var data = new BoundChartData
            {
                ChartType = SeriesChartType.Column,
                Title = "Column",
                xValueMember = "DateLabel",
                yValueMembers = new string[] {"AvgTargetVsAchieved", "CompleteCount"},
                DataSource = GetReportData(reportName),
                pointUrl = POINT_URL_HEAD + string.Format(POINT_URL_TAIL, "Date")
            };

            return BindChartData(data);
        }

        private Chart BuildChart2()
        {
            string reportName = "rptCurrentStatusSummary";
            var data = new BoundChartData
            {
                ChartType = SeriesChartType.Pie,
                Title = "Pie",
                xValueMember = "JobStatus",
                yValueMembers = new string[] {"JobCount"},
                DataSource = GetReportData(reportName),
                pointUrl = POINT_URL_HEAD + string.Format(POINT_URL_TAIL, "JobStatus")
            };

            return BindChartData(data);
        }

        private Chart BuildChart3()
        {
            /*
            var data = new RawChartData
            {
                ChartType = SeriesChartType.Line,
                Title = "Line",
                Data = new string[] { "A", "B", "C", "D" },
                xValues = new string[] { "A", "B", "C", "D" },
                yValues = new decimal[] { 1, 2, 3, 4 }
            };
            */

            string reportName = "rptTimeToCompletion";
            var data = new BoundChartData
            {
                ChartType = SeriesChartType.Line,
                Title = "Line",
                xValueMember = "DateLabel",
                yValueMembers = new string[] {"AvgTimeToCompletion"},
                DataSource = GetReportData(reportName),
                pointUrl = POINT_URL_HEAD + string.Format(POINT_URL_TAIL, "Date")
            };

            return BindChartData(data);
        }

        private Chart BuildChart4()
        {
            /*
            var data = new RawChartData
            {
                ChartType = SeriesChartType.Bar,
                Title = "Bar",
                Data = new string[] { "A", "B", "C", "D" },
                xValues = new string[] { "A", "B", "C", "D" },
                yValues = new decimal[] { 1, 2, 3, 4 }
            };
            */

            string reportName = "rptJobsInJeopardy";
            var data = new BoundChartData
            {
                ChartType = SeriesChartType.Bar,
                Title = "Bar",
                xValueMember = "DateLabel",
                yValueMembers = new string[] {"JeopardyCount"},
                DataSource = GetReportData(reportName),
                pointUrl = POINT_URL_HEAD + string.Format(POINT_URL_TAIL, "Date")
            };

            return BindChartData(data);
        }

        private Chart BuildChart5()
        {
            string reportName = "rptUserWorkload";
            var data = new BoundChartData
            {
                ChartType = SeriesChartType.Doughnut,
                Title = "Doughnut",
                xValueMember = "UserID",
                yValueMembers = new string[] {"JobCount"},
                DataSource = GetReportData(reportName),
                pointUrl = POINT_URL_HEAD + string.Format(POINT_URL_TAIL, "UserID")
            };

            return BindChartData(data);
        }

        private Chart BuildChart6()
        {
            var data = new RawChartData
            {
                ChartType = SeriesChartType.Area,
                Title = "Area",
                Data = new string[] { "A", "B", "C", "D" },
                xValues = new string[] { "A", "B", "C", "D" },
                yValues = new decimal[] { 1, 2, 3, 4 }
            };

            return BindChartData(data);
        }

        private System.Data.DataSet GetReportData(string storedProcedure)
        {
            System.Data.DataSet dsData = null;

            FinalBuild.DataAccess objADO = new FinalBuild.DataAccess();
            dsData = objADO.GetDataSet(storedProcedure, null);

            return dsData;
        }

        private Chart BindChartData(RawChartData data)
        {
            Chart chart = new Chart();
            chart.Width = CHART_WIDTH;
            chart.Height = CHART_HEIGHT;
            chart.Attributes.Add("align", "left");

            chart.Titles.Add(data.Title); // Display a Title  
            chart.ChartAreas.Add(new ChartArea());

            chart.Legends.Add(new Legend("Legend"));
            chart.Legends[0].TableStyle = LegendTableStyle.Auto;
            chart.Legends[0].Docking = Docking.Bottom;

            chart.Series.Add(new Series());
            chart.Series[0].Legend = chart.Legends[0].Name;
            chart.Series[0].ChartType = data.ChartType; // SeriesChartType.Pie, SeriesChartType.Bar
            chart.Series[0].BackGradientStyle = GradientStyle.DiagonalLeft;
            chart.Series[0].BackSecondaryColor = System.Drawing.Color.LightGray;

            switch (data.ChartType)
            {
                case SeriesChartType.Pie:
                    {
                        chart.Series[0]["PieLabelStyle"] = "Inside";
                        chart.Series[0]["PieLabelStyle"] = "Disabled";
                        chart.Series[0]["PieLineColor"] = "Black";
                        chart.Series[0]["PieDrawingStyle"] = "Concave";
                        break;
                    }
            }

            for (int i = 0; i < data.xValues.Length; i++)
            {
                string x = data.xValues[i];
                decimal y = data.yValues[i];
                int ptIdx = chart.Series[0].Points.AddXY(x, y);

                /*
                var c = data.Data[i];
                int dbaId = data.Data[i].ContactID;
                DataPoint pt = chart.Series[0].Points[ptIdx];
                pt.Url = "/Instance/Index/" + dbaId.ToString();
                pt.ToolTip = c.FirstName + " " + c.LastName + ": #VALY";
                pt.LegendText = "#VALX: #VALY";
                pt.LegendUrl = "/Contact/Details/" + dbaId.ToString();
                pt.LegendToolTip = "Click to view " + c.FirstName + "'s contact information...";
                */

                string pointId = data.Data[i];
                DataPoint pt = chart.Series[0].Points[ptIdx];
                pt.Url = data.pointUrl + pointId;
                pt.ToolTip = "Drilldown";
                pt.LegendText = "#VALX: #VALY";
                pt.LegendUrl = data.legendUrl + pointId;
                pt.LegendToolTip = "Click to view " + pointId + "'information...";
            }

            return chart;
        }
        private Chart BindChartData(BoundChartData data)
        {
            Chart chart = new Chart();
            chart.Width = CHART_WIDTH;
            chart.Height = CHART_HEIGHT;
            chart.Attributes.Add("align", "left");

            chart.Titles.Add(data.Title); // Display a Title  
            chart.ChartAreas.Add(new ChartArea());

            chart.Legends.Add(new Legend("Legend"));
            chart.Legends[0].TableStyle = LegendTableStyle.Auto;
            chart.Legends[0].Docking = Docking.Bottom;


            for (int index = 0; index < data.yValueMembers.Length; index++)
            {
                chart.Series.Add(new Series());
                chart.Series[index].XValueMember = data.xValueMember;
                chart.Series[index].YValueMembers = data.yValueMembers[index];
                chart.Series[index].ChartType = data.ChartType; // SeriesChartType.Pie, SeriesChartType.Bar
                chart.Series[index].BackGradientStyle = GradientStyle.DiagonalLeft;
                chart.Series[index].BackSecondaryColor = System.Drawing.Color.LightGray;
            }

            chart.Series[0].Legend = chart.Legends[0].Name;

            switch (data.ChartType)
            {
                case SeriesChartType.Pie:
                    {
                        chart.Series[0]["PieLabelStyle"] = "Inside";
                        chart.Series[0]["PieLabelStyle"] = "Disabled";
                        chart.Series[0]["PieLineColor"] = "Black";
                        chart.Series[0]["PieDrawingStyle"] = "Concave";
                        break;
                    }
            }

            chart.DataSource = data.DataSource;
            chart.DataBind();

            foreach (DataPoint pt in chart.Series[0].Points)
            {
                string pointId = !string.IsNullOrEmpty(pt.AxisLabel) ? pt.AxisLabel : (pt.XValue + "," + pt.YValues);

                pt.Url = data.pointUrl + pointId;
                pt.ToolTip = "Drilldown";
                pt.LegendText = "#VALX: #VALY";
                pt.LegendUrl = data.legendUrl + pointId;
                pt.LegendToolTip = "Click to view " + pointId + "'information...";
            }

            return chart;
        }
    }
}