#from kernels_and_cover.main_new import objects
import kernels_and_cover.main_new as GD
from pickle import dump
import time
import numpy
import gather

#task = [N meas=[props & surq]]

# task == [properties]
#   properties == {'meas_object', 'object_params':prop, 'number_of_measurements', 'meas_params': parameters}
#      parameters == {'corr_steps', 'therm_steps', 'meas_len'}
#      prop == {'Ns', 'Nt', 'beta', 'flux', 'kappa'}
# rewrite make_task

#def measurement(work_space, measure_subject, how_many_times, corr_step):
def measurement(work_space, parameters,
                #where_files="/home/alex/magn_mass_meas/results/",
                where_files="/home/alex/magn_mass_meas/test/",
                #where_pickles="/home/alex/magn_mass_meas/results/pickled_numpy_arrays/"):
                where_pickles="/home/alex/magn_mass_meas/test/"):
   '''measurement(work_space, parameters, where_files=, where_pickles=):
   work_space -- object for mesurements
   parameters -- params for the measurement
   where_files -- directory where to store results in SciLab format
   where_pickles -- directory for pickles
   '''
   how_many_times=parameters['meas_len']

   for i in xrange(parameters['therm_steps']):
     work_space.Update()

   T=time.time()
   local_time = time.asctime( time.localtime(T) )

   namee='.'
   #namee+=str(work_space.prop)
   namee+="Ns "+str(work_space.prop['Ns'])+" Nt "+str(work_space.prop['Nt']) \
         +" Beta "+str(work_space.prop['beta'])+" Flux "+str(work_space.prop['flux']) \
         +" Kappa "+str(work_space.prop['kappa'])
   namee+=" _ "+"HMT "+str(how_many_times)+" corr "+str(parameters['corr_steps'])+" _ "+local_time +"_100c"

   numpy_name=where_pickles + ".pickl."+str(T)

   f=open(where_files+namee, 'ab')

   data = numpy.empty((how_many_times, work_space.reduct_format))

   for i in xrange(how_many_times):
       for u in xrange(parameters['corr_steps']):
           work_space.Update()
       # result.append(eval("work_space."+measure_subject))
       data[i]=work_space.reduction()

   f.write("A=[\n")
   for i in data:
      f.write(str(i))
   f.write("\n]\n")
   print f.name
   f.close()

   metadata = {"workSpace":work_space.prop, "measurement": parameters}
   res= {'metadata':metadata, 'data':data}
   picklefile=open(numpy_name,'wb')
   dump(res, picklefile)
   print picklefile.name
   picklefile.close()

   print local_time
   print time.asctime( time.localtime(time.time()) )

def measurement_succession( properties ):
  '''measurement_succession( properties ):
  properties == {'meas_object', 'object_params':prop, 'number_of_measurements', 'meas_params': parameters}
     parameters == {'corr_steps', 'therm_steps', 'meas_len'}
     prop == {'Ns', 'Nt', 'beta', 'flux', 'kappa'}
  'meas_object': a class from objects
  '''
#!!!!!!!!!!!!!!!
  work_space = properties['meas_object'](properties['object_params'])
#!!!!!!!!!!!!!!!
  measurement(work_space, properties['meas_params'])
  for i in xrange(properties['number_of_measurements']-1):
     work_space.renew_seed_links()
     measurement(work_space, properties['meas_params'])
  del work_space
'''
  success_file_names = []
  success_file_names.append( measurement(work_space, measurement_parameters) )
  return success_file_names
'''

def do_task(task):
   """
   tasks = [ properties ]
   properties = {...}
   """
   #results_names=[]
   for properties in task:
      #results_names.append(measurement_succession(properties))
      measurement_succession(properties)
   #return results_names

#   properties == {'meas_object', 'object_params':prop, 'number_of_measurements', 'meas_params': parameters}
def make_properties():
   '''make_properties():
   '''
   try:
      print GD.objects.keys()
      obj = GD.objects[raw_input('Choose what to measure ').split()[0]]

      print "fill object's parameters"
      prop = {}
      for p in obj.prop.keys():
         prop[p] = int(raw_input(p + ' = ').split()[0])

      numb_of_meass = int(raw_input('Number of measurements = ').split()[0])

      meas_params = {'corr_steps':0, 'therm_steps':0, 'meas_len':0}
      print "Input measurements' parameters"
      for p in meas_params.keys():
         meas_params[p] = int(raw_input(p + ' = ').split()[0])

   except KeyError:
      raise KeyError( "wrong object" )
      return None
   except ValueError:
      raise ValueError( "wrong input" )
      return None

   return {'meas_object':obj, 'object_params':prop, 'number_of_measurements': numb_of_meass, 'meas_params': meas_params}

basic_ns = [4, 6, 8, 10, 12, 16, 20, 24, 28, 32]
basic_ml = [400, 300, 200, 128, 64, 32, 16, 8, 4, 2]

def expand_prop(basic_prop, expansion_dict = {'Ns':basic_ns, 'meas_len':basic_ml}):
   '''expand_prop(basic_prop, expansion_dict = {'Ns':basic_ns, 'meas_len':basic_ml}):
   basic_ns = [4, 6, 8, 10, 12, 16, 20, 24, 28, 32]
   basic_ml = [400, 300, 200, 128, 64, 32, 16, 8, 4, 2]
   '''
   if any((key not in basic_prop) or (key not in basic_prop['object_params']) or (key not in basic_prop['meas_params']) for key in expansion_dict):
     print "wrong expansion dictionary"
     return None
   from copy import deepcopy
   task = [basic_prop]

   for i in xrange(min([len(value) for value in expansion_dict.values()])):
      new_prop = deepcopy(basic_prop)
      for key in expansion_dict:
         if key in new_prop: new_prop[key] = expansion_dict[key][i]
         if key in new_prop['object_params']: new_prop[key]['object_params'] = expansion_dict[key][i]
         if key in new_prop['meas_params']: new_prop[key]['meas_params'] = expansion_dict[key][i]
      task.append(new_prop)
   return task
