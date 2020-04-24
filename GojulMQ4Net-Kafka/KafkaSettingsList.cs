using Conditions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Org.Gojul.GojulMQ4Net.Kafka
{
    /// <summary>
    /// Due to the recent evolutions with Kafka client and .Net we have
    /// to list all the settings allowed by librdkafka.
    /// </summary>
    internal static class KafkaSettingsList
    {
        // The settings list comes from librdkafka configuration.md file.

        private static readonly ICollection<string> AllowedParams = new ReadOnlyCollection<string>(new List<string>(new[]
        {
            "acks", "api.version.fallback.ms", "api.version.request", "api.version.request.timeout.ms", "auto.commit.enable", "auto.commit.interval.ms", "auto.offset.reset", "background_event_cb", "batch.num.messages", "bootstrap.servers",
            "broker.address.family", "broker.address.ttl", "broker.version.fallback", "builtin.features", "check.crcs", "client.id", "closesocket_cb", "compression.codec", "compression.level", "compression.type", "connect_cb", "consume.callback.max.messages", "consume_cb",
            "coordinator.query.interval.ms", "debug", "default_topic_conf", "delivery.report.only.error", "delivery.timeout.ms", "dr_cb", "dr_msg_cb", "enable.auto.commit", "enable.auto.offset.store", "enabled_events", "enable.gapless.guarantee", "enable.idempotence",
            "enable.partition.eof", "enable.sasl.oauthbearer.unsecure.jwt", "error_cb", "fetch.error.backoff.ms", "fetch.max.bytes", "fetch.message.max.bytes", "fetch.min.bytes", "fetch.wait.max.ms", "group.id", "group.protocol.type", "heartbeat.interval.ms", "interceptors",
            "internal.termination.signal", "linger.ms", "log_cb", "log.connection.close", "log_level", "log.queue", "log.thread.name", "max.in.flight", "max.in.flight.requests.per.connection", "max.partition.fetch.bytes", "max.poll.interval.ms", "message.copy.max.bytes",
            "message.max.bytes", "message.send.max.retries", "message.timeout.ms", "metadata.broker.list", "metadata.max.age.ms", "metadata.request.timeout.ms", "msg_order_cmp", "oauthbearer_token_refresh_cb", "offset_commit_cb", "offset.store.method", "offset.store.path",
            "offset.store.sync.interval.ms", "opaque", "open_cb", "partition.assignment.strategy", "partitioner", "partitioner_cb", "plugin.library.paths", "produce.offset.report", "queue.buffering.backpressure.threshold", "queue.buffering.max.kbytes", "queue.buffering.max.messages",
            "queue.buffering.max.ms", "queued.max.messages.kbytes", "queued.min.messages", "queuing.strategy", "rebalance_cb", "receive.message.max.bytes", "reconnect.backoff.jitter.ms", "reconnect.backoff.max.ms", "reconnect.backoff.ms", "request.required.acks", "request.timeout.ms",
            "retries", "retry.backoff.ms", "sasl.kerberos.keytab", "sasl.kerberos.kinit.cmd", "sasl.kerberos.min.time.before.relogin", "sasl.kerberos.principal", "sasl.kerberos.service.name", "sasl.mechanism", "sasl.mechanisms", "sasl.oauthbearer.config", "sasl.password", "sasl.username",
            "security.protocol", "session.timeout.ms", "socket.blocking.max.ms", "socket_cb", "socket.keepalive.enable", "socket.max.fails", "socket.nagle.disable", "socket.receive.buffer.bytes", "socket.send.buffer.bytes", "socket.timeout.ms", "ssl.ca.location", "ssl.certificate.location",
            "ssl.cipher.suites", "ssl.crl.location", "ssl.curves.list", "ssl.key.location", "ssl.key.password", "ssl.keystore.location", "ssl.keystore.password", "ssl.sigalgs.list", "statistics.interval.ms", "stats_cb", "throttle_cb", "topic.blacklist", "topic.metadata.refresh.fast.cnt",
            "topic.metadata.refresh.fast.interval.ms", "topic.metadata.refresh.interval.ms", "topic.metadata.refresh.sparse"
        }));

        /// <summary>
        /// Sanitize the configuration by removing all the properties which are not allowed.
        /// </summary>
        /// <param name="kafkaConf">the Kafka configuration to sanitize.</param>
        /// <returns>the configuration, sanitized.</returns>
        /// <exception cref="ArgumentNullException">if <code>kafkaConf</code> is <code>null</code>.</exception>
        static internal IDictionary<string, string> SanitizeConfiguration(IDictionary<string, string> kafkaConf)
        {
            Condition.Requires(kafkaConf).IsNotNull();

            // Perf optimization : searching in a set is much cheaper than in a list.
            // Now there's no read only set in C# (contrary to Java).
            var allowed = new HashSet<string>(AllowedParams);

            var result = new Dictionary<string, string>();
            foreach (var entry in kafkaConf)
            {
                var key = entry.Key;
                if (allowed.Contains(key))
                {
                    result[key] = entry.Value;
                }
            }
            return result;
        }
    }
}
