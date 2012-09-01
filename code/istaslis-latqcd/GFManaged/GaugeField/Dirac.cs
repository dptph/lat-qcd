using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GaugeField
{
    struct Site
    {
        public int x, y, z, t;
        public Site(int X, int Y, int Z, int T)
        {
            x = X; y = Y; z = Z; t = T;
        }
        public static bool operator ==(Site s1, Site s2)
        {
            if (s1.x == s2.x && s1.y == s2.y && s1.z == s2.z && s1.t == s2.t) return true; else return false;
        }

        public static bool operator !=(Site s1, Site s2)
        {
            return !(s1 == s2);
        }
    }

    struct Vector
    {
        Complex[, ,] V;
        public int N;
        public Vector(int n)
        {
            N = n;
            V = new Complex[n, 4, 3];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < 4; j++)
                    for (int k = 0; k < 3; k++)
                        V[i, j, k] = Complex.Zero;

           rand  = new Random(DateTime.Now.Millisecond);
        }

        public Vector(int n, int seed)
        {
            N = n;
            V = new Complex[n, 4, 3];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < 4; j++)
                    for (int k = 0; k < 3; k++)
                        V[i, j, k] = Complex.Zero;

            rand = new Random(seed);
        }
        public Complex this[int n, byte alpha, byte a]
        { get { return V[n, alpha, a]; } set { V[n, alpha, a] = value; } }

        Random rand; //reuse this if you are generating many
        public Complex GetGaussRandom()
        {
            double u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
            double u2 = rand.NextDouble();
            double t = Math.Sqrt(-2.0 * Math.Log(u1));
            return new Complex(t * Math.Cos(2.0 * Math.PI * u2), t * Math.Sin(2.0 * Math.PI * u2));

            //return (rand.NextDouble() < 0.5) ? Complex.MinOne : Complex.One;
        }

        public void FillRandom()
        {
            Random r = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < N; i++)
                for (int j = 0; j < 4; j++)
                    for (int k = 0; k < 3; k++)
                        V[i, j, k] = new Complex(r.NextDouble(), r.NextDouble());
        }

        public void FillGaussRandom()
        {
          
            for (int i = 0; i < N; i++)
                for (int j = 0; j < 4; j++)
                    for (int k = 0; k < 3; k++)
                        V[i, j, k] = GetGaussRandom();//new Complex(rand.NextDouble()*4-2, rand.NextDouble()*4-2);// 
        }

        public void FillUnity()
        {

            for (int i = 0; i < N; i++)
                for (int j = 0; j < 4; j++)
                    for (int k = 0; k < 3; k++)
                        V[i, j, k] = new Complex(1, 0);
            //V[0, 0, 0] = Complex.One;
        }
    }

    class Dirac
    {
        Complex[,] OneMinusGamma1 = {{Complex.One,Complex.Zero,Complex.Zero,Complex.I},
                                     {Complex.Zero,Complex.One,Complex.I,Complex.Zero},
                                     {Complex.Zero,Complex.MinI,Complex.One, Complex.Zero},
                                     {Complex.MinI,Complex.Zero,Complex.Zero, Complex.One}};

        Complex[,] OneMinusGamma2 = {{Complex.One,Complex.Zero,Complex.Zero,Complex.One},
                                     {Complex.Zero,Complex.One,Complex.MinOne,Complex.Zero},
                                     {Complex.Zero,Complex.MinOne,Complex.One, Complex.Zero},
                                     {Complex.One,Complex.Zero,Complex.Zero, Complex.One}};

        Complex[,] OneMinusGamma3 = {{Complex.One,Complex.Zero,Complex.I,Complex.Zero},
                                     {Complex.Zero,Complex.One,Complex.Zero,Complex.MinI},
                                     {Complex.MinI,Complex.Zero,Complex.One, Complex.Zero},
                                     {Complex.Zero,Complex.I,Complex.Zero, Complex.One}};

        Complex[,] OneMinusGamma4 = {{Complex.One,Complex.Zero,Complex.MinOne,Complex.Zero},
                                     {Complex.Zero,Complex.One,Complex.Zero,Complex.MinOne},
                                     {Complex.MinOne,Complex.Zero,Complex.One, Complex.Zero},
                                     {Complex.Zero,Complex.MinOne,Complex.Zero, Complex.One}};

        Complex[][,] OneMinusgamma;

        Complex[,] OnePlusGamma1 = {{Complex.One,Complex.Zero,Complex.Zero,Complex.MinI},
                                     {Complex.Zero,Complex.One,Complex.MinI,Complex.Zero},
                                     {Complex.Zero,Complex.I,Complex.One, Complex.Zero},
                                     {Complex.I,Complex.Zero,Complex.Zero, Complex.One}};

        Complex[,] OnePlusGamma2 = {{Complex.One,Complex.Zero,Complex.Zero,Complex.MinOne},
                                     {Complex.Zero,Complex.One,Complex.One,Complex.Zero},
                                     {Complex.Zero,Complex.One,Complex.One, Complex.Zero},
                                     {Complex.MinOne,Complex.Zero,Complex.Zero, Complex.One}};

        Complex[,] OnePlusGamma3 = {{Complex.One,Complex.Zero,Complex.MinI,Complex.Zero},
                                     {Complex.Zero,Complex.One,Complex.Zero,Complex.I},
                                     {Complex.I,Complex.Zero,Complex.One, Complex.Zero},
                                     {Complex.Zero,Complex.MinI,Complex.Zero, Complex.One}};

        Complex[,] OnePlusGamma4 = {{Complex.One,Complex.Zero,Complex.One,Complex.Zero},
                                     {Complex.Zero,Complex.One,Complex.Zero,Complex.One},
                                     {Complex.One,Complex.Zero,Complex.One, Complex.Zero},
                                     {Complex.Zero,Complex.One,Complex.Zero, Complex.One}};

        Complex[][,] OnePlusgamma;//first index - Lorentz, 2,3 - Dirac
        SU3 U;
        double kappa = 0.1;//kappa = 0.125 then BiCGStab diverges!!!!
        public Dirac(SU3 GaugeField)
        {
            U = GaugeField;
            OneMinusgamma = new Complex[4][,] { OneMinusGamma1, OneMinusGamma2, OneMinusGamma3, OneMinusGamma4 };
            OnePlusgamma = new Complex[4][,] { OnePlusGamma1, OnePlusGamma2, OnePlusGamma3, OnePlusGamma4 };

            Nx = U.Ns; Nxy = Nx * Nx; Nxyz = Nxy * Nx; N = Nxyz * U.Nt;
            Ny = U.Ns; Nz = U.Ns; Nt = U.Nt;

            phi = new Vector(N);

            r0 = new Vector(N);
            rhat0 = new Vector(N);
            rprev = new Vector(N);
            pi = new Vector(N);
            vi = new Vector(N);
            t = new Vector(N);
            s = new Vector(N);
            xprev = new Vector(N);

            vprev = new Vector(N);
            pprev = new Vector(N);

            ri = new Vector(N);

            x0 = new Vector(N);
            x = new Vector(N);
        }

        public Complex D(Site n, Site m, byte alpha, byte beta, byte a, byte b)
        {
            Complex r = new Complex(); int c = 0;
            if (n == m && alpha == beta && a == b) { r += Complex.One; c++; }

            Site t;
            for (int i = 1; i <= 4; i++)
            {
                t = n;
                U.GetNode(ref t.x, ref t.y, ref t.z, ref t.t, i);
                if (t == m) { r += new Complex(-kappa, 0) * OneMinusgamma[i - 1][alpha, beta] * U.U[n.x, n.y, n.z, n.t, i].A[a, b]; 
                    c++;
                   // if (c > 1) 
                   //     System.Windows.Forms.MessageBox.Show("WTF?");
                }
            }
            for (int i = 1; i <= 4; i++)
            {
                t = n;
                U.GetNode(ref t.x, ref t.y, ref t.z, ref t.t, -i);
                if (t == m)
                {
                    r += new Complex(-kappa, 0) * OnePlusgamma[i - 1][alpha, beta] * U.U[t.x, t.y, t.z, t.t, i].A[b, a].Conj();//.HermConj().A[a, b]; - much more efficient!
                    c++; // if (c > 1)
                       // System.Windows.Forms.MessageBox.Show("WTF?");
                }
            }
            
            return r;
        }

        public static Complex V1hermV2(Vector Vector1, Vector Vector2)
        {
            Complex res = Complex.Zero;
            for (int i = 0; i < Vector1.N; i++)
                for (byte j = 0; j<4;j++)
                    for (byte k = 0; k<3;k++)
                res += (Vector1[i,j,k]).Conj() * Vector2[i,j,k];
            return res;
        }


        public Vector AXPY(Complex A, Vector X, Vector Y)
        {
            Vector res = new Vector(X.N);

            for (int i = 0; i < X.N; i++)
                for (byte j = 0; j < 4; j++)
                    for (byte k = 0; k < 3; k++)
                        res[i, j, k] = A * X[i, j, k] + Y[i, j, k];
            return res;
        }

        public static int N, Nxyz, Nxy,Nx,Ny,Nz,Nt;

        public Vector phi;

        Vector r0;
        Vector rhat0;
        Vector rprev;
        Vector pi;
        Vector vi;
        Vector t;
        Vector s;
        Vector xprev;

        Vector vprev;
        Vector pprev;

        public Vector ri;
        Complex roprev, roi, alpha, wprev, wi, beta;

        public Vector x0;
        public Vector x;
        public Vector MulDold(Vector V)
        {
            Vector res = new Vector(N);
            for (int i = 0; i < N; i++)
                for (byte j = 0; j < 4; j++)
                    for (byte k = 0; k < 3; k++)
                        for (int i1 = 0; i1 < N; i1++)
                            for (byte j1 = 0; j1 < 4; j1++)
                                for (byte k1 = 0; k1 < 3; k1++)
                                    res[i, j, k] += D(GetSite(i), GetSite(i1), j, j1, k, k1) * V[i1, j1, k1];
            return res;
        }

        int GetSiteNumber(Site t)
        {
            return t.t * Nxyz + t.z * Nxy + t.y * Nx + t.x;
        }


        public void GetNode(Site s, int dir)
        {
            if (dir == 1) s.x++; if (dir == -1) s.x--;
            if (dir == 2) s.y++; if (dir == -2) s.y--;
            if (dir == 3) s.z++; if (dir == -3) s.z--;
            if (dir == 4) s.t++; if (dir == -4) s.t--;

            if (s.x == -1) s.x = Nx - 1; if (s.x == Nx) s.x = 0;//comment if (x == Ns) x = 0; to use flux!!!
            if (s.y == -1) s.y = Ny - 1; if (s.y == Ny) s.y = 0;
            if (s.z == -1) s.z = Nz - 1; if (s.z == Nz) s.z = 0;
            if (s.t == -1) s.t = Nt - 1; if (s.t == Nt) s.t = 0;
        }

        public Vector MulD(Vector V)
        {
            //Site s = new Site(1, 1, 1, 1);
            //GetNode(s, 1);
            //System.Windows.Forms.MessageBox.Show(s.x.ToString());
            //Vector res = new Vector(N);
            //Site mainsite, adjacentsite; Complex g;
            //for (int i = 0; i < N; i++)
            //{
            //    mainsite = GetSite(i);
            //    for (int ai = 1; ai < 5; ai++)
            //    {
            //        adjacentsite = mainsite;
            //        U.GetNode(ref adjacentsite.x, ref adjacentsite.y, ref adjacentsite.z, ref adjacentsite.t, ai);
            //        int i1 = GetSiteNumber(adjacentsite);
            //        for (byte j = 0; j < 4; j++)
            //            for (byte j1 = 0; j1 < 4; j1++)
            //            {
            //                //g = OneMinusgamma[ai - 1][j, j1];
            //                //if (g.x != 0 || g.y != 0)
            //                //{

            //                    for (byte k = 0; k < 3; k++)
            //                        for (byte k1 = 0; k1 < 3; k1++)
            //                        {

            //                            res[i, j, k] += D(mainsite, adjacentsite, j, j1, k, k1) * V[i1, j1, k1];
            //                        }
            //             //   }
            //            }
            //    }
            //    for (int ai = 1; ai < 5; ai++)
            //    {
            //        adjacentsite = mainsite;
            //        U.GetNode(ref adjacentsite.x, ref adjacentsite.y, ref adjacentsite.z, ref adjacentsite.t, -ai);
            //        int i1 = GetSiteNumber(adjacentsite);
            //        for (byte j = 0; j < 4; j++)
            //            for (byte j1 = 0; j1 < 4; j1++)
            //            {
            //                //g = OneMinusgamma[ai - 1][j, j1];
            //                //if (g.x != 0 || g.y != 0)
            //                //{

            //                    for (byte k = 0; k < 3; k++)
            //                        for (byte k1 = 0; k1 < 3; k1++)
            //                            res[i, j, k] += D(mainsite, adjacentsite, j, j1, k, k1) * V[i1, j1, k1];
            //              //  }
            //            }
            //    }

            //}

            //Site[] AdjacentSites = new Site[9]; Complex g; int mu = 0;
            //for (int i = 0; i < N; i++)
            //{
            //    AdjacentSites[0] = GetSite(i);
            //    for (int ai = 1; ai < 5; ai++)
            //    {
            //        AdjacentSites[ai] = AdjacentSites[0];
            //        U.GetNode(ref AdjacentSites[ai].x, ref AdjacentSites[ai].y, ref AdjacentSites[ai].z, ref AdjacentSites[ai].t, ai);
            //    }
            //    for (int ai = 5; ai < 9; ai++)
            //    {
            //        AdjacentSites[ai] = AdjacentSites[0];
            //        U.GetNode(ref AdjacentSites[ai].x, ref AdjacentSites[ai].y, ref AdjacentSites[ai].z, ref AdjacentSites[ai].t, -(ai - 4));
            //    }

            //    for (int ai = 0; ai < 9; ai++)
            //    {
            //        mu = (ai - 1) % 4;
            //        int i1 = GetSiteNumber(AdjacentSites[ai]);
            //        for (byte j = 0; j < 4; j++)
            //            for (byte j1 = 0; j1 < 4; j1++)
            //            {
            //                if (ai == 0) g = Complex.One;
            //                else
            //                    g = OneMinusgamma[mu][j, j1];
            //                if (g.x != 0 || g.y != 0)
            //                    for (byte k = 0; k < 3; k++)
            //                        for (byte k1 = 0; k1 < 3; k1++)
            //                        {
            //                            res[i, j, k] += D(AdjacentSites[0], AdjacentSites[ai], j, j1, k, k1) * V[i1, j1, k1];
            //                        }

            //            }
            //    }
            //}
            //return res;


            Vector res = new Vector(N);
            Site[] AdjacentSites = new Site[9];
            Link[] AdjacentLinks = new Link[9];
            Complex g; double flipsign = 1.0; int mu = 0; Complex c;
            for (int i = 0; i < N; i++)
            {
                AdjacentSites[0] = GetSite(i);
                AdjacentLinks[0] = new Link(true);
                for (int ai = 1; ai < 5; ai++)
                {
                    mu = (ai - 1) % 4 + 1;
                    AdjacentSites[ai] = AdjacentSites[0];
                    U.GetNode(ref AdjacentSites[ai].x, ref AdjacentSites[ai].y, ref AdjacentSites[ai].z, ref AdjacentSites[ai].t, ai);
                    AdjacentLinks[ai] = U.U[AdjacentSites[ai].x, AdjacentSites[ai].y, AdjacentSites[ai].z, AdjacentSites[ai].t, mu];
                }
                for (int ai = 5; ai < 9; ai++)
                {
                    mu = (ai - 1) % 4 + 1;
                    AdjacentSites[ai] = AdjacentSites[0];
                    U.GetNode(ref AdjacentSites[ai].x, ref AdjacentSites[ai].y, ref AdjacentSites[ai].z, ref AdjacentSites[ai].t, -(ai - 4));
                    AdjacentLinks[ai] = U.U[AdjacentSites[ai].x, AdjacentSites[ai].y, AdjacentSites[ai].z, AdjacentSites[ai].t, mu];
                }

                for (int ai = 0; ai < 9; ai++)
                {
                    flipsign = ai < 5 ? 1.0 : -1.0;
                    mu = (ai - 1) % 4 + 1;
                    int i1 = GetSiteNumber(AdjacentSites[ai]);
                    for (byte j = 0; j < 4; j++)
                        for (byte j1 = 0; j1 < 4; j1++)
                        {
                            if (ai == 0) g = Complex.One;
                            else
                                g = (flipsign == 1.0) ? OneMinusgamma[mu - 1][j, j1] : OnePlusgamma[mu - 1][j, j1];
                            if (g.x != 0 || g.y != 0)
                            {
                                c = ai == 0 ? Complex.One : new Complex(-kappa, 0) * g;
                                for (byte k = 0; k < 3; k++)
                                    for (byte k1 = 0; k1 < 3; k1++)
                                    {
                                        if (ai == 0 && j == j1 && k == k1) res[i, j, k] += Complex.One * V[i1, j1, k1];
                                        else
                                        {
                                            if (ai != 0)
                                            {
                                                if (flipsign == 1.0)
                                                    res[i, j, k] += c * AdjacentLinks[ai].A[k, k1] * V[i1, j1, k1];
                                                else
                                                    res[i, j, k] += c * AdjacentLinks[ai].A[k1, k].Conj() * V[i1, j1, k1];//u.HermConj().A[k, k1]
                                            }
                                        }
                                    }
                            }
                        }
                }
            }
            return res;
        }
        public Vector MulDprime(Vector V)
        {
            Vector res = new Vector(N);
            Site[] AdjacentSites = new Site[9];
            Link[] AdjacentLinks = new Link[9];
            Complex g; double flipsign = 1.0; int mu = 0; Complex c;
            for (int i = 0; i < N; i++)
            {

                AdjacentSites[0] = GetSite(i);
                AdjacentLinks[0] = AdjacentSites[0] == U.Xsite ? U.Uprime : new Link(true);
                for (int ai = 1; ai < 5; ai++)
                {
                    mu = (ai - 1) % 4 + 1;
                    AdjacentSites[ai] = AdjacentSites[0];
                    U.GetNode(ref AdjacentSites[ai].x, ref AdjacentSites[ai].y, ref AdjacentSites[ai].z, ref AdjacentSites[ai].t, ai);
                    AdjacentLinks[ai] = AdjacentSites[ai] == U.Xsite ? U.Uprime : U.U[AdjacentSites[ai].x, AdjacentSites[ai].y, AdjacentSites[ai].z, AdjacentSites[ai].t, mu];
                }
                for (int ai = 5; ai < 9; ai++)
                {
                    mu = (ai - 1) % 4 + 1;
                    AdjacentSites[ai] = AdjacentSites[0];
                    U.GetNode(ref AdjacentSites[ai].x, ref AdjacentSites[ai].y, ref AdjacentSites[ai].z, ref AdjacentSites[ai].t, -(ai - 4));
                    AdjacentLinks[ai] = AdjacentSites[ai] == U.Xsite ? U.Uprime : U.U[AdjacentSites[ai].x, AdjacentSites[ai].y, AdjacentSites[ai].z, AdjacentSites[ai].t, mu];
                }

                for (int ai = 0; ai < 9; ai++)
                {
                    flipsign = ai < 5 ? 1.0 : -1.0;
                    mu = (ai - 1) % 4 + 1;
                    int i1 = GetSiteNumber(AdjacentSites[ai]);
                    for (byte j = 0; j < 4; j++)
                        for (byte j1 = 0; j1 < 4; j1++)
                        {
                            if (ai == 0) g = Complex.One;
                            else
                                g = (flipsign == 1.0) ? OneMinusgamma[mu - 1][j, j1] : OnePlusgamma[mu - 1][j, j1];
                            if (g.x != 0 || g.y != 0)
                            {
                                c = ai == 0 ? Complex.One : new Complex(-kappa, 0) * g;
                                for (byte k = 0; k < 3; k++)
                                    for (byte k1 = 0; k1 < 3; k1++)
                                    {
                                        if (ai == 0 && j == j1 && k == k1) res[i, j, k] += Complex.One * V[i1, j1, k1];
                                        else
                                        {
                                            if (ai != 0)
                                            {
                                                if (flipsign == 1.0)
                                                    res[i, j, k] += c * AdjacentLinks[ai].A[k, k1] * V[i1, j1, k1];
                                                else
                                                    res[i, j, k] += c * AdjacentLinks[ai].A[k1, k].Conj() * V[i1, j1, k1];//u.HermConj().A[k, k1]
                                            }
                                        }
                                    }
                            }
                        }
                }
            }
            return res;
        }
        Site GetSite(int n)
        {
            Site t = new Site(); int tempn;
            t.t = n / Nxyz; tempn = n - t.t * Nxyz;
            t.z = tempn / Nxy; tempn -= t.z * Nxy;
            t.y = tempn / Nx;
            t.x = tempn - t.y * Nx;
            return t;
        }

        public void Prepare()
        {
            x = phi;
            ri = AXPY(Complex.MinOne, MulD(x0), phi);
            rhat0 = ri;
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
                pi = AXPY(beta, AXPY(-wi, vi, pi), ri);

                vi = MulD(pi);
                alpha = roi / V1hermV2(rhat0, vi);//roprev
                s = AXPY(-alpha, vi, ri);

                t = MulD(s);
                wi = V1hermV2(t, s) / V1hermV2(t, t);
                x = AXPY(alpha, pi, AXPY(wi, s, x));
                ri = AXPY(-wi, t, s);


                //rprev = ri; pprev = pi; vprev = vi; xprev = x;
                //roprev = roi; wprev = wi;

                if (Dirac.V1hermV2(ri, ri).Module() < 1E-5) return i;
            }

            return 100;
        }

        //public void Prepare()
        //{
        //    rprev = AXPY(Complex.MinOne, MulD(x0), phi);
        //    rhat0 = rprev;
        //    alpha = wprev = Complex.One;
        //    roprev = V1hermV2(rhat0, rhat0);
        //}

        //public int Iter()
        //{
        //    int Niter = 100;
        //    for (int i = 0; i < Niter; i++)
        //    {
        //        roi = V1hermV2(rhat0, rprev);
        //        if (roi.x == 0)
        //            return -i;
        //        beta = (roi / roprev) * (alpha / wprev);
        //        pi = AXPY(beta, AXPY(-wprev, vprev, pprev), rprev);

        //        vi = MulD(pi);
        //        alpha = roi / V1hermV2(rhat0, vi);//roprev
        //        s = AXPY(-alpha, vi, rprev);

        //        t = MulD(s);
        //        wi = V1hermV2(t, s) / V1hermV2(t, t);
        //        x = AXPY(alpha, pi, AXPY(wi, s, xprev));
        //        ri = AXPY(-wi, t, s);


        //        rprev = ri; pprev = pi; vprev = vi; xprev = x;
        //        roprev = roi; wprev = wi;

        //        if (Dirac.V1hermV2(ri, ri).Module() < 1E-5) return i;
        //    }

        //    return 100;
        //}

        public int IterPrime()
        {
            int Niter = 100; Complex roprev;
            for (int i = 0; i < Niter; i++)
            {
                roprev = roi;
                roi = V1hermV2(rhat0, ri);
                if (roi.x == 0)
                    return -i;
                beta = (roi / roprev) * (alpha / wi);
                pi = AXPY(beta, AXPY(-wi, vi, pi), ri);

                vi = MulDprime(pi);
                alpha = roi / V1hermV2(rhat0, vi);//roprev
                if (alpha.x==1 && alpha.y == 0) break;
                s = AXPY(-alpha, vi, ri);

                t = MulDprime(s);
                wi = V1hermV2(t, s) / V1hermV2(t, t);
                x = AXPY(alpha, pi, AXPY(wi, s, x));
                ri = AXPY(-wi, t, s);


                //rprev = ri; pprev = pi; vprev = vi; xprev = x;
                //roprev = roi; wprev = wi;

                if (Dirac.V1hermV2(ri, ri).Module() < 1E-1) return i;
            }

            return Niter;
        }

    }
}
