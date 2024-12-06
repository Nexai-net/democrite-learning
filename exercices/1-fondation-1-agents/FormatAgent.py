import sys

equation = sys.argv[1]
result = sys.argv[2]
type = sys.argv[3]

if (type.casefold() == "DETAIL".casefold()):
    print(equation + " = " + result)
elif (type.casefold() == "RESULT".casefold()):
    print(result)