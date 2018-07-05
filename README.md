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

## Note to RHEL / CentOS users using Kafka
On RHEL / CentOS you may run into the following error when running the program :
```
Unhandled Exception: System.DllNotFoundException: Failed to load the librdkafka native library.
```

To solve this, you must install packages librdkafka and librdkafka-devel using command :
```
sudo yum install librdkafka librdkafka-devel
```
