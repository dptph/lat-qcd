/*
double2 CM(double2 a, double2 b);
double2 Conj(double2 a);
*/

// the kernel for scalar multiplication of two vectors
// vectors have size == N*12
//   (N == SiteNumber == Nt*Nx*Ny*Nz)
// so 12N threads (aka kernels for work-items) are opened in the program
// and %d amount of local shared memory % (64)
__kernel void scalarMul(__global double2* left, __global double2* right, __local double2* ldata, __global double2* groups)
//                        __global double2* res,
{
uint n = get_global_id(0);
//uint size = get_global_size(0);
uint lid = get_local_id(0);
uint gid = get_group_id(0);

ldata[lid] = CM(Conj(left[n]), right[n]);//!!!!!!!!!!!!!!!!!!!!!

barrier(CLK_LOCAL_MEM_FENCE);

for (unsigned int s=get_local_size(0)/2;s>0;s>>=1)
	{
	if (lid<s) ldata[lid]+=ldata[lid+s];
	barrier(CLK_LOCAL_MEM_FENCE);
	}

if (lid==0) groups[gid]=ldata[0];
/*	
if (n==0)
	{
	double res=0;
	for (int i=0;i<get_num_groups(0);i++) res+=groups[i];
	Result[0] = res;
	}
*/
}
