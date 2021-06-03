using System.Collections.Generic;
using System.Linq;

namespace Bunny.Messages
{
    public class RequestGeneration
    {
        public int Id { get; set; }

        public IEnumerable<Step> Steps { get; set; } = Enumerable.Empty<Step>();
    }
}
