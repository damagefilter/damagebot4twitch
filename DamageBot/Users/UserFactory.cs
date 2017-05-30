using System;
using DamageBot.Events.Database;

namespace DamageBot.Users {
    public class UserFactory {
        
        private UserManager userManagaer = new UserManager();

        /// <summary>
        /// Get a user by name.
        /// Mind you that some information such as Bio and Twitch ID may
        /// not be ready directly after receiving the IUser object as that is 
        /// populated in an async call via twitch api.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IUser GetUserByName(string name) {
            var chatUser = new ChatUser();
            
            SelectEvent ev = new SelectEvent();
            ev.TableList = "users";
            ev.FieldList.Add("first_joined");
            ev.FieldList.Add("last_visit");
            ev.FieldList.Add("last_remote_update");
            ev.FieldList.Add("display_name");
            ev.FieldList.Add("id");
            ev.FieldList.Add("twitch_id");
            ev.WhereClause = $"display_name = {name}";
            ev.Call();
            if (ev.ResultSet.Read()) {
                chatUser.DateJoinedFirst = ev.ResultSet.GetDateTime(0);
                chatUser.DateLastVisited = ev.ResultSet.GetDateTime(1);
                chatUser.DateLastRemoteUpdate = ev.ResultSet.GetDateTime(2);
                chatUser.DisplayName = ev.ResultSet.GetString(3);
                chatUser.LocalId = ev.ResultSet.GetString(4);
                chatUser.TwitchId = ev.ResultSet.GetString(5);
            }
            else {
                chatUser.DateJoinedFirst = DateTime.UtcNow;
                chatUser.DateLastVisited = DateTime.UtcNow;
                chatUser.DisplayName = name;
            }
            
            // Auto update remote stats about twice a month.
            if (chatUser.TwitchId == null || (DateTime.UtcNow - chatUser.DateLastRemoteUpdate).Days >= 15) {
                userManagaer.GetUserByName(name, twitchUser => {
                    chatUser.DateLastRemoteUpdate = DateTime.UtcNow;
                    chatUser.Bio = twitchUser.Bio;
                    chatUser.TwitchId = twitchUser.Id;
                });
            }
            
        }

        /// <summary>
        /// Writes the user object as is now back to database.
        /// </summary>
        /// <param name="usr"></param>
        public void WriteUser(IUser usr) {
            if (usr.LocalId != null) {
                UpdateEvent ev = new UpdateEvent();
                ev.TableName = "users";
                ev.DataList.Add("last_visit", DateTime.UtcNow);
            }
        }
        
    }
}