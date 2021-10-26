using System;

namespace Cronus.AtomicAction.Consul
{
    public partial class ConsulClient
    {
        public enum SessionBehavior
        {
            Release = 0,
            Delete = 1
        }

        public class CreateSessionRequest
        {
            public CreateSessionRequest(string name, TimeSpan ttl, TimeSpan? lockDelay, SessionBehavior behavior = SessionBehavior.Delete)
            {
                if (string.IsNullOrEmpty(name)) throw new ArgumentException(nameof(name));

                Name = name;
                Behavior = behavior.ToString().ToLower();

                if (ttl.TotalSeconds < 10)
                    Ttl = "10s";
                else if (ttl.TotalSeconds > 86400)
                    Ttl = "86400s";
                else
                    Ttl = $"{ttl.TotalSeconds}s";

                if (lockDelay.HasValue && lockDelay.Value >= TimeSpan.Zero)
                    LockDelay = $"{lockDelay.Value.TotalSeconds}s";
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

        public class ReadSessionResponse
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Node { get; set; }
            public string[] Checks { get; set; }
            public long LockDelay { get; set; }
            public string Behavior { get; set; }
            public string Ttl { get; set; }
            public int CreateIndex { get; set; }
            public int ModifyIndex { get; set; }
        }

        public class CreateKeyValueRequest
        {
            public CreateKeyValueRequest(string key, object value) : this(key, value, string.Empty, 0) { }

            public CreateKeyValueRequest(string key, object value, string sessionId) : this(key, value, sessionId, 0) { }

            public CreateKeyValueRequest(string key, object value, int cas) : this(key, value, string.Empty, cas) { }

            private CreateKeyValueRequest(string key, object value, string sessionId, int cas)
            {
                if (string.IsNullOrEmpty(key)) throw new ArgumentException(nameof(key));

                Key = key;
                Value = value ?? new object();
                SessionId = sessionId;
                Cas = cas;
            }

            public string Key { get; }
            public object Value { get; }
            public string SessionId { get; }
            public int Cas { get; set; }
        }

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
    }
}
