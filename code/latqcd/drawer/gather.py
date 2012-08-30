# this python script goes through given files gathering data in pickled results of calculations
# pickled files have special names:
# .pickl.

import sys
import os
import pickle
import numpy
plaquette_map={0:'S', 1:'pl12', 2:'pl13', 3:'pl14', 4:'pl23', 5:'pl24', 6:'pl34'}
from matplotlib import pyplot as plt

# one needs points for the plot - X, Y coordinates
# X - Ns, Y - mean of S or plaquettes
# also one should save Nt, flux and beta.
# All these qantities for a measurement lets save in a dictionary
# final result is a list of such dictionaries
# Further the algorithm is even simplier: in a measurement dictionary one saves the whole dictionary "metadata" from the pickled file, and and adds 7 3-key dictionaries for "data": each dict is for "S, PL12, PL13, ...", so it is 7 'mean' values, 7 'std' deviations and "errors" calculated by jacknife
def gath(filenames):
  gathered_result = []

  for filename in filenames:
    if ".pickl." in filename:
      f=open(filename, 'rb')
      calc_result = pickle.load(f)
      f.close()
      if type(calc_result)!=dict:
  	print "Wrong pickled object"
  	continue
      data = calc_result['data']
      # SOL` ----------------------------
      means = numpy.mean(data, 0)
      std = numpy.std(data, 0)
      gathered_data={}
      # no jacknife at the moment
      for i, val in enumerate(zip(means,std)):
        gathered_data[plaquette_map[i]]={'mean':val[0], 'std':val[1]}
      gathered_result.append({'metadata': calc_result['metadata'], 'data': gathered_data})
    else: print "No '.pickl.' in the name"
  
  i=1
  outFileName = '/home/alex/magn_mass_meas/results/pickled_numpy_arrays/final_ress/pickle' + str(i)
  while os.path.exists(outFileName):
    i+=1
    outFileName = outFileName[:-1] + str(i)
  f=open(outFileName, 'wb')
  pickle.dump(gathered_result, f)
  f.close()
  return gathered_result

"""
def plotres(all_result_dicts, X="Ns", Y="S mean", fixed_stuf={'beta':6, 'flux':1, 'Nt':2}, plot_style='go'):
  '''
  result_dicts -- list of embedded dictionaries:
  {'metadata', 'data'},
    ['metadata']=={'workSpace', 'measurement'}
      ['workSpace']=={'Ns', 'Nt', 'flux', 'beta'
      ['measurement']=={'meas_object', 'meas_len', 'corr_steps' ...}
    ['data']=={'S', 'pl12', 'pl13', 'pl14', 'pl23', 'pl24', 'pl34'}
      ['S']=={'mean', 'std'}
      ...
  Y -- keys for Y-axis, separated by a space: "S mean", "pl23 std"
    it referes to result_dict['data']['key0']['key1']
  X -- formula for X-axis using result_dict['metadata']['workSpace']:
    "Ns", "Ns * flux" ...
  fixed_stuf is what doesnot go to X axis: beta, flux and so on
  '''
  result_dicts = []
  for d in all_result_dicts:
    if all(d['metadata']['workSpace'][key]==fixed_stuf[key] for key in fixed_stuf): result_dicts.append(d)
  plotingXY = numpy.empty((2,len(result_dicts)))
  Y_keys = Y.split()

  for i, d in enumerate(result_dicts):
   #if all(d['metadata']['workSpace'][key]==fixed_stuf[key] for key in fixed_stuf):
    X_formula = ''
    for word in X.split():
      if word in d['metadata']['workSpace'].keys():
         word = "d['metadata']['workSpace']" + "['" + word +"']"
      X_formula = X_formula + word
    plotingX = eval(X_formula)
    plotingY = d['data'][Y_keys[0]][Y_keys[1]]
    plotingXY[0][i] = plotingX
    plotingXY[1][i] = plotingY

  plt.plot(plotingXY[0], plotingXY[1], plot_style)
  title = ''
  for key in d['metadata']['workSpace'].keys():
    if key not in X: title = title + " " + key + " = " + str(d['metadata']['workSpace'][key])
  plt.title(title, position=(1,1.03), horizontalalignment='right')
  plt.xlabel(X)
  plt.ylabel(Y)
  #plt.savefig(outFileName)
  return plt
  #plt.close()

def pdfplot(figure):
  i=1
  outFileName = '/home/alex/magn_mass_meas/results/pickled_numpy_arrays/final_ress/res' + str(i)
  while os.path.exists(outFileName + '.pdf'):
    i+=1
    outFileName = outFileName[:-1] + str(i)
  outFileName += '.pdf'
  plt.savefig(outFileName)
  #plt.close()
"""

if __name__ == "__main__":
   file_names = sys.argv[1:]
   gath(file_names)
   #figure = plotres(gath(file_names))
   #pdfplot(figure)
   #figure.close()
