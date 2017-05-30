using System;
using DamageBot.Events.Database;

namespace DamageBot.Users {
    public class UserFactory {
        private UserManager userManagaer = new UserManager();

        public void GetUserByName(string name, Action<IUser> receivedCallback) {
            userManagaer.GetUserByName(name, (usr) => {
                var chatUser = new ChatUser(usr);
                SelectEvent ev = new SelectEvent();
                ev.TableList = "users";
                ev.
                receivedCallback(chatUser);
            });
        }
        
    }
}