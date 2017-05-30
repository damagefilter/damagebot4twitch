using System;
using System.Collections.Generic;
using DamageBot.Events.Users;
using DamageBot.EventSystem;
using TwitchLib;
using TwitchLib.Models.API.v5.Subscriptions;
using TwitchLib.Models.API.v5.Users;

namespace DamageBot.Users {
    
    /// <summary>
    /// Global object managing users.
    /// Basically.
    /// </summary>
    public class UserManager : IDisposable {
        private readonly Dictionary<string, User> nameToUserMap;
        private long cacheTimeout = 600;

        public UserManager() {
            nameToUserMap = new Dictionary<string, User>();
            EventDispatcher.Instance.Register<RequestTwitchUserEvent>(OnUserRequested);
        }
        
        public void Dispose() {
            EventDispatcher.Instance.Unregister<RequestTwitchUserEvent>(OnUserRequested);
        }

        /// <summary>
        /// Async call that will yield if given user has subscribed to the channel or not.
        /// This is really low-level API you probably don't want to call this directly.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="userId"></param>
        /// <param name="setterCallback">callback to pass in when the api call returns and delivers the data</param>
        public async void GetSubscriptionInfoForUserInChannel(string channelId, string userId, Action<Subscription> setterCallback) {
            var stat = await TwitchAPI.Channels.v5.CheckChannelSubscriptionByUser(channelId, userId);
            setterCallback(stat);
        }
        
        /// <summary>
        /// Async call that will yield if given user has subscribed to the channel or not.
        /// This is really low-level API you probably don't want to call this directly.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="user"></param>
        /// <param name="setterCallback"></param>
        public async void GetSubscriptionInfoForUserInChannel(string channelId, User user, Action<Subscription> setterCallback) {
            var stat = await TwitchAPI.Channels.v5.CheckChannelSubscriptionByUser(channelId, user.Id);
            setterCallback(stat);
        }

        /// <summary>
        /// Get first user that was found by the given name.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="setterCallback"></param>
        public async void GetUserByName(string userName, Action<User> setterCallback) {
            var usr = CheckNameCache(userName);
            if (usr != null) {
                setterCallback(usr);
                return;
            }
            var usrs = await TwitchAPI.Users.v5.GetUserByName(userName);
            var user = usrs.Matches.Length > 0 ? usrs.Matches[0] : null;
            if (user != null) {
                UpdateNameCache(userName, user);
            }
            setterCallback(user);
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
            this.GetUserByName(ev.UserName, ev.Setter);
        }
    }
}