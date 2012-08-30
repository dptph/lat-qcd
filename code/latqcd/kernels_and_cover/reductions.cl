
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

//__kernel void CalcSREDUCTLEN(__global int* intconsts, __global float* floatconsts , __global Link* U, __global double* SGroupResult, __global double* SResult, __local double* ldata, __global uint* seed, __global float4* seed_deb)
__kernel void CalcSREDUCTLEN(__global int* intconsts, __global float* floatconsts , __global Link* U, __global double* SGroupResult, __local double* ldata, __global uint* seed)
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
        Update(intconsts,floatconsts,seed, U);//, seed_deb);
// 2 updates - include 1 update between each measurement to get rid off autocorrelations
        Update(intconsts,floatconsts,seed, U);//, seed_deb);
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


//__kernel void PLoop(__global int* intconsts, __global Link* buf, __global double* PloopResult, __global double* Result, __local double* ldata)
__kernel void Ploop_corr(__global int* intconsts, __global Link* buf, __global double* Result)
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

        //Result[n] = tr.x/3.0;
        Result[2*n] = tr.x;
        Result[2*n+1] = tr.y;
// ????????????????????????????????????

	//double PL = (tr.x)/3.0;
	//double tt[1];
	//tt[0] = 0.2;tt[1] = 0.9;GetRandom(&tt[1]);GetRandom(tt);
/*
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
*/
}

//__kernel void Ploop_mean(__global int* intconsts, __global Link* buf, __global double* Result)
__kernel void Ploop_mean(__global int* intconsts, __global Link* buf, __global double* PloopResult, __global double* Result, __local double* ldata)
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
