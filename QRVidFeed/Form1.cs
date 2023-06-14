using System;
using System.Drawing;
using System.Windows.Forms;
using TouchlessLib;
using System.Diagnostics;
using ZXing;
using System.Data.SqlClient;

namespace QRVidFeed
{
    public partial class Form1 : Form
    {
        SqlConnection con = new SqlConnection("data source = localhost; database = BevPizza ; integrated security = true;");
        TouchlessMgr touch = new TouchlessMgr();
        BarcodeReader barcodeReader = new BarcodeReader();
        Result result;

        public Form1()
        {
            InitializeComponent();
        }

        private void initializeCam()
        {
            touch.CurrentCamera = touch.Cameras[0];
            touch.CurrentCamera.CaptureWidth = 500; // Set width
            touch.CurrentCamera.CaptureWidth = 500; // Set height
            touch.CurrentCamera.OnImageCaptured += new EventHandler<CameraEventArgs>(OnImageCaptured); // Set preview callback function
        }

        private void OnImageCaptured(object sender, CameraEventArgs args)
        {
            // Get the bitmap
            Bitmap bitmap = args.Image;

            // Read barcode and show results in UI thread
            this.Invoke((MethodInvoker)delegate
            {
                pictureBox1.Image = bitmap;
                ReadBarcode(bitmap);
            });
        }

        private void ReadBarcode(Bitmap bitmap)
        {
            // Read barcodes with Dynamsoft Barcode Reader
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();
            result = barcodeReader.Decode(bitmap);
            sw.Stop();
            Console.WriteLine(sw.Elapsed.TotalMilliseconds + "ms");

            // Clear previous results
            //textBox1.Clear();

            if (result == null)
            {
                textBox1.Text = "No barcode detected!";
                return;
            }

            textBox1.Text = "scanned";

            con.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM order_archive WHERE qr_code=@qrBin", con);
            cmd.Parameters.AddWithValue("@qrBin", result.ToString());
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string value = reader["qr_code"].ToString();
                    if (value.Equals(result.ToString()))
                    {
                        MessageBox.Show("served!");
                        //dto ka magdagdag ng info na ipprint
                        return;
                    }
                }
            }
            con.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            initializeCam();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.ExitThread();
        }
    }
}
