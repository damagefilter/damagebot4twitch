using System;
using System.Collections.Generic;
using DamageBot.Events.Users;
using DamageBot.EventSystem;
using TwitchLib;
using TwitchLib.Models.API.v5.Users;

namespace DamageBot.Users {
    
    /// <summary>
    /// Global object managing users.
    /// Basically.
    /// This is blocking. Possibly very long.
    /// So be aware of that.
    /// </summary>
    public class TwitchUserApi : IDisposable {
        private readonly Dictionary<string, User> nameToUserMap;
        private long cacheTimeout = 600;

        public TwitchUserApi() {
            nameToUserMap = new Dictionary<string, User>();
            EventDispatcher.Instance.Register<RequestTwitchUserEvent>(OnUserRequested);
        }
        
        public void Dispose() {
            EventDispatcher.Instance.Unregister<RequestTwitchUserEvent>(OnUserRequested);
        }

        /// <summary>
        /// Get first user that was found by the given name.
        /// </summary>
        /// <param name="userName"></param>
        public User GetUserByName(string userName) {
            var usr = CheckNameCache(userName);
            if (usr != null) {
                return usr;
            }
            var usrs = TwitchAPI.Users.v5.GetUserByName(userName);
            var user = usrs.Result.Matches.Length > 0 ? usrs.Result.Matches[0] : null;
            if (user != null) {
                UpdateNameCache(userName, user);
            }
            return user;
        }

        /// <summary>
        /// Gives already cached user.
        /// Respects cache timeout.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private User CheckNameCache(string name) {
            if (nameToUserMap.ContainsKey(name)) {
                var usr = nameToUserMap[name];
                if ((DateTime.UtcNow - usr.CreatedAt).Seconds < cacheTimeout) {
                    return usr;
                }
            }
            return null;
        }

        private void UpdateNameCache(string name, User user) {
            if (nameToUserMap.ContainsKey(name)) {
                nameToUserMap[name] = user;
            }
            else {
                nameToUserMap.Add(name, user);
            }
        }

        private void OnUserRequested(RequestTwitchUserEvent ev) {
            ev.ResolvedUser = this.GetUserByName(ev.UserName);
        }
    }
}