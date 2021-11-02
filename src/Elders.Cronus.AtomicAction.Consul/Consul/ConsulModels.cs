using System;

namespace Cronus.AtomicAction.Consul
{
    public class CreateSessionRequest
    {
        public const string _10Seconds = "10s";
        public const string MaxTtl = "86400s";

        public CreateSessionRequest(string name, TimeSpan ttl)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException(nameof(name));

            Name = name;
        }

        public string Name { get; }
        public string Ttl { get; } = _10Seconds;
        public string Behavior { get; } = "delete";
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
