using Serilog.Core;
using Serilog.Events;

namespace API.Filters
{
    public class TruncateAllStringPropertiesEnricher : ILogEventEnricher
    {
        private readonly int _maxLength;

        public TruncateAllStringPropertiesEnricher(int maxLength)
        {
            _maxLength = maxLength;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            foreach (var key in logEvent.Properties.Keys.ToList())
            {
                if (logEvent.Properties[key] is ScalarValue scalar &&
                    scalar.Value is string str &&
                    str.Length > _maxLength)
                {
                    var truncated = str.Substring(0, _maxLength) + "...(truncated)";
                    logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(key, truncated));
                }
            }
        }
    }
}
