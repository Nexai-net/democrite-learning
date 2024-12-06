import sys
import os 
import subprocess

dir_path = os.path.dirname(os.path.realpath(__file__))

equation = sys.argv[1]
remainArgs = sys.argv[2:]

result = eval(equation)

args = [ "python", dir_path + "/FormatAgent.py", equation, str(result)] + remainArgs
subprocess.run(args)