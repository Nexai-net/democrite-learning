# Reference Id

A reference is an URI format like follow:

|Schema|User|Host|Path|Fragment (Optional)|
|--|--|--|--|--|
|refid://|KIND|namespace|simple-name-identifier|extra-info|

- **KIND** : Is a formated string corresponding to an enum value to define the kind of target, sequence, artifact, method, grain, ...
- **namespace** : This part could be configured to group element from a same **bag** or **company** this prevent name conflict when using mulitple bags for example, if not configured a default namespace is applied
- **simple-name-identifier (SNI)**: Is a simple formated name used for develloper/user to identify the target (Definition) more properly