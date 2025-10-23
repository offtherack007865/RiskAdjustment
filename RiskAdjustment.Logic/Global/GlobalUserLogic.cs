using RiskAdjustment.Data;
using RiskAdjustment.Data.Models.RefTables;
using RiskAdjustment.Logic.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace RiskAdjustment.Logic.Global
{
    public interface IGlobalUserLogic
    {
        List<User> GetActiveUsers();
        List<string> GetActiveUserDisplayNames();
        User GetUserById(int id);
        User GetUserByEmail(string email);
        User GetUserByUsername(string username);
        int AddUser(NameValueCollection form, string auditUsername);
        int UpdateUser(NameValueCollection form, string auditUsername);
        int UpdateUser(User user, string auditUsername);
        bool IsValidUser(string userName);

    }
    public class GlobalUserLogic : IGlobalUserLogic
    {
        public List<User> GetActiveUsers()
        {
            string sql = $"SELECT * FROM Users WHERE InactiveDate IS NULL OR InactiveDate = ' '";
            DbQuery query = new DbQuery();
            return query.Execute<User>(sql);

        }

        public List<string> GetActiveUserDisplayNames()
        {
            List<string> result = new List<string>();
            List<User> users = GetActiveUsers();
            foreach(User u in users)
            {
                result.Add(u.DisplayName);
            }
            return result;
        }

        public User GetUserById(int id)
        {
            string sql = $"SELECT * FROM Users WHERE Id = {id}";
            DbQuery query = new DbQuery();
            return query.ExecuteSingle<User>(sql);
        }

        public User GetUserByEmail(string email)
        {
            string sql = $"SELECT * FROM Users WHERE Email = '{email}'";
            DbQuery query = new DbQuery();
            return query.ExecuteSingle<User>(sql);
        }

        public User GetUserByUsername(string username)
        {
            string sql = $"SELECT * FROM Users WHERE Email = '{username}@summithealthcare.com'";
            DbQuery query = new DbQuery();
            return query.ExecuteSingle<User>(sql);
        }

        public int AddUser(NameValueCollection form, string auditUserName)
        {
            //form = SanatizeNewUserFormParams(form);
            User user = ParseUserFormParams(form);
            string sql = $"INSERT INTO Users (Initials, UserName, FirstName, LastName, Email, ActiveDate, Note, AthenaId, Contract, Role) VALUES ('{user.Initials.ToUpper()}', '{ user.UserName.ToLower()}', '{user.FirstName.Capitalize()}', '{user.LastName.Capitalize()}', '{user.Email.ToLower()}', '{user.ActiveDate}', '{user.Note}', '{user.AthenaId}', '{user.Contract}', '{user.Role}');";
            DbQuery query = new DbQuery();
            query.ActionExecuted += new EventHandler<DbQueryEventArgs>((sender, e) => AuditUserInsert(sender, e, user, query, DbAction.Insert, auditUserName));
            return query.ExecuteInsert<User>(sql);
        }


        public int UpdateUser(NameValueCollection values, string auditUsername)
        {
           return UpdateUser(ParseUserFormParams(values), auditUsername);
        }

        public int UpdateUser(User user, string auditUsername)
        {
            string sql = $@"Update Users SET Initials = '{user.Initials.ToUpper()}', UserName = '{user.UserName.ToLower()}', FirstName = '{user.FirstName.Capitalize()}', LastName = '{user.LastName.Capitalize()}', Email = '{user.Email}', InactiveDate = '{user.InactiveDate}'," +
                         $"Note = '{user.Note}', AthenaId = '{user.AthenaId}', Role = '{user.Role}' WHERE Id = {user.Id}";
            DbQuery query = new DbQuery();
            query.ActionExecuted +=
                new EventHandler<DbQueryEventArgs>((sender, e) => AuditUserInsert(sender, e, user, query, DbAction.Update, auditUsername));
            return query.ExecuteUpdate<User>(sql);
        }

        public bool IsValidUser(string userName)
        {
            DbQuery query = new DbQuery();
            string sql = $"SELECT COUNT(*) FROM Users WHERE Email LIKE '{userName}%'";
            int result = query.ExecuteScalar(sql);
            if (result > 0)
                return true;
            else
                return false;
        }

        private User ParseUserFormParams(NameValueCollection values)
        {
            //TODO:  Talk with DevExpress regarding double quote bug
            User user = new User()
            {
                Initials = values["Initials"].Trim('"'),
                UserName = values["UserName"].Trim('"'),
                FirstName = values["FirstName"].Trim('"'),
                LastName = values["LastName"].Trim('"'),
                Email = values["Email"].Trim('"'),
                ActiveDate = values["ActiveDate"].Trim('"'),
                Note = values["Note"].Trim('"'),
                AthenaId = values["AthenaId"].Trim('"'),
                Contract = values["Contract"].Trim('"'),
                Role = values["Role"]
            };

            if (values["InactiveDate"] != null)
                user.InactiveDate = values["InactiveDate"];
            if (values["Id"] != null)
                user.Id = Convert.ToInt32(values["Id"].Trim('"'));

            return user;
        }

        //The edit templates from the DevExpress UI tools cannot have the same field names as the gridview e.g. Email, FirstName, etc.  Therefore
        //we have to name them things like new_Email, new_FirstName, etc.  This method strips the new_ from the edit template fields so we can 
        //process them.
        private NameValueCollection SanatizeNewUserFormParams(NameValueCollection values)
        {
            NameValueCollection returnValue = new NameValueCollection();
            for (int i = 0; i < values.Count; i++)
            {
                string newKey = values.GetKey(i).Remove(0, 4);
                returnValue.Add(newKey, values[i]);
            }

            return returnValue;
        }

        private void AuditUserInsert(object sender, DbQueryEventArgs e, User user, DbQuery query, DbAction action, string auditUsername)
        {
            if (action == DbAction.Update)
                e.RecordId = user.Id;

            string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            string sql =
                $"INSERT INTO Audit_Users (Initials, FirstName, LastName, Email, ActiveDate, InactiveDate, Note, AthenaId, Contract, Role, AuditId, AuditDate, AuditUser, AuditAction) VALUES " +
                $"('{user.Initials.ToUpper()}', '{user.FirstName.Capitalize()}', '{user.LastName.Capitalize()}', '{user.Email.ToLower()}', " +
                $"'{user.ActiveDate}', '{user.InactiveDate}' ,'{user.Note}', '{user.AthenaId}', '{user.Contract}', '{user.Role}', {e.RecordId}, '{dateTime}', '{auditUsername}', '{action.ToString()}')";

            query.ExecuteCommand(sql);
        }
    }
}
