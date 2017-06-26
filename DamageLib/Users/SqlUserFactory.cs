using System;
using System.Collections.Generic;
using System.Linq;
using DamageBot.Events.Chat;
using DamageBot.Events.Database;
using DamageBot.Events.Users;
using DamageBot.EventSystem;

namespace DamageBot.Users {
    public class SqlUserFactory {

        private readonly Dictionary<string, IUser> userCache = new Dictionary<string, IUser>();


        public SqlUserFactory() {
            EventDispatcher.Instance.Register<RequestUserEvent>(OnUserRequest);
            EventDispatcher.Instance.Register<RequestAllUsersEvent>(OnRequestAllUsers);
            EventDispatcher.Instance.Register<UserLeftEvent>(OnUserLeft);
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
                twitchId = "INVALID"; // will never hit and query will go for the username which is also unique
            }
            select.WhereClause = $"twitch_id = '{twitchId}' or username = '{username}'";
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
                // new user!
                SqliteUser user = new SqliteUser(username) {
                    FirstJoined = DateTime.UtcNow,
                    LastJoined = DateTime.UtcNow
                };
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