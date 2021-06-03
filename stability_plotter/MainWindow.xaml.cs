using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.IO;
using ScottPlot;
using System.Text.RegularExpressions;


namespace stability_plotter
{

    public class Report
    {

        public List<float> time = new List<float>();
        public List<float> voc = new List<float>();
        public List<float> jsc = new List<float>();
        public List<float> ff = new List<float>();
        public List<float> pce = new List<float>();

        public string filename_path;
        public int cell_id;

    }





    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();


            var file_dict = new Dictionary<String, Report>();



            vocPlot.plt.YLabel("Voc (V)", fontSize: 28);
            jscPlot.plt.YLabel("Jsc (mA/cm²)", fontSize: 28);
            ffPlot.plt.YLabel("Fill Factor", fontSize: 28);
            pcePlot.plt.YLabel("PCE (%)", fontSize: 28);
            pcePlot.plt.XLabel("Time (h)", fontSize: 28);

            vocPlot.plt.Ticks(fontSize: 22);
            jscPlot.plt.Ticks(fontSize: 22);
            ffPlot.plt.Ticks(fontSize: 22);
            pcePlot.plt.Ticks(fontSize: 22);



        }

        void render_selected_data(string file)
        {
            float irradience = float.Parse(irradienceText.Text);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (string.IsNullOrEmpty(comboBox1.Text))
            {
                return;
            }

            using (TextFieldParser parser = new TextFieldParser(file))
            {
                Report report1 = new Report();



                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(", ");
                bool firstLine = true;



                while (!parser.EndOfData)
                {
                    //Process row
                    string[] fields = parser.ReadFields();

                    // Skip the header line
                    if (firstLine)
                    {
                        firstLine = false;

                        continue;
                    }

                    foreach (string field in fields)
                    {

                        //Debug.Print("New field: {0}", field);
                        string str = field.Substring(0, field.Length);

                        string[] parts = str.Split('\t');
                        String time = parts[0];
                        time = time.Substring(time.IndexOf('_') + 1);
                        time = time.Substring(0, time.LastIndexOf("_") + 1);
                        time = time.Remove(time.Length - 1);


                        float jsc = Math.Abs(float.Parse(parts[1]));
                        float voc = float.Parse(parts[2]);
                        float pce = Math.Abs(float.Parse(parts[3])) / irradience;
                        float ff = float.Parse(parts[4]);

                        if (voc < 0 || ff < 0.05 || jsc < 0.01)
                        {
                            Debug.Print("Some data out of range. Trimming...");
                            break;
                        }

                        report1.time.Add(float.Parse(time));
                        report1.jsc.Add(jsc);
                        report1.voc.Add(voc);
                        report1.pce.Add(pce);
                        report1.ff.Add(ff);

                    }
                }


                double[] time_array_d = new double[report1.time.Count];
                double[] voc_array_d = new double[report1.voc.Count];
                double[] jsc_array_d = new double[report1.jsc.Count];
                double[] ff_array_d = new double[report1.ff.Count];
                double[] pce_array_d = new double[report1.pce.Count];

                for (int i = 0; i < report1.time.Count; i++)
                {
                    time_array_d[i] = (double)report1.time[i];
                    voc_array_d[i] = (double)report1.voc[i];
                    jsc_array_d[i] = (double)report1.jsc[i];
                    ff_array_d[i] = (double)report1.ff[i];
                    pce_array_d[i] = (double)report1.pce[i];
                }

                vocPlot.plt.Clear();
                jscPlot.plt.Clear();
                ffPlot.plt.Clear();
                pcePlot.plt.Clear();




                try
                {
                    vocPlot.plt.PlotScatter(time_array_d, voc_array_d, color: System.Drawing.Color.Red, lineWidth: 4);
                    jscPlot.plt.PlotScatter(time_array_d, jsc_array_d, color: System.Drawing.Color.Red, lineWidth: 4);
                    ffPlot.plt.PlotScatter(time_array_d, ff_array_d, color: System.Drawing.Color.Red, lineWidth: 4);
                    pcePlot.plt.PlotScatter(time_array_d, pce_array_d, color: System.Drawing.Color.Red, lineWidth: 4);

                    vocPlot.Render(skipIfCurrentlyRendering: true);
                    jscPlot.Render(skipIfCurrentlyRendering: true);
                    ffPlot.Render(skipIfCurrentlyRendering: true);
                    pcePlot.Render(skipIfCurrentlyRendering: true);
                }
                catch
                {
                    Debug.Print("Rendering failed. Correct file format?");
                }



            }

            stopwatch.Stop();
            long elapsed_time = stopwatch.ElapsedMilliseconds;

            Debug.Print("Parsing time: {0} ms", elapsed_time);
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.Print("Combobox selection changed: {0}", comboBox1.Text);
            string file = saveDirectoryText.Text + "\\" + comboBox1.Text + ".txt";

            render_selected_data(file);
        }

        private void saveDirectory_Click(object sender, RoutedEventArgs e)
        {
            // Create a "Save As" dialog for selecting a directory (HACK)
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.InitialDirectory = saveDirectoryText.Text; // Use current value for initial dir
            dialog.Title = "Select a Directory"; // instead of default "Save As"
            dialog.Filter = "Directory|*.this.directory"; // Prevents displaying files
            dialog.FileName = "select"; // Filename will then be "select.this.directory"
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                // Remove fake filename from resulting path
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                // If user has changed the filename, create the new directory
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                // Our final value is in path
                saveDirectoryText.Text = path;
            }

            dynamic dir = saveDirectoryText.Text;

            string[] data_files = System.IO.Directory.GetFiles(dir, "*.txt");

            // Sort list of files numerically instead of alphabetically
            NumericalSort(data_files);

            foreach (string file in data_files)
            {
                comboBox1.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
            }
        }

        private void comboBox1_DropDownClosed(object sender, EventArgs e)
        {
            Debug.Print("Combobox selection changed: {0}", comboBox1.Text);
            string file = saveDirectoryText.Text + "\\" + comboBox1.Text + ".txt";
            render_selected_data(file);
        }

        private void parse_btn_Click(object sender, RoutedEventArgs e)
        {
            parserStatus.Text = "Converting files...";

            string[] files = Directory.GetFiles(@saveDirectoryText.Text, "*.txt", System.IO.SearchOption.TopDirectoryOnly);

            float irradience = float.Parse(irradienceText.Text);

            foreach (string device in files)
            {
                using (TextFieldParser parser = new TextFieldParser(device))
                {
                    Report report1 = new Report();

                    string cell_id = device.Substring(0, device.LastIndexOf("_") + 1); // Remote the "_report.txt" part
                    cell_id = cell_id.Remove(cell_id.Length - 1);
                    cell_id = cell_id.Substring(cell_id.LastIndexOf("\\") + 1); // Remove the path directory

                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    bool firstLine = true;

                    while (!parser.EndOfData)
                    {
                        //Process row
                        string[] fields = parser.ReadFields();

                        // Skip the header line
                        if (firstLine)
                        {
                            firstLine = false;
                            continue;
                        }

                        foreach (string field in fields)
                        {
                            try
                            {
                                string str = field.Substring(0, field.Length);
                                string[] parts = str.Split('\t');
                                String time = parts[0];


                                // Original formatting is COMx_HH.HHH.dat.
                                // Change to pure HH.HHH
                                time = time.Substring(time.IndexOf('_') + 1);
                                time = time.Substring(0, time.LastIndexOf("_") + 1);
                                time = time.Remove(time.Length - 1);


                                // Return absolute values of Jsc and PCE
                                float jsc = Math.Abs(float.Parse(parts[1]));
                                float voc = float.Parse(parts[2]);
                                float pce = Math.Abs(float.Parse(parts[3])) / irradience;
                                float ff = float.Parse(parts[4]);

                                if (voc < 0 || ff < 0.05 || jsc < 0.01)
                                {
                                    break;
                                }

                                report1.time.Add(float.Parse(time));
                                report1.jsc.Add(jsc);
                                report1.voc.Add(voc);
                                report1.pce.Add(pce);
                                report1.ff.Add(ff);
                            }
                            catch
                            {

                            }


                        }
                    }

                    String parsed_dir = saveDirectoryText.Text + "\\" + "parsed";
                    DirectoryInfo di = Directory.CreateDirectory(parsed_dir);

                    String dataout = parsed_dir + "\\";
                    dataout = dataout + cell_id;
                    dataout = dataout + ".txt";

                    using (StreamWriter file = new StreamWriter(dataout))
                    {

                        file.WriteLine("Time (h), JSC (mA/cm2), VOC (V), PCE (%), FF");


                        for (int i = 0; i < report1.time.Count; i++)
                        {
                            file.Write(report1.time[i] + ",");
                            file.Write(report1.jsc[i] + ",");
                            file.Write(report1.voc[i] + ",");
                            file.Write(report1.pce[i] + ",");
                            file.WriteLine(report1.ff[i]);

                        }

                    }


                }
            }
            parserStatus.Text = "Conversion complete";


        }

        private void irradienceText_SelectionChanged(object sender, RoutedEventArgs e)
        {

            Debug.Print("Irradience changed: {0}", irradienceText.Text);
            string file = saveDirectoryText.Text + "\\" + comboBox1.Text + ".txt";

            render_selected_data(file);
        }

        public static void NumericalSort(string[] ar)
        {
            Regex rgx = new Regex("([^0-9]*)([0-9]+)");
            Array.Sort(ar, (a, b) =>
            {
                var ma = rgx.Matches(a);
                var mb = rgx.Matches(b);
                for (int i = 0; i < ma.Count; ++i)
                {
                    int ret = ma[i].Groups[1].Value.CompareTo(mb[i].Groups[1].Value);
                    if (ret != 0)
                        return ret;

                    ret = int.Parse(ma[i].Groups[2].Value) - int.Parse(mb[i].Groups[2].Value);
                    if (ret != 0)
                        return ret;
                }

                return 0;
            });
        }
    }

}





