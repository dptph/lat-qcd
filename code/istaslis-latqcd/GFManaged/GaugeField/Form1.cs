using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace GaugeField
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        bool CloseMe = false;

        private void DrawPicture(SU3 F)
        {
            int Rsize = 10;
            int Ns = F.Ns, Nt = F.Nt;
            Bitmap B = new Bitmap(Ns, Ns);
            
            Graphics g;
            g=pictureBox1.CreateGraphics();
            g.Clear(Color.Wheat);

            int n = 0; double PL = 0;
            for (int i = 0; i < Ns; i++)
                for (int j = 0; j < Ns; j++)
                {
                    n = 0; PL = 0;
                    for (int k = 0; k < Ns; k++)
                    {
                        Link L = F.U[i, j, k, 0, 4];

                        for (int t = 1; t < Nt; t++)
                        {
                            L = F.Mul(L, F.U[i, j, k, t, 4]);
                        }
                        n++;

                        PL += F.ReTr(L);
                    }
                    PL = Math.Abs(PL / n / SU3.d);

                   // B.SetPixel(i,j,0);
                    SolidBrush Br = new SolidBrush(Color.FromArgb((int)Math.Round((1-PL)*255),255,0));
                    g.FillRectangle(Br, i * Rsize, j * Rsize, Rsize, Rsize);
                }
            //here put the point
            g.DrawImage(B,0,0);
        }

        private void StartField()
        {
            chart1.Invoke(new MethodInvoker(delegate() { chart1.Series[0].Points.Clear(); }));
            chart2.Invoke(new MethodInvoker(delegate() { chart2.Series[0].Points.Clear(); }));
            //       chart1.Series[0].Points.Clear(); chart2.Series[0].Points.Clear();

            CloseMe = false;
            DateTime T1;
            int Ns = Convert.ToInt32(tbNs.Text); int Nt = Convert.ToInt32(tbNt.Text);

            SU3 Field = new SU3(Ns,Nt, Convert.ToDouble(tbBeta.Text),Convert.ToDouble(tbflux.Text), checkBox1.Checked);
            for (int i = 0; i < 100000; i++)
            {
                T1 = DateTime.Now;
                
                //for (int or = 0; or < 5; or++) Field.Sweep(false); //SU(2) ONLY!!!
                this.Text = Field.Sweep(false).ToString();

                chart1.Invoke(new MethodInvoker(delegate() { chart1.Series[0].Points.AddY(Field.MeasureS()); }));
                chart2.Invoke(new MethodInvoker(delegate() { chart2.Series[0].Points.AddY(Field.MeasurePL()); }));
               
                lblTime1.Invoke(new MethodInvoker(delegate() { lblTime1.Text = Math.Round((DateTime.Now - T1).TotalMilliseconds).ToString() ; }));
                if (CloseMe) break;

               // if (cbDraw.Checked) DrawPicture(Field);

                Application.DoEvents();
            }
            CloseMe = false;
        }
        Thread SU3Thread;

        private void button1_Click(object sender, EventArgs e)
        {
            //SU3Thread = new Thread(new ThreadStart(StartField));
            StartField();
            //SU3Thread.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CloseMe = true;
            //SU3Thread.Suspend();
            //SU3Thread.Interrupt();
           // SU3Thread.Abort();
        }


        void InvokeMe(MethodInvoker t)
        {
            this.Invoke(t);
        }

        private void GetData()
        {
            var l = new List<int>(new int[]{1,2,3});
            //l.Where(

            DateTime T, Tfull; double Chi = 0;
            double BetaStart = Convert.ToDouble(tbBS.Text);
            double BetaEnd = Convert.ToDouble(tbBE.Text);
            double BetaStep = Convert.ToDouble(tbBSt.Text);

            int Tsteps = Convert.ToInt16(tbTsteps.Text);
            int Msteps = Convert.ToInt16(tbMsteps.Text);
            for (double b = BetaStart; b <= BetaEnd; b = b + BetaStep)
            {
                SU3 Field = new SU3(Convert.ToInt32(tbNs.Text), Convert.ToInt32(tbNt.Text), b,Convert.ToDouble(tbflux.Text), checkBox1.Checked);
                double PL = 0, PL2 = 0, PLt; Chi = 0;
                int n = 0;
                Tfull = DateTime.Now;
                InvokeMe(() => Text = "1");
                InvokeMe(() => lblProcess.Text = "Thermalizing");
                for (int i = 0; i < Tsteps; i++)
                {
                    //T = DateTime.Now;
                    Field.Sweep(false);
                    InvokeMe(() => lblProcess.Text = "Thermalizing " + i.ToString());
                   // lblTime1.Invoke(new MethodInvoker(delegate() { lblTime1.Text = (DateTime.Now - T).TotalMilliseconds.ToString(); }));
                }

                //chart2.Invoke(new MethodInvoker(delegate() { chart2.Series[0].Points.AddXY(b, 0); }));
                for (int i = 0; i < Msteps; i++)
                {
                   
                    Field.Sweep(false);
                    //  for (int or = 0; or < 5; or++) Field.Sweep(false);
                    if (rbS.Checked) PLt = Field.MeasureS(); else PLt = Field.MeasurePL();
                    PL += PLt; n++;
                   PL2 += PLt * PLt;

                    Chi = PL2 / n - (PL / n) * (PL / n);
                   // chart2.Invoke(new MethodInvoker(delegate() { chart2.Series[0].Points.Last().YValues = new double[1] { Chi }; }));
                    InvokeMe(() => chart1.Series[0].Points.Add(Chi));
                    InvokeMe(() => chart2.Series[0].Points.Add(PLt));
                    InvokeMe(() => lblProcess.Text = "Measuring " + i.ToString());
                   
                }

               // Chi = PL2 / n - (PL / n) * (PL / n);
               // chart2.Invoke(new MethodInvoker(delegate() { chart2.Series[0].Points.Last().YValues = new double[1] { Chi }; }));
                
              //  chart2.Invoke(new MethodInvoker(delegate() { chart2.Series[0].Points.AddXY(b, PL/n); }));
                double t = (DateTime.Now - Tfull).TotalMilliseconds * (BetaEnd - b) / BetaStep;
                InvokeMe(() => lblRemain.Text = (DateTime.Now.AddMilliseconds(t)).ToString());
                InvokeMe(() => chart2.Update());

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (cbClear.Checked) { chart1.Series[0].Points.Clear(); chart2.Series[0].Points.Clear(); }
            SU3Thread = new Thread(new ThreadStart(GetData));

          

          //  chart2.Series[0].Points.Add(new double[3] { 0, 1, 2 });
          //  chart2.Series[0]["ErrorBarCenterMarkerStyle"] = "Cross";

            SU3Thread.Start();

        }

        private void chart2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "Ns="+tbNs.Text+"_Nt="+tbNt.Text+"_b="+tbBeta.Text+".txt";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamWriter SW = new StreamWriter(saveFileDialog1.FileName);
                SW.WriteLine("Ns=" + tbNs.Text);
                SW.WriteLine("Nt=" + tbNt.Text);
                SW.WriteLine("b =" + tbBeta.Text);
                SW.WriteLine();
                for (int i = 0; i < chart2.Series[0].Points.Count; i++)
                {
                   // SW.WriteLine(chart2.Series[0].Points[i].XValue.ToString() + (char)9 + chart2.Series[0].Points[i].YValues[0].ToString());
                    SW.WriteLine(chart2.Series[0].Points[i].YValues[0].ToString().Replace(',','.'));
                }
                SW.Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
           // SU3 F = new SU3(4, 2, 0.2, true);
            /*for (int i = 0; i < 10000; i++)
            {
               // double x = F.GetRandom()*10;
                Link M = F.GetHeatBath(6);
                double x0 = M.a0, x1 = M.a1, x2 = M.a2, x3 = M.a3;
                chart2.Series[0].Points.Add((1-x0*x0)/(x1*x1+x2*x2+x3*x3)); Application.DoEvents();
                //double x = F.GetRandom(); double y = F.GetRandom();
                //chart2.Series[0].Points.AddXY(x, y); Application.DoEvents();
            }
             */

           // Link A = new Link();
          //  A.a0 = 1; A.a1 = 1; A.a2 = 1; A.a3 = 1;
          //  A = F.Mul(A, A);
          //  A.a0 = 1;
            chart2.ChartAreas[0].RecalculateAxesScale();
            
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (SU3Thread!=null)
            SU3Thread.Abort();
            CloseMe = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (SU3Thread != null)
                SU3Thread.Abort();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            chart2.Series[0].Points.Clear();
            SU3Thread = new Thread(new ThreadStart(GetSample));

            SU3Thread.Start();
        }
        private void GetSample()
        {
            double Beta = Convert.ToDouble(tbBeta.Text);
            int Ns = Convert.ToInt32(tbNs.Text);
            int Nt = Convert.ToInt32(tbNt.Text);


            int Tsteps = Convert.ToInt32(tbTsteps.Text);
            int Msteps = Convert.ToInt32(tbMsteps.Text);

          //  double[] Sample = new double[N];

            SU3 Field = new SU3(Ns, Nt, Beta, Convert.ToDouble(tbflux.Text),checkBox1.Checked);
            double sum = 0, sum2 = 0, t = 0;
            int n = 0;
            int Mark = Convert.ToInt32(tbMark.Text);

            InvokeMe(() => lblProcess.Text = "Thermalizing");
            for (int i = 0; i < Tsteps; i++) { Field.Sweep(false); lblProcess.Invoke(new MethodInvoker(delegate() { lblProcess.Text = "Thermalizing " + i.ToString(); })); }
            while (true)
            {
                Field.Sweep(false); Field.Sweep(false); //Field.Sweep(false); Field.Sweep(false);
                t = Field.MeasureS();
               // Sample[n] = t;
                n++;
                sum += t; sum2 += t * t;
                InvokeMe(() => lblProcess.Text = "Measuring " + n.ToString());
                //
                if (n % Mark == 0)
                {
                    InvokeMe(() => chart2.Series[0].Points.AddY(sum / n));
                    InvokeMe(() => lblValue.Text = (sum / n).ToString());

                }
            }

            sum = sum / n;

            //finding error
            /*double sigma2=0, Sn=0, Ssample=0;
            for (int i=0;i<N;i++)
            {
                Sn = 1 / (N - 1.0) * (N*sum - Sample[i]);
                Ssample += Sn;
                sigma2 += (Sn - sum) * (Sn - sum);
                chart2.Invoke(new MethodInvoker(delegate() { chart2.Series[0].Points.AddY(Sample[i]); }));
            }
            Ssample = Ssample / N;
            sigma2 = Math.Sqrt((N-1.0)/N*sigma2);
            Ssample = sum - (N - 1f) * (Ssample - sum);
            */

            //   int k = 10;
            //   int d = N / k; sum2 = 0;
            //   for (int i = 0; i < d; i++)
            //   {
            //       sum = 0;
            //       for (int j = 0; j < k; j++) sum += Sample[i * k + j];
            //       sum /= k;
            //       chart2.Invoke(new MethodInvoker(delegate() { chart2.Series[1].Points.AddY(sum); }));
            //       sum2 = (sum2 * i + sum) / ((i + 1));
            //       chart2.Invoke(new MethodInvoker(delegate() { chart2.Series[0].Points.AddY(sum2); }));


            //   }

            // lblSampleValue.Invoke(new MethodInvoker(delegate() { lblSampleValue.Text = Ssample.ToString(); }));
            //    lblSampleError.Invoke(new MethodInvoker(delegate() { lblSampleError.Text = sigma2.ToString(); }));

            //average and dispersion
            sum2 = Math.Sqrt(sum2 / n - sum * sum);

            InvokeMe(() => lblValue.Text = sum.ToString());
            InvokeMe(() => lblError.Text = sum2.ToString());

        }
        bool IfSuspend = true;
        private void button8_Click(object sender, EventArgs e)
        {
            if (IfSuspend)
            {
                SU3Thread.Suspend();
                IfSuspend = false;
                button8.Text = "Resume";
            }
            else
            {
                SU3Thread.Resume();
                IfSuspend = true;
                button8.Text = "Suspend";
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void button10_Click(object sender, EventArgs e)
        {

            double beta = 10;
            SU3 Field = new SU3(int.Parse(tbNs.Text), int.Parse(tbNt.Text), beta, 0, true);//4х2 field beta=6 flux=0
            for (int i = 0; i < 10; i++) Field.Sweep(false);
            Dirac Dt = new Dirac(Field);
            Complex res = Complex.Zero;

            for (int n = 0; n < Dirac.N; n++)
                for (byte alpha = 0; alpha < 4; alpha++)
                    for (byte a = 0; a < 3; a++)
                    {
                        DateTime t = DateTime.Now;
                        Dirac D = new Dirac(Field);

                        D.phi[n, alpha, a] = Complex.One;
                        //double philen = Dirac.V1hermV2(D.phi, D.phi).Module();

                        D.Prepare();
                        for (int i = 0; i < 5; i++)
                        {
                            D.Iter();
                            //Vector phinew = D.MulD(D.x);
                            //double r = 0;// D.V1hermV2(phinew, phinew).Module(); //D.V1hermV2(D.ri, D.ri).Module();
                            //Text = "i = " + i.ToString() + "    philen=" + philen.ToString() + "  =?  " + r.ToString() + "     " + (DateTime.Now - t).ToString();
                            //t = DateTime.Now;
                        }
                        res += D.x[n, alpha, a];
                        Text = n.ToString() + " " + (res / new Complex((n + 1) * 12, 0)).ToString();
                        Application.DoEvents();
                        if (CloseMe) break;
                    }
            Text = (res / new Complex(Dirac.N, 0)).ToString();

            //for (int exp = 0; exp < 1; exp++)
            //{
            //    double beta = 6;// 1 + 10 * exp;
            //    SU3 Field = new SU3(4, 2, beta, 0, true);//4х2 field beta=6 flux=0
            //    for (int i = 0; i < 10; i++) Field.Sweep(false);

            //    DateTime t = DateTime.Now;


            //    int K = 100;
            //    Vector[] chi = new Vector[K];
            //    Complex tr = new Complex();
            //    for (int k = 0; k < K; k++)
            //    {
            //        Dirac D = new Dirac(Field);
            //        chi[k] = new Vector(Dirac.N, rand.Next());
            //        chi[k].FillGaussRandom();
            //        D.phi = chi[k];
            //        //double philen = Dirac.V1hermV2(D.phi, D.phi).Module();
            //        D.Prepare();
            //        for (int i = 0; i < 4; i++)
            //        {
            //            D.Iter();
            //            //Vector v = D.MulD(D.x);
            //            //Text = Dirac.V1hermV2(D.ri, D.ri).ToString();//"Dx = " + Dirac.V1hermV2(v, v).Module() + "   phi = " + philen.ToString();
            //            //Application.DoEvents();
            //        }
            //        tr += Dirac.V1hermV2(D.phi, D.x);
            //        Text = beta.ToString() + " " + k.ToString();

            //        Application.DoEvents();
            //    }

            //    tr *= new Complex(1 / (double)K / (4 * 4 * 4 * 2 * 12 * 2), 0);
            //    Text = tr.ToString();
            //    chart1.Series[0].Points.AddXY(beta, tr.x);

            //}
        }
        Random rand = new Random();
        Complex GetGaussRandom()
        {
            //double u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
            //double u2 = rand.NextDouble();
            //double t = Math.Sqrt(-2.0 * Math.Log(u1));
            //return new Complex(t * Math.Cos(2.0 * Math.PI * u2), t * Math.Sin(2.0 * Math.PI * u2));

            return (rand.NextDouble() < 0.5) ? Complex.MinOne : Complex.One;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            ////check Gaussness
            //Vector v = new Vector(100);
            //int N = 100; double a = 0, b = 10;
            //int[] inters = new int[N];
            //for (int i = 0; i < 10000; i++)
            //{
            //    double r = v.GetGaussRandom()+5;
            //    if (r>a && r < b)
            //    inters[(int)(r * N/(b-a))]++;

            //}

            //for (int i = 0; i < N; i++)
            //    chart1.Series[0].Points.AddXY((double)i / (double)N, inters[i]);

            ////basic delta
            //int K = 100, n = 3;
            //Complex[][] chi = new Complex[K][];
            //for (int i = 0; i < K; i++)
            //{
            //    chi[i] = new Complex[n];
            //    for (int j = 0; j < n; j++)
            //        chi[i][j] = GetGaussRandom();
            //}

            //double[,] delta = new double[n, n];
            //double res1 = 0, res2 = 0;
            //for (int i = 0; i < n; i++)
            //    for (int j = 0; j < n; j++)
            //    {
            //        Complex t = new Complex();
            //        for (int k = 0; k < K; k++)
            //            t += (chi[k][i] * chi[k][j].Conj());
            //        delta[i, j] = 1 / (double)(2 * K) * t.x;// OneOverK;
            //        if (i == j) res1 += delta[i, j]; else res2 += delta[i, j];
            //    }
            //Text = res1.ToString() + "   " + (res2 / n).ToString();

            //real delta
            int K = 1, n = 10;
            Vector[] chi = new Vector[K];
            for (int i = 0; i < K; i++)
            {
                chi[i] = new Vector(n,rand.Next());
                chi[i].FillGaussRandom();

            }
            double[, , , , ,] delta = new double[n, n, 4, 4, 3, 3];
            double res1 = 0, res2 = 0;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    for (byte alpha = 0; alpha < 4; alpha++)
                        for (byte beta = 0; beta < 4; beta++)
                            for (byte a = 0; a < 3; a++)
                                for (byte b = 0; b < 3; b++)
                                {
                                    Complex OneOverK = new Complex(1 / (double)K, 0);
                                    Complex t = new Complex();
                                    for (int k = 0; k < K; k++)
                                        t += (chi[k][i, alpha, a] * chi[k][j, beta, b].Conj());
                                    delta[i, j, alpha, beta, a, b] = 1 / (double)(K) * t.x;// OneOverK;
                                    if (i == j && alpha == beta && a == b) res1 += delta[i, j, alpha, beta, a, b]; else res2 += delta[i, j, alpha, beta, a, b];
                                }
            Text = res1.ToString() + "   " + (res2 / n / 12).ToString();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            double beta = 6;
            SU3 Field = new SU3(int.Parse(tbNs.Text), int.Parse(tbNt.Text), beta, 0, true);//4х2 field beta=6 flux=0
            //for (int i = 0; i < 10; i++) Field.Sweep(false);
            Complex res = Complex.Zero;
            Dirac D = new Dirac(Field);

            D.phi.FillRandom();
            
            DateTime t = DateTime.Now;

            D.Prepare();
            //for (int i = 0; i < 10; i++)
                D.Iter();
            

            Text = ((DateTime.Now - t).TotalMilliseconds).ToString();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            //double beta = 6;
            //SU3 Field = new SU3(int.Parse(tbNs.Text),int.Parse(tbNt.Text), beta, 0, true);//4х2 field beta=6 flux=0
            //for (int i = 0; i < 10; i++) Field.Sweep(false);
            //Dirac Dt = new Dirac(Field);
            //Complex res = Complex.Zero;

            //DateTime t = DateTime.Now;

            //Dirac D = new Dirac(Field);

            ////D.phi[0, 0, 0] = Complex.One;
            //D.phi.FillGaussRandom();
            //double philen = Dirac.V1hermV2(D.phi, D.phi).Module();

            //D.Prepare();
            //for (int i = 0; i < 100; i++)
            //{
            //    D.Iter();
            //    Vector phinew = D.MulD(D.x);
            //    double pn = Dirac.V1hermV2(phinew, phinew).Module(); //D.V1hermV2(D.ri, D.ri).Module();
            //    double r = Dirac.V1hermV2(D.ri, D.ri).Module(); 
            //    Text = "i = " + i.ToString() + "    philen=" + philen.ToString() + "  =?  " + pn.ToString() + " r= " + r.ToString()+"   "+(DateTime.Now - t).ToString();
            //    t = DateTime.Now;
            //    Application.DoEvents();
            //    if (r < 1E-5) break;
            //}

            int Ns = 4; int Nt = 2;
            SU3 UnityField = new SU3(Ns, Nt, 6, 0, true);

            for (int i = 0; i < Ns; i++)
                for (int j = 0; j < Ns; j++)
                    for (int k = 0; k < Ns; k++)
                        for (int t = 0; t < Nt; t++)
                            for (int m = 1; m <= 4; m++)
                            {

                                for (int a = 0; a < 3; a++)
                                    for (int b = 0; b < 3; b++)
                                        UnityField.U[i, j, k, t, m].A[a, b] = new Complex(1, 1);

                            }


            Complex sum = Complex.Zero;

            Dirac D = new Dirac(UnityField);

            D.phi.FillUnity();
            Vector v1 = D.MulD(D.phi);

            Text = Dirac.V1hermV2(v1,v1).ToString();

            //Complex sum = Complex.Zero;
            //for (int i = 0; i < Ns * Ns * Ns * Nt; i++)
            //{
            //    for (byte alpha = 0; alpha < 4; alpha++)
            //        for (byte a = 0; a < 3; a++)
            //        {
            //            Dirac D = new Dirac(UnityField);

            //            //D.phi.FillUnity();
            //            //Vector v1 = D.MulD(D.phi);

            //            D.phi[i, alpha, a] = Complex.One;
            //            D.Prepare();
            //            int it = D.Iter();
            //            Text = it.ToString();
            //            //Vector v2 = D.x;
            //            sum += D.x[i, alpha, a];
            //            //Text = it.ToString()+Dirac.V1hermV2(v2, v2).ToString();

            //        }
            //    Text = i.ToString() + " temps=" + sum.ToString(); Application.DoEvents();
            //}
            //Text = sum.ToString();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            double beta = 6;
            SU3 Field = new SU3(int.Parse(tbNs.Text), int.Parse(tbNt.Text), beta, 0, true);//4х2 field beta=6 flux=0
            DateTime t = DateTime.Now;
            for (int i = 0; i < 4; i++)
            {
                Field.Sweep(false); 
                MessageBox.Show(Field.MeasureS().ToString()); 

                Text = (DateTime.Now - t).TotalMilliseconds.ToString(); Application.DoEvents();
            }
        }

    }
}
