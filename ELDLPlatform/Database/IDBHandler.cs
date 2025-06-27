using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELDLPlatform.Database
{
    // Interface for interacting with DBHandler
    internal interface IDBHandler
    {
        internal (string, string)[] GetLoginPasswordPairs();
        // User search
        internal string GetPassword(string login);
        internal DBData GetUser(int id);
        internal DBData GetUserWithoutPassword(string login);
        internal DBData[] GetUsersByUsernameWithoutPassword(string username);
        internal DBData[] GetUsersByUsernameAndRolesWithoutPassword(string username, string[] roles);

        internal OperationStatus DeleteUser(string login);
        internal OperationStatus UpdateUser(DBData user);
        internal OperationStatus AddUser(DBData user);



        // Articles
        internal string GetUniqueArticleCategories();
        internal DBData GetArticle(int id);
        internal DBData GetArticleByKeyword(string keyword);
        internal DBData GetArticleByKeywordWithCategories(string keyword, string[] categories);
        internal OperationStatus DeleteArticle(string articleID);
        internal OperationStatus UpdateArticle(DBData article);
        internal OperationStatus AddArticle(DBData article);

        // Any
        internal string[] GetFieldsName(string table);



    }
}
