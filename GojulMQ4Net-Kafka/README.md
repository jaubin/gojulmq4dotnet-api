# GojulMQ4Net-Kafka
GojulMQ4Net-Kafka is the Kafka implementation of GojulMQ4Net-Api. In this document
we deal with some common issues you may encounter and how to solve them.

## Note to RHEL / CentOS users using Kafka
On RHEL / CentOS you may run into the following error when running the program :
```
Unhandled Exception: System.DllNotFoundException: Failed to load the librdkafka native library.
```

To solve this, you must install packages librdkafka and librdkafka-devel using command :
```
sudo yum install librdkafka librdkafka-devel
```

## Connection closed issue on consumers
A widely known issue with Kafka .Net and Python consumer is the connection closed issue, which appears
from time to time. This is a false positive, but it can be really annoying. In order to solve it, 
set the following configuration setting :
```
log.connection.close=false
```
