Multi-Agent Distributed System - Fondation
___

In Multi-Agent Distributed System they are two main architecture pattern combine to extract the best of each.

# Agents

In general what we call an agent in an autonomus entity in a system capable of execution a task or take decision.

The concept of an agent can be traced back to Hewitt's Actor Model (Hewitt, 1977) - "A self-contained, interactive and concurrently-executing object, possessing internal state and communication capability. [Wiki](https://en.wikipedia.org/wiki/Software_agent#:~:text=The%20concept%20of%20an%20agent%20can%20be%20traced%20back%20to,%2DAgent%20Systems%20(MAS).)

The goal is to inspire for an individue in a collective place where task must be done to acheived a goal.

A example often used is the ant. Each ant have a specific job to do to allow the collonie to survive and evolve.
Each ant beahvior is motivated by his goal but take account of the other to adapt his strategy and behavior in consequence.

In computer system, a multi-agents system (MAS) is a complet way of thinking, architecture and devellopte.
If we take object oriented paradigm as referential is like create classes that will not have strong link to other classes but will be able to communicate to acheived a common goal.

Through time many debat occured on the scope cover by an agent, what he could do, what information should he keep ...

We can identify three category of agent.

- **Worker**, this kind is comparable to a function. It has no historical context and do his job only on the input information. This can be illustrate like a member in a working chain that take the piece do his modification and send back the result.
- **Engineer**, this kind will have a state (historical or not) that main influ on the work done.
- **Orchestrator**, this kind will often have a state (historical or not) but his goal is to decide how to process, Often this agent will send the work to **worker** 
or **Engineer** to to the job.
- **Historian**, this kind of agent goal is to manage the memory of the system. Often used in IA to consolidate, correct or deteriorate a model 

In each solution an agent could have one or multiple roles, limitations or right. An agent could be also compose of multiple sub-agent like a team leader.
Those parameters MUST be defined by the devellopment team in function of the projet. Otherwise each develloper will have his own view of those parameter and will create chaos.

To work together is like in a team rules, limitation and way of thinking need to be common, for computer science agent is the same.

***Idee practice***
- Create 3 small programs (each program call the other with the needed input, python or wanted language)
    - 1) Choose an matematical operation and ouput format
    - 2) Execute operation
    - 3) Format the result

# Distributed Systems

Nowaday (2024) more than 5.35 billions people have access to internet [(source)](https://datareportal.com/reports/digital-2024-deep-dive-the-state-of-internet-adoption). Most on the time to access similare web site, service, Amazon, Facebook, Instagram, ... The workload for those company server is huge. To solve this issue we start to deploy the software solution on multiple server and **distribute** the request those using load balancing system like reverse proxy, gateway ...

A distributed system is multiple program that will communicate to each other to distribute the work to multiple program, servers to process more with an equivalent capacity.

Imagine a line at the market where one cashier process one person at time, a kind of distributed system will be to have one line but multiple cashier and more with multiple market.

Multiple the elements of a distrubuted system are often group. This group is called a **Cluster**.

The main difficulty in a cluster is to choose how to distribute the work. Multiple classic strategy exist:
- **Random** : This strategy will choose randomly the executor from a selected list. This one will tend due to stastique law to distribute the same amount of jobs to all the executors but doesn't take in consideration the executor availability.

- **Pulling** : This strategy will create a queue of process to do and each executor will come a take a job to execute if available. This one distribute the work automatily based on each executor capacity and workload. But required a constant pull from each executor to see if theire is job to do. Often the pull request is timed (every 300 ms) and could overload the dispatcher or create a delay between the incomming request and his execution.

- **Sharding** : This strategy will use a information in the request to redirect if to a specific configured cluster. This one is mainly used on international solution. Example if the request came from france then we will redirect to french executor. This solution required pre-configured sharding rules and are limited by them. For example a rule based on the location only will overload the french executor based on day hours when executor on the otherside of the planet are free.

- **Negociation** : This strategy will let the executor who execute the request. In previous strategy describe (Random, pulling, ...) we have a dispatcher who choose the executor and this one execute without protest. In this strategy we don't need to differenciate the dispatcher and the executor. Each executor will meet and discuce to each other and decide who process the request. And then it open a new algorithm choose how to preform an efficient negociation in computer science (cf. [distribution](#distribution), [Master Slave](https://www.geeksforgeeks.org/master-slave-architecture/), [Paxos](https://en.wikipedia.org/wiki/Paxos_(computer_science))). This strategy combine efficient, direct treatment, correct distribution based on capacity BUT introduce a latency due to executor choice algorithm.

***Idee practice***
- Create 3 small programs (each program call the other with the needed input, python or wanted language)
    - 1) Entry api point (Dispatcher)
    - 2) Program A
    - 3) Program B

# Multi-Agents Distributed Systems (MADS)

A multi-agents distributed systems combine agent paradigm in a distributed configuration. The goal is to create a system resilient, performant and easy to maintain and evolve.

A query is split in small ordered interconnected tasks that will be distributed through a cluster of servers where each task will be managed by a dedicated agent choose with different strategy in function of the context and the nature od the task.

It's like requested to build a new house to the system. This will split in small tasks, command material, plan work teams, command funrniture, create bills, ...
Each teams will works one after the other or in parallel to build the house. 

# Orleans
## Distribution
# Conclusion

If we look at the programming maner, best practice, architecture ... through time we will remark that most of the multi-agent paratigm tend to be used more and more.

At start most of the program was small script communicating by files with a structure format and drop at a specific place.

Then to reduce the latency due to filesystem read/write and due to memory (RAM) gross it was more efficient to have one program doing all the task that multiple script did before. This programming manner is more common to the monolitic way. The big issue with this programming way is the maintenance and evolution. Even if achitecture (ognion, Domain Driven Devellopment, SOLID) tend to isolate by layer devellopment part to more easily do evolution or maintenance all remain in one program. With the popularisation of the computer system and more focus the web usage a probleme of workload appear. The quick solution was to duplicate the monolitic program and redirect queries to distribute the charge. But it's a simple solution that doesn't match requirement of lots of program and structure. 

Then was the time of the queuing and function, those architure stack all the jobs to do in queue (RabbitMQ, AzureQueue, ...) and muliple small program pick job went they are available. This solution tend to easy the treament flow, reducing the workload due to a better **distribution**. Look like we have apply the old way (multi small script) with newer techology : network communication. Those system are hard to maintained due to communication complexity, message treatement order, ... but are very efficient in evolution and heavy load treatment.

what next ?

Looking at history and cyclng way the computer system work we will need to easy the maintenace by creating more centralized controlable system.

This is where microsft orleans and democrite kick in.
