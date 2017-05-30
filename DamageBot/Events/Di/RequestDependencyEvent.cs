using System;
using DamageBot.EventSystem;

namespace DamageBot.Events.Di {
    public class RequestDependencyEvent : Event<RequestDependencyEvent> {
        
        /// <summary>
        /// The service you request.
        /// </summary>
        public Type RequestedResource {
            get;
        }

        /// <summary>
        /// The resolved resource which can be cast to the type of the requested resource safely.
        /// </summary>
        public object ResolvedResource {
            get;
            set;
        }
        /// <summary>
        /// The dependency request.
        /// After you called this event the ResolvedResource will be set.
        /// Or null if the resource cannot be resolved.
        /// </summary>
        /// <param name="requestedResource"></param>
        public RequestDependencyEvent(Type requestedResource) {
            this.RequestedResource = requestedResource;
        }
    }
}