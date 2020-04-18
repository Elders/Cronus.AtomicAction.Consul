using System.ComponentModel.DataAnnotations;

namespace Cronus.AtomicAction.Consul
{
    public class ConsulClientOptions
    {
        [Required(AllowEmptyStrings = false)]
        public string Endpoint { get; set; }

        public string Token { get; set; }
    }
}
