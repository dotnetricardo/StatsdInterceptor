using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using CodeCop.Core;
using CodeCop.Core.Contracts;
using CodeCop.Core.Extensions;
using StatsdClient;

namespace Statsd.CodeCop
{
    public class StatsdInterceptor : ICopIntercept, ICopOverride
    {
        private readonly object _lock;
        private readonly Dictionary<string, int> _callCounter;
        private readonly Dictionary<string, int> _exceptionCounter; 


        public StatsdInterceptor()
        {
            _lock = new object();
            _callCounter = new Dictionary<string, int>();
            _exceptionCounter = new Dictionary<string, int>();
            
            var metricsConfig = new MetricsConfig
            {
                StatsdServerName = ConfigurationManager.AppSettings["StatsdServerName"],
                Prefix = ConfigurationManager.AppSettings["StatsdServerName"],
                StatsdServerPort = int.Parse(ConfigurationManager.AppSettings["StatsdServerPort"])
            };

            Metrics.Configure(metricsConfig);

        }

        public void OnBeforeExecute(InterceptionContext context)
        {
          
             int counter = 1;
            
            if(_callCounter.ContainsKey(context.InterceptedMethod.Name))
            {
               
                _callCounter.TryGetValue(context.InterceptedMethod.Name, out counter);
                _callCounter.Remove(context.InterceptedMethod.Name);
                counter += 1;

            }

             _callCounter.Add(context.InterceptedMethod.Name, counter);

            Metrics.Counter(
                string.Format(
                    "{0}.{1}",
                    context.InterceptedMethod.Name,
                    "times_called"),
                counter);
            context.Parameters.Add(new Parameter { Name = "StartTimeStamp", Value = context.TimeStamp });
        }

        public void OnAfterExecute(InterceptionContext context)
        {
            var startTimeStamp = context.Parameters.FirstOrDefault(x => x.Name == "StartTimeStamp");

            if (startTimeStamp != null)
            {
                var start = (DateTime)startTimeStamp.Value;
                var end = context.TimeStamp;

                var elapsed = end - start;
                Metrics.Timer(string.Format("{0}.{1}", "total_executiontime", context.InterceptedMethod.Name), (int)elapsed.TotalMilliseconds);
            }

        }

        public object OnOverride(InterceptionContext context)
        {
            lock (_lock)
            {
                object result = null;
                try
                {
                    result = context.InterceptedMethod.Execute(
                        context.Sender,
                        context.Parameters.Any()
                            ? context.Parameters.Where(p => p.Name != "StartTimeStamp").Select(x => x.Value)
                                .ToArray()
                            : null);
                }
                catch (Exception)
                {
                    int counter = 1;

                    if (_exceptionCounter.ContainsKey(context.InterceptedMethod.Name))
                    {
                        _exceptionCounter.TryGetValue(context.InterceptedMethod.Name, out counter);
                        _exceptionCounter.Remove(context.InterceptedMethod.Name);
                        counter += 1;

                    }
                    _exceptionCounter.Add(context.InterceptedMethod.Name, counter);
                    
                    Metrics.Counter(
                        string.Format(
                            "{0}.{1}",
                            context.InterceptedMethod.Name,
                            "num_errors"),
                        counter);
                }

                return result;
            }
        }
    }
}
