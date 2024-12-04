Multi-Agent Distributed System - Fondation - Agents
___

# Goals

In this practice will experience a way to build a multi-agent system.
The goal is to create a small caculatrice that can have different outpout based about arguments.

We will split the process in three steps:
- Managing the arguments (Parsing and validation)
- Compute the equation
- Format the result

**Advise:**<br/>
*Technology:*  Python<br/>
*IDE:* VS Code<br/>

## Entry

Create a console application that will that two arguments from the command line : "-e" "EQUATION" "-o" "RESULT|DETAIL".
This first agent will check if the argument are present and valid.

If all is valid, the second agent must be called with two argments "Equation" and Output format.

## Compute

Create a console application that will received in command line argument the equation and the output format : "EQUATION" "RESULT|DETAIL".
This agent will compute the equation and called the last agent toformat the result.

## Format

Create a console application that will received in command line argument the equation, the result and the output format  : "EQUATION" "RESULT" "RESULT|DETAIL".
This agent will display in the console the result formated.

input: '5+6' '11' RESULT

|Output Format|Expected Output|
|--|--|
|RESULT|11|
|DETAIL|5+6 = 11|

# Conclusion

In this practice we show the agent principe. Isolated process with a single job to do managing it's output is only based on the input they received.

|Pros|Cons|
|--|--|
|Each application is isolate and can be maintains individualy.||
||Each application need to know the next application to call for a process.<br/> Due to this constraint it's hard to reuse a agents in a different process|

