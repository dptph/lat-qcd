import kernels_and_cover.main_GD_modul as class_GD_cover
from pickle import dump
import time
import numpy
import gather

#measurement_subjects={"S":"S_reduction()"}
measurement_subjects={"S":"S_reduction100()"}
hash_task_descript={0:"measurement object ", 1:"number of measurements in one succession ", 2:"therm steps ", 3:"one measurement length ", 4:"corr steps ", 5:"Ns ", 6:"Nt ", 7:"Beta ", 8:"Flux "}
hash_task={'0':"meas_object", '1':"succession_len", '2':"therm_steps_number", '3':"meas_len", '4':"corr_steps", '5':"Ns", '6':"Nt", '7':"beta", '8':"flux"}

#task = [N meas=[props & surq]]

def set_up_workspace():
   Ns=int(raw_input("Enter N_s_"))
   Nt=int(raw_input("Enter N_t_"))
   b=float(raw_input("Enter b"))
   f=float(raw_input("Enter flux"))

   work_space = class_GD_cover.GD()

   work_space.Nspace=Ns
   work_space.Ntime=Nt
   work_space.bconst=b
   work_space.flux=f

   # Slocalsize, Plocalsize ?
   work_space.set_depended_variables()

   #work_space.loadProgram("/home/alex/programming/ati_app/gluodynamics/kernels_and_cover/gluodynamic_xor128.cl")
   work_space.loadProgram("./kernels_and_cover/gluodynamic_xor128.cl")
   work_space.popCorn()
   return work_space

def thermolisation(work_space, therm_steps):
    for i in xrange(therm_steps): work_space.Update()

#def measurement(work_space, measure_subject, how_many_times, corr_step):
def measurement(work_space, parameters):
   measure_subject=parameters['meas_object']
   how_many_times=parameters['meas_len']
   corr_step=parameters['corr_steps']
   T=time.time()
   local_time = time.asctime( time.localtime(T) )
   # measure_subject can be enything, its just a key in the measument_subjects hash
   ok = False
   if measure_subject in measurement_subjects and type(how_many_times)==int and type(corr_step)==int:
      ok = True
      measure_subject=measurement_subjects[measure_subject]
   if ok:
      data = numpy.empty((how_many_times,7))

      available_name="/home/alex/magn_mass_meas/results/"
      numpy_name="/home/alex/magn_mass_meas/results/pickled_numpy_arrays/"
      namee='.'
      namee+="Ns "+str(work_space.prop['Ns'])+" Nt "+str(work_space.prop['Nt'])+" Beta "+str(work_space.prop['beta'])+" Flux "+str(work_space.prop['flux'])
      namee+=" _ "+"HMT "+str(how_many_times)+" corr "+str(corr_step)+" _ "+local_time +"_100c"
      available_name+=namee
      numpy_name+=".pickl."+str(T)

      f=open(available_name,'ab')
#      f.write(scilab_name+ "=["+ "\n")
      f.write("A=["+ "\n")
      # result=[]
      for i in xrange(how_many_times):
          for u in xrange(corr_step):
              work_space.Update()
          # result.append(eval("work_space."+measure_subject))
          data[i]=eval("work_space."+measure_subject)
          #f.write(str(data[i])[1:-1]+"\n")
      f.write(str(data)[1:-1]+"\n")
      f.write("]"+ "\n")
      print f.name
      f.close()
      metadata = {"workSpace":work_space.prop, "measurement": parameters}
      res= {'metadata':metadata, 'data':data}
      picklefile=open(numpy_name,'wb')
      dump(res, picklefile)
      print picklefile.name
      print local_time
      picklefile.close()
      return numpy_name
   else: print "Unknown measurement_subject"

def measurement_succession( properties ):
  '''
  properties == { 'meas_object' : ?? , 'succession_len' : ?? , 'therm_steps_number' : ?? , 'meas_len' : ?? , 'corr_steps' : ?? , 'Ns' : ?? , 'Nt' : ?? , 'beta' : ?? , 'flux' : ?? }
  meas_object = properties[0]
  succession_len = properties[1]
  therm_steps_number = properties[2]
  meas_len = properties[3]
  corr_steps = properties[4]
  Ns = properties[5]
  Nt = properties[6]
  beta = properties[7]
  flux = properties[8]
  '''
  """meas_object = properties['meas_object']
  succession_len = properties['succession_len']
  therm_steps_number = properties['therm_steps_number']
  meas_len = properties['meas_len']
  corr_steps = properties['corr_steps']
  Ns = properties['Ns']
  Nt = properties['Nt']
  beta = properties['beta']
  flux = properties['flux']"""

  success_file_names = []
  measurement_parameters={'meas_object':0, 'meas_len':0, 'corr_steps':0, 'therm_steps_number':0}
  for key in measurement_parameters.keys():
     measurement_parameters[key]=properties[key]

  for i in xrange(properties['succession_len']):
    work_space = class_GD_cover.GD()
    print "On the", work_space.ctx.devices
    #work_space.set_Ns_Nt_Beta_Flux(Ns, Nt, beta, flux)
    work_space.set_properties(properties)
    work_space.set_depended_variables()
    #work_space.loadProgram("/home/alex/programming/ati_app/gluodynamics/kernels_and_cover/gluodynamic_xor128.cl")
    work_space.loadProgram("./kernels_and_cover/gluodynamic_xor128.cl")
    work_space.popCorn()

    #thermolisation(work_space, therm_steps_number)
    thermolisation(work_space, properties['therm_steps_number'])
    #measurement(work_space, properties['meas_object'], properties['meas_len'], properties['corr_steps'])
    success_file_names.append( measurement(work_space, measurement_parameters) )
    del work_space
  return success_file_names


def do_tasks(tasks):
   """
   tasks = [ properties ]
   properties = {...}
   """
   results_names=[]
   for properties in tasks:
      results_names.append(measurement_succession(properties))
   return results_names

def make_tasks():
   """
   tasks = [ properties ]
   properties = {...}
   """
   N = int(raw_input("Number of successions = "))#
   tasks =[dict(meas_object=-1,succession_len=-1, therm_steps_number=-1,meas_len=-1,corr_steps=-1, Ns=-1,Nt=-1,beta=-1,flux=-1) for i in range(N)]
   print hash_task_descript
   lst = raw_input("Choose parameters to be the same in all successions: ")
   for key in hash_task:
      if lst.count(key):
         if key=='0':
           print "Reduction kernels: ", measurement_subjects
           val = raw_input(hash_task[key]+' = ').split()[0]
           for i in range(N): tasks[i][hash_task[key]] = val
         elif key=='7' or key=='8':
           val = float(raw_input(hash_task[key]+' = ').split()[0])
           for i in range(N): tasks[i][hash_task[key]] = val
         else:
           val = int(raw_input(hash_task[key]+' = ').split()[0])
           for i in range(N): tasks[i][hash_task[key]] = val

   for i, task in enumerate(tasks):
     for key in hash_task:
        if task[hash_task[key]]==-1:
           print "Succession - ", i+1
           if key=='0':
             print "Reduction kernels: ", measurement_subjects
             val = raw_input(hash_task[key]+' = ').split()[0]
             task[hash_task[key]] = val
           elif key=='7' or key=='8':
             val = float(raw_input(hash_task[key]+' = ').split()[0])
             task[hash_task[key]] = val
           else:
             val = int(raw_input(hash_task[key]+' = ').split()[0])
             task[hash_task[key]] = val
   return tasks

def change_in_all_tasks(tasks, change_dict):
  __doc__='''
  (tasks, change_dict) - tasks are [ {task1}, {task2}, ... }
  change_dict - is smth like {'flux': 1.5} - the change to aply to all tasks
  '''
  if any((key not in hash_task.values()) for key in change_dict):
    print "Wrong change_dict"
    return -1
  else:
    for task in tasks:
      for change_key, value in change_dict.items():
        task[change_key]=value

def lets_work():
  tasks = make_tasks()
  res_names = do_tasks(tasks)
  #gather.pdfplot(gather.plotres( (gather.gath(res_names), plotX, plotY) ))
  print "Done"

"""
def do_task():
   N = int(raw_input("Number of successions"))#
   task = [[-1,-1, -1,-1,-1, -1,-1,-1,-1] for i in range(N)]

   #print "Measurement object - O , length of one succession - L,"
   #print "therm steps - T, a measurement length - M, corr steps - C,"
   #print "Nspace - Ns, Ntime - Nt, Beta - B, flux - F"
   print hash_task
   lst = raw_input("Choose parameters to be the same in all successions")

   if lst.count('0'):
      print "Reduction kernels: ", measurement_subjects
      input =raw_input(hash_task[0]).split()[0]
      for i in range(N): task[i][0] = input
   if lst.count("1"):
      input = int(raw_input(hash_task[1]))
      for i in range(N): task[i][1] = input

   if lst.count("2"):
      input = int(raw_input(hash_task[2]))
      for i in range(N): task[i][2] = input
   if lst.count("3"):
      input = int(raw_input(hash_task[3]))
      for i in range(N): task[i][3] = input
   if lst.count("4"):
      input = int(raw_input(hash_task[4]))
      for i in range(N): task[i][4] = input

   if lst.count("5"):
      input = int(raw_input(hash_task[5]))
      for i in range(N): task[i][5] = input
   if lst.count("6"):
      input = int(raw_input(hash_task[6]))
      for i in range(N): task[i][6] = input
   if lst.count("7"):
      input= float(raw_input(hash_task[7]))
      for i in range(N): task[i][7] = input
   if lst.count("8"):
      input = float(raw_input(hash_task[8]))
      for i in range(N): task[i][8] = input

   for i in range(N):
       for u in range(len(task[i])):
          if task[i][u]==-1:
             print "Succession - ", i
             task[i][u]=define_value(u)
   for t in task:
      measurement_succession(t[0],t[1],t[2],t[3],t[4],t[5],t[6],t[7],t[8])

def define_value(u):
   # u - integer
   if u==0:
      print "Reduction kernels: ", measurement_subjects
      return raw_input(hash_task[0]).split()[0]
   elif u==1:
      return int(raw_input(hash_task[1]))
   elif u==2:
      return int(raw_input(hash_task[2]))
   elif u==3:
      return int(raw_input(hash_task[3]))
   elif u==4:
      return int(raw_input(hash_task[4]))
   elif u==5:
      return int(raw_input(hash_task[5]))
   elif u==6:
      return int(raw_input(hash_task[6]))
   elif u==7:
      return float(raw_input(hash_task[7]))
   elif u==8:
      return float(raw_input(hash_task[8]))
"""

if __name__=="main":
  lets_work()
