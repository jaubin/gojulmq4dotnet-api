# gojulmq4dotnet-api
gojulmq4dotnet-api provides an abstraction over message queues so that anyone can be abstracted
from the MQ actual implementation. The goal is to make it possible to abstract yourself
from a specific MQ implementation and have your MQ as an implementation detail.

## Usage examples
For each module there's a corresponding test program, in the <module>-test subproject. We
sadly must do this instead of using proper unit tests because these test programs need a
MQ broker up and running prior to being run. 

## Build with dotnet core on Linux
In order to build this project you need dotnet core 2. The project compiles by running :
```
./build.sh
``` 

## Additional implementation specific remarks
In each solution directory we've created a README file which contains the remarks dedicated
to that implementation.
