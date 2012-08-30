import pyopencl as cl
mf = cl.mem_flags
import numpy
numpy.set_printoptions(linewidth=250)
from sys import getsizeof as sizeof

#from reductions import *
#import reductions

class GD:
    local_group_benchmark = 64
    prop = {}
    Type = numpy.dtype(numpy.float32)
    prop['Ns'] =4
    prop['Nt'] =2
    prop['beta'] =3.0
    prop['flux'] =1
    prop['kappa'] =1
    #prop['Slocalsize'] = 64
    SiteNumber = 128
    SpaceSites = 64
    ThreadsNumb = SiteNumber/2
    Slocalsize = 64
    SNumGroups = ThreadsNumb / Slocalsize
    consts_int = numpy.array((prop['Ns'], prop['Nt']))
    consts_float = numpy.array((prop['beta'], prop['flux']), dtype=Type)

    def set_dependent_properties(self):
       self.SiteNumber = self.prop['Nt']*self.prop['Ns']**3
       self.SpaceSites = self.prop['Ns']**3
       self.ThreadsNumb = self.SiteNumber/2
       if self.ThreadsNumb % 2 == 1:
          raise ValueError("Odd number of threads : " + `self.ThreadsNumb`)
       self.Slocalsize = 2
       while not self.ThreadsNumb % (self.Slocalsize*2) and self.Slocalsize <= self.local_group_benchmark:
          self.Slocalsize *= 2

       #self.SNumGroups = self.ThreadsNumbes / self.prop['Slocalsize']
       self.SNumGroups = self.ThreadsNumb / self.Slocalsize
       self.consts_int = numpy.array((self.prop['Ns'],self.prop['Nt']))
       self.consts_float = numpy.array((self.prop['beta'], self.prop['flux']), dtype=self.Type)

    def set_properties(self, properties):
       self.prop.update(properties)
       self.set_dependent_properties()

    def set_seed_links_cpu(self):
        self.Seed = numpy.random.randint(0,1000000, 4*self.SiteNumber/2).astype(numpy.uint32) # GO XOR128
        self.Link = numpy.zeros((self.SiteNumber*4*9*2), dtype=self.Type)
	for i in xrange(4*self.SiteNumber):
            self.Link[i*9*2]=1
            self.Link[i*9*2+8]=1
            self.Link[i*9*2+16]=1

    def renew_seed_links(self):
        self.Seed = numpy.random.randint(0,1000000, 4*self.ThreadsNumb).astype(numpy.uint32) # GO XOR128
        # self.ThreadNumb == self.SiteNumber / 2
        self.Link = numpy.zeros((self.SiteNumber*4*9*2), dtype=self.Type)
	for i in xrange(4*self.SiteNumber):
            self.Link[i*9*2]=1
            self.Link[i*9*2+8]=1
            self.Link[i*9*2+16]=1
	cl.enqueue_copy(self.queue, self.Seed_buf, self.Seed).wait()
	cl.enqueue_copy(self.queue, self.Link_buf, self.Link).wait()

    def load_program(self, filename="kernels_and_cover/gluodynamic_xor128.cl"):
        #read in the OpenCL source file as a string
#        fstr = "#define Ns " +str(self.prop['Ns']) + "\n" \
#               +"#define Nt " +str(self.prop['Nt']) + "\n"\
#               +"#define Ns2 "+str(self.prop['Ns']**2) + "\n" 
#               +"#define Ns3 "+str(self.prop['Ns']**3)+"\n"
        f = open(filename, 'r')
        fstr = "".join(f.readlines())
        fstr = "constant uint Ns="+str(self.prop['Ns'])+",Nt="+str(self.prop['Nt'])+",Ns2="+str(self.prop['Ns']**2)+", Ns3="+str(self.prop['Ns']**3)+";\n" + fstr
        #print "fstr-line starts with: ", fstr[0:15]
        #print "ends with: ", fstr[-15:]
        print "Start building prgram"
        #create the program
        self.program = cl.Program(self.ctx, fstr).build(options=['-I', '.'])
        print "Load done"

    def Update(self):
        #self.program.Update(self.queue, self.global_work_shape.shape, None, self.Const_int_buf, self.Const_float_buf, self.Seed_buf, self.Link_buf)#, self.Seed_debug_buf)
        self.program.Update(self.queue, (self.ThreadsNumb,), None, self.Const_int_buf, self.Const_float_buf, self.Seed_buf, self.Link_buf)#, self.Seed_debug_buf)

    '''
    def thermolization(self, N):
        while N>0:
          self.Update()
          N-=1
    '''

    def init_speciality(self):
      return None

    def __init__(self, properties={}):
     #self.ctx = cl.create_some_context(0)
     plat = cl.get_platforms()[0]
     device_list = plat.get_devices(cl.device_type.GPU)
     # set self.local_group_benchmark accordingly to the device's capabilities
     self.ctx = cl.Context(device_list)
     self.queue = cl.CommandQueue(self.ctx)

     self.set_properties(properties)
     self.set_seed_links_cpu()
     self.load_program()

     self.Seed_buf = cl.Buffer(self.ctx, mf.READ_WRITE | mf.COPY_HOST_PTR, hostbuf=self.Seed)
     self.Link_buf = cl.Buffer(self.ctx, mf.READ_WRITE | mf.COPY_HOST_PTR, hostbuf=self.Link)
     self.Const_int_buf = cl.Buffer(self.ctx, mf.READ_ONLY | mf.COPY_HOST_PTR, hostbuf=self.consts_int)
     self.Const_float_buf = cl.Buffer(self.ctx, mf.READ_ONLY | mf.COPY_HOST_PTR, hostbuf=self.consts_float)

     self.init_speciality()
     print "Init done"


class S(GD):
   def init_speciality(self):
     self.S_out_of_groups_cpu = numpy.empty((self.SNumGroups*7), dtype=self.Type)
     self.S_out_of_groups_buf = cl.Buffer(self.ctx, mf.READ_WRITE, self.S_out_of_groups_cpu.nbytes)
     self.reduct_format=7

   def S_reduction100(self):
        self.program.CalcSREDUCTLEN(self.queue, (self.ThreadsNumb,), (self.Slocalsize,), self.Const_int_buf, self.Const_float_buf, self.Link_buf, self.S_out_of_groups_buf,  cl.LocalMemory(self.Slocalsize*self.Type.itemsize), self.Seed_buf)
	cl.enqueue_copy(self.queue, self.S_out_of_groups_cpu, self.S_out_of_groups_buf).wait()
	r=numpy.zeros(7)
	for i in xrange(len(self.S_out_of_groups_cpu)):
		r[i%7]+=self.S_out_of_groups_cpu[i]
        return r/(self.prop['Ns']**3*self.prop['Nt']*6)
   reduction = S_reduction100

class PL(GD):
   def init_speciality(self):
     self.Plocalsize = self.Slocalsize # not always
     self.PNumGroups = self.SpaceSites / self.Slocalsize
     self.PL_out_of_groups_cpu = numpy.empty((self.PNumGroups), dtype=self.Type)
     self.PL_out_of_groups_buf = cl.Buffer(self.ctx, mf.READ_WRITE, self.PL_out_of_groups_cpu.nbytes)

     self.PL_res_cpu = numpy.empty((1), dtype=self.Type)
     self.PL_res_buf = cl.Buffer(self.ctx, mf.READ_WRITE, self.PL_out_of_groups_cpu.nbytes)
     self.reduct_format=1

   def PL_mean(self):
        # making numpy-array with local-memory shape
        self.program.Ploop_mean(self.queue, (self.SpaceSites,), (self.Plocalsize,), self.Const_int_buf,  self.Link_buf, self.PL_out_of_groups_buf, self.PL_res_buf, cl.LocalMemory(self.Plocalsize*self.Type.itemsize) ) #, self.Seed_buf,self.Seed_debug_buf)
	cl.enqueue_copy(self.queue, self.PL_res_cpu, self.PL_res_buf).wait()
        return self.PL_res_cpu[0]/self.SpaceSites
   reduction = PL_mean

class PL_corr(GD):
    def init_speciality(self):
      self.PL_res_cpu=numpy.empty((self.SpaceSites * 2), dtype=self.Type)
      self.PL_res_buf = cl.Buffer(self.ctx, mf.READ_WRITE, self.PL_res_cpu.nbytes)
      self.reduct_format=1

    def n2coord(self, n):
        Ns = self.prop['Ns']
        n = n%Ns**3
        z = n/Ns**2
        y = (n%Ns**2)/Ns
        x = (n%Ns**2)%Ns
        return x,y,z
    def coord2n(self, coord):
        Ns = self.prop['Ns']
        return ( coord[2]*Ns**2 + coord[1]*Ns + coord[0] ) % Ns**3
    def give_correlations(self, ploops, vect):
        
        a_ploops_raw = numpy.zeros_like(ploops[0])
        for slyce in ploops:
            a_ploops_raw += slyce
        a_ploops_raw /= len(ploops)

        a_ploops = numpy.zeros_like(a_ploops_raw)
        m_ploops = numpy.zeros_like(ploops[0])
        for i in xrange(self.SpaceSites):
            coord = self.n2coord(i)
            x = self.coord2n((coord[0]+vect[0],
                              coord[1]+vect[1],
                              coord[2]+vect[2]))
            a_ploops[2*i] = a_ploops_raw[2*i]*a_ploops_raw[2*x] - \
                         a_ploops_raw[2*i+1]*(-a_ploops_raw[2*x+1])
            a_ploops[2*i+1] = a_ploops_raw[2*i+1]*a_ploops_raw[2*x] + \
                         a_ploops_raw[2*i]*(-a_ploops_raw[2*x+1])
            for slyce in xrange(len(ploops)):
                i_r = ploops[slyce][2*i]
                i_i = ploops[slyce][2*i + 1]
                x_r = ploops[slyce][2*x]
                x_i = ploops[slyce][2*x + 1]
                m_ploops[2*i] += i_r*x_r - i_i*(-x_i)
                m_ploops[2*i+1] += i_i*x_r + i_r*(-x_i)
        m_ploops /= len(ploops)

        corr = m_ploops - a_ploops
        result = numpy.zeros((len(corr)/2))
        for i in xrange(len(result)):
            result[i] = numpy.sqrt(corr[2*i]**2 + corr[2*i+1]**2)
        return result

    def P_loop_correlation(self, vect, N_update):
        # making numpy-array with local-memory shape
        print "Start PL reduction"
        ploops = []
        for i in xrange(N_update):
            self.program.Ploop_corr(self.queue, self.space_work_shape, None, self.Const_int_buf, self.Link_buf, self.PL_res_buf)
            self.Update()
	    cl.enqueue_copy(self.queue, self.PL_res, self.PL_res_buf).wait()
            ploops.append(self.PL_res.copy())
        print ploops[0][:10]
        print ploops[1][:10]

        corr = self.give_correlations(ploops, vect)
        print corr
        return sum(corr)/len(corr)
    #reduction = ? -- P_loop_correlation -- isn't end-function

objects = {'GD': GD, 'S': S, 'PL_mean': PL, 'PL_corr': PL_corr}
