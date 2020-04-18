using System;

namespace Cronus.AtomicAction.Consul
{
    public partial class ConsulClient
    {
        public class ReadKeyValueResponse
        {
            public int CreateIndex { get; set; }
            public int ModifyIndex { get; set; }
            public int LockIndex { get; set; }
            public string Key { get; set; }
            public int Flags { get; set; }
            public string Value { get; set; }
            public string Session { get; set; }
        }

        public class CreateSessionRequest
        {
            public CreateSessionRequest(string name, string behavior = "delete", int ttl = 10, long lockDelay = 0)
            {
                if (string.IsNullOrEmpty(name)) throw new ArgumentException(nameof(name));
                if (behavior != "delete" && behavior != "release") throw new ArgumentException("Allowed behavior values are 'delete' and 'release'.", nameof(behavior));
                if (ttl < 10 || ttl > 86400) throw new ArgumentException("Sesstion ttl can be between 10 and 86400 seconds");

                Name = name;
                Behavior = behavior;
                Ttl = $"{ttl}s";
                LockDelay = $"{lockDelay}s";
            }

            public string Name { get; }
            public string Ttl { get; }
            public string Behavior { get; }
            public string LockDelay { get; }
        }

        public class CreateSessionResponse
        {
            public string Id { get; set; }
            public bool Success { get => string.IsNullOrEmpty(Id) == false; }
        }

        public class CreateKeyValueRequest
        {
            public CreateKeyValueRequest(string key, object value) : this(key, value, string.Empty, 0) { }

            public CreateKeyValueRequest(string key, object value, string sessionId) : this(key, value, sessionId, 0) { }

            public CreateKeyValueRequest(string key, object value, int cas) : this(key, value, string.Empty, cas) { }

            private CreateKeyValueRequest(string key, object value, string sessionId, int cas)
            {
                if (string.IsNullOrEmpty(key)) throw new ArgumentException(nameof(key));
                if (value is null) throw new ArgumentException(nameof(value));

                Key = key;
                Value = value;
                SessionId = sessionId;
                Cas = cas;
            }

            public string Key { get; }
            public object Value { get; }
            public string SessionId { get; }
            public int Cas { get; set; }
        }
    }
}
