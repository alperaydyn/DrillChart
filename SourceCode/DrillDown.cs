using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace DrillChart
{
    class DrillChart
    {
        private DataSet ds;

        public _Properties Properties;
        public class _Properties
        {
            public int NumFields { get; set; }
            public int MinimumDepth { get; set; }
            public int MaximumDepth { get; set; }
            public int SelectedCol { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }
        public _Error Error;
        public class _Error
        {
            public List<string> Message = new List<string>();
            public string Get()
            {
                return this.Message==null?"":string.Join(";", this.Message);
            }
        }

        public DrillChart(string filename, int colnum, int minimum, int maximum, int width, int height)
        {
            Properties = new _Properties();
            Error = new _Error();
            Properties.MinimumDepth = minimum;
            Properties.MaximumDepth = maximum;
            Properties.SelectedCol = colnum;
            Properties.Width = width;
            Properties.Height = height;
            try
            {
                ds = CreateDataTables(filename);
            }
            catch (Exception ex)
            {
                Error.Message.Add(ex.Message.ToString());
            }
        }

        public Bitmap ShowResult()
        {
            Bitmap bmp = new Bitmap(0, 0);
            return bmp;
        }

        private DataSet CreateDataTables(string filename)
        {
            int k = 19; /* data start row number */
            DataTable dt1 = new DataTable();
            DataTable dt2 = new DataTable();

            using (StreamReader sr = new StreamReader(filename))
            {
                string line = "";

                dt1.Columns.Add("Property");
                // read until the data header row (k) */
                for (int i = 1; i < k; i++)
                {
                    line = sr.ReadLine();
                    dt1.Rows.Add(line);
                };

                // nex line is the k th line which is the header row */
                line = sr.ReadLine();
                // split header row with tab and create columns for datatable #2
                foreach (string fieldName in line.Split('\t'))
                {
                    dt2.Columns.Add(fieldName, typeof(Double));
                }
                this.Properties.NumFields = dt2.Columns.Count - 1;

                dt2.Rows.Clear();
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    int r = 0;
                    //Double[] farray = line.Split('\t').Select(s => Double.Parse(s.Replace(',','.'),CultureInfo.CurrentCulture)).ToArray();
                    DataRow row = dt2.NewRow();
                    foreach (string val in line.Split('\t'))
                    {
                        row[r] = val.Replace('.', '.');
                        r++;
                    }
                    dt2.Rows.Add(row);
                }
            }

            DataView dv = new DataView(dt2);
            if (Properties.MinimumDepth != -1 && Properties.MaximumDepth != -1)
            {
                dv.RowFilter = dt2.Columns[0].ColumnName + ">=" + Properties.MinimumDepth.ToString() + " and " + dt2.Columns[0].ColumnName + "<=" + Properties.MaximumDepth.ToString();
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(dt1);
            ds.Tables.Add(dv.ToTable());
            return ds;
        }

        public Chart CreateChart()
        {
            DataTable dt = ds.Tables[1];
            Chart chart = new Chart();
            chart.DataSource = dt;

            chart.Width = Properties.Width-30;
            chart.Height = Properties.Height-30;

            ChartArea ca = new ChartArea();
            chart.ChartAreas.Add(ca);

            Series s = new Series();
            s.ChartType = SeriesChartType.Line;
            s.Name = dt.Columns[0].ColumnName;
            s.XValueMember = dt.Columns[0].ColumnName;
            s.YValueMembers = dt.Columns[Properties.SelectedCol].ColumnName;
            chart.Series.Add(s);

            return chart;
        }

        public Bitmap CreateImage()
        {
            Chart chart = CreateChart();
            MemoryStream ms = new MemoryStream();
            //rotate olunca width-height yer degistiriyor;
            chart.Width = Properties.Height;
            chart.Height = Properties.Width;
            chart.SaveImage(ms, ChartImageFormat.Png);
            Bitmap bmp = new Bitmap(ms);
            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
            return bmp;
        }
    }
}
