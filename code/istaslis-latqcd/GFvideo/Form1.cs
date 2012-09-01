using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Drawing.Imaging;
using OpenCLNet;
using System.Threading;
using Microsoft.Win32;

namespace GFVideo
{
    public partial class Form1 : Form
    {
        Core MyKernel;

      //  bool IsAutoStart = false;

        public Form1(bool IsAuto)
        {
            InitializeComponent();
            //IsAutoStart = IsAuto;
        }


        double timems = 0; int timetimes = 0;
        double Pcumul = 0,Pgeneral=0, P2cumul=0, P2general;
        double Scumul = 0, S2cumul = 0, Sgeneral = 0, S2general = 0;
        private void UpdateMyKernel()
        {

            MyKernel.Calculate();

             Pcumul += MyKernel.PL[0];
             P2cumul += MyKernel.PL[0] * MyKernel.PL[0];
             Scumul += MyKernel.S[0];
             S2cumul += MyKernel.S[0] * MyKernel.S[0];

             timems += MyKernel.CalculationTimeMS; 
            timetimes++;
        }
        bool working = true; int Scount = 0;
        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FCount = 0;  Pcumul = 0; Scount = 0; Pgeneral = 0; Therm = Convert.ToInt32(tbTherm.Text); Meas = Convert.ToInt32(tbMeas.Text);
            P2cumul = 0; P2general = 0;
            MyKernel.betagauge = (float)Convert.ToDouble(tbBeta.Text);
            MyKernel.flux = (float)Convert.ToDouble(tbflux.Text);
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;

            //ZoomStart = DateTime.Now;
            label6.Text = "Thermalization...";
            for (int i = 0; i < Therm; i++) {  MyKernel.Calculate(); Application.DoEvents(); }

            while (working)
            {
                UpdateMyKernel(); nframes++;
                FCount++;
               
                if (FCount % Meas == 0) {
                    //Thread.Sleep(1000);

                    Pcumul /= Meas;P2cumul/=Meas;
                    Scumul /= Meas; S2cumul /= Meas;
                    Pgeneral = (Scount * Pgeneral + Pcumul) / (Scount + 1);
                    P2general = (Scount * P2general + P2cumul) / (Scount + 1);
                    Sgeneral = (Scount * Sgeneral + Scumul) / (Scount + 1);
                    S2general = (Scount * S2general + S2cumul) / (Scount + 1);

                    Scount++;

                    tbPL.Text += "PL = " + (Pgeneral).ToString() + (char)13 + (char)10;
                    tbS.Text += "S = " + (Sgeneral).ToString() + (char)13 + (char)10;
                    lblNum.Text = Scount.ToString();


                    double chi = Math.Sqrt((P2general - Pgeneral * Pgeneral)/(double)(Meas*Scount));
                    tbPLchi.Text += chi.ToString() + (char)13 + (char)10;//chi in resmid textbox
                    chi = Math.Sqrt((S2general - Sgeneral * Sgeneral) / (double)(Meas * Scount));
                    tbSchi.Text += chi.ToString() + (char)13 + (char)10;//chi in resmid textbox
                    Pcumul = 0; P2cumul = 0; Scumul = 0; S2cumul = 0;

                }
                if ((DateTime.Now - begTime).TotalMilliseconds >= 1000)
                {
                    label6.Text = nframes.ToString()+" fps"; nframes = 0; begTime = DateTime.Now;
                    label10.Text = (timems / (double)timetimes).ToString(); timems = 0; timetimes = 0;

                }
                Application.DoEvents();
            }
            working = true;
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startToolStripMenuItem.Enabled = true;
            stopToolStripMenuItem.Enabled = false;
            
            //ZoomStart = DateTime.Now;
            begTime = DateTime.Now;
            working = false;
        }

        int nframes = 0;
        DateTime begTime = DateTime.Now;

        int FCount = 0;
        int Therm = 1000, Meas = 1000;

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateMyKernel();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DateTime t = DateTime.Now;
            try
            {

                MyKernel = new Core(Convert.ToInt32(tbNx.Text),int.Parse(tbNy.Text),int.Parse(tbNz.Text), Convert.ToInt32(tbNt.Text), Convert.ToDouble(tbBeta.Text), Convert.ToDouble(tbflux.Text));
                MyKernel.betagauge = (float)Convert.ToDouble(tbBeta.Text);

                MessageBox.Show(MyKernel.openCLPlatform.Name + ": " + (DateTime.Now - t).ToString());
                //  UpdateMyKernel();
            }
            catch (Exception oex)
            {
                MessageBox.Show(oex.ToString(), "OpenCL Initialization failed");
                //Application.Exit();
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MyKernel = null;
            GC.Collect();
        }

        private void initFieldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Array.Clear(MyKernel.HostBuf, 0, MyKernel.HostBuf.Length);

            for (int i = 0; i < Core.N * 4; i++)
            {
                MyKernel.HostBuf[i * 9 * 2] = 1f; MyKernel.HostBuf[i * 9 * 2 + 8] = 1f; MyKernel.HostBuf[i * 9 * 2 + 16] = 1f;

            }
            Random RND = new Random();
            for (int i = 0; i < Core.N / 2; i++) { MyKernel.SeedBuf[i] = Math.Abs(RND.Next(1000000)); }
            unsafe
            {
                System.Runtime.InteropServices.Marshal.Copy(MyKernel.HostBuf, 0, MyKernel.ip, MyKernel.HostBuf.Length);
            }
            Core.openCLCQ.EnqueueWriteBuffer(Core.SeedMem, true, 0, MyKernel.SeedBufLen, MyKernel.ipseed);
            Core.openCLCQ.EnqueueWriteBuffer(Core.LinkMem, true, 0, MyKernel.BufferLength, MyKernel.ip);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //float d = Convert.ToSingle(Math.Cos(0.1));//!!!!!!!!!!!!!!!!!
            //float sum = 0;
            double d = Math.Sin(0.1)+0.1;
            double sum = 0;
            Int64 N = 1000000000000;
            for (Int64 i = 0; i < N; i++)
            {
                sum += d;
                //if (i % 1000000 == 0) { chart1.Series[0].Points.AddY(sum/(i+1));  }
            }
            textBox1.Text = (sum / N).ToString();
        }

        private void button5_Click(object sender, EventArgs e)
        {
           // chart1.Series[0].Points.Clear();
           // chart2.Series[0].Points.Clear();

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            working = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //button2_Click(sender, e);
            
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StreamWriter SW = new StreamWriter("HostBuf.txt",false);
            string[] s = Array.ConvertAll(MyKernel.HostBuf,x => x.ToString());
            SW.Write(string.Join("%",s));
            SW.Close();

            SW = new StreamWriter("Results.txt",true);
            SW.Write(tbPL.Text);
            SW.Close();

        }

        private void commandToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void restoreAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StreamReader SR = new StreamReader("HostBuf.txt");
            string s = SR.ReadToEnd();
            string[] strs = s.Split('%');

            MyKernel.HostBuf = Array.ConvertAll(strs, x => float.Parse(x));

            SR.Close();
           // Text = MyKernel.HostBuf[0].ToString();

            unsafe
            {
                System.Runtime.InteropServices.Marshal.Copy(MyKernel.HostBuf, 0, MyKernel.ip, MyKernel.HostBuf.Length);
            }
          //  MyKernel.openCLCQ.EnqueueWriteBuffer(MyKernel.SeedMem, true, 0, MyKernel.SeedBufLen, MyKernel.ipseed);
            Core.openCLCQ.EnqueueWriteBuffer(Core.LinkMem, true, 0, MyKernel.BufferLength, MyKernel.ip);


        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are yue realy want to exit?","Exit program",MessageBoxButtons.YesNo)==DialogResult.Yes)  Application.Exit();
        }

        private void showWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showWindowToolStripMenuItem.Text == "Hide Window") { Hide(); showWindowToolStripMenuItem.Text = "Show Window"; }
            else { Show(); showWindowToolStripMenuItem.Text = "Hide Window"; }
        }

        //private void button6_Click(object sender, EventArgs e)
        //{
        //    Therm = Convert.ToInt32(tbTherm.Text); Meas = Convert.ToInt32(tbMeas.Text);
        //    double beginbeta = Convert.ToDouble(tbBegin.Text);
        //    double endbeta = Convert.ToDouble(tbEnd.Text);
        //    double stepbeta = Convert.ToDouble(tbStep.Text);
        //    double curbeta = beginbeta;
        //    int resnum = Convert.ToInt32(tbRes.Text);


        //    while (curbeta <= endbeta)
        //    {
        //         label10.Text = "beta="+curbeta.ToString();
        //         tbResult.Text += "beta=" + curbeta.ToString() + (char)13 + (char)10 + (char)13 + (char)10;
        //         tbResMid.Text += "beta=" + curbeta.ToString() + (char)13 + (char)10 + (char)13 + (char)10;


        //         Pcumul = 0; Scount = 0; Pgeneral = 0; 
        //         P2cumul = 0; P2general = 0;
        //        MyKernel Kern = new MyKernel(Convert.ToInt32(tbNs.Text), Convert.ToInt32(tbNt.Text),  curbeta,Convert.ToDouble(tbflux.Text));

        //        //ZoomStart = DateTime.Now;
        //        label6.Text = "Thermalization...";
        //        for (int i = 0; i < Therm; i++) { Kern.Calculate(); Application.DoEvents(); }

        //        for (int i = 0; i < resnum; i++)
        //        {
        //            Pcumul = 0; P2cumul = 0;
        //            for (int j = 0; j < Meas; j++)
        //            {

        //                Kern.Calculate();

        //                Pcumul += Kern.PL[0];
        //                P2cumul += Kern.PL[0] * Kern.PL[0];

        //                Application.DoEvents();
        //            }
        //            label6.Text = "result №" + i.ToString();

        //            //Thread.Sleep(1000);

        //            Pcumul /= Meas; P2cumul /= Meas;
        //            Pgeneral = (i * Pgeneral + Pcumul) / (i + 1);
        //            P2general = (i * P2general + P2cumul) / (i + 1);
                    
        //            tbResult.Text += "PL = " + (Pgeneral).ToString() + (char)13 + (char)10;
        //            lblNum.Text = Scount.ToString();


        //            double chi = P2general - Pgeneral * Pgeneral;
        //            tbResMid.Text += chi.ToString() + (char)13 + (char)10;//chi in resmid textbox
        //            Pcumul = 0; P2cumul = 0;
        //        }

        //        Application.DoEvents();
        //        curbeta += stepbeta;
        //    }
        //    working = true;

        //}
        private void button6_Click(object sender, EventArgs e)
        {
            Therm = Convert.ToInt32(tbTherm.Text); Meas = Convert.ToInt32(tbMeas.Text);
            double beginbeta = Convert.ToDouble(tbBegin.Text);
            double endbeta = Convert.ToDouble(tbEnd.Text);
            double stepbeta = Convert.ToDouble(tbStep.Text);
            double curbeta = beginbeta;
            int resnum = Convert.ToInt32(tbRes.Text);



            while (curbeta <= endbeta)
            {
                label10.Text = "beta=" + curbeta.ToString();
                tbB.Text += curbeta.ToString() + (char)13 + (char)10;
                //tbPLchi.Text += "beta=" + curbeta.ToString() + (char)13 + (char)10 + (char)13 + (char)10;


                Pcumul = 0; Scount = 0; Pgeneral = 0;
                P2cumul = 0; P2general = 0;

                double Scumul = 0, S2cumul = 0;
                Core Kern = new Core(Convert.ToInt32(tbNx.Text), int.Parse(tbNy.Text), int.Parse(tbNz.Text), Convert.ToInt32(tbNt.Text), curbeta, Convert.ToDouble(tbflux.Text));

                //ZoomStart = DateTime.Now;
                label6.Text = "Thermalization...";
                for (int i = 0; i < Therm; i++) { Kern.Calculate(); Application.DoEvents(); }

                for (int i = 0; i < resnum; i++)
                {
                    Pcumul = 0; P2cumul = 0; Scumul = 0; S2cumul = 0;
                    if (!working) { working = true; return; }
                    for (int j = 0; j < Meas; j++)
                    {

                        Kern.Calculate();

                        Pcumul += Kern.PL[0];
                        P2cumul += Kern.PL[0] * Kern.PL[0];
                        Scumul += Kern.S[0];
                        S2cumul += Kern.S[0] * Kern.S[0];


                    }

                    label6.Text = "result №" + i.ToString();

                    //Thread.Sleep(1000);

                    Pcumul /= Meas; P2cumul /= Meas;
                    Scumul /= Meas; S2cumul /= Meas;
                    //Pgeneral = (i * Pgeneral + Pcumul) / (i + 1);
                    //P2general = (i * P2general + P2cumul) / (i + 1);

                    tbPL.Text += (Pcumul).ToString() + (char)13 + (char)10;
                    tbS.Text += (Scumul).ToString() + (char)13 + (char)10;
                    lblNum.Text = Scount.ToString();


                    double chi = Math.Sqrt(P2cumul - Pcumul * Pcumul);//P2general - Pgeneral * Pgeneral;
                    tbPLchi.Text += chi.ToString() + (char)13 + (char)10;//chi in resmid textbox
                    chi = Math.Sqrt(S2cumul - Scumul * Scumul);//P2general - Pgeneral * Pgeneral;
                    tbSchi.Text += chi.ToString() + (char)13 + (char)10;//chi in resmid textbox

                    Application.DoEvents();
                }

                Application.DoEvents();
                curbeta += stepbeta;
            }
            working = true;

        }
        //private void button5_Click_1(object sender, EventArgs e)
        //{
        //    int Nx = 16; int Ny = 16; int Nt = 8; double flux = 0.5; double beta = 6.0;
        //    int Therm = 1000; int Meas = 10000; int tDiscard = 1;
        //    DateTime t;
        //    double Scumul = 0, S2cumul = 0;
        //    for (int Ntr = 6; Ntr <= 16; Ntr += 4)
        //    {

        //        MyKernel Kern = new MyKernel(Nx, Ny, Ntr, Nt, beta, flux);

        //        //ZoomStart = DateTime.Now;
        //        label6.Text = "Thermalization..."; Application.DoEvents();
        //        for (int i = 0; i < Therm; i++) { Kern.Calculate(); }

        //        Scumul = 0; S2cumul = 0;
        //        if (!working) { working = true; return; }

        //        label6.Text = "Measuring..."; Application.DoEvents();
        //        t = DateTime.Now;
        //        for (int i = 0; i < Meas; i++)
        //        {
        //            label6.Text = "Measuring..." + i.ToString();
        //            Application.DoEvents();
        //            Kern.Calculate();//discard one measure
        //            Kern.Calculate();

        //            Scumul += Kern.S[0];
        //            S2cumul += Kern.S[0] * Kern.S[0];
        //        }

        //        //write results
        //        Scumul /= Meas; S2cumul /= Meas;
        //        tbB.Text += Ntr.ToString() + (char)13 + (char)10;
        //        tbS.Text += (Scumul).ToString() + (char)13 + (char)10;

        //        double chi = Math.Sqrt(S2cumul - Scumul * Scumul);//P2general - Pgeneral * Pgeneral;
        //        tbSchi.Text += chi.ToString() + (char)13 + (char)10;//chi in resmid textbox
        //        label23.Text = "Last measure: Ntr = " + Ntr.ToString() + ", T = " + (DateTime.Now - t).TotalSeconds.ToString();
        //        Application.DoEvents();

        //        MyKernel = null;
        //        GC.Collect();
        //    }

        //}
        private void button5_Click_1(object sender, EventArgs e)
        {
            //int Nx = 16; int Ny = 16; 
            int Nt = 4; int Nstart = 4, Nend = 18;
            double startflux = 0, flux = 1;
            double beta = 6.0;
            int Therm = 1000; int Meas = 10000; int tDiscard = 1;
            DateTime t;
            double Scumul=0, S2cumul = 0;
            for (int Ntr = Nstart; Ntr <= Nend; Ntr += 2)
            {
               // flux = startflux / (double)Nstart * (double)Ntr;

                Core Kern = new Core(Ntr, Ntr, Ntr, Nt, beta, flux);

                //ZoomStart = DateTime.Now;
                label6.Text = "Thermalization..."; Application.DoEvents(); 
                for (int i = 0; i < Therm; i++) { Kern.Calculate();}

                Scumul = 0; S2cumul = 0;
                if (!working) { working = true; return; }

                label6.Text = "Measuring..."; Application.DoEvents(); 
                t = DateTime.Now;
                for (int i = 0; i < Meas; i++)
                {
                    if (i % 1000==0)
                    {
                        label6.Text = "Measuring..." + i.ToString();
                        Application.DoEvents();
                    }
                    //Kern.Calculate();//discard one measure
                    Kern.Calculate();

                    Scumul += Kern.S[0];
                    S2cumul += Kern.S[0] * Kern.S[0];
                }

                //write results
                Scumul /= Meas; S2cumul /= Meas;
                tbB.Text += Ntr.ToString() + (char)13 + (char)10;
                tbPLchi.Text += flux.ToString() + (char)13 + (char)10;
                tbS.Text += (Scumul).ToString() + (char)13 + (char)10;

                double chi = Math.Sqrt(S2cumul - Scumul * Scumul);//P2general - Pgeneral * Pgeneral;
                tbSchi.Text += chi.ToString() + (char)13 + (char)10;//chi in resmid textbox
                label23.Text = "Last measure: Ntr = "+Ntr.ToString()+", T = "+(DateTime.Now - t).TotalSeconds.ToString();
                Application.DoEvents(); 

                MyKernel = null;
                GC.Collect();
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            
            Text = DateTime.Now.ToString();
            tbPL.Text += "4x2" + (char)13 + (char)10;
            for (int i = 9; i >= 0 ; i--)
            {
                double beta = 2 + i;
                Core kern = new Core(4, 4, 4, 2, beta, 0);
                for (int j = 0; j < 150; j++)
                {
                    label6.Text = j.ToString();
                    label23.Text = DateTime.Now.ToString(); Application.DoEvents();
                    kern.SweepWithFermions();
                }
                kern.CalculateS();
                double res = kern.BiCGStab();
                tbB.Text += beta.ToString() + (char)13 + (char)10;
                tbPL.Text += res.ToString() + (char)13 + (char)10;
                tbS.Text += kern.S[0].ToString() + (char)13 + (char)10;
                kern.Dispose();
                Application.DoEvents();
            }
            tbPL.Text += "4x4" + (char)13 + (char)10;
            for (int i = 0; i < 10; i++)
            {
                double beta = 2 + i;
                Core kern = new Core(4, 4, 4, 4, beta, 0);
                for (int j = 0; j < 150; j++)
                {
                    label6.Text = j.ToString();
                    label23.Text = DateTime.Now.ToString(); Application.DoEvents();
                    kern.SweepWithFermions();
                }
                kern.CalculateS();
                double res = kern.BiCGStab();
                tbB.Text += beta.ToString() + (char)13 + (char)10;
                tbPL.Text += res.ToString() + (char)13 + (char)10;
                tbS.Text += kern.S[0].ToString() + (char)13 + (char)10;
                kern.Dispose();
                Application.DoEvents();
            }
            Text += "      " + DateTime.Now.ToString();
        }

        private void button8_Click(object sender, EventArgs e)
        {
           // for (int i = 0; i < 10; i++) MyKernel.Calculate();
            DateTime t;
            for (int i = 0; i < 100; i++)
            {
                t = DateTime.Now;
                MyKernel.SweepWithFermions();
                Text = (DateTime.Now - t).TotalSeconds.ToString();
                    //MyKernel.Sweep();
                MyKernel.CalculateS();
                MyKernel.CalculatePL();
                string s = "s = "+MyKernel.S[0].ToString() + (char)9+ "pl = "+MyKernel.PL[0].ToString()+(char)13 + (char)10;
                File.AppendAllText("result.txt",s);
                tbS.Text += MyKernel.S[0].ToString() + (char)13 + (char)10;
                tbPL.Text += MyKernel.PL[0].ToString() + (char)13 + (char)10;
                Application.DoEvents();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            float kstart = 0.1f; float kend = 0.1f; float dk = 0.005f;
            for (float k = kstart; k <= kend; k += dk)
            {
                Core.kappa = k;
                Text = DateTime.Now.ToString();
                tbPL.Text += "8x2 k = " + Core.kappa.ToString() + (char)13 + (char)10;
                for (int i = 0; i <= 100; i++)
                {
                    double beta = 5 + i * 0.01;
                    Core kern = new Core(8, 8, 8, 2, beta, 0);
                    for (int j = 0; j < 10000; j++)
                    {
                        if (j % 1000 == 0)
                        {
                            label6.Text = j.ToString(); Application.DoEvents();
                        }
                        //label23.Text = DateTime.Now.ToString(); 
                        kern.Calculate();//SweepWithFermions();
                    }
                    kern.CalculateS();double res = 0;
                    for (int n = 0; n < 1000; n++) 
                    { kern.Calculate(); kern.Calculate();
                    kern.CalculatePL();
                    res += kern.PL[0];//kern.BiCGStab();
                    label10.Text = n.ToString(); Application.DoEvents();
                    }
                    res /= 1000.0;
                    tbB.Text += beta.ToString() + (char)13 + (char)10;
                    tbPL.Text += res.ToString() + (char)13 + (char)10;
                    tbS.Text += kern.S[0].ToString() + (char)13 + (char)10;
                    kern.Dispose();
                    Application.DoEvents();
                }
            } 
        
        }
    }

}

#if false
            ErrorCode result;
            uint numPlatforms;
            IntPtr[] platformIDs;

            // Get number of platforms
            result = cl.GetPlatformIDs( 0, null, out numPlatforms );
            if( result!=ErrorCode.SUCCESS )
                throw new Exception( "GetPlatformIDs failed with ErrorCode."+result );
            Debug.WriteLine( "Number of platforms: "+numPlatforms );
            if( numPlatforms==0 )
                throw new Exception( "No openCL platforms available." );

            // Create an array of platform IDs
            platformIDs = new IntPtr[numPlatforms];
            result = cl.GetPlatformIDs( numPlatforms, platformIDs, out numPlatforms );
            if( result!=ErrorCode.SUCCESS )
                throw new Exception( "GetPlatformIDs failed with ErrorCode."+result );
            Debug.WriteLine("");


        int[] globSize = new int[3];
        int[] globID = new int[3];
        float left;
        float top;
        float right;
        float bottom;
        AlignedArrayFloat aaf;

        private void KernelIterator2d()
        {
            int activeIndex = globSize.Length-1;

            while( activeIndex>=0 )
            {
                MyKernelKernel( left, top, right, bottom, aaf );
                while( activeIndex>=0 )
                {
                    globID[activeIndex]++;
                    if( globID[activeIndex]>=globSize[activeIndex] )
                    {
                        globID[activeIndex] = 0;
                        activeIndex--;
                    }
                    else
                    {
                        activeIndex = globSize.Length-1;
                        break;
                    }
                }
            }
        }

        private int get_global_size( int dimension ) { return globSize[dimension]; }
        private int get_global_id( int dimension ) { return globID[dimension]; }
        private void MyKernelKernel( float left, float top, float right, float bottom, AlignedArrayFloat af )
        {
            int width = get_global_size(0);
            int height = get_global_size(1);
            int cx = get_global_id(0);
            int cy = get_global_id(1);
            float dx = (right-left)/(float)width;
            float dy = (bottom-top)/(float)height;

            float x0 = left+dx*(float)cx;
            float y0 = top+dy*(float)cy;
            float x = 0.0f;
            float y = 0.0f;
            int iteration = 0;
            int max_iteration = 1000;

            while( x*x-y*y<=(2*2) && iteration<max_iteration )
            {
                float xtemp = x*x-y*y+x0;
                y = 2*x*y+y0;
                x = xtemp;
                iteration++;
            }
            float color;
            color = iteration==max_iteration?0.0f: (float)iteration/(float)max_iteration;
            af[width*4*cy+cx*4+0] = 1.0f;
            af[width*4*cy+cx*4+1] = color;
            af[width*4*cy+cx*4+2] = color;
            af[width*4*cy+cx*4+3] = color;
        }

#endif


