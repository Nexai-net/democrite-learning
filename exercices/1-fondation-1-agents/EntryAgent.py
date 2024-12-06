import sys
import re
import subprocess
import os 

dir_path = os.path.dirname(os.path.realpath(__file__))
count = len(sys.argv);

if (count == 0):
    print("Provide the equation and output format")
    print("  -e \"Equation\" ")
    print("  -o \"RESULT|DETAIL\" ")
    exit(-1)
    
equation = None
output = "RESULT"

for indx in range(count):
    
    current = sys.argv[indx]
    if (current.casefold() == "-e"):
        indx = indx + 1
        
        if (indx >= count):
            print("Equation missing")
            exit(-1)
            
        equation = sys.argv[indx]
        continue
    
    if (current.casefold() == "-o"):
        indx = indx + 1
        
        if (indx >= count):
            print("Output type missing")
            exit(-1)
            
        output = sys.argv[indx]
        continue
        
        
if (equation is None):
    print("Provide the equation and output format")
    print("  -e \"Equation\" ")
    print("  -o \"RESULT|DETAIL\" ")
    exit(-1)

equation_regex = re.compile("^(\\s*([0-9]+([.,]{1}[0-9]+)?)|(\\s*[+*\\/-]{1})|(\\s*[\\(\\)]{1}))*$")

if (equation_regex.search(equation) is None):
    print("Invalid equation : " + equation);
    exit(-1)
    
if (output.casefold() != "detail".casefold() and output.casefold() != "RESULT".casefold()):
    print("Only DETAIL or RESULT as output type as accepted")
    exit(-1)
    
subprocess.run([ "python", dir_path + "/ComputeAgent.py", equation , output])