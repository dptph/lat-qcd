

//#pragma OPENCL EXTENSION cl_amd_printf : enable

// this is the kernel for multipling vector in buffer "vector"
// with dirak matrix, implied by buffer "links"
// result goes to vector in buffer "resvec"
//   dirak oparator's matrix has 12*N columns and rows, where N -- number of nodes
//   (i.e. it has N node indecies, each node has 4 dirak indecies and 3 color)
//   so vector and resvec have 12*N items (complex values, represented by floattype2)
//     for convinience vector and resvec are devided into blocks with 3 values,
//     each corresponding to a dirak index
//     (so each node-index correspondes to 4 such blocks)
//     (each block has 3 color indecies)
//     (worksize*3 == size of the vectors)
//     N nodes --> 4*N working blocks (== col) --> 3*4*N complex values in vectors
// synchronization issue: vector ans resvec should be different buffers
__kernel void dirakMatrMul(__global Link *links,
                           __global floattype2 *vector,
                           __global floattype2 *resvec)
//                           __global float* consts)
{
uint col = get_global_id(0); // the part of resvec, what current kernels works on
//uint n = get_node_number(col); // containing node (n==col/4)
uint n = col/4; // index of the node corresponding for current kernel
//uint dir_index = get_dirak_index(col); // == col%4 == col - n*4
uint dir_index = col%4;
Coord site = GetPosAllSpace(n);
uint m[8]; // neibours of the n
//uint m_col[8]; // their col numbers, actually m_col[x] = m[x] + dir_index

// get neibours numbers: {m},
// get corresponding parts of the vector,
// multiply links n-m with the gamma-matrix number and vector parts
// add all the products and put to resvec

//lets find neibours
// go through positive and negative directions
for (int Mu = -4, i = 0; Mu <= 4; Mu++)
	{
	if (Mu==0) {continue;}
	m[i] = GetSiteIndex(GetNode(site, Mu));
	i++;
	}
// so in m[] naibours are stored in this order: -4, -3, -2, -1, 1, 2, 3, 4

floattype2 vec[8][4][3];// 12-component parts of the vector, corresponding for neibours
//vec[#1][..][..] -- number of neibour
//vec[..][#2][..] -- dirak index
//vec[..][..][#3] -- color index
for (int i = 0; i < 8; i++)
	{
	//m_col = 4*m[i] + dir_index;
	for (int j = 0; j < 4; j++)
		{
		for (int c = 0; c < 3; c++)
			{
			//int numb = 12*m[i]+3*j+c;
			vec[i][j][c] = vector[12*m[i]+3*j+c];// vector[numb];// m[i];// 1;// 
			}
		}
	}
// so in vec[] vector's parts are stored in this order: -4, -3, -2, -1, 1, 2, 3, 4

floattype2 res[3];// 3-component part for the resvec
for (int i = 0; i < 3; i++)
	{
	// since the diagonal part of the dirak matrix == 1
	// we initialy take the corresponding part of the vector
	res[i] = vector[3*col+i];
	//res[i] = (floattype2)(0, 0);
	}

//if (col==100) printf("%d\n", n);

// !! add gamma1 for -mu directions

floattype2 gamma1[8][4] = {{(floattype2)(0,0), (floattype2)(0,0), (floattype2)(0,0), (floattype2)(0,0)},
                        {(floattype2)(0,0), (floattype2)(0,0), (floattype2)(0,0), (floattype2)(0,0)},
                        {(floattype2)(0,0), (floattype2)(0,0), (floattype2)(0,0), (floattype2)(0,0)},
                        {(floattype2)(0,0), (floattype2)(0,0), (floattype2)(0,0), (floattype2)(0,0)},
                        {(floattype2)(0,0), (floattype2)(0,0), (floattype2)(0,0), (floattype2)(0,0)},
                        {(floattype2)(0,0), (floattype2)(0,0), (floattype2)(0,0), (floattype2)(0,0)},
                        {(floattype2)(0,0), (floattype2)(0,0), (floattype2)(0,0), (floattype2)(0,0)},
                        {(floattype2)(0,0), (floattype2)(0,0), (floattype2)(0,0), (floattype2)(0,0)}};

// gamma1 == (1-gamma_mu) or (1+gamma_mu) <-- (-) for positiv mu, (+) -- negative
// first -- put 1 to all gamma1
for (int i=0; i<8; i++)
	{
	gamma1[i][dir_index] = (floattype2)(1, 0);
	}
// let directions be saved in order: [-4, -3, -2, -1, 1, 2, 3, 4]
// let evidently fill gamma1s
gamma1[0][2*(1 - dir_index/2) + dir_index%2] += (floattype2)(1, 0); // -4 direction
gamma1[7][2*(1 - dir_index/2) + dir_index%2] -= (floattype2)(1, 0); // 4 direction

if (dir_index/2)
	{
	gamma1[3][3-dir_index] += (floattype2)(0, 1); // -1 direction
	gamma1[4][3-dir_index] -= (floattype2)(0, 1); // 1 direction

	if (dir_index%2)
		{
		gamma1[1][2*(1 - dir_index/2) + dir_index%2] += (floattype2)(0, -1); // -3 direction
		gamma1[6][2*(1 - dir_index/2) + dir_index%2] -= (floattype2)(0, -1); // 3 direction

		gamma1[2][3-dir_index] += (floattype2)(-1, 0); // -2 direction
		gamma1[5][3-dir_index] -= (floattype2)(-1, 0); // 2 direction
		}
	else
		{
		gamma1[1][2*(1 - dir_index/2) + dir_index%2] += (floattype2)(0, 1); // -3 direction
		gamma1[6][2*(1 - dir_index/2) + dir_index%2] -= (floattype2)(0, 1); // 3 direction

		gamma1[2][3-dir_index] += (floattype2)(1, 0); // -2 direction
		gamma1[5][3-dir_index] -= (floattype2)(1, 0); // 2 direction
		}
	}
else
	{
	gamma1[3][3-dir_index] += (floattype2)(0, -1); // -1 direction
	gamma1[4][3-dir_index] -= (floattype2)(0, -1); // 1 direction
	if (dir_index%2)
		{
		gamma1[1][2*(1 - dir_index/2) + dir_index%2] += (floattype2)(0, 1); // -3 direction
		gamma1[6][2*(1 - dir_index/2) + dir_index%2] -= (floattype2)(0, 1); // 3 direction

		gamma1[2][3-dir_index] += (floattype2)(1,0); // -2 direction
		gamma1[5][3-dir_index] -= (floattype2)(1,0); // 2 direction
		}
	else
		{
		gamma1[1][2*(1 - dir_index/2) + dir_index%2] += (floattype2)(0, -1); // -3 direction
		gamma1[6][2*(1 - dir_index/2) + dir_index%2] -= (floattype2)(0, -1); // 3 direction

		gamma1[2][3-dir_index] += (floattype2)(-1, 0); // -2 direction
		gamma1[5][3-dir_index] -= (floattype2)(-1, 0); // 2 direction
		}
	}

Link current;
// go through links with positive directions
for (int Mu = 1; Mu <= 4; Mu++)
	{
	current = links[GetEl(site, Mu)];
	// here we've got the link-part of the Dirak's operator
	// external field added somewhere here.. if one wants to

	// go through gamma-indecies (dirak-indecies)
	// and add stuff to res
	for (int i=0; i<4; i++)
		{
		if (gamma1[3+Mu][i].x==0 && gamma1[3+Mu][i].y==0) continue;
//		maddLinkToRes(&res[0], &vec[3+Mu][i][0], gamma1[Mu-1][dir_index][i], current);
//		maddLinkToRes(&res, &vec[3+Mu][i], gamma1[Mu-1][dir_index][i], current);

floattype2 temp = (floattype2)(0, 0);

//if (col==500) printf("# %d\n", i);
//if (col==500) printf("%1.0f %1.0f -- %1.0f %1.0f -- %1.0f %1.0f\n", current.a20.x, current.a20.y, current.a21.x, current.a21.y, current.a22.x, current.a22.y);
//if (col==500) printf("%1.0f %1.0f -- %1.0f %1.0f -- %1.0f %1.0f\n", CM(current.a00, vec[3+Mu][i][0]).x, CM(current.a00, vec[3+Mu][i][0]).y, CM(current.a01, vec[3+Mu][i][1]).x, CM(current.a01, vec[3+Mu][i][1]).y, CM(current.a02, vec[3+Mu][i][1]).x, CM(current.a02, vec[3+Mu][i][2]).y);

temp = CM(current.a00, vec[3+Mu][i][0]) + CM(current.a01, vec[3+Mu][i][1]) + CM(current.a02, vec[3+Mu][i][2]);
/*
if (col==500) printf("%d\n", KAPPA);
if (col==500) printf("%1.0f %1.0f\n", gamma1[3+Mu][i].x, gamma1[3+Mu][i].y);
if (col==500) printf("%1.0f %1.0f\n", temp.x, temp.y);
if (col==500) printf("%1.0f %1.0f\n", (KAPPA*CM(gamma1[3+Mu][i], temp)).x, (KAPPA*CM(gamma1[3+Mu][i], temp)).y);
*/

//res[0] -= temp;
res[0] -= KAPPA*CM(gamma1[3+Mu][i], temp);// vec[3+Mu][i][0];//1;// gamma1[Mu-1][dir_index][i];// current.a00;//

temp = CM(current.a10, vec[3+Mu][i][0]) + CM(current.a11, vec[3+Mu][i][1]) + CM(current.a12, vec[3+Mu][i][2]);
//res[1] -= temp;
res[1] -= KAPPA*CM(gamma1[3+Mu][i], temp);// vec[3+Mu][i][1];//1;// gamma1[Mu-1][dir_index][i];// current.a00;//

temp = CM(current.a20, vec[3+Mu][i][0]) + CM(current.a21, vec[3+Mu][i][1]) + CM(current.a22, vec[3+Mu][i][2]);
//res[2] -= temp;
res[2] -= KAPPA*CM(gamma1[3+Mu][i], temp);// vec[3+Mu][i][2];//1;// gamma1[Mu-1][dir_index][i];// current.a00;//

		}
	}

//and links with negative diractions
for (int Mu = -4; Mu <= -1; Mu++)
	{
	current = HermConj(links[GetEl( GetNode(site,Mu), -Mu)]);
//if (col>=500 && col<504) printf("%d # %1.0f %1.0f ; %1.0f %1.0f ; %1.0f %1.0f ; %1.0f %1.0f \n", Mu, gamma1[4+Mu][0].x, gamma1[4+Mu][0].y, gamma1[4+Mu][1].x, gamma1[4+Mu][1].y, gamma1[4+Mu][2].x, gamma1[4+Mu][2].y, gamma1[4+Mu][3].x, gamma1[4+Mu][3].y);
	for (int i=0; i<4; i++)
		{
		if (gamma1[4+Mu][i].x==0 && gamma1[4+Mu][i].y==0) continue;
//		maddLinkToRes(&res[0], &vec[4+Mu][i][0], gamma1[4+Mu][dir_index][i], current);
//		maddLinkToRes(&res, &vec[4+Mu][i], gamma1[4+Mu][dir_index][i], current);
//		maddLinkToRes(res, &vec[4+Mu][i][0], gamma1[4+Mu][dir_index][i], &current);

floattype2 temp; // = (floattype2)(0, 0);
//if (col==500) printf("MU %d#%d\n", Mu, i);

//if (col==500 && Mu==-4) printf("%1.0f %1.0f -- %1.0f %1.0f -- %1.0f %1.0f\n", CM(current.a00, vec[4+Mu][i][0]).x, CM(current.a00, vec[4+Mu][i][0]).y, CM(current.a01, vec[4+Mu][i][1]).x, CM(current.a01, vec[4+Mu][i][1]).y, CM(current.a02, vec[4+Mu][i][2]).x, CM(current.a02, vec[4+Mu][i][2]).y);

temp = CM(current.a00, vec[4+Mu][i][0]) + CM(current.a01, vec[4+Mu][i][1]) + CM(current.a02, vec[4+Mu][i][2]);
res[0] -= KAPPA*CM(gamma1[4+Mu][i], temp);// current.a00;//

temp = CM(current.a10, vec[4+Mu][i][0]) + CM(current.a11, vec[4+Mu][i][1]) + CM(current.a12, vec[4+Mu][i][2]);
res[1] -= KAPPA*CM(gamma1[4+Mu][i], temp);// current.a00;//

temp = CM(current.a20, vec[4+Mu][i][0]) + CM(current.a21, vec[4+Mu][i][1]) + CM(current.a22, vec[4+Mu][i][2]);
res[2] -= KAPPA*CM(gamma1[4+Mu][i], temp);// current.a00;//

		}
	}



//put res to resvec
	for (int i = 0; i < 3; i++)
	{
	resvec[3*col+i] = res[i];// 1;//
	}

}

__kernel void FillWith(floattype valueX, floattype valueY, __global floattype2 *vec)
{
	int n = get_global_id(0);
    //printf("%d",n);
	vec[3*n] = (floattype2)(valueX,valueY);
    vec[3*n+1] = (floattype2)(valueX,valueY);
    vec[3*n+2] = (floattype2)(valueX,valueY);
}

__kernel void FillWithRandom(__global floattype2 *vec, char normaldistribution, __global uint4* seed)
{
	int n = get_global_id(0);
    //printf("%d",n);
	uint4 rng[1];
	rng[0] = seed[n];

	floattype2 rnd = (floattype2)(GetRandom(rng),GetRandom(rng));
	if (normaldistribution) rnd = GetGaussRandom(rnd);
	vec[3*n] =  rnd;

	rnd = (floattype2)(GetRandom(rng),GetRandom(rng));
	if (normaldistribution) rnd = GetGaussRandom(rnd);
    vec[3*n+1] = rnd;

 	rnd = (floattype2)(GetRandom(rng),GetRandom(rng));
	if (normaldistribution) rnd = GetGaussRandom(rnd);
    vec[3*n+2] = rnd;

	seed[n] = rng[0];
}


__kernel void FillLinkWith(floattype valueX, floattype valueY, __global Link *U)
{
	int n = get_global_id(0);
    //printf("%d",n);
	Link L;
	floattype2 c = (floattype2)(valueX, valueY);
    L.a00=c;L.a01=c;L.a02=c;
    L.a10=c;L.a11=c;L.a12=c;
    L.a20=c;L.a21=c;L.a22=c;
	U[n] = L;
	
}


__kernel void AXPY(floattype2 alpha, __global floattype2 *X, __global floattype2 *Y, __global floattype2 *res)
{
	int n = get_global_id(0);
    //printf("%d",n);
	
	res[3*n] = CM(alpha,X[3*n])+Y[3*n];
    res[3*n+1] = CM(alpha,X[3*n+1])+Y[3*n+1];
    res[3*n+2] =  CM(alpha,X[3*n+2])+Y[3*n+2];
}


__kernel void XhermY(__global floattype2 *X, __global floattype2 *Y, __global floattype2 *res, __global floattype2 *resgroups, __local floattype2 *ldata)
{
	int n = get_global_id(0);
	int lid = get_local_id(0);
    //printf("%d",n);
	
	floattype2 mul = CM(Conj(X[3*n]),Y[3*n])+CM(Conj(X[3*n+1]),Y[3*n+1])+CM(Conj(X[3*n+2]),Y[3*n+2]);



	ldata[lid] = mul;

	barrier(CLK_LOCAL_MEM_FENCE);

	for (unsigned int s=get_local_size(0)/2;s>0;s>>=1)
	{
		if (lid<s) ldata[lid]+=ldata[lid+s];
		barrier(CLK_LOCAL_MEM_FENCE);
	}

	if (lid==0) resgroups[get_group_id(0)]=ldata[0];
	
	if (n==0){
		floattype2 sum=0;
		for (int i=0;i<get_num_groups(0);i++) sum+=resgroups[i];

		res[0] = sum;
	}
	
}

void GetSU2Random(floattype2 *a00,floattype2 *a01,floattype2 *a10,floattype2 *a11, uint4 *rnd)
{
            floattype e = 0.2;//0.1
            floattype x0m = 0, x1m = 0, x2m = 0, x3m = 0; 
					floattype r0 = GetRandom(rnd) - 0.5, r1 = GetRandom(rnd) - 0.5, r2 = GetRandom(rnd) - 0.5, r3 = GetRandom(rnd) - 0.5;
                    x0m = sqrt(1 - e * e);
                    // if (r0 < 0) x0m = -x0m;
                    floattype len = sqrt(r1 * r1 + r2 * r2 + r3 * r3);
                    x1m = e * r1 / len; x2m = e * r2 / len; x3m = e * r3 / len;

                    *a00 = (floattype2)(x0m, x3m);
                    *a01 = (floattype2)(x2m, x1m);
                    *a10 = (floattype2)(-x2m, x1m);
                    *a11 = (floattype2)(x0m, -x3m);
}

Link GetRandomMetroLink(uint4 *rnd)
        {



			Link X = Fill(1.0);

                    floattype2 A00[1], A01[1], A10[1], A11[1];

                    Link T = Fill(1.0);

					GetSU2Random(A00, A01, A10, A11, rnd);

                    T.a00 = A00[0];
                    T.a01 = A01[0];
                    T.a10 = A10[0];
                    T.a11 = A11[0];
                    X = Mul(T, X);

					GetSU2Random(A00, A01, A10, A11, rnd);

					T = Fill(1.0);
                    T.a00 = *A00;
                    T.a02 = *A01;
                    T.a20 = *A10;
                    T.a22 = *A11;
                    X = Mul(T, X);

					GetSU2Random(A00, A01, A10, A11, rnd);

					T = Fill(1.0);
                    T.a11 = *A00;
                    T.a12 = *A01;
                    T.a21 = *A10;
                    T.a22 = *A11;
                    X = Mul(T, X);

            return X;
        }

__kernel void BackupLink(__global Link *U, int x, int y, int z, int t, int mu, __global Link *Storage, __global uint4* seed, __global floattype *dS)
{
	Coord s;
	s.x = x; s.y = y; s.z=z; s.t=t;
	int n = GetEl(s,mu);
	uint4 rng[1];
	rng[0] = seed[n/8];

	Link X = Ortho(GetRandomMetroLink(rng));

	//printf("X %f %f %f %f %f %f\n",X.a00.x, X.a11.x,X.a22.x,X.a00.y, X.a11.y,X.a22.y);
	Link oldlink =  U[n];
	//printf("backup %f %f %f\n",oldlink.a00.x, oldlink.a11.x,oldlink.a22.x);
	Storage[0] = oldlink;
	U[n] = Mul(X,oldlink);
	
	Link T[1];
	T[0] = Fill(0.0);
	Link A = GetSumOfStaples(U,s,mu,T);

	//X-I
	X.a00-=1;X.a11-=1;X.a22-=1;
	Link temp = Mul(X, Mul(oldlink, A));
	
	dS[0] = -1.0 / 3.0 * (temp.a00+temp.a11+temp.a22).x;//without beta!!!
	//printf("backup %f %f %f dS = %f\n",oldlink.a00.x, oldlink.a11.x,oldlink.a22.x,dS[0]);
 seed[n/8] = rng[0];
}

__kernel void RestoreLink(__global Link *U, int x, int y, int z, int t, int mu, __global Link *Storage)
{
	Coord s;
	s.x = x; s.y = y; s.z=z; s.t=t;
	U[GetEl(s,mu)] = Storage[0];
	//printf("restore %f %f %f\n",Storage[0].a00.x, Storage[0].a11.x,Storage[0].a22.x);
	
}

