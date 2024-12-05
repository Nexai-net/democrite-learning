Multi-Agent Distributed System - Fondation
___

# Agents

An agent, in the context of computer science, is an autonomous entity capable of performing tasks or making decisions within a system. This concept, rooted in Hewitt's Actor Model (1977), "A self-contained, interactive and concurrently-executing object, possessing internal state and communication capability. [Wiki](https://en.wikipedia.org/wiki/Software_agent#:~:text=The%20concept%20of%20an%20agent%20can%20be%20traced%20back%20to,%2DAgent%20Systems%20(MAS).)

A common analogy is the ant colony. Individual ants, each with specific roles, work collectively towards the colony's survival and evolution. Their behaviors, while motivated by personal goals, are influenced by the actions of others, leading to adaptive strategies.

Multi-Agent Systems (MAS)
In computer systems, MAS provides a comprehensive approach to system design and development. Similar to object-oriented programming, MAS involves creating entities (agents) that interact to achieve common goals. However, unlike traditional objects, agents are less tightly coupled, enabling more flexible and dynamic systems.

Agent Categorization and Roles
Agents can be categorized based on their capabilities and roles:

- **Worker Agents**: These agents execute tasks based on input data, akin to static functions. They have no internal state and operate in a straightforward, linear manner.

- **Engineer Agents**: These agents maintain an internal state, which influences their decision-making and actions. They can learn from past experiences and adapt their behavior accordingly.

- **Orchestrator Agents**: These agents oversee the overall system, making decisions about task allocation and resource management. They often have an internal state to track progress and adjust strategies.

- **Historian Agents**: These agents are responsible for storing and managing system data. They can be used to analyze past behavior, identify trends, and improve future performance.

To ensure effective collaboration, agents must be designed with clear roles, limitations, and communication protocols. This requires careful planning and coordination among development teams. A well-defined agent architecture can prevent inconsistencies and chaos.

By understanding the principles of agent-based systems, developers can create more intelligent, adaptable, and robust software solutions.

## **Practice**
To demonstrate the concept of agents in practice, you can refer to the exercise outlined in our documentation ( [/docs/1-fondations-practice-agents.md](/docs/1-fondations-practice-agents.md) ). This exercise showcases the implementation of a simple multi-agent system, highlighting both the advantages and limitations of this approach.


# Distributed Systems
**Distributed Systems: A Scalable Approach to Handling Increased Workload**

In today's digital age, a significant portion of the global population (over 5.35 billion people in 2024  [(source)](https://datareportal.com/reports/digital-2024-deep-dive-the-state-of-internet-adoption)) accesses the internet daily, frequently interacting with large-scale services like Amazon, Facebook, and Instagram. To accommodate this massive user base, these companies employ distributed systems to distribute the workload across multiple servers.

A distributed system comprises multiple programs that communicate and collaborate to process tasks. This approach enhances scalability, reliability, and fault tolerance. Consider a physical store: instead of a single checkout line, multiple cashiers and perhaps multiple stores can handle customer traffic more efficiently.

Key Concepts in Distributed Systems:

Cluster: A group of interconnected computers working together as a single system.
Load Balancing: The process of distributing incoming network traffic across multiple servers to optimize resource utilization.
Common Strategies for Work Distribution:

- **Random**: Tasks are assigned randomly to available servers. While this approach can distribute the load evenly over time, it doesn't consider individual server capacities or current workloads.

- **Pulling**: Servers actively request tasks from a central queue. This strategy adapts to varying server loads, ensuring efficient resource utilization. However, it can introduce overhead due to frequent polling.

- **Sharding**: Tasks are assigned based on specific criteria, such as geographic location or data partitioning. This approach can improve performance and reduce latency, but it requires careful configuration and can limit flexibility.

- **Negotiation**: Servers negotiate among themselves to determine the best allocation of tasks. This strategy offers a high degree of flexibility and adaptability, but it can introduce complexity and overhead. (cf. [Master Slave](https://www.geeksforgeeks.org/master-slave-architecture/), [Paxos](https://en.wikipedia.org/wiki/Paxos_(computer_science))

**Challenges and Considerations**
While distributed systems offer numerous benefits, they also present challenges:

- **Complexity**: Managing and coordinating multiple components can be intricate.
- **Consistency**: Ensuring data consistency across multiple servers can be difficult.
- **Fault Tolerance**: Designing systems to gracefully handle failures is crucial.
- **Network Latency**: Communication delays between servers can impact performance.

By understanding the fundamental concepts and strategies of distributed systems, developers can design and implement scalable, reliable, and efficient applications to meet the demands of modern computing.

# Multi-Agents Distributed Systems (MADS)

A multi-agent distributed system (MADS) leverages the agent paradigm within a distributed architecture to create resilient, high-performance, and maintainable systems.

In a MADS, a complex task is broken down into smaller, interconnected subtasks. These subtasks are then distributed across a cluster of servers, each managed by a dedicated agent. The selection of agents for specific tasks can be based on various factors, such as task complexity, resource requirements, and real-time system conditions.

Analogy: Building a House
Imagine building a house as a complex task. A MADS approach would involve breaking down this task into smaller, manageable subtasks:

- **Planning**: Architects design the blueprint, engineers calculate structural loads, and project managers create timelines.
- **Material Procurement**: Procurement teams source building materials, negotiate contracts, and coordinate deliveries.
- **Construction**: Construction crews build the foundation, erect walls, install plumbing, and complete electrical wiring.
- **Finishing**: Interior designers select furniture and decor, painters apply finishing touches, and landscaping crews beautify the exterior.

Each of these subtasks can be assigned to specific agents, which may collaborate and coordinate their efforts to achieve the overall goal of building the house.

By employing a MADS approach, organizations can improve system performance, scalability, and fault tolerance. Additionally, the modular nature of MADS facilitates easier maintenance and upgrades.

# Microsoft Orleans

[Microsoft orleans](https://learn.microsoft.com/en-us/dotnet/orleans/overview) is a powerful .NET framework that simplifies the development of distributed systems. It allows developers to create distributed applications using familiar object-oriented programming techniques, similar to building monolithic applications.

**Key Concepts in Orleans:**

- **Interface**: An interface defines a contract, specifying the methods and properties that a class must implement. Interfaces promote loose coupling between components, making systems more modular and maintainable.

- **Grain**: A grain is the fundamental building block of an Orleans application. It represents a self-contained, stateful or stateless entity that can be activated and deactivated as needed. Grains communicate with each other through asynchronous message passing.

- **Silo**: A silo is a cluster node that hosts grains and manages their execution. Silos can be distributed across multiple machines to improve scalability and fault tolerance.

By leveraging these core concepts, Orleans enables developers to build highly scalable, fault-tolerant, and distributed applications with ease.

<img src="https://learn.microsoft.com/en-us/dotnet/orleans/media/cluster-silo-grain-relationship.svg" style="max-width:1024;text-align:center">

Orleans allows developers to build distributed systems using familiar object-oriented programming techniques. Instead of directly injecting services via an *IServiceProvider*, developers access grains through an *IGrainFactory*. This approach enables seamless communication between grains, regardless of their physical location within the cluster.

By treating grains as if they were local objects, developers can focus on writing business logic without worrying about the underlying distributed infrastructure. This simplifies development and promotes a more modular and scalable architecture.

## Grain

A grain is a unique entity within an Orleans cluster, identified by a GrainId. This ID comprises information about the grain's type, primary key, and silo location. This unique identifier allows the system to route requests to the correct grain instance, ensuring data consistency and efficient processing.

<img src="https://learn.microsoft.com/en-us/dotnet/orleans/media/grain-formulation.svg" style="max-width:512;text-align:center">

Orleans employs a virtual actor model, where grains are activated and deactivated as needed to optimize resource utilization. When a grain is inactive, its state is persisted to storage. Upon reactivation, the grain's state is restored, allowing it to continue processing requests seamlessly.

To create a grain, developers define an interface that specifies the public methods (NO properties) of the grain. The grain implementation class then implements this interface. Grain interfaces must adhere to specific constraints, such as having all methods return Task or ValueTask to support asynchronous operations.

**Example**: User Activity Grain
Consider a grain that tracks user activity. When a user logs in, an instance of the UserActivity grain is activated. This grain records user actions, such as page views, button clicks, and form submissions. When the user logs out or the session expires, the grain is deactivated.

Key Benefits of Orleans:

- **Scalability**: Orleans can easily scale to handle increasing workloads by adding more silos to the cluster.
- **Fault Tolerance**: The framework automatically handles failures and restarts failed grains.
- **Simplified Development**: Orleans provides a high-level abstraction, making it easier to build distributed systems.
- **Performance**: By leveraging asynchronous programming and efficient state management, Orleans can deliver high performance.

By understanding the core concepts of grains, silos, and the Orleans framework, developers can build robust, scalable, and fault-tolerant distributed applications.

## Silo

A silo is a process that hosts and executes grains. Multiple silos form a cluster, enabling distributed computing and high availability.

Cluster Membership and Synchronization
To form a cluster, silos must discover and communicate with each other. Orleans typically uses a database as a membership service. When a silo starts, it registers itself in the database and discovers other silos in the cluster. This information is used to establish connections and synchronize the cluster's configuration and capacity.

Key Benefits of the Silo-Based Architecture:

- **Scalability**: By adding more silos, the cluster can handle increasing workloads.
- **Fault Tolerance**: If a silo fails, other silos can take over its workload.
- **Distributed State Management**: Orleans handles the complexities of distributed state management, ensuring data consistency and reliability.
- **Simplified Development**: Developers can focus on writing grain logic without worrying about the underlying infrastructure.

By understanding the role of silos in an Orleans cluster, developers can effectively design and implement scalable and resilient distributed applications.

for more advanced information and tutorial you have [Microsoft documentation](https://learn.microsoft.com/en/dotnet/orleans/overview) and/or the project on [Github](https://github.com/dotnet/orleans).

***Idee practice***
- Create a small chat system with room name and multiple client
    - 1) One client
    - 2) One Silo server

# Conclusion
**The Evolution of Distributed Systems**

The evolution of software architecture has mirrored the increasing complexity and scale of computing systems. Early programming paradigms often involved simple scripts that communicated via file-based mechanisms. As systems grew, monolithic architectures emerged, consolidating multiple functionalities into a single program. While this approach streamlined development, it introduced challenges in maintenance and scalability.

To address these limitations, distributed systems gained prominence. These systems distribute tasks across multiple nodes, enhancing performance, reliability, and scalability. Early distributed systems relied on message queues, such as RabbitMQ or Azure Queue Storage, to coordinate tasks. However, managing complex communication patterns and ensuring data consistency became increasingly difficult.

Modern Distributed Systems: **A Paradigm Shift**

Modern distributed systems, exemplified by frameworks like **Microsoft Orleans** and **Democrite**, offer a more sophisticated approach. These frameworks facilitate the development of distributed applications by providing a high-level abstraction over complex infrastructure concerns.

Key advantages of these frameworks include:

- **Simplified Development**: They allow developers to write distributed applications using familiar programming models, reducing the learning curve.
- **Improved Scalability**: They enable seamless scaling of applications to handle increasing workloads.
- **Enhanced Reliability**: They provide built-in mechanisms for fault tolerance and automatic recovery.
- **Data Consistency**: They ensure data consistency across distributed nodes. Bring the request to the data an not the opposite.

By abstracting away the complexities of distributed systems, these frameworks empower developers to focus on core business logic, leading to faster development cycles and more reliable applications. As technology continues to advance, distributed systems will play an increasingly critical role in shaping the future of software development.