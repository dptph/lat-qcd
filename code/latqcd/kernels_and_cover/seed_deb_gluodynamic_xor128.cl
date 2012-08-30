/*
﻿// constant uint Ns=16,Nt=4,Ns2=16*16, Ns3=16*16*16; 

// #pragma OPENCL EXTENSION  cl_amd_fp64 : enable
*/
//#pragma OPENCL EXTENSION  cl_amd_fp64 : enable

#define double float
#define double2 float2
#define double4 float4


constant uint arnd=16807, crnd=0;//mrnd=1<<31,arnd=134775813,crnd=1;//
//constant uint mrnd=1<<31-1;
constant uint mrnd=(uint)-1;
constant double PI=3.141592653589793;

//constant double beta=4.7;


typedef struct 
{
    int x,y,z,t;
} Coord;

typedef struct 
{
    double2 a00,a01,a02;
    double2 a10,a11,a12;
    double2 a20,a21,a22;
} Link;

float GetRandom_L(uint* oldrnd) //old linear generator
{
	oldrnd[0] = (oldrnd[0] * arnd+crnd) % mrnd;
	//double res = ((double)oldrnd[0]/(double)(mrnd))/10000000;
	//oldrnd[0] = (oldrnd[0] * arnd+crnd) % mrnd;
	return (float)oldrnd[0]/(float)(mrnd);// + res;
}

float GetRandom(uint4* oldrnd)
{
uint t = (oldrnd[0].x ^ (oldrnd[0].x << 11));
oldrnd[0].w = oldrnd[0].w ^ (oldrnd[0].w >> 19) ^ (t ^ (t >> 8));
oldrnd[0].xyzw=oldrnd[0].yzwx;

return (float)oldrnd[0].w/(float)(mrnd);
}

Coord GetPosOdd(uint n)
{
    n=n*2+1;
    Coord t; uint tempn;
    t.t=n/Ns3; tempn=n-t.t*Ns3;
    t.z=tempn/Ns2; tempn-=t.z*Ns2;
    t.y=tempn/Ns;
    t.x=tempn-t.y*Ns;

    //if even then x--;
    if ((t.x+t.y+t.z+t.t) % 2 == 0) t.x--;
    return t;
}

Coord GetPosEven(uint n)
{
    n*=2;
    Coord t; uint tempn;
    t.t=n/Ns3; tempn=n-t.t*Ns3;
    t.z=tempn/Ns2; tempn-=t.z*Ns2;
    t.y=tempn/Ns;
    t.x=tempn-t.y*Ns;

    //if odd then x++;
    if ((t.x+t.y+t.z+t.t) % 2 == 1) t.x++;
    return t;
}

Coord GetPosAllSpace(uint n)
{
    Coord t; uint tempn=n;
	t.t=0;
    t.z=tempn/Ns2; tempn-=t.z*Ns2;
    t.y=tempn/Ns;
    t.x=tempn-t.y*Ns;

    return t;
}

uint GetSiteIndex (Coord Site) {return Site.t*Ns3+Site.z*Ns2+Site.y*Ns+Site.x;}
uint GetEl (Coord Site, int mu) {return GetSiteIndex(Site)*4+mu-1;}//mu-1 because mu=1..4

Coord GetNode(Coord Site, int dir)
{
    if (dir == 1) Site.x++; if (dir == -1) Site.x--;
    if (dir == 2) Site.y++; if (dir == -2) Site.y--;
    if (dir == 3) Site.z++; if (dir == -3) Site.z--;
    if (dir == 4) Site.t++; if (dir == -4) Site.t--;

    if (Site.x == -1) Site.x = Ns - 1; if (Site.x == Ns) Site.x = 0;
    if (Site.y == -1) Site.y = Ns - 1; if (Site.y == Ns) Site.y = 0;
    if (Site.z == -1) Site.z = Ns - 1; if (Site.z == Ns) Site.z = 0;
    if (Site.t == -1) Site.t = Nt - 1; if (Site.t == Nt) Site.t = 0;

return Site;
}

Coord GetNodeTwist(Coord Site, int dir)
{
    if (dir == 1) Site.x++; if (dir == -1) Site.x--;
    if (dir == 2) Site.y++; if (dir == -2) Site.y--;
    if (dir == 3) Site.z++; if (dir == -3) Site.z--;
    if (dir == 4) Site.t++; if (dir == -4) Site.t--;

    if (Site.x == -1) Site.x = Ns - 1; //if (Site.x == Ns) Site.x = 0;//!!!
    if (Site.y == -1) Site.y = Ns - 1; if (Site.y == Ns) Site.y = 0;
    if (Site.z == -1) Site.z = Ns - 1; if (Site.z == Ns) Site.z = 0;
    if (Site.t == -1) Site.t = Nt - 1; if (Site.t == Nt) Site.t = 0;

return Site;
}

double2 CM(double2 a, double2 b)
{
  return (double2)(a.x*b.x-a.y*b.y,a.x*b.y+a.y*b.x);
}

double CMod(double2 x)
{
	return sqrt(x.x*x.x+x.y*x.y);
}

double2 Conj(double2 a)
{
	return (double2)(a.x,-a.y);
}

Link Add(Link U,Link V)
{
    U.a00+=V.a00;
    U.a01+=V.a01;
    U.a02+=V.a02;
    U.a10+=V.a10;
    U.a11+=V.a11;
    U.a12+=V.a12;
    U.a20+=V.a20;
    U.a21+=V.a21;
    U.a22+=V.a22;
    return U;
}

Link Add2(Link U,Link V, Link W)
{
    U.a00+=V.a00+W.a00;
    U.a01+=V.a01+W.a01;
    U.a02+=V.a02+W.a02;
    U.a10+=V.a10+W.a10;
    U.a11+=V.a11+W.a11;
    U.a12+=V.a12+W.a12;
    U.a20+=V.a20+W.a20;
    U.a21+=V.a21+W.a21;
    U.a22+=V.a22+W.a22;
    return U;
}

Link Sub(Link U,Link V)
{
    U.a00-=V.a00;
    U.a01-=V.a01;
    U.a02-=V.a02;
    U.a10-=V.a10;
    U.a11-=V.a11;
    U.a12-=V.a12;
    U.a20-=V.a20;
    U.a21-=V.a21;
    U.a22-=V.a22;
    return U;
}

Link Mul(Link U, Link V)
{
    Link R;
    R.a00 = CM(U.a00,V.a00) + CM(U.a01,V.a10) + CM(U.a02,V.a20);
    R.a01 = CM(U.a00,V.a01) + CM(U.a01,V.a11) + CM(U.a02,V.a21);
    R.a02 = CM(U.a00,V.a02) + CM(U.a01,V.a12) + CM(U.a02,V.a22);
    R.a10 = CM(U.a10,V.a00) + CM(U.a11,V.a10) + CM(U.a12,V.a20);
    R.a11 = CM(U.a10,V.a01) + CM(U.a11,V.a11) + CM(U.a12,V.a21);
    R.a12 = CM(U.a10,V.a02) + CM(U.a11,V.a12) + CM(U.a12,V.a22);
    R.a20 = CM(U.a20,V.a00) + CM(U.a21,V.a10) + CM(U.a22,V.a20);
    R.a21 = CM(U.a20,V.a01) + CM(U.a21,V.a11) + CM(U.a22,V.a21);
    R.a22 = CM(U.a20,V.a02) + CM(U.a21,V.a12) + CM(U.a22,V.a22);
    return R;
}

Link HermConj(Link U)
{
    Link L;
    L.a00=(double2)(U.a00.x,-U.a00.y);
    L.a01=(double2)(U.a10.x,-U.a10.y);
    L.a02=(double2)(U.a20.x,-U.a20.y);
    L.a10=(double2)(U.a01.x,-U.a01.y);
    L.a11=(double2)(U.a11.x,-U.a11.y);
    L.a12=(double2)(U.a21.x,-U.a21.y);
    L.a20=(double2)(U.a02.x,-U.a02.y);
    L.a21=(double2)(U.a12.x,-U.a12.y);
    L.a22=(double2)(U.a22.x,-U.a22.y);
return L;
}

Link Identity()
{
    Link L; L.a00=(double2)(1.0,0.0);L.a11=(double2)(1.0,0.0);L.a22=(double2)(1.0,0.0);
    return L;
}

Link Fill(double a)
{
    double2 Z = (double2)(0.0,0.0);
    double2 Z1 = (double2)(a,0.0);
    Link L;
    L.a00=Z1;L.a01=Z;L.a02=Z;
    L.a10=Z;L.a11=Z1;L.a12=Z;
    L.a20=Z;L.a21=Z;L.a22=Z1;
    return L;

}

double4 GetHeatBath(double det, uint4* oldrnd, double beta, float4* seed_debug)
{

		//
        //calculating elements of SU(2) matrix according to distribution dP~exp(-S(u))du

        float a = sqrt(fabs((float)det)); 

        bool accepted = false;
        float r1 = 1.0; float r2 = 1.0; float r3 = 1.0; float r = 0.0; float lambda2 = 0.0;
        while (!accepted)
        {
           
            r1=GetRandom(oldrnd);r2=GetRandom(oldrnd);r3=GetRandom(oldrnd);
            r=GetRandom(oldrnd);
            seed_debug[0].x=r1;
            seed_debug[0].y=r2;
            seed_debug[0].z=r3;
            seed_debug[0].w=r;
            //return (double4)(a,r1,r2,r3);
			float a1 = cos(2.0 * PI * r2);
			float a2 = log(r1);
			float a3 = log(r3);
			lambda2 = - 3.0 / (4.0 * a * beta) * (a2 + a1 *a1 * a3);//3 is d!!!

            //and accept it with probability Math.Sqrt(1-lambda2)
            //if (lambda2>1) lambda2=0;
			if (r * r < 1.0 - lambda2) accepted = true;
        }
     
        double x0=0.0, x1=0.0, x2=0.0, x3=0.0;
	   x0 = 1.0 - 2.0 * lambda2;
           

        //x1,x2,x3 with probability dOmega = d(cos(teta))d(fi), but on practice, may be (!check it!!!)
        double len = sqrt(fabs(1 - x0 * x0));
        accepted = false;
        double templen=1.0;
        while (!accepted)
        {
            x1 = GetRandom(oldrnd) * 2.0 - 1.0; x2 = GetRandom(oldrnd) * 2.0 - 1.0; x3 = GetRandom(oldrnd) * 2.0 - 1.0;
            templen = x1 * x1 + x2 * x2 + x3 * x3;
            if (templen < 1.0)
			 accepted = true;
        }
        //normalizing to length len
        templen = sqrt(templen);
		//if (!(templen>0.1)) templen=0.1;
        x1 *= len/templen; x2 *= len/templen; x3 *= len/templen;

        return (double4)(x0,x1,x2,x3);


}

double GetDet(Link W)
{
//return (W.A[0, 0] * W.A[1, 1] * W.A[2, 2] + W.A[0, 1] * W.A[1, 2] * W.A[2, 0] + W.A[0, 2] * W.A[1, 0] * W.A[2, 1] - W.A[0, 2] * W.A[1, 1] * W.A[2, 0] - W.A[0, 1] * W.A[1, 0] * W.A[2, 2] - W.A[1, 2] * W.A[2, 1] * W.A[0, 0]).Module();
double2 d1 = CM(CM(W.a00 , W.a11) , W.a22);
double2 d2 = CM(CM(W.a01 , W.a12) , W.a20);
double2 d3 = CM(CM(W.a02 , W.a10) , W.a21);
double2 d4 = -CM(CM(W.a02 , W.a11) , W.a20);
double2 d5 = -CM(CM(W.a01 , W.a10) , W.a22);
double2 d6 = -CM(CM(W.a12 , W.a21) , W.a00);
double2 res = (d1+d2+d3+d4+d5+d6);
return (double)CMod(res);
}

double abs2(double2 a)
{
	return a.x*a.x+a.y*a.y;
}

Link Ortho(Link Uold)
{
		Link U;
		U = Uold;
		double a1 = abs2(U.a00);
		double a2 = abs2(U.a01);
		double a3 = abs2(U.a02);
		double module = sqrt(a1+a2+a3);
             
		//U'=U/|U|
		U.a00.x /= module; U.a00.y /= module;
		U.a01.x /= module; U.a01.y /= module;
		U.a02.x /= module; U.a02.y /= module;
            
		//V'=V-(V U'*)U'
		double2 VU = CM(U.a10 , Conj(U.a00)) + CM(U.a11 , Conj(U.a01)) + CM(U.a12 , Conj(U.a02));

		U.a10=U.a10-CM(VU,U.a00);
		U.a11=U.a11-CM(VU,U.a01);
		U.a12=U.a12-CM(VU,U.a02);

		//V'=V/|V|
		a1 = abs2(U.a10);
		a2 = abs2(U.a11);
		a3 = abs2(U.a12);
		module = sqrt(a1+a2+a3);
            
		U.a10.x /= module; U.a10.y /= module;
		U.a11.x /= module; U.a11.y /= module;
		U.a12.x /= module; U.a12.y /= module;

		U.a20=Conj(CM(U.a01, U.a12) - CM(U.a02, U.a11));
		U.a21=Conj(CM(U.a02, U.a10) - CM(U.a00, U.a12));
		U.a22=Conj(CM(U.a00, U.a11) - CM(U.a01, U.a10));
	return U;
}


double UpdateLink(global Link* U, Coord P, int dir, uint4* oldrnd, Link* T, double beta, float4* seed_debug)
{
            Link A=Fill(0); 
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
                    Link A1=Fill(0);
                    Coord P1;
                    Coord P2;

                    P1 = GetNodeTwist(P, dir);
                    P2 = GetNodeTwist(P, i);

					Link U1,U2;

					if (P1.x == Ns) {P1.x=0; if (i==2) U1 = Mul(T[0], Mul(U[GetEl(P1,i)], T[0])); else U1 = U[GetEl(P1,i)];}
                                        else U1 = U[GetEl(P1,i)];
					if (P2.x == Ns) {P2.x=0; if (dir == 2) U2 = Mul(T[0], Mul(U[GetEl(P2,dir)], T[0])); else U2 = U[GetEl(P2,dir)]; }
                                        else U2 = U[GetEl(P2,dir)];
					Link U3 = U[GetEl(P,i)];
 

                    A1=Mul(U1,HermConj(U2));
                    A1=Mul(A1,HermConj(U3));

                    /*     r  ->dir r+dir
                     *     |         |
                     *  ^i ^         \/    
                     *     |         |
                     *   r-i----<----r+dir-i
                     */
                    //down
                    Link A2=Fill(0);
                    P1=P;
                    P1=GetNodeTwist(P1,-i);
                    P2=P1;
                    P1=GetNodeTwist(P1,dir);
					
					if (P1.x == Ns) { P1.x=0; if (i == 2) U1 = Mul(T[0], Mul(U[GetEl(P1,i)], T[0])); else U1 =U[GetEl(P1,i)];} else 
U1 = U[GetEl(P1,i)];
					U2 = U[GetEl(P2,dir)];
					U3 = U[GetEl(P2,i)];

		// change		
                    /*A2=Mul(HermConj(U1),HermConj(U2));
                    A2=Mul(A2,U3);

                    A=Add2(A,A1,A2);*/
            A2=Mul(HermConj(U1),HermConj(U2));
            A2=Mul(A2,U3);
     
            if (P.x==0 && dir==2)
                 A2=Mul(T[0], Mul(A2, T[0]));
            A=Add2(A,A1,A2);
            //change
                }
            }
            //here A is the sum of staples!

            Link W = Mul(U[GetEl(P,dir)], A); double det;

            Link X=Fill(1.0);//

				//beginning of sampling the x-y plane
                    double2 a=W.a00;
                    double2 B=W.a01;
                    double2 C=W.a10;
                    double2 D=W.a11;

                    double x0 = 0.5 * (a.x + D.x);
                    double x1 = 0.5 * (B.y + C.y);
                    double x2 = 0.5 * (B.x - C.x);
                    double x3 = 0.5 * (a.y - D.y);

                    det = x0 * x0 + x1 * x1 + x2 * x2 + x3 * x3;
                   
					Link hb = Fill(1.0);double4 HB=(double4)(1.0,0.0,0.0,0.0);

					HB=GetHeatBath(det,oldrnd,beta, seed_debug);

					hb.a00=(double2)(HB.s0, HB.s3);
					hb.a01=(double2)(HB.s2, HB.s1);
					hb.a10=(double2)(-HB.s2, HB.s1);
					hb.a11=(double2)(HB.s0, -HB.s3);
					//double result = GetDet(hb);
					//dbg[n]=hb;

                    Link r = Fill(1.0);

                    det = sqrt(det);
                    x0 /= det;
                    x1 /= det;
                    x2 /= det;
                    x3 /= det;

                    r.a00 = (double2)(x0, x3);
                    r.a01 = (double2)(x2, x1);
                    r.a10 = (double2)(-x2, x1);
                    r.a11 = (double2)(x0, -x3);

                    r = Mul(hb, HermConj(r));

                    W = Mul(r, W);
                    X = Mul(r, X);
				//end of sampling the x-y plane

				//beginning of sampling the x-z plane
                     a=W.a00;
                     B=W.a02;
                     C=W.a20;
                     D=W.a22;

                     x0 = 0.5 * (a.x + D.x);
                     x1 = 0.5 * (B.y + C.y);
                     x2 = 0.5 * (B.x - C.x);
                     x3 = 0.5 * (a.y - D.y);

                    det = x0 * x0 + x1 * x1 + x2 * x2 + x3 * x3;
                   
					 hb = Fill(1.0); HB=(double4)(1.0,0.0,0.0,0.0);

					HB=GetHeatBath(det,oldrnd,beta, seed_debug);

					hb.a00=(double2)(HB.s0, HB.s3);
					hb.a02=(double2)(HB.s2, HB.s1);
					hb.a20=(double2)(-HB.s2, HB.s1);
					hb.a22=(double2)(HB.s0, -HB.s3);
					
					//dbg[n]=hb;

                     r = Fill(1.0);

                    det = sqrt(det);
                    x0 /= det;
                    x1 /= det;
                    x2 /= det;
                    x3 /= det;

                    r.a00 = (double2)(x0, x3);
                    r.a02 = (double2)(x2, x1);
                    r.a20 = (double2)(-x2, x1);
                    r.a22 = (double2)(x0, -x3);

                    r = Mul(hb, HermConj(r));

                    W = Mul(r, W);
                    X = Mul(r, X);
				//end of sampling the x-z plane

				//beginning of sampling the y-z plane
                     a=W.a11;
                     B=W.a12;
                     C=W.a21;
                     D=W.a22;

                     x0 = 0.5 * (a.x + D.x);
                     x1 = 0.5 * (B.y + C.y);
                     x2 = 0.5 * (B.x - C.x);
                     x3 = 0.5 * (a.y - D.y);

                    det = x0 * x0 + x1 * x1 + x2 * x2 + x3 * x3;
                   
					 hb = Fill(1.0); HB=(double4)(1.0,0.0,0.0,0.0);

					HB=GetHeatBath(det,oldrnd,beta, seed_debug);

					hb.a11=(double2)(HB.s0, HB.s3);
					hb.a12=(double2)(HB.s2, HB.s1);
					hb.a21=(double2)(-HB.s2, HB.s1);
					hb.a22=(double2)(HB.s0, -HB.s3);
					
					//dbg[n]=hb;

                     r = Fill(1.0);

                    det = sqrt(det);
                    x0 /= det;
                    x1 /= det;
                    x2 /= det;
                    x3 /= det;

                    r.a11 = (double2)(x0, x3);
                    r.a12 = (double2)(x2, x1);
                    r.a21 = (double2)(-x2, x1);
                    r.a22 = (double2)(x0, -x3);

                    r = Mul(hb, HermConj(r));

                    W = Mul(r, W);
                    X = Mul(r, X);
				//end of sampling the y-z plane
           
			Link Res = Mul(X, U[GetEl(P,dir)]);
            U[GetEl(P,dir)] = Ortho(Res);

return 0;// GetDet(U[GetEl(P,dir)]);

}



//__kernel void MyKernel(int Nspace, int Ntime, double bconst, double flux, __global uint* seed, __global Link* buf)
__kernel void Update(__global int* intconsts, __global float* floatconsts, __global uint4* seed, __global Link* buf, __global float4* seed_deb)
{
//int Nspace, int Ntime, double bconst, double flux,
//int Nspace = intconsts[0]; int Ntime = intconsts[1];
double bconst = floatconsts[0];
double flux = floatconsts[1];

	//Ns=Nspace; Nt=Ntime; beta=bconst;
	//Ns2= Ns*Ns; Ns3=Ns2*Ns;
    uint4 oldrnd[1];
    float4 seed_debug[1];
	uint n = get_global_id(0);

	oldrnd[0]=seed[n];
	//field initialization
	Link T = Fill(1.0);
	double2 expposxi = (double2)(cos(flux/2.0), sin(flux/2.0));
    double2 expnegxi = (double2)(cos(flux/2.0),-sin(flux/2.0));
	T.a00 = expposxi; T.a11=expnegxi;
  
	//coords in buf of (x,y,z,t,mu=0) link as matrix 3x3 elements 
//for (int times = 0; times<1;times++) {
	double res =0; Coord Site;
	for (uint j = 0;j<=7;j++) 
	{
		if (j / 4 == 0) Site = GetPosEven(n); else Site = GetPosOdd(n); 

		res= UpdateLink(buf,Site,(j % 4) +1,oldrnd, &T, bconst, seed_debug);
	}

	seed[n]=oldrnd[0];
        seed_deb[n]=seed_debug[0];

}
/* Nice try
void Up_date(int* intconsts, float* floatconsts, uint* seed, Link* buf)
{
//int Nspace, int Ntime, double bconst, double flux,
//int Nspace = intconsts[0]; int Ntime = intconsts[1];

double bconst = floatconsts[0];
double flux = floatconsts[1];

	//Ns=Nspace; Nt=Ntime; beta=bconst;
	//Ns2= Ns*Ns; Ns3=Ns2*Ns;
    uint oldrnd[1];
	uint n = get_global_id(0);

	oldrnd[0]=seed[n];
	//field initialization
	Link T = Fill(1.0);
	double2 expposxi = (double2)(cos(flux/2.0), sin(flux/2.0));
    double2 expnegxi = (double2)(cos(flux/2.0),-sin(flux/2.0));
	T.a00 = expposxi; T.a11=expnegxi;
  
	//coords in buf of (x,y,z,t,mu=0) link as matrix 3x3 elements 
//for (int times = 0; times<1;times++) {
	double res =0;Coord Site;
	for (uint j = 0;j<=7;j++) 
	{
		if (j / 4 == 0) Site = GetPosEven(n); else Site = GetPosOdd(n); 

		res= UpdateLink(buf,Site,(j % 4) +1,oldrnd, &T, bconst);
	}

	seed[n]=oldrnd[0];

}*/

Link U_mu_nu(Coord Site,int Mu,int Nu, __global Link* U)
{
   Link A1;
   Coord P1 = GetNode(Site,Mu);
   Coord P2 = GetNode(Site,Nu);

   A1 = Mul(U[GetEl(Site,Mu)],U[GetEl(P1,Nu)]);
   A1 = Mul(A1, HermConj(U[GetEl(P2,Mu)]));
   A1 = Mul(A1,HermConj(U[GetEl(Site,Nu)]));
   //here A1 is U_mu_nu
   return A1;
}

//__kernel void CalcS(int Nspace, int Ntime, __global Link* U, __global double* SGroupResult, __global double* SResult, __local double* ldata)
__kernel void CalcS(__global int* intconsts, __global float* floatconsts , __global Link* U, __global double* SGroupResult, __global double* SResult, __local double* ldata, __global uint* seed, __global float4* seed_deb)
{
//int Nspace, int Ntime,

	//Ns=Nspace; Nt=Ntime;
	//Ns2= Ns*Ns; Ns3=Ns2*Ns;
	uint n = get_global_id(0);
	uint lid = get_local_id(0);

	double res =0;Coord Site;
	double S[7];
/*	double S12 = 0;
	double S13 = 0;
	double S14 = 0;
	double S23 = 0;
	double S24 = 0;
	double S34 = 0;*/

//        Update(intconsts,floatconsts,seed, U, seed_deb);

        for (int qn=0; qn<7; qn++) {S[qn]=0;}

	for (int j = 0;j<=1;j++) //odd and even sites
	{
	if (j==0) Site = GetPosEven(n); else Site = GetPosOdd(n); 
            for (int Mu = 1; Mu <= 3; Mu++)
            {
                for (int Nu = Mu + 1; Nu <= 4; Nu++)
                {
					Link A1;
		/*
					Coord P1 = GetNode(Site,Mu);
					Coord P2 = GetNode(Site,Nu);

					A1 = Mul(U[GetEl(Site,Mu)],U[GetEl(P1,Nu)]);
					A1 = Mul(A1, HermConj(U[GetEl(P2,Mu)]));
					A1 = Mul(A1,HermConj(U[GetEl(Site,Nu)]));
*/
 //                    //here A1 is U_mu_nu
                   A1=U_mu_nu(Site, Mu, Nu, U);
                   if (Mu==1) S[Mu+Nu-2]+=(double2)(A1.a00+A1.a11+A1.a22).x/3.0; //S += ReTr(A1)/3;
                   else S[Mu+Nu-1]+=(double2)(A1.a00+A1.a11+A1.a22).x/3.0;
/* Mu--Nu --> S[#]
                   1--2 --> 1
                   1--3 --> 2
                   1--4 --> 3
                   2--3 --> 4
                   2--4 --> 5
                   3--4 --> 6
*/
		}
	    }
	}

/*for (int qn=0; qn<7; qn++) {S[qn]=0;}
for (int k=4; k<7; k++) {S[k]=1;}
*/
S[0]=S[1]+S[2]+S[3]+S[4]+S[5]+S[6];
//for (int k=0; k<7; k++) {S[k]=0;}

		//here S in a quantity, which should be reduced
     for (int k=0;k<7;k++)
     {
	barrier(CLK_LOCAL_MEM_FENCE);
//        ldata[lid] =0;
//        ldata[lid] =1;
        ldata[lid] =  S[k];//!!
	barrier(CLK_LOCAL_MEM_FENCE);

	for (unsigned int s=get_local_size(0)/2;s>0;s>>=1)
	{
		if (lid<s) ldata[lid]+=ldata[lid+s];
		barrier(CLK_LOCAL_MEM_FENCE);
	}
	if (lid==0) 
           {
//           SGroupResult[get_group_id(0)]=ldata[0];
           SGroupResult[7*get_group_id(0)+k]=ldata[0];
           }

 	barrier(CLK_LOCAL_MEM_FENCE);
 	barrier(CLK_GLOBAL_MEM_FENCE);
/*
 	barrier(CLK_GLOBAL_MEM_FENCE);
	if (n==0){
		res=0;
		for (int g=0;g<get_num_groups(0);g++) 
                    {
                     res+=SGroupResult[g];
                     }
                SResult[k] = res;
        	}

 	barrier(CLK_LOCAL_MEM_FENCE);
 	barrier(CLK_GLOBAL_MEM_FENCE);
*/
    }
 	barrier(CLK_LOCAL_MEM_FENCE);
 	barrier(CLK_GLOBAL_MEM_FENCE);
}

#define REDUCTLEN 100

__kernel void CalcSREDUCTLEN(__global int* intconsts, __global float* floatconsts , __global Link* U, __global double* SGroupResult, __global double* SResult, __local double* ldata, __global uint* seed, __global float4* seed_deb)
{
//int Nspace, int Ntime,

	//Ns=Nspace; Nt=Ntime;
	//Ns2= Ns*Ns; Ns3=Ns2*Ns;
	uint n = get_global_id(0);
	uint lid = get_local_id(0);
	uint gid = get_group_id(0);

	double res =0;Coord Site;
	double S[7];

// Put output buffer to 0
     if (lid==0)
     {
        for (int k=0;k<7;k++)
        {

           SGroupResult[7*gid+k]=0;
         }
      }

  for (int h=0; h<REDUCTLEN; h++)
  {
        Update(intconsts,floatconsts,seed, U, seed_deb);
// 2 updates - include 1 update between each measurement to get rid off autocorrelations
        Update(intconsts,floatconsts,seed, U, seed_deb);
        for (int qn=0; qn<7; qn++) {S[qn]=0;}

	for (int j = 0;j<=1;j++) //odd and even sites
	{
	if (j==0) Site = GetPosEven(n); else Site = GetPosOdd(n); 
            for (int Mu = 1; Mu <= 3; Mu++)
            {
                for (int Nu = Mu + 1; Nu <= 4; Nu++)
                {
					Link A1;
 //                    //here A1 is U_mu_nu
                   A1=U_mu_nu(Site, Mu, Nu, U);
                   if (Mu==1) S[Mu+Nu-2]+=(double2)(A1.a00+A1.a11+A1.a22).x/3.0; //S += ReTr(A1)/3;
                   else S[Mu+Nu-1]+=(double2)(A1.a00+A1.a11+A1.a22).x/3.0;
/* Mu--Nu --> S[#]
                   1--2 --> 1
                   1--3 --> 2
                   1--4 --> 3
                   2--3 --> 4
                   2--4 --> 5
                   3--4 --> 6
*/
		}
	    }
//for (int k=4; k<7; k++) {S[k]+=1;}
	}

//for (int qn=0; qn<7; qn++) {S[qn]=0;}
//for (int k=4; k<7; k++) {S[k]=1;}

S[0]=S[1]+S[2]+S[3]+S[4]+S[5]+S[6];
//for (int k=0; k<7; k++) {S[k]=0;}

		//here S are quantities, that should be reduced
     for (int k=0;k<7;k++)
     {
//	barrier(CLK_LOCAL_MEM_FENCE);
//        ldata[lid] =0;
//        ldata[lid] =1;
        ldata[lid] =  S[k];//!!
	barrier(CLK_LOCAL_MEM_FENCE);

	for (unsigned int s=get_local_size(0)/2;s>0;s>>=1)
	{
		if (lid<s) ldata[lid]+=ldata[lid+s];
		barrier(CLK_LOCAL_MEM_FENCE);
	}
	if (lid==0)
           {
//           SGroupResult[get_group_id(0)]=ldata[0];
// Add results from all of the REDUCTLEN steps, further we'll devide the sum by REDUCTLEN to get the arithmetic mean
           SGroupResult[7*gid+k]+=ldata[lid];
           }

// 	barrier(CLK_LOCAL_MEM_FENCE);
    }
  }
// Devide the sum of the REDUCTLEN reductions by the REDUCTLEN itself -- to get mean value
     if (lid==0) 
     {
        for (int k=0;k<7;k++)
        {
           SGroupResult[7*gid+k]/=REDUCTLEN;
         }
      }

//	barrier(CLK_LOCAL_MEM_FENCE);
// 	barrier(CLK_GLOBAL_MEM_FENCE);

}


__kernel void PLoop(__global int* intconsts, __global Link* buf, __global double* PloopResult, __global double* Result, __local double* ldata)
{
//int Nspace, int Ntime
//int Nspace = intconsts[0]; int Ntime = intconsts[1];
	//Ns=Nspace; Ns2=Ns*Ns; Ns3=Ns2*Ns; Nt=Ntime;
	uint n = get_global_id(0);
	uint lid = get_local_id(0);

	//calculate PLoop
	Link L;
	Coord Site=GetPosAllSpace(n);

	L=buf[GetEl(Site,4)];

	for (int t = 1; t < Nt; t++){Site.t++;  L=Mul(L,buf[GetEl(Site,4)]);} 
	double2 tr = (double2)(L.a00+L.a11+L.a22);
	double PL = (tr.x)/3.0;
	//double tt[1];
	//tt[0] = 0.2;tt[1] = 0.9;GetRandom(&tt[1]);GetRandom(tt);
	ldata[lid] =  fabs(PL);//!!!!!!!!!!!!!!!!!!!!!

	barrier(CLK_LOCAL_MEM_FENCE);

	for (unsigned int s=get_local_size(0)/2;s>0;s>>=1)
	{
		if (lid<s) ldata[lid]+=ldata[lid+s];
		barrier(CLK_LOCAL_MEM_FENCE);
	}

	if (lid==0) PloopResult[get_group_id(0)]=ldata[0];
	
	if (n==0){
		double res=0;
		for (int i=0;i<get_num_groups(0);i++) res+=PloopResult[i];

		Result[0] = res;
	}

}
