using DamageBot.Events.Database;
using DamageBot.Users;

namespace ScoresCore {
    public class Score {
        public int Id {
            get;
            set;
        }

        public int UserId {
            get;
            set;
        }

        public int ScoreValue {
            get;
            set;
        }

        public static Score GetForUser(IUser user) {
            var select = new SelectEvent();
            select.TableList = "user_score";
            select.FieldList.Add("*");
            select.WhereClause = $"user_id = {user.UserId}";
            select.Call();

            if (select.ReadNext()) {
                return new Score {
                    Id = select.GetInteger("id"),
                    UserId = select.GetInteger("user_id"),
                    ScoreValue = select.GetInteger("score")
                };
            }
            return new Score {UserId = (int)user.UserId};
        }
        
        public void Save() {
            if (this.Id > 0) {
                Update();
            }
            else {
                Insert();
            }
        }

        private void Insert() {
            var insert = new InsertEvent();
            insert.TableName = "user_score";
            insert.DataList.Add("user_id", this.UserId);
            insert.DataList.Add("score", this.ScoreValue);
            insert.Call();
        }

        private void Update() {
            var update = new UpdateEvent();
            update.TableName = "user_score";
            update.DataList.Add("score", this.ScoreValue);
            update.WhereClause = $"id = {this.Id}";
            update.Call();
        }

    }
}