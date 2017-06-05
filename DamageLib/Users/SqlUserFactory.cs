using System;
using System.Collections.Generic;
using DamageBot.Events.Database;
using DamageBot.Events.Users;
using DamageBot.EventSystem;

namespace DamageBot.Users {
    public class SqlUserFactory {

        private readonly Dictionary<string, IUser> userCache = new Dictionary<string, IUser>();

        private readonly TwitchUserApi twitchApi;

        public SqlUserFactory(TwitchUserApi api) {
            this.twitchApi = api;
            EventDispatcher.Instance.Register<RequestUserEvent>(OnUserRequest);
        }
        
        public IUser GetUserByName(string username) {
            if (userCache.ContainsKey(username)) {
                return userCache[username];
            }
            var select = new SelectEvent();
            select.TableList = "users";
            select.FieldList.Add("*");
            select.WhereClause = $"username = '{username}'";
            select.Call();

            if (select.ResultSet.Read()) {
                SqliteUser user = new SqliteUser(select.GetString("username")) {
                    TwitchId = select.GetString("twitch_id"),
                    UserId = select.GetInteger("user_id"),
                    FirstJoined = select.GetDateTime("first_joined"),
                    LastJoined = select.GetDateTime("last_joined")
                };
                userCache.Add(username, user);
                return user;
            }
            else {
                SqliteUser user = new SqliteUser(username) {
                    FirstJoined = DateTime.UtcNow,
                    LastJoined = DateTime.UtcNow
                };
                var twitchUser = this.twitchApi.GetUserByName(username);
                user.TwitchId = twitchUser.Id;
                // Check if we have this user but with a different name.
                select.TableList = "users";
                select.FieldList.Add("*");
                select.WhereClause = $"twitch_id = '{user.TwitchId}'";
                select.Call();
                // If we do, update this instance and send it back to database.
                if (select.ReadNext()) {
                    user.UserId = select.GetInteger("user_id");
                    user.FirstJoined = select.GetDateTime("first_joined");
                    user.LastJoined = select.GetDateTime("last_joined");
                    
                    var update = new UpdateEvent();
                    update.TableName = "users";
                    update.DataList.Add("username", username);
                    update.WhereClause = $"twitch_id = '{user.TwitchId}'";
                    update.Call();
                }
                else {
                    user.FirstJoined = DateTime.UtcNow;
                    user.LastJoined = DateTime.UtcNow;
                    user.Username = twitchUser.Name;
                    user.TwitchId = twitchUser.Id;
                    
                    var insert = new InsertEvent();
                    insert.TableName = "users";
                    insert.DataList.Add("username", username);
                    insert.DataList.Add("twitch_id", twitchUser.Id);
                    insert.DataList.Add("last_joined", user.LastJoined);
                    insert.DataList.Add("first_joined", user.FirstJoined);
                    insert.Call();
                }
               
                userCache.Add(username, user);
                return user;
            }
        }

        private void OnUserRequest(RequestUserEvent ev) {
            ev.ResolvedUser = GetUserByName(ev.Username);
        }
        
    }
}