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

namespace GFVideo
{
    using floattype = System.Single;
    using floattype2 = Float2;

    public class Core:IDisposable
    {

        public Platform openCLPlatform;
        Device[] openCLDevices;
        public static Context openCLContext;
        public static CommandQueue openCLCQ;
        Program MyKernelProgram;
        Kernel MyKernelKernel;
        Kernel PReductionKernel;
        Kernel SReductionKernel;
        public static Kernel DiralMulKernel, FillWithKernel, FillLinkWithKernel, AXPYKernel, XhermYKernel, FillWithRandomKernel, BackupLinkKernel, RestoreLinkKernel;
        public static Mem SeedMem, LinkMem, PGroupMem, PResMem, SGroupMem, SResMem, XhermYGroupMem, XhermYresMem, SeedVectorMem, StorageMem, dSmem;

        IntPtr XhermYrespointer; IntPtr dSpointer;


        static int Nx = 4, Ny = 4, Nz = 4, Nt = 2;
        byte EvenOdd = 0;
        public floattype betagauge, flux;
        public static floattype kappa=0.12f;
        static public int N = Nx*Ny*Nz*Nt, Nspace = Nx*Ny*Nz;
        public int BufferLength = N * 4 * 9 * 2 * sizeof(floattype);
        public int SeedBufLen = N * sizeof(Int32);
        static int Plocalsize = 8, Slocalsize = 8;
        static int XhermYlocalsize = 8;
        int PNumGroups = Nx * Ny * Nz / Plocalsize;
        int SNumGroups = Nx * Ny * Nz * Nt / 2 / Slocalsize;
        int XhermYNumGroups = N / XhermYlocalsize;
        public static int floatsize = sizeof(floattype);
        public int linksize = 9 * 2 * floatsize;

        //vectors for BiCGStab

        public Vector phi;

        Vector r0;
        Vector rhat0;
        //Vector rprev;
        Vector pi;
        Vector vi;
        Vector t;
        Vector s;
       // Vector xprev;

       // Vector vprev;
       // Vector pprev;

        Vector temp;

        public Vector ri;
        Complex // roprev, 
            roi, alpha, //wprev, 
            wi, beta;

        public Vector x;

        //vector for fermupdate
        Vector chi;
        

        int AdjustLocalSize(int Nall)
        {
            int res = 128;
            while (Nall % res != 0) res /= 2;
            return res;
        }

        public Core(int Nxp,int Nyp, int Nzp, int Ntm, double Bbeta, double Flux)
        {
            Nx = Nxp; Ny = Nyp; Nz = Nzp; Nt = Ntm; betagauge = (floattype)Bbeta; flux = (floattype)Flux;
            N = Nx * Ny * Nz * Nt; Nspace = Nx * Ny * Nz;


            string strforcompiler =  "-D Nt=" + Nt.ToString() + " -D Nxyz=" + (Nx * Ny * Nz).ToString() + " -D Nxy=" + (Nx*Ny).ToString() +
                                            " -D Nx="+(Nx).ToString()+" -D Ny="+(Ny).ToString()+" -D Nz="+(Nz).ToString();
            strforcompiler += typeof(floattype) == typeof(double) ? " -D floattype=double -D floattype2=double2 -D floattype4=double4" :
                                                                " -D floattype=float -D floattype2=float2 -D floattype4=float4";
            strforcompiler += " -D phi=" + flux.ToString().Replace(',', '.') + " -D KAPPA=" + kappa.ToString().Replace(',', '.');
            string fp64support = "#pragma OPENCL EXTENSION  cl_khr_fp64 : enable\n";

            Plocalsize = AdjustLocalSize(Nspace);
            Slocalsize = AdjustLocalSize(N / 2);
            XhermYlocalsize = AdjustLocalSize(4 * N);

           // Plocalsize = 16; Slocalsize = 16;

            PNumGroups = Nx * Ny * Nz / Plocalsize;
            SNumGroups = N/2 / Slocalsize;
            XhermYNumGroups = 4*4*N / XhermYlocalsize;
            BufferLength = N * 4 * 9 * 2 * sizeof(floattype);
            SeedBufLen = N * sizeof(Int32)/2 * 4;

            AllocBuffers();
            
            openCLPlatform = OpenCL.GetPlatform(0);
            openCLDevices = openCLPlatform.QueryDevices(DeviceType.ALL);
            openCLContext = openCLPlatform.CreateDefaultContext();
            openCLCQ = openCLContext.CreateCommandQueue(openCLDevices[0], CommandQueueProperties.PROFILING_ENABLE);
            MyKernelProgram = openCLContext.CreateProgramWithSource(
                (typeof(floattype)==typeof(double)?fp64support:"") + File.ReadAllText("MyKernel.cl")+File.ReadAllText("dirak_mul.cl"));
            try
            {
                MyKernelProgram.Build(openCLDevices, strforcompiler, null, IntPtr.Zero);
            }
            catch (OpenCLException)
            {
                string buildLog = MyKernelProgram.GetBuildLog(openCLDevices[0]);
                MessageBox.Show(buildLog, "Build error(64 bit debug sessions in vs2008 always fail like this - debug in 32 bit or use vs2010)");
                //  Application.Exit();
            }
            MyKernelKernel = MyKernelProgram.CreateKernel("MyKernel");
            PReductionKernel = MyKernelProgram.CreateKernel("PLoop");
            SReductionKernel = MyKernelProgram.CreateKernel("CalcS");
            DiralMulKernel = MyKernelProgram.CreateKernel("dirakMatrMul");
            FillWithKernel = MyKernelProgram.CreateKernel("FillWith");
            FillLinkWithKernel = MyKernelProgram.CreateKernel("FillLinkWith");
            FillWithRandomKernel = MyKernelProgram.CreateKernel("FillWithRandom");
            AXPYKernel = MyKernelProgram.CreateKernel("AXPY");
            XhermYKernel = MyKernelProgram.CreateKernel("XhermY");
            BackupLinkKernel = MyKernelProgram.CreateKernel("BackupLink");
            RestoreLinkKernel = MyKernelProgram.CreateKernel("RestoreLink");

            SeedMem = openCLContext.CreateBuffer((MemFlags)((long)MemFlags.READ_WRITE), SeedBufLen, IntPtr.Zero);
            LinkMem = openCLContext.CreateBuffer((MemFlags)((long)MemFlags.READ_WRITE), BufferLength, IntPtr.Zero);
            PGroupMem = openCLContext.CreateBuffer((MemFlags)((long)MemFlags.READ_WRITE), floatsize * PNumGroups, IntPtr.Zero);
            PResMem = openCLContext.CreateBuffer((MemFlags)((long)MemFlags.READ_WRITE), floatsize, IntPtr.Zero);
            SGroupMem = openCLContext.CreateBuffer((MemFlags)((long)MemFlags.READ_WRITE), floatsize * SNumGroups, IntPtr.Zero);
            SResMem = openCLContext.CreateBuffer((MemFlags)((long)MemFlags.READ_WRITE), floatsize, IntPtr.Zero);

            XhermYGroupMem = openCLContext.CreateBuffer((MemFlags)((long)MemFlags.READ_WRITE), floatsize * 2*XhermYNumGroups, IntPtr.Zero);
            XhermYresMem = openCLContext.CreateBuffer((MemFlags)((long)MemFlags.READ_WRITE), floatsize * 2, IntPtr.Zero);
            XhermYrespointer = System.Runtime.InteropServices.Marshal.AllocHGlobal(floatsize * 2);

            SeedVectorMem = openCLContext.CreateBuffer((MemFlags)((long)MemFlags.READ_WRITE), SeedVectorBuf.Length * sizeof(int), IntPtr.Zero);
            StorageMem = openCLContext.CreateBuffer((MemFlags)((long)MemFlags.READ_WRITE), linksize, IntPtr.Zero);
            dSmem = openCLContext.CreateBuffer((MemFlags)((long)MemFlags.READ_WRITE), floatsize, IntPtr.Zero);
            dSpointer = System.Runtime.InteropServices.Marshal.AllocHGlobal(floatsize);

            MyKernelKernel.SetArg(0, (byte)EvenOdd);
            MyKernelKernel.SetArg(1, (floattype)betagauge);
            MyKernelKernel.SetArg(2, (floattype)flux);
            MyKernelKernel.SetArg(3, SeedMem);
            MyKernelKernel.SetArg(4, LinkMem);

            PReductionKernel.SetArg(0, LinkMem);
            PReductionKernel.SetArg(1, PGroupMem);
            PReductionKernel.SetArg(2, PResMem);
            IntPtr ptr = new IntPtr(Plocalsize * floatsize);
            PReductionKernel.SetArg(3, ptr, IntPtr.Zero);

            SReductionKernel.SetArg(0, LinkMem);
            SReductionKernel.SetArg(1, SGroupMem);
            SReductionKernel.SetArg(2, SResMem);
            IntPtr ptr1 = new IntPtr(Slocalsize * floatsize);
            SReductionKernel.SetArg(3, ptr1, IntPtr.Zero);

            XhermYKernel.SetArg(2, XhermYresMem);
            XhermYKernel.SetArg(3, XhermYGroupMem);
            XhermYKernel.SetArg(4, new IntPtr(XhermYlocalsize*floatsize*2),IntPtr.Zero);
           

            openCLCQ.EnqueueWriteBuffer(SeedMem, true, 0, SeedBufLen, ipseed);
            openCLCQ.EnqueueWriteBuffer(LinkMem, true, 0, BufferLength, ip);
            openCLCQ.EnqueueWriteBuffer(SeedVectorMem, true, 0, SeedVectorBuf.Length*sizeof(int), ipseedvector);
            rhat0 = new Vector();
            //init BICGStab vectors
            phi = new Vector();

            r0 = new Vector();

            //rprev = new Vector();
            pi = new Vector();
            vi = new Vector();
            t = new Vector();
            s = new Vector();
           // xprev = new Vector();

           // vprev = new Vector();
           // pprev = new Vector();

            temp = new Vector();

            ri = new Vector();
            
            x = new Vector();

            //for fermion update

            chi = new Vector();

            CalculateS();
            double s1 = S[0];
            BackupLink(0, 0,1, 0, 1);
            CalculateS();
            double s2 = S[0];
            RestoreLink(0, 0, 1, 0, 1);
            CalculateS();
            double s3 = S[0];

            //MessageBox.Show(s1.ToString() + s2.ToString() + s3.ToString());
        }

        void UpdateLinkArray()
        {
            openCLCQ.EnqueueReadBuffer(LinkMem, true, 0, BufferLength, ip);
            System.Runtime.InteropServices.Marshal.Copy(ip, HostBuf, 0, HostBuf.Length);
        }
        
        ~Core()
        {

         
        }

        public floattype[] HostBuf,DebugBuf,PL,S;

        public IntPtr ip,ipseed,ipdebug,ipPLRes,ipSRes, ipseedvector;
        public int[] SeedBuf, SeedVectorBuf;

        Random RND;
        public void AllocBuffers()
        {
            HostBuf = new floattype[N * 4 * 9 * 2];
            DebugBuf = new floattype[N * 4 * 9 * 2];
            PL = new floattype[1];S = new floattype[1];

            SeedBuf = new int[N/2 * 4];
            SeedVectorBuf = new int[4 * N * 4];

            RND = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < N/2 * 4; i++) { SeedBuf[i] = Math.Abs(RND.Next()); }//Next(1000000) - changed not so far...
            for (int i = 0; i < 4 * N * 4; i++) { SeedVectorBuf[i] = Math.Abs(RND.Next()); }

            for (int i = 0; i < N * 4; i++) { 
                HostBuf[i * 9 * 2] = 1f; HostBuf[i * 9 * 2 + 8] = 1f; HostBuf[i * 9 * 2 + 16] = 1f;
                      
            }

            ip = System.Runtime.InteropServices.Marshal.AllocHGlobal(HostBuf.Length*sizeof(floattype));
            System.Runtime.InteropServices.Marshal.Copy(HostBuf, 0, ip, HostBuf.Length);

            ipseed = System.Runtime.InteropServices.Marshal.AllocHGlobal(SeedBuf.Length*sizeof(int));
            System.Runtime.InteropServices.Marshal.Copy(SeedBuf, 0, ipseed, SeedBuf.Length);

            ipPLRes = System.Runtime.InteropServices.Marshal.AllocHGlobal(floatsize);
            ipSRes = System.Runtime.InteropServices.Marshal.AllocHGlobal(floatsize);

            ipseedvector = System.Runtime.InteropServices.Marshal.AllocHGlobal(SeedVectorBuf.Length * sizeof(int));
            System.Runtime.InteropServices.Marshal.Copy(SeedVectorBuf, 0, ipseedvector, SeedVectorBuf.Length);
        }
        public floattype smth = 0;
        public bool sweep = false;
        public void Calculate()
        {

            MyKernelKernel.SetArg(1, (floattype)betagauge);
            MyKernelKernel.SetArg(2, (floattype)flux);

            Event calculationStart;
            Event calculationEnd;

            openCLCQ.EnqueueMarker(out calculationStart);

            //Go through even sites
                MyKernelKernel.SetArg(0, (byte)0);
                openCLCQ.EnqueueNDRangeKernel(MyKernelKernel, 1, null, new int[1] { N / 2 }, null);
            //through odd sites
                MyKernelKernel.SetArg(0, (byte)1);
                openCLCQ.EnqueueNDRangeKernel(MyKernelKernel, 1, null, new int[1] { N / 2 }, null);
               // openCLCQ.Finish();
                //openCLCQ.EnqueueReadBuffer(SeedMem, true, 0, SeedBufLen, ipseed);
                //System.Runtime.InteropServices.Marshal.Copy(ipseed, SeedBuf, 0, SeedBuf.Length);

                //openCLCQ.EnqueueReadBuffer(LinkMem, true, 0, BufferLength, ip);
                //System.Runtime.InteropServices.Marshal.Copy(ip, HostBuf, 0, HostBuf.Length);


                //openCLCQ.EnqueueNDRangeKernel(PReductionKernel, 1, null, new int[1] { SpaceSites }, new int[1] { Plocalsize });
                //openCLCQ.EnqueueReadBuffer(PResMem, true, 0, NumBytes, ipPLRes);
                //System.Runtime.InteropServices.Marshal.Copy(ipPLRes, PL, 0, 1);
                //PL[0] /= Nx * Ny * Nz;


                //openCLCQ.EnqueueNDRangeKernel(SReductionKernel, 1, null, new int[1] { N / 2 }, new int[1] { Slocalsize });
                //openCLCQ.EnqueueReadBuffer(SResMem, true, 0, floatsize, ipSRes);
                //System.Runtime.InteropServices.Marshal.Copy(ipSRes, S, 0, 1);
                //S[0] /= Nx * Ny * Nz * Nt * 6;//6 is the plaquette number in 4dspace!
                //S[0] = 1 - S[0];

                CalculateS();
                CalculatePL();

            openCLCQ.Finish();
            openCLCQ.EnqueueMarker(out calculationEnd);
            openCLCQ.Finish();

            ulong start = 0;
            ulong end = 0;
            try
            {
                calculationEnd.Wait();
                calculationStart.GetEventProfilingInfo(ProfilingInfo.END, out start);
                calculationEnd.GetEventProfilingInfo(ProfilingInfo.END, out end);
            }
            catch (OpenCLException)
            {
            }
            finally
            {
                CalculationTimeMS = (floattype)(end - start) / 1000000;
                calculationStart.Dispose();
                calculationEnd.Dispose();
            }

        }
        public floattype CalculationTimeMS;

        public void CalculateS()
        {
            openCLCQ.EnqueueNDRangeKernel(SReductionKernel, 1, null, new int[1] { N / 2 }, new int[1] { Slocalsize });
            openCLCQ.EnqueueReadBuffer(SResMem, true, 0, floatsize, ipSRes);
            System.Runtime.InteropServices.Marshal.Copy(ipSRes, S, 0, 1);
            S[0] /= Nx * Ny * Nz * Nt * 6;//6 is the plaquette number in 4dspace!
            S[0] = (1 - S[0]);//*=betagauge;
        }

        public void CalculatePL()
        {
            openCLCQ.EnqueueNDRangeKernel(PReductionKernel, 1, null, new int[1] { Nspace }, new int[1] { Plocalsize });
            openCLCQ.EnqueueReadBuffer(PResMem, true, 0, floatsize, ipPLRes);
            System.Runtime.InteropServices.Marshal.Copy(ipPLRes, PL, 0, 1);
            PL[0] /= Nx * Ny * Nz;
        }

        void FillLinkWith(Complex c)
        {
            FillLinkWithKernel.SetArg(0, c.x);
            FillLinkWithKernel.SetArg(1, c.y);
            FillLinkWithKernel.SetArg(2, LinkMem);
            openCLCQ.EnqueueNDRangeKernel(FillLinkWithKernel, 1, null, new int[1] { 4 * N }, null);
        }

        void MulD(Vector x, Vector result)//y=MulD(x)
        {
            DiralMulKernel.SetArg(0, LinkMem);
            DiralMulKernel.SetArg(1, x.buf);
            DiralMulKernel.SetArg(2, result.buf);
            openCLCQ.EnqueueNDRangeKernel(DiralMulKernel, 1, null, new int[1] { 4 * N }, null);
            openCLCQ.Finish();
            openCLCQ.Flush();
           // result.Updatearray();
        }

        void AXPY(Complex a, Vector x, Vector y,Vector result)
        {           
            AXPYKernel.SetArg(0, new floattype2(a.x,a.y));
            AXPYKernel.SetArg(1, x.buf);
            AXPYKernel.SetArg(2, y.buf);
            AXPYKernel.SetArg(3, result.buf);
            openCLCQ.EnqueueNDRangeKernel(AXPYKernel, 1, null, new int[1] { 4 * N }, null);
            openCLCQ.Finish();
            //result.Updatearray();

        }

        Complex V1hermV2(Vector v1, Vector v2)
        {
            floattype[] c = new floattype[2];
            for (int i = 0; i < 2; i++)
            {
                XhermYKernel.SetArg(0, v1.buf);
                XhermYKernel.SetArg(1, v2.buf);
                openCLCQ.EnqueueNDRangeKernel(XhermYKernel, 1, null, new int[1] { 4 * N }, new int[1] { XhermYlocalsize });
                openCLCQ.Finish();
                openCLCQ.Flush();

                openCLCQ.EnqueueReadBuffer(XhermYresMem, true, 0, floatsize * 2, XhermYrespointer);

                System.Runtime.InteropServices.Marshal.Copy(XhermYrespointer, c, 0, 2);
            }
            return new Complex(c[0], c[1]);
        }

        public void Prepare()
        {

            rhat0.FillWith(Complex.Zero);
           
            r0.FillWith(Complex.Zero);
            pi.FillWith(Complex.Zero);
            vi.FillWith(Complex.Zero);
            t.FillWith(Complex.Zero);
            s.FillWith(Complex.Zero);
            temp.FillWith(Complex.Zero);
            ri.FillWith(Complex.Zero);








            //make x = phi
            //AXPY(Complex.One, phi, ri, x);
            x.FillWith(Complex.Zero);


            vi.FillWith(Complex.Zero);
            pi.FillWith(Complex.Zero);

            MulD(x,temp);//here temp = 0!!!!!!!!

            AXPY(Complex.MinOne, temp , phi, ri);
            AXPY(Complex.MinOne, temp, phi,  rhat0);
            alpha = wi = Complex.One;
            roi = V1hermV2(rhat0, rhat0);
        }

        public int Iter()
        {
            int Niter = 100; Complex roprev;
            for (int i = 0; i < Niter; i++)
            {
                roprev = roi;
                roi = V1hermV2(rhat0, ri);
                if (roi.x == 0)
                    return -i;
                beta = (roi / roprev) * (alpha / wi);
                AXPY(-wi, vi, pi,temp);
                AXPY(beta, temp, ri,pi);

                MulD(pi, vi);
                alpha = roi / V1hermV2(rhat0, vi);//roprev
                if (alpha.x == 1 && alpha.y == 0) return i;
                AXPY(-alpha, vi, ri,s);

                MulD(s, t);
                wi = V1hermV2(t, s) / V1hermV2(t, t);
                AXPY(wi, s, x, temp);
                AXPY(alpha, pi, temp,x);
                 AXPY(-wi, t, s,ri);


                //rprev = ri; pprev = pi; vprev = vi; xprev = x;
                //roprev = roi; wprev = wi;
                 floattype r = (floattype)V1hermV2(ri, ri).Module();
                if ( r < 1E-5) return i;
            }

            return 100;
        }

        public floattype BiCGStab()
        {
            Complex sum = Complex.Zero;
            int Kmax = 200;
            for (int k = 0; k < Kmax; k++)
            {
                phi.FillWithRandom(true);

                //DateTime dt = DateTime.Now;
                Prepare();
                int it = Iter();
                if (it == -1) File.AppendAllText("log.txt", "bicgstabfails " + it.ToString() + " " + kappa.ToString() + " " + betagauge.ToString() + "\n");
               
                //MessageBox.Show((DateTime.Now - dt).TotalMilliseconds.ToString() + " i = " + it.ToString());
                sum += V1hermV2(phi, x)/new Complex(24*N*Kmax,0);
            }
            return sum.x;
            //MessageBox.Show(sum.ToString());
            //x.Updatearray();
            //phi.Updatearray();

        }

        floattype BackupLink(int x, int y, int z, int t, int mu)
        {
            BackupLinkKernel.SetArg(0, LinkMem);
            BackupLinkKernel.SetArg(1, x);
            BackupLinkKernel.SetArg(2, y);
            BackupLinkKernel.SetArg(3, z);
            BackupLinkKernel.SetArg(4, t);
            BackupLinkKernel.SetArg(5, mu);
            BackupLinkKernel.SetArg(6, StorageMem);
            BackupLinkKernel.SetArg(7, SeedMem);
            BackupLinkKernel.SetArg(8, dSmem);

            openCLCQ.EnqueueNDRangeKernel(BackupLinkKernel, 1, null, new int[1] {1 }, null);

            floattype[] ds = new floattype[1];
            openCLCQ.EnqueueReadBuffer(dSmem, true, 0, floatsize, dSpointer);
            System.Runtime.InteropServices.Marshal.Copy(dSpointer, ds, 0, 1);
            return ds[0];
        }

        void RestoreLink(int x, int y, int z, int t, int mu)
        {
            RestoreLinkKernel.SetArg(0, LinkMem);
            RestoreLinkKernel.SetArg(1, x);
            RestoreLinkKernel.SetArg(2, y);
            RestoreLinkKernel.SetArg(3, z);
            RestoreLinkKernel.SetArg(4, t);
            RestoreLinkKernel.SetArg(5, mu);
            RestoreLinkKernel.SetArg(6, StorageMem);

            openCLCQ.EnqueueNDRangeKernel(RestoreLinkKernel, 1, null, new int[1] { 1 }, null);
        }

        public void Dispose()
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(ip);
            System.Runtime.InteropServices.Marshal.FreeHGlobal(ipseed);
            SeedMem.Dispose();
            LinkMem.Dispose();
            PGroupMem.Dispose();
            MyKernelKernel.Dispose();
            MyKernelProgram.Dispose();
            openCLCQ.Dispose();
            openCLContext.Dispose();
        }

        public void SweepWithFermions()
        {
            bool accepted = false;
            for (int x=0;x<Nx;x++)
                for (int y =0;y<Ny;y++)
                    for (int z = 0;z<Nz;z++)
                        for (int t=0;t<Nt;t++)
                            for (int mu = 1; mu <= 4; mu++)
                            {
                                for (int hit = 0; hit < 1; hit++)
                                {
                                    accepted = false;
                                    while (!accepted)
                                    {
                                        //gauge only metropolis
                                        double dSg = betagauge * BackupLink(x, y, z, t, mu);//and Update it!!!
                                        double r = RND.NextDouble();
                                        double dS = (dSg);

                                        ////fermions
                                        //chi.FillWithRandom(true);
                                        //floattype Sf1 = V1hermV2(chi, chi).x;

                                        //MulD(chi, phi);

                                        //double dSg = betagauge * BackupLink(x, y, z, t, mu);//and Update it!!!

                                        //Prepare();

                                        //Iter();

                                        //floattype Sf2 = V1hermV2(this.x, this.x).x;

                                        //double r = RND.NextDouble();
                                        ////double dSg = (Sg2 - Sg1);
                                        //double dSf = (Sf2 - Sf1);
                                        //double dS = (dSg + dSf);


                                        if (r <= Math.Exp(-dS)) accepted = true;
                                        else

                                            RestoreLink(x, y, z, t, mu);
                                    }
                                }

                            }

        }

        void Update(int x, int y, int z, int t, int mu)
        {
            bool accepted = false;
            for (int hit = 0; hit < 1; hit++)
            {
                accepted = false;
                while (!accepted)
                {
                    //CalculateS();
                    //floattype Sg1 = S[0];

                    chi.FillWithRandom(true);
                    floattype Sf1 = V1hermV2(chi, chi).x;

                    MulD(chi, phi);

                    double dSg = betagauge * BackupLink(x, y, z, t, mu);//and Update it!!!

                    Prepare();

                    Iter();

                    floattype Sf2 = V1hermV2(this.x, this.x).x;

                    //CalculateS();
                    //floattype Sg2 = S[0];

                    double r = RND.NextDouble();
                    //double dSg = (Sg2 - Sg1);
                    double dSf = (Sf2 - Sf1);
                    double dS = (dSg + dSf);
                    if (r <= Math.Exp(-dS)) accepted = true;
                    else

                        RestoreLink(x, y, z, t, mu);
                }
            }

        }

        public double Sweep()
        {

            int Ns = Nx;
            //because of necessity of processing even and odd separately, Mu-Nu cycles are before
            double sum = 0; int n = 0;
            for (int Mu = 1; Mu <= 4; Mu++)
            {
                //even cycles. If N is even, it goes for:0,2,4...N-2
                for (int i = 0; i < Ns; i++)
                    for (int j = 0; j < Ns; j++)
                        for (int k = 0; k < Ns; k++)
                            for (int t = 0; t < Nt; t++)
                            {
                                if ((i + j + k + t) % 2 == 0)
                                {
                                    Update(i, j, k, t, Mu);
                                    //sum += GetDet(U[i, j, k, t, Mu]); n++;
                                }
                            }
                //odd cycles. If N is even, it goes for:1,3,5...N-1
                for (int i = 0; i < Ns; i++)
                    for (int j = 0; j < Ns; j++)
                        for (int k = 0; k < Ns; k++)
                            for (int t = 0; t < Nt; t++)
                            {
                                if ((i + j + k + t) % 2 == 1)
                                {
                                    Update(i, j, k, t, Mu);
                                   // sum += GetDet(U[i, j, k, t, Mu]); n++;
                                }
                            }
            }

            return sum / (Convert.ToDouble(n));

        }
    }
}
