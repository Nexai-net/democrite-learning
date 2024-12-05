Multi-Agent Distributed System - Fondation - Agents
___

# Goals

In this exercise, we'll explore one way to construct a multi-agent system. 
Our objective is to develop a simple calculator capable of producing diverse outputs depending on its input arguments.

We'll break down the process into three main steps (Three agents):

1) Argument Handling:
    - Parsing input arguments to extract relevant information.
    - Validating input to ensure correctness and consistency.

2) Equation Computation:
    - Performing the necessary calculations to determine the result.

3) Result Formatting:
    - Structuring the computed result into a desired output format.
    - Presenting the result in a clear and understandable manner.

**Advise:**<br/>
*Technology:*  Python<br/>
*IDE:* VS Code<br/>

## Entry

Create a console application that accepts two command-line arguments:

- **-e EQUATION**: Specifies the mathematical equation to be evaluated.
- **-o RESULT|DETAIL**: Determines the desired output format (either a simple result or a detailed breakdown of the calculation).

The first agent’s responsibilities are:

**Argument Validation:**

1) Verify the presence of both -e and -o arguments.<br/>

2) Ensure the EQUATION is a valid mathematical expression.<br/>
*regex: ^(\\s*([0-9]+([.,]{1}[0-9]+)?)|(\\s*[+*\\/-]{1})|(\\s*[\\(\\)]{1}))*$*

3) Check if the RESULT or DETAIL output format is specified correctly.<br/>

**Agent Invocation:**

If all arguments are valid, invoke the second agent with the following arguments: 'EQUATION' 'Output Format'
The second agent will then process the equation and generate the appropriate output based on the specified format.

## Compute

Create a console application that accepts two command-line arguments: 'EQUATION' 'Output Format'

**Equation Parsing and Calculation:**

1) Parse the input EQUATION to identify its components (numbers, operators, etc.).
2) Perform the necessary calculations to determine the result.

**Result Formatting:**

Invoke the third agent, passing it the calculated result and the specified output format: 'EQUATION' 'RESULT' 'Output Format'
The third agent will then format the result according to the desired style (e.g., simple numerical result or a detailed breakdown).

## Format

Create a console application that accepts three command-line arguments:

- **EQUATION**: The mathematical expression that was evaluated.
- **RESULT**: The calculated result of the equation.
- **Output Format**: Determines the desired output format (either "RESULT" or "DETAIL").

**This agent’s responsibility is to format and Display the Result:**

Based on the specified Output Format, format the RESULT in the desired style.
Display the formatted result to the console.
For example:

- If the Output Format is "RESULT":
    - Display only the numerical result.

- If the Output Format is "DETAIL":
    - Display the equation and the result

Example:

input: '5+6' '11' RESULT

|Output Format|Expected Output|
|--|--|
|RESULT|11|
|DETAIL|5+6 = 11|

# Conclusion

This exercise demonstrates the core principles of agent-based systems. We explore how to create isolated processes, each with a specific task, that operate independently based solely on their input.

|Pros|Cons|
|--|--|
|Each application operates independently and can be maintained separately.||
||Each application requires knowledge of the subsequent application in the process,<br/> making it difficult to reuse them in different contexts.|
||Command-line arguments are limited to string formats, necessitating the transmission of all information as strings.|
||Launching a new program instance for each task incurs performance overhead.|
||Although agents operate independently, there are situations where access to shared knowledge is advantageous.<br/> However, inter-process communication to access this shared knowledge can introduce performance overhead.|

