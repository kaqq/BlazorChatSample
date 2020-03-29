using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorChatSample.Shared
{
    public static class BrowserAgentHelper
    {
        public static void RegisterServerEventsForType<Target, TInterface>(Target target, HubConnection hubConnection) 
            where Target:class, TInterface where TInterface : class
        {
            if (!typeof(TInterface).IsInterface)
                throw new ArgumentException(nameof(TInterface));
            var targetType = target.GetType();
            Console.WriteLine("Binding for:" + targetType.ToString());
            var interfaceType = typeof(TInterface);
            var methods = interfaceType.GetMethods();
            foreach (var method in methods)
            {
                Console.WriteLine("Binding method:" + method.Name);
                MethodInfo targetMethod = targetType.GetMethod(method.Name);
                var methodParameters = targetMethod.GetParameters().Select(param => param.ParameterType);
                Action<object[]> methodCall = new Action<object[]>((x) => targetMethod.Invoke(target, x));
                Console.WriteLine("Binding method parameters:" +
                                  String.Join(", ", methodParameters.Select(x => x.ToString())));
                hubConnection.On(method.Name, methodParameters.ToArray(), (parameters, state) =>
                {
                    var currentHandler = (Action<object[]>) state;
                    currentHandler(parameters);
                    return Task.CompletedTask;
                }, methodCall);
                Console.WriteLine("Binding complete:" + method.Name);
            }
        }
    }

}