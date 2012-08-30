/*
#define double float
#define double2 float2
#define double4 float4

typedef struct 
{
    double2 a00,a01,a02;
    double2 a10,a11,a12;
    double2 a20,a21,a22;
} Link;

typedef struct 
{
    int x,y,z,t;
} Coord;

double2 CM(double2 a, double2 b);
Coord GetPosEven(uint n);
Coord GetPosOdd(uint n);
Coord GetPosAllSpace(uint n);
Coord GetNode(Coord Site, int dir);
Link HermConj(Link U);
Link Identity();
uint GetSiteIndex (Coord Site);
uint GetEl(Coord Site, int mu);
*/

/*
uint get_node_number(uint col)
{ return col/4; }

uint get_dirak_index(uint col)
{ return col%4; }
//{ return col - 4*(col/4); }
*/

// res = (current X vec) * gamma1_coef
// (here X is matrix-product, * is simple product)
void maddLinkToRes(double2 *res, // 3-comp vector for result
                   double2 *vec, // 3-comp input vector
                   double2 gamma1_coef, // coeficient from gamma matrix
                   Link *current) // 3x3 Link matrix
{
double2 temp; // = (double2)(0, 0);

//temp = CM(current.a00, vec[0]) + CM(current.a01, vec[1]) + CM(current.a02, vec[2]);
res[0] += vec[0];// (*current).a00;// gamma1_coef;// CM(gamma1_coef, temp);// (double2)(1, 0);//

//temp = CM(current.a10, vec[0]) + CM(current.a11, vec[1]) + CM(current.a12, vec[2]);
res[1] += vec[1];// (*current).a00;// gamma1_coef;// CM(gamma1_coef, temp);// 0;//

//temp = CM(current.a20, vec[0]) + CM(current.a21, vec[1]) + CM(current.a22, vec[2]);
res[2] += vec[2];// (*current).a00;// gamma1_coef;// CM(gamma1_coef, temp);//
}

/*
double2 temp; // = (double2)(0, 0);
temp = CM(current.a00, vec[0]) + CM(current.a01, vec[1]) + CM(current.a02, vec[2]);
res[0] += CM(gamma1_coef, temp);//

temp = CM(current.a10, vec[0]) + CM(current.a11, vec[1]) + CM(current.a12, vec[2]);
res[1] += CM(gamma1_coef, temp);//

temp = CM(current.a20, vec[0]) + CM(current.a21, vec[1]) + CM(current.a22, vec[2]);
res[2] += CM(gamma1_coef, temp);//
*/

// this is the kernel for multipling vector in buffer "vector"
// with dirak matrix, implied by buffer "links"
// result goes to vector in buffer "resvec"
//   dirak oparator's matrix has 12*N columns and rows, where N -- number of nodes
//   (i.e. it has N node indecies, each node has 4 dirak indecies and 3 color)
//   so vector and resvec have 12*N items (complex values, represented by double2)
//     for convinience vector and resvec are devided into blocks with 3 values,
//     each corresponding to a dirak index
//     (so each node-index correspondes to 4 such blocks)
//     (each block has 3 color indecies)
//     (worksize*3 == size of the vectors)
//     N nodes --> 4*N working blocks (== col) --> 3*4*N complex values in vectors
// synchronization issue: vector ans resvec should be different buffers
__kernel void dirakMatrMul(__global Link* links,
                           __global double2* vector,
                           __global double2* resvec)
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

double2 vec[8][4][3];// 12-component parts of the vector, corresponding for neibours
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

double2 res[3];// 3-component part for the resvec
for (int i = 0; i < 3; i++)
	{
	// since the diagonal part of the dirak matrix == 1
	// we initialy take the corresponding part of the vector
	res[i] = vector[3*col+i];
	}

// !! add gamma1 for -mu directions

double2 gamma1[4][4][4] = {
                           {{(double2)(0,0),(double2)(0,0),(double2)(0,0),(double2)(0,0)},
                            {(double2)(0,0),(double2)(0,0),(double2)(0,0),(double2)(0,0)},
                            {(double2)(0,0),(double2)(0,0),(double2)(2,0),(double2)(0,0)},
                            {(double2)(0,0),(double2)(0,0),(double2)(0,0),(double2)(2,0)}},

                           {{(double2)(1,0),(double2)(0,0),(double2)(0,0),(double2)(-1,0)},
                            {(double2)(0,0),(double2)(1,0),(double2)(-1,0),(double2)(0,0)},
                            {(double2)(0,0),(double2)(1,0),(double2)(1,0),(double2)(0,0)},
                            {(double2)(1,0),(double2)(0,0),(double2)(0,0),(double2)(1,0)}},

                           {{(double2)(1,0),(double2)(0,0),(double2)(0,0),(double2)(0,-1)},
                            {(double2)(0,0),(double2)(1,0),(double2)(0,1),(double2)(0,0)},
                            {(double2)(0,0),(double2)(0,1),(double2)(1,0),(double2)(0,0)},
                            {(double2)(0,-1),(double2)(0,0),(double2)(0,0),(double2)(1,0)}},

                           {{(double2)(1,0),(double2)(0,0),(double2)(-1,0),(double2)(0,0)},
                            {(double2)(0,0),(double2)(1,0),(double2)(0,0),(double2)(1,0)},
                            {(double2)(1,0),(double2)(0,0),(double2)(1,0),(double2)(0,0)},
                            {(double2)(0,0),(double2)(-1,0),(double2)(0,0),(double2)(1,0)}},
                           };

// 1-gamma_mu matricies -- each of the 4 directions has matrix
// double2 gamma1[4][4][4]; ?

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
		if (gamma1[Mu-1][dir_index][i].x==0 && gamma1[Mu-1][dir_index][i].y==0) continue;
//		maddLinkToRes(&res[0], &vec[3+Mu][i][0], gamma1[Mu-1][dir_index][i], current);
//		maddLinkToRes(&res, &vec[3+Mu][i], gamma1[Mu-1][dir_index][i], current);

double2 temp = (double2)(0, 0);

temp = CM(current.a00, vec[3+Mu][i][0]) + CM(current.a01, vec[3+Mu][i][1]) + CM(current.a02, vec[3+Mu][i][2]);
res[0] += CM(gamma1[Mu-1][dir_index][i], temp);// vec[3+Mu][i][0];//1;// gamma1[Mu-1][dir_index][i];// current.a00;//

temp = CM(current.a10, vec[3+Mu][i][0]) + CM(current.a11, vec[3+Mu][i][1]) + CM(current.a12, vec[3+Mu][i][2]);
res[1] += CM(gamma1[Mu-1][dir_index][i], temp);// vec[3+Mu][i][1];//1;// gamma1[Mu-1][dir_index][i];// current.a00;//

temp = CM(current.a20, vec[3+Mu][i][0]) + CM(current.a21, vec[3+Mu][i][1]) + CM(current.a22, vec[3+Mu][i][2]);
res[2] += CM(gamma1[Mu-1][dir_index][i], temp);// vec[3+Mu][i][2];//1;// gamma1[Mu-1][dir_index][i];// current.a00;//

		}
	}

//and links with negative diractions
for (int Mu = -4; Mu <= -1; Mu++)
	{
	current = HermConj(links[GetEl( GetNode(site,Mu), -Mu)]);
	for (int i=0; i<4; i++)
		{
		if (gamma1[4+Mu][dir_index][i].x==0 && gamma1[4+Mu][dir_index][i].y==0) continue;
//		maddLinkToRes(&res[0], &vec[4+Mu][i][0], gamma1[4+Mu][dir_index][i], current);
//		maddLinkToRes(&res, &vec[4+Mu][i], gamma1[4+Mu][dir_index][i], current);
//		maddLinkToRes(res, &vec[4+Mu][i][0], gamma1[4+Mu][dir_index][i], &current);

double2 temp; // = (double2)(0, 0);

temp = CM(current.a00, vec[4+Mu][i][0]) + CM(current.a01, vec[4+Mu][i][1]) + CM(current.a02, vec[4+Mu][i][2]);
res[0] += CM(gamma1[4+Mu][dir_index][i], temp);// current.a00;//

temp = CM(current.a10, vec[4+Mu][i][0]) + CM(current.a11, vec[4+Mu][i][1]) + CM(current.a12, vec[4+Mu][i][2]);
res[1] += CM(gamma1[4+Mu][dir_index][i], temp);// current.a00;//

temp = CM(current.a20, vec[4+Mu][i][0]) + CM(current.a21, vec[4+Mu][i][1]) + CM(current.a22, vec[4+Mu][i][2]);
res[2] += CM(gamma1[4+Mu][dir_index][i], temp);// current.a00;//

		}
	}

//if (col==500) printf("%d\n", n);

//put res to resvec
for (int i = 0; i < 3; i++)
	{
	resvec[3*col+i] = res[i];// 1;//
	}

}
