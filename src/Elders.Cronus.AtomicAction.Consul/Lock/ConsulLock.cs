//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Elders.Cronus;
//using Elders.Cronus.AtomicAction;
//using Microsoft.Extensions.Logging;

//namespace Cronus.AtomicAction.Consul
//{
//    public class ConsulLock : ILock
//    {
//        private static readonly ILogger logger = CronusLogger.CreateLogger<ConsulLock>();

//        private readonly IConsulClient consul;

//        public ConsulLock(IConsulClient consulClient)
//        {
//            this.consul = consulClient;
//        }

//        public bool IsLocked(string resource)
//        {
//            if (string.IsNullOrEmpty(resource))
//                throw new ArgumentNullException(nameof(resource));

//            try
//            {
//                IEnumerable<ReadKeyValueResponse> response = consul.ReadKeyValueAsync(resource).ConfigureAwait(false).GetAwaiter().GetResult();
//                return response.Any(x => x.Key == resource);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, $"Unable to determine if resource '{resource}' is locked");
//                return false;
//            }
//        }

//        public bool Lock(string resource, TimeSpan ttl)
//        {
//            if (string.IsNullOrEmpty(resource)) throw new ArgumentException(nameof(resource));

//            try
//            {
//                var keyValueResponse = consul.CreateKeyValueAsync(new CreateKeyValueRequest(resource, null, resource)).GetAwaiter().GetResult(); ;
//                return keyValueResponse;
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, $"Unable to acquire lock for resource '{resource}'");
//                return false;
//            }
//        }

//        public void Unlock(string resource)
//        {
//            try
//            {
//                consul.DeleteSessionAsync(resource).ConfigureAwait(false).GetAwaiter().GetResult();
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, $"Unable to release lock for resource '{resource}'");
//            }
//        }
//    }
//}
