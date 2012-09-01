using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GaugeField
{

    struct Complex
    {
        public static Complex One = new Complex(1, 0);
        public static Complex MinOne = new Complex(-1, 0);
        public static Complex Zero = new Complex(0, 0);
        public static Complex I = new Complex(0, 1);
        public static Complex MinI = new Complex(0, -1);

        public double x, y;
        public Complex(double X, double Y)
        {
            x = X; y = Y;
        }
        static public Complex operator *(Complex A, Complex B)
        {
            return new Complex(A.x * B.x - A.y * B.y, A.x * B.y + A.y * B.x);
        }
        public static Complex operator /(Complex A, Complex B)//THIS METHOD WAS ADDED WITH DIRAC!!!!
        {
            return new Complex((A.x * B.x + A.y * B.y) / (B.x * B.x + B.y * B.y), (A.y * B.x - A.x * B.y) / (B.x * B.x + B.y * B.y));
        }
        static public Complex operator -(Complex A, Complex B)
        {
            return new Complex(A.x - B.x, A.y - B.y);
        }
        static public Complex operator +(Complex A, Complex B)
        {
            return new Complex(A.x+B.x,A.y+B.y);
        }
        public static Complex operator -(Complex A)//THIS METHOD WAS ADDED WITH DIRAC!!!!
        {
            return new Complex(-A.x, -A.y);
        }

        public override string ToString()
        {
            return x.ToString() + "+" + y.ToString() + "i";
        }

        public Complex Conj()
        {
            return new Complex(x,-y);
        }
        public double Module()
        {
            return Math.Sqrt(x * x + y * y);
        }
    }

    struct Link
    {
        //public double a0, a1, a2, a3;
        public Complex[,] A;

        public Link(bool identity)
        {
            A = new Complex[SU3.d,SU3.d];
                for (int i = 0; i < SU3.d; i++)
                    for (int j = 0; j < SU3.d; j++)
                        A[i, j] = new Complex();
                if (identity) for (int i = 0; i < SU3.d; i++) A[i, i].x = 1;
        }
        
        public Link HermConj()
        {
            Link L = new Link(false);
            for (int i = 0; i < SU3.d; i++)
                for (int j = 0; j < SU3.d; j++)
                    L.A[i, j] = A[j, i].Conj();

            
            return L;
        }

        public Complex[,] GetMatrix()
        {
            return A;
            /*return new Complex[2, 2] { {new Complex(a0,a3), new Complex(a2,a1)},
                                      {new Complex(-a2,a1), new Complex(a0,-a3)}};*/
        }
    }
   
    class SU3
    {
        static Complex c0 = new Complex(0, 0);
        static Complex c1 = new Complex(1, 0);

        public const int d = 3;

        public Link[, , , ,] U;
        public int Ns, Nt;
        double beta = 0;

        Complex exppos2xi;
        Complex expneg2xi;
        Complex expposxi;
        Complex expnegxi;

        Link Temp = new Link(true);

        Random RND;
        const int a =97;// 55;  //a>b!!!
        const int b = 33;//24;

        double[] Last;
        int LastPos = 0;

        void InitializeRandom(int seed)
        {
           RND = new Random(seed);

            Last = new double[a];
            for (int i=0;i<a;i++)  Last[i]=RND.NextDouble();
        }

        public double GetRandom()
        {
            int posa = LastPos + 1;//previous in queue
            int posb = (LastPos - b + a) % a;
            double Xka = Last[posa]; double Xkb = Last[posb]; double Xk;
            if (Xka > Xkb) { Xk = Xka - Xkb; } else { Xk = Xka - Xkb + 1; }
            LastPos++; if (LastPos == a-1) LastPos = 0;
            Last[LastPos] = Xk;
            return Xk;
           
          //  return RND.NextDouble();

        }

        

        public Link Mul(Link U, Link V)
        {
            Link R = new Link(false);

            for (int i = 0; i < SU3.d; i++)
                for (int j = 0; j < SU3.d; j++)
                    for (int k = 0; k < SU3.d; k++)
                        R.A[i, j] += U.A[i, k] * V.A[k, j];

           /* R.a0 = U.a0 * V.a0 - U.a1 * V.a1 - U.a2 * V.a2 - U.a3 * V.a3;
            R.a1 = U.a0 * V.a1 + U.a1 * V.a0 - U.a2 * V.a3 + U.a3 * V.a2;
            R.a2 = U.a0 * V.a2 + U.a2 * V.a0 - U.a3 * V.a1 + U.a1 * V.a3;
            R.a3 = U.a0 * V.a3 + U.a3 * V.a0 - U.a1 * V.a2 + U.a2 * V.a1;
            */

            /*

            //multiplying the matrices
            U.U1 = Link1.U1 * Link2.U1 + Link1.U2 * Link2.V1 + Link1.U3 * Link2.W1();
            U.U2 = Link1.U1 * Link2.U2 + Link1.U2 * Link2.V2 + Link1.U3 * Link2.W2();
            U.U3 = Link1.U1 * Link2.U3 + Link1.U3 * Link2.V1 + Link1.U3 * Link2.W3();

            U.V1 = Link1.V1 * Link2.U1 + Link1.V2 * Link2.V1 + Link1.V3 * Link2.W1();
            U.V2 = Link1.V1 * Link2.U2 + Link1.V2 * Link2.V2 + Link1.V3 * Link2.W2();
            U.V3 = Link1.V1 * Link2.U3 + Link1.V3 * Link2.V1 + Link1.V3 * Link2.W3();
            */
            return R;
        }

        public SU3(int Nspace, int Ntemp, double b, double flux, bool ColdStart)
        {
            Ns = Nspace; Nt = Ntemp; beta = b;
            U = new Link[Ns+1, Ns, Ns, Nt, 5];//x1,x2,x3,x4 (x0 omitted)
            InitializeRandom(DateTime.Now.Millisecond);

            if (ColdStart)
            {
                for (int d = 1; d <= 4; d++)
                    for (int i = 0; i < Ns; i++)
                        for (int j = 0; j < Ns; j++)
                            for (int k = 0; k < Ns; k++)
                                for (int t = 0; t < Nt; t++)
                                {
                                  /*  U[i, j, k, t, d].a0 = 1;
                                    U[i, j, k, t, d].a1 = 0;
                                    U[i, j, k, t, d].a2 = 0;
                                    U[i, j, k, t, d].a3 = 0;
                                    */
                                    U[i, j, k, t, d] = new Link(true);
                                }
            }
            else
            {
                for (int d = 1; d <= 4; d++)
                    for (int i = 0; i < Ns; i++)
                        for (int j = 0; j < Ns; j++)
                            for (int k = 0; k < Ns; k++)
                                for (int t = 0; t < Nt; t++)
                                {
                                  
                                    //for SU(2)

                                    double x0 = GetRandom() * 2 - 1; 
                                    double x1 = GetRandom() - 0.5, x2 = GetRandom() - 0.5, x3 = GetRandom() - 0.5;
                                    double tlen = Math.Sqrt(x1 * x1 + x2 * x2 + x3 * x3);
                                    double len = Math.Sqrt(1-x0 * x0);

                                    x1 *= len/tlen; x2 *= len/tlen; x3 *= len/tlen;
                                   /* U[i, j, k, t, d].a0 = x0;
                                    U[i, j, k, t, d].a1 = x1;
                                    U[i, j, k, t, d].a2 = x2;
                                    U[i, j, k, t, d].a3 = x3;
                                    */
                                    
                                    //Orthogonalize2(ref U[i, j, k, t, d]);
                                   // double det = GetDet(U[i, j, k, t, d].GetMatrix());
                                }
            }
            //exppos2xi = new Complex(Math.Cos(flux), Math.Sin(flux));
            expposxi = new Complex(Math.Cos(flux/2), Math.Sin(flux/2));
            //expneg2xi = new Complex(Math.Cos(flux), -Math.Sin(flux));
            expnegxi = new Complex(Math.Cos(flux / 2),- Math.Sin(flux / 2));

            Temp.A[0, 0] = expposxi;
            Temp.A[1, 1] = expnegxi;
        }
        public void GetNode(ref int x, ref int y, ref int z, ref int t, int dir)
        {
            if (dir == 1) x++; if (dir == -1) x--;
            if (dir == 2) y++; if (dir == -2) y--;
            if (dir == 3) z++; if (dir == -3) z--;
            if (dir == 4) t++; if (dir == -4) t--;

            if (x == -1) x = Ns - 1; if (x == Ns) x = 0;//comment if (x == Ns) x = 0; to use flux!!!
            if (y == -1) y = Ns - 1; if (y == Ns) y = 0;
            if (z == -1) z = Ns - 1; if (z == Ns) z = 0;
            if (t == -1) t = Nt - 1; if (t == Nt) t = 0;
        }

        public double ReTr(Link L)
        {
            Complex C= new Complex();
            for (int i = 0; i < SU3.d; i++)
                C += L.A[i, i];
            return C.x;
        }

        double dSF()
        {
            Dirac D = new Dirac(this);
            Vector chi = new Vector(Dirac.N, DateTime.Now.Millisecond);
            D.phi = D.MulD(chi);
            

            return 0;
        }

        Link GetRandomMetroLink()
        {
            double e = 0.1;

            double x0m = 0, x1m = 0, x2m = 0, x3m = 0; Link X = new Link(true);

            for (int i = 0; i < d; i++)
                for (int j = i + 1; j < d; j++)
                {

                    double r0 = GetRandom() - 0.5, r1 = GetRandom() - 0.5, r2 = GetRandom() - 0.5, r3 = GetRandom() - 0.5;
                    x0m = Math.Sqrt(1 - e * e);
                    // if (r0 < 0) x0m = -x0m;
                    double len = Math.Sqrt(r1 * r1 + r2 * r2 + r3 * r3);
                    x1m = e * r1 / len; x2m = e * r2 / len; x3m = e * r3 / len;



                    Complex A00 = new Complex(x0m, x3m);
                    Complex A01 = new Complex(x2m, x1m);
                    Complex A10 = new Complex(-x2m, x1m);
                    Complex A11 = new Complex(x0m, -x3m);

                    Link T = new Link(true);
                    T.A[i, i] = A00;
                    T.A[i, j] = A01;
                    T.A[j, i] = A10;
                    T.A[j, j] = A11;
                    X = Mul(T, X);
                }
            return X;
        }

        Link GetSumOfStaples(int x, int y, int z, int t, int dir)
        {
            Link A = new Link(false);
            //calculating staples  
            for (int i = 1; i <= 4; i++)
            {

                if (i != dir)
                {
                    //create staple
                    /*   r+i----<----r+dir+i
                     *     |         |
                     *  ^i \/        ^    
                     *     |         |
                           r  ->dir r+dir
                     * 
                     */
                    //up
                    Link A1 = new Link(false);
                    int x1 = x, y1 = y, z1 = z, t1 = t; int x2 = x, y2 = y, z2 = z, t2 = t;
                    GetNode(ref x1, ref y1, ref z1, ref t1, dir);
                    GetNode(ref x2, ref y2, ref z2, ref t2, i);
                    Link U1, U2, U3;
                    U1 = U[x1, y1, z1, t1, i];
                    U2 = U[x2, y2, z2, t2, dir];
                    U3 = U[x, y, z, t, i];
                    if (x1 == Ns) { if (i == 2) U1 = Mul(Temp, Mul(U[0, y1, z1, t1, i], Temp)); else U1 = U[0, y1, z1, t1, i]; }
                    if (x2 == Ns)
                    {
                        if (dir == 2) U2 = Mul(Temp, Mul(U[0, y2, z2, t2, dir], Temp)); else U2 = U[0, y2, z2, t2, dir];
                    }
                    //if (x == Ns) {
                    //     if (i == 2) U3 = Mul(Temp, Mul(U[0, y, z, t, i], Temp)); else U3 = U[0, y, z, t, i]; }
                    A1 = Mul(U1, U2.HermConj());//CONJ!
                    A1 = Mul(A1, U3.HermConj());//CONJ!

                    /*     r  ->dir r+dir
                     *     |         |
                     *  ^i ^         \/    
                     *     |         |
                     *   r-i----<----r+dir-i
                     */
                    //down
                    Link A2 = new Link(false);
                    x1 = x; y1 = y; z1 = z; t1 = t;
                    GetNode(ref x1, ref y1, ref z1, ref t1, -i);
                    x2 = x1; y2 = y1; z2 = z1; t2 = t1;//2 - r-i
                    GetNode(ref x1, ref y1, ref z1, ref t1, dir);
                    U1 = U[x1, y1, z1, t1, i];
                    U2 = U[x2, y2, z2, t2, dir];
                    U3 = U[x2, y2, z2, t2, i];

                    if (x1 == Ns)
                    {
                        if (i == 2)
                            U1 = Mul(Temp, Mul(U[0, y1, z1, t1, i], Temp));
                        else U1 = U[0, y1, z1, t1, i];
                    }
                    //if (x2 == Ns) {
                    //    if (dir == 2) U2 = Mul(Temp, Mul(U[0, y2, z2, t2, dir], Temp)); else U2 =U[0, y2, z2, t2, dir];}
                    //if (x2 == Ns)
                    //{  if (i == 2) U3 = Mul(Temp, Mul(U[0, y2, z2, t2, i], Temp)); else U3 = U[0, y2, z2, t2, i];}

                    A2 = Mul(U1.HermConj(), U2.HermConj());//CONJ!
                    A2 = Mul(A2, U3);

                    for (int m = 0; m < SU3.d; m++)
                        for (int n = 0; n < SU3.d; n++)
                            A.A[m, n] += A1.A[m, n] + A2.A[m, n];
                }
            }
            return A;
        }

        public Link Uprime = new Link(true);
        public Site Xsite = new Site();
        public int Xdir = 0;

        public void Update(int x, int y, int z, int t, int dir, bool withOR)
        {

            Link A = GetSumOfStaples(x, y, z, t, dir);
            //here A is the sum of staples!

            //heatbath
            //Link W = Mul(U[x, y, z, t, dir], A); double det;
            //Link X = new Link(true);
            ////for heat-bath
            //for (int i = 0; i < d; i++)
            //    for (int j = i + 1; j < d; j++)
            //    {
            //        Complex a = W.A[i, i];
            //        Complex B = W.A[i, j];
            //        Complex C = W.A[j, i];
            //        Complex D = W.A[j, j];

            //        // det = (W.A[i, i] * W.A[j, j] - W.A[i, j] * W.A[j, i]).x;

            //        double x0 = 1 / 2f * (a.x + D.x);
            //        double x1 = 1 / 2f * (B.y + C.y);
            //        double x2 = 1 / 2f * (B.x - C.x);
            //        double x3 = 1 / 2f * (a.y - D.y);

            //        //  det = GetDet(A);
            //        det = x0 * x0 + x1 * x1 + x2 * x2 + x3 * x3;
            //        Link a_ = GetHeatBath(det, i, j);
            //        Link r = new Link(true);

            //        det = Math.Sqrt(det);
            //        x0 /= det;
            //        x1 /= det;
            //        x2 /= det;
            //        x3 /= det;


            //        r.A[i, i] = new Complex(x0, x3);
            //        r.A[i, j] = new Complex(x2, x1);
            //        r.A[j, i] = new Complex(-x2, x1);
            //        r.A[j, j] = new Complex(x0, -x3);


            //        r = Mul(a_, r.HermConj());

            //        W = Mul(r, W);
            //        X = Mul(r, X);
            //    }

            //U[x, y, z, t, dir] = Mul(X, U[x, y, z, t, dir]);
            //
              
             //for overrelaxation
            //if (GetRandom() < 0.1)
 /*           if (withOR)
            {
                //for (int or = 0; or < 0; or++)
                //{
                    //calculating determinant of SU(2)
                    double det = Math.Pow(GetDet(A),1.0/3.0);
                    for (int i = 0; i < SU3.d; i++)
                        for (int j = 0; j < SU3.d; j++)
                        {
                            A.A[i, j].x /= det;
                            A.A[i, j].y /= det;
                        }

                    A = Mul(A.HermConj(), Mul(U[x, y, z, t, dir].HermConj(), A.HermConj()));
                    Orthogonalize(ref A);
                    U[x, y, z, t, dir]=A;
            }
            else
          {*/
            //for Metropolis

            //for (int hit = 0; hit <= 10; hit++)
            //{
            //    bool accepted = false; double dSG; Link X = new Link(true);
            //    while (!accepted)
            //    {

            //        X = GetRandomMetroLink();
            //        //-I
            //        for (int i = 0; i < d; i++) X.A[i, i].x--;

            //        dSG = -beta / SU3.d * ReTr(Mul(X, Mul(U[x, y, z, t, dir], A)));

            //        double r = GetRandom();
            //        if (r <= Math.Exp(-dSG)) accepted = true;
            //    }

            //    for (int i = 0; i < d; i++) X.A[i, i].x++;
            //    U[x, y, z, t, dir] = Mul(X, U[x, y, z, t, dir]);
            //}

            //for poor dynamical fermions
            for (int hit = 0; hit <= 10; hit++)
            {
                bool accepted = false; double dSG; Link X = new Link(true);
                while (!accepted)
                {

                    X = GetRandomMetroLink();
                    Xsite = new Site(x, y, z, t); Xdir = dir;
                    Uprime = Mul(X, U[x, y, z, t, dir]);

                    Dirac D = new Dirac(this);
                    Vector chi = new Vector(Dirac.N);
                    chi.FillGaussRandom();

                    D.phi = D.MulD(chi);

                    double Fold = Dirac.V1hermV2(chi, chi).x;

                    D.Prepare();
                    D.IterPrime();

                    //here - D.x = D^-1*phi

                    double Fnew = Dirac.V1hermV2(D.x, D.x).x;

                    double dSF =  Fnew - Fold;

                    //X-I
                    for (int i = 0; i < d; i++) X.A[i, i].x--;

                    dSG = -beta / SU3.d * ReTr(Mul(X, Mul(U[x, y, z, t, dir], A)));

                    double r = GetRandom();
                    if (r <= Math.Exp(-(dSG + dSF))) accepted = true;
                }

                for (int i = 0; i < d; i++) X.A[i, i].x++;
                U[x, y, z, t, dir] = Mul(X, U[x, y, z, t, dir]);
            }

        }


        public void Orthogonalize(ref Link U)
        {
           // double x0 = U.a0, x1 = U.a1, x2 = U.a2, x3 = U.a3;
            //double len = Math.Sqrt(x1 * x1 + x2 * x2 + x3 * x3);
            //x1 /= len; x2 /= len; x3 /= len;
          //  x0 = Math.Sqrt(1 - x1 * x1 - x2 * x2 - x3 * x3);
          //  if (x0<=1) U.a0 = x0;
            double module = Math.Sqrt(U.A[0, 0].Module() * U.A[0, 0].Module() +
                                        U.A[0, 1].Module() * U.A[0, 1].Module() +
                                            U.A[0, 2].Module() * U.A[0, 2].Module());
             
            //U'=U/|U|
            U.A[0,0].x /= module; U.A[0,0].y /= module;
            U.A[0,1].x /= module; U.A[0,1].y /= module;
            U.A[0,2].x /= module; U.A[0,2].y /= module;
            
            //V'=V-(V U'*)U'
            Complex VU = U.A[1,0] * U.A[0,0].Conj() + U.A[1,1] * U.A[0,1].Conj() +U.A[1,2] * U.A[0,2].Conj();

            U.A[1,0]=U.A[1,0]-VU*U.A[0,0];
            U.A[1,1]=U.A[1,1]-VU*U.A[0,1];
            U.A[1,2]=U.A[1,2]-VU*U.A[0,2];

            //V'=V/|V|
            module = Math.Sqrt(U.A[1, 0].Module() * U.A[1, 0].Module() +
                                    U.A[1, 1].Module() * U.A[1, 1].Module() +
                                        U.A[1, 2].Module()*U.A[1, 2].Module());
            
            U.A[1,0].x /= module; U.A[1,0].y /= module;
            U.A[1,1].x /= module; U.A[1,1].y /= module;
            U.A[1,2].x /= module; U.A[1,2].y /= module;

            U.A[2,0]=(U.A[0,1] * U.A[1,2] - U.A[0,2] * U.A[1,1]).Conj();
            U.A[2,1]=(U.A[0,2] * U.A[1,0] - U.A[0,0] * U.A[1,2]).Conj();
            U.A[2,2]=(U.A[0,0] * U.A[1,1] - U.A[0,1] * U.A[1,0]).Conj();
        }


        double GetDet(Link W)
        {
            return (W.A[0, 0] * W.A[1, 1] * W.A[2, 2] + W.A[0, 1] * W.A[1, 2] * W.A[2, 0] + W.A[0, 2] * W.A[1, 0] * W.A[2, 1] - W.A[0, 2] * W.A[1, 1] * W.A[2, 0] - W.A[0, 1] * W.A[1, 0] * W.A[2, 2] - W.A[1, 2] * W.A[2, 1] * W.A[0, 0]).Module();
           // return (W.A[0,0]*W.A[1,1]-W.A[0,1]*W.A[1,0]).x;
        }


        //return 2x2 SU(2) matrix by given determinant of staples near link.
        public Link GetHeatBath(double det, int I, int J)
        {
            Complex[,] SU2 = new Complex[2, 2];
            //calculating elements of SU(2) matrix according to distribution dP~exp(-S(u))du

            double a = Math.Sqrt(Math.Abs(det)); 
            double x0=0, x1=0, x2=0, x3=0;
         /*     
            //dP(x0)~exp(beta*a*x0) dx0
            //make t from [exp(-b*a),1]
            double pow = Math.Exp(-beta*a);
            bool accepted = false; 
            while (!accepted)
            {

                double t = GetRandom() * (pow-1/pow)+1/pow;
                x0 =  1 / (beta * a) * Math.Log(t);
                //and accept it with probability Math.Sqrt(1-x0^2)
                t = GetRandom();
                if (t * t < 1 - x0 * x0) accepted = true;
            }
        */

          bool accepted = false;
            double r1 = 0; double r2 = 0; double r3 = 0; double r = 0; double lambda2 = 0;
            while (!accepted)
            {
                r1=GetRandom();r2=GetRandom();r3=GetRandom();
                r=GetRandom();
                lambda2 = -d / (4 * a * beta) * (Math.Log(r1) + Math.Pow(Math.Cos((2 * Math.PI * r2)), 2) * Math.Log(r3));
                //and accept it with probability Math.Sqrt(1-lambda2)
                if (r * r < 1 - lambda2) accepted = true;
            }
            x0 = 1 - 2 * lambda2;
           

            //x1,x2,x3 with probability dOmega = d(cos(teta))d(fi), but on practice, may be (!check it!!!)
            double len = Math.Sqrt(1 - x0 * x0);
            accepted = false;
            double templen=1;
            while (!accepted)
            {
                x1 = GetRandom() * 2 - 1; x2 = GetRandom() * 2 - 1; x3 = GetRandom() * 2 - 1;
                templen = x1 * x1 + x2 * x2 + x3 * x3;
                if (templen < 1) accepted = true;
            }
            //normalizing to length len
            templen = Math.Sqrt(templen);
            x1 *= len/templen; x2 *= len/templen; x3 *= len/templen;

            Link Res = new Link(true);
          //  Res.a0 = x0; Res.a1 = x1; Res.a2= x2; Res.a3 = x3;

            Res.A[I, I] = new Complex(x0, x3);
            Res.A[I, J] = new Complex(x2, x1);
            Res.A[J, I] = new Complex(-x2, x1);
            Res.A[J, J] = new Complex(x0, -x3);


            return Res;
        }

        public double Sweep(bool withOR)
        {
            //Link Temp = new Link(true);
            //Temp.A[0, 0] = expposxi;
            //Temp.A[1, 1] = expnegxi;
            //for (int i = 0; i < Ns; i++)
            //    for (int j = 0; j < Ns; j++)
            //        for (int t = 0; t < Nt; t++)
            //        {
            //            U[Ns, i, j, t, 1] = U[0, i, j, t, 1];
            //            // U[Ns, i, j, t, 2] = U[0, i, j, t, 2];
            //            U[Ns, i, j, t, 2] = Mul(Temp, Mul(U[0, i, j, t, 2], Temp));
            //            U[Ns, i, j, t, 3] = U[0, i, j, t, 3];
            //            U[Ns, i, j, t, 4] = U[0, i, j, t, 4];

            //            //U[Ns, i, j, t, 2].A[0, 0] *= exppos2xi;
            //            //U[Ns, i, j, t, 2].A[0, 2] *= expposxi;
            //            //U[Ns, i, j, t, 2].A[1, 1] *= expneg2xi;
            //            //U[Ns, i, j, t, 2].A[1, 2] *= expnegxi;
            //            //U[Ns, i, j, t, 2].A[2, 0] *= expposxi;
            //            //U[Ns, i, j, t, 2].A[2, 1] *= expnegxi;
            //        }



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
                                    Update(i, j, k, t, Mu, withOR);
                                    sum += GetDet(U[i, j, k, t, Mu]); n++;
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
                                    Update(i, j, k, t, Mu,withOR);
                                    sum += GetDet(U[i, j, k, t, Mu]); n++;
                                }
                            }
            }

            return sum / (Convert.ToDouble(n));

        }

        public double MeasureS()
        {
            int n = 0; double S = 0;
            for (int Mu = 1; Mu <= 3; Mu++)
            {
                for (int Nu = Mu + 1; Nu <= 4; Nu++)
                {
                    
                    for (int i = 0; i < Ns; i++)
                        for (int j = 0; j < Ns; j++)
                            for (int k = 0; k < Ns; k++)
                                for (int t = 0; t < Nt; t++)
                                {
                                    Link A1 = new Link(false);
                                    int i1 = i, j1 = j, k1 = k, t1 = t; int i2 = i, j2 = j, k2 = k, t2 = t;

                                    GetNode(ref i1, ref j1, ref k1, ref t1, Mu);
                                    GetNode(ref i2, ref j2, ref k2, ref t2, Nu);

                                    if (i == Ns) i = 0; if (i1 == Ns) i1 = 0; if (i2 == Ns) i2 = 0;
                                    A1 = Mul(U[i, j, k, t, Mu], U[i1, j1, k1, t1, Nu]);//U_mu(x)*U_nu(x+mu)
                                    A1 = Mul(A1, U[i2, j2, k2, t2, Mu].HermConj());//*U_mu+(x+Nu)
                                    A1 = Mul(A1, U[i, j, k, t, Nu].HermConj());//*U_nu+(x)
                                    //here A1 is U_mu_nu

                                    n++;
                                  //  S += A1.a0; //1/2 Re(Tr(U_mu_nu)
                                    S += ReTr(A1)/3;
                                }
                }
            }

           // S = /*beta / 3 **/ (1 - S/(3*n));//S=beta/3*Sum(x,Sum(mu<nu,{Re(Tr(1-U_mu_nu))}))
            return Math.Abs(1 - S /(n));
        }
        public double MeasurePL()
        {
            int n=0; double PL=0;
            for (int i = 0; i < Ns; i++)
                for (int j = 0; j < Ns; j++)
                    for (int k = 0; k < Ns; k++)
                    {
                        Link L =U[i, j, k, 0, 4];

                        for (int t = 1; t < Nt; t++)
                        {
                            L=Mul(L,U[i, j, k, t, 4]);
                        }
                            n++;
                           // PL += L.a0; //1/2 Tr(L)
                            PL += ReTr(L);
                    }
            PL = Math.Abs(PL / n /SU3.d);
            return PL;
        }

        public double MeasureNonPL()
        {
            int n = 0; double PL = 0;
            for (int i = 0; i < Ns; i++)
                for (int j = 0; j < Ns; j++)
                    for (int k = 0; k < Nt; k++)
                    {
                        Link L = U[i, j, 0, k, 3];

                        for (int t = 1; t < Ns; t++)
                        {
                            L = Mul(L, U[i, j, t, k, 3]);
                        }
                        n++;
                        PL += ReTr(L); //Tr(L)
                    }
            PL = PL / n;
            return PL;
        }
    }
}
