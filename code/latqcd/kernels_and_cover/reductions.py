
def PL_mean(self):
        # making numpy-array with local-memory shape
        #print "Start reduction"
        self.program.Ploop_mean(self.queue, self.global_work_shape.shape, self.local_work_shape.shape, self.Const_int_buf,  self.Link_buf, self.S_out_of_groups, self.S_res, cl.LocalMemory(self.prop['Slocalsize']*self.prop['Type'].itemsize) ) #, self.Seed_buf,self.Seed_debug_buf)
	cl.enqueue_copy(self.queue, self.S_out_of_groups_shape, self.S_out_of_groups).wait()
	r=numpy.zeros(7)
	for i in xrange(len(self.S_out_of_groups_shape)):
		r[i%7]+=self.S_out_of_groups_shape[i]
        return r/(self.prop['Ns']**3*self.prop['Nt']*6)

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

def P_loop_correlation(self, vect, N_update):
        # making numpy-array with local-memory shape
        print "Start PL reduction"
        ploops = []
	for i in xrange(N_update):
            self.program.PLoop(self.queue, self.space_work_shape, None, self.Const_int_buf, self.Link_buf, self.PL_res_buf)
            self.Update()
	    cl.enqueue_copy(self.queue, self.PL_res, self.PL_res_buf).wait()
            ploops.append(self.PL_res)

        result = numpy.zeros((len(self.PL_res)/2, 2))
        #result = numpy.zeros((self.SpaceSites))
        Nspace = self.prop['Ns']

        m_ploops = numpy.zeros_like(ploops[0])
        for i in xrange(len(m_ploops)):
            for slyce in ploops:
                m_ploops[i] += slyce[i]
            m_ploops[i] /= len(ploops)

        #for i in xrange(len(self.PL_res)/2):
        for i in xrange(self.SpaceSites):
            coord = self.n2coord(i)
            x = self.coord2n((coord[0]+vect[0], coord[1]+vect[1], coord[2]+vect[2]))
            for n in xrange(len(ploops)**2):
                i_r = ploops[n % N_update][2*i] - m_ploops[2*i]
                i_i = ploops[n % N_update][2*i + 1] - m_ploops[2*i + 1]
                x_r = ploops[n / N_update][2*x] - m_ploops[2*x]
                x_i = -( ploops[n / N_update][2*x + 1] - m_ploops[2*x + 1] )   # Conjugate
                result[i][0] += x_r*i_r - x_i*i_i
                result[i][1] += x_r*i_i + x_i*i_r
                #i_r = ploops[n % N_update][i]
                #x_r = ploops[n / N_update][x]
                #result[i] += x_r*i_r
            result[i][0]/=(len(ploops)**2)
            result[i][1]/=(len(ploops)**2)
            #result[i]/=(len(ploops)**2)

        r = 0
        for i in result:
            r+=numpy.sqrt(i[0]**2 + i[1]**2)
        r/=len(result)
        print "PL Reduction done"
        return r

def S_reduction100(self):
        self.program.CalcSREDUCTLEN(self.queue, self.global_work_shape.shape, self.local_work_shape.shape, self.Const_int_buf, self.Const_float_buf, self.Link_buf, self.S_out_of_groups,  cl.LocalMemory(self.prop['Slocalsize']*self.prop['Type'].itemsize), self.Seed_buf)
	cl.enqueue_copy(self.queue, self.S_out_of_groups_shape, self.S_out_of_groups).wait()
	r=numpy.zeros(7)
	for i in xrange(len(self.S_out_of_groups_shape)):
		r[i%7]+=self.S_out_of_groups_shape[i]
        return r/(self.prop['Ns']**3*self.prop['Nt']*6)

task_types = {"S":S_reduction100, 'PLmean': PL_mean, 'PLcorr': P_loop_correlation}
