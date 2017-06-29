using System;
using System.Collections.Generic;
using System.Linq;
using DamageBot.Events.Chat;
using DamageBot.Events.Database;
using DamageBot.Events.Users;
using DamageBot.EventSystem;
using DamageBot.Logging;

namespace DamageBot.Users {
    public class SqlUserFactory {
        private readonly Dictionary<string, IUser> userCache = new Dictionary<string, IUser>();
        private readonly Logger log;
        private readonly TwitchUserApi api;


        public SqlUserFactory(TwitchUserApi api) {
            log = LogManager.GetLogger(GetType());
            EventDispatcher.Instance.Register<RequestUserEvent>(OnUserRequest);
            EventDispatcher.Instance.Register<RequestAllUsersEvent>(OnRequestAllUsers);
            EventDispatcher.Instance.Register<UserLeftEvent>(OnUserLeft);
            this.api = api;
        }
        
        public IUser GetUserByTwitchId(string twitchId, string username) {
            if (twitchId == null && username == null) {
                throw new ArgumentException("Cannot allow username and twitch id to be null. You gotta take one of em at the very least!");
            }
            if (userCache.ContainsKey(username)) {
                return userCache[username];
            }
            var select = new SelectEvent();
            select.TableList = "users";
            select.FieldList.Add("*");
            if (twitchId == null) {
                log.Info("No twitch ID for user lookup. Attempting to find by user name.");
                select.WhereClause = $"username = '{username}'";
            }
            else {
                log.Info("Using twitch ID for user lookup.");
                select.WhereClause = $"twitch_id = '{twitchId}'";
            }
            
            select.Call();
            if (select.ResultSet.Read()) {
                SqliteUser user = new SqliteUser(username) {
                    TwitchId = select.GetString("twitch_id"),
                    UserId = select.GetInteger("user_id"),
                    FirstJoined = select.GetDateTime("first_joined"),
                    LastJoined = select.GetDateTime("last_joined")
                };
                userCache.Add(username, user);
                
                if (username != select.GetString("username")) {
                    var update = new UpdateEvent();
                    update.TableName = "users";
                    update.DataList.Add("username", username);
                    update.WhereClause = $"twitch_id = '{user.TwitchId}'";
                    update.Call();
                }
                return user;
            }
            else {
                log.Info("Nothing found. Trying to create new User.");
                // new user!
                SqliteUser user = new SqliteUser(username) {
                    FirstJoined = DateTime.UtcNow,
                    LastJoined = DateTime.UtcNow
                };
                if (twitchId == null) {
                    // Get the ID from twitch.
                    var twitchUser = api.GetUserByName(username);
                    twitchId = twitchUser.Id;
                }
                user.TwitchId = twitchId;
                // Check if we have this user but with a different name.
                user.FirstJoined = DateTime.UtcNow;
                user.LastJoined = DateTime.UtcNow;
                user.Name = username;
                user.TwitchId = twitchId;

                var insert = new InsertEvent();
                insert.TableName = "users";
                insert.DataList.Add("username", username);
                insert.DataList.Add("twitch_id", twitchId);
                insert.DataList.Add("last_joined", user.LastJoined);
                insert.DataList.Add("first_joined", user.FirstJoined);
                insert.Call();
                user.UserId = (int)insert.LastInsertId;
               
                userCache.Add(username, user);
                return user;
            }
        }

        private void OnUserRequest(RequestUserEvent ev) {
            ev.ResolvedUser = GetUserByTwitchId(ev.TwitchId, ev.Username);
        }

        private void OnRequestAllUsers(RequestAllUsersEvent ev) {
            ev.ResolvedUsers = this.userCache.Values.ToList();
        }
        
        private void OnUserLeft(UserLeftEvent ev) {
            var update = new UpdateEvent();
            update.TableName = "users";
            update.DataList.Add("last_joined", ev.User.LastJoined);
            update.WhereClause = $"twitch_id = '{ev.User.TwitchId}'";
            update.Call();
            try {
                this.userCache.Remove(ev.User.Name);
            }
            catch {}
        }
        
    }
}