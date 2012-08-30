from pickle import load
import sys
import tasker

ar = sys.argv[1:]

if ar:
  if len(ar)<2:
    t = load(file("standart_tasks", 'rb'))
  else:
    t = load(file(ar[1], 'rb'))

  flux = float(ar[0])

  tasker.change_in_all_tasks(t, {'flux': flux})
  #print flux
  #for i in t: print i
  tasker.do_tasks(t)
else:
 print "python fluxtasks.py [file] flux -- usage"
