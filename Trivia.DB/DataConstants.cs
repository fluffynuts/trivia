using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trivia.DB
{
    public static class DataConstants
    {
        public static class Tables
        {
            public class _CommonColumns
            {
                public const string ENABLED = "Enabled";
                public const string CREATED = "Created";
                public const string LASTMODIFIED = "LastModified";
            }
            public class Category
            {
                public const string NAME = "Category";
                public class Columns: _CommonColumns
                {
                    public const string CATEGORYID = "CategoryID";
                    public const string NAME = "Name";
                }
            }
            public class Question
            {
                public const string NAME = "Question";
                public class Columns : _CommonColumns
                {
                    public const string QUESTIONID = "QuestionID";
                    public const string QUESTION = "Question";
                    public const string ANSWER = "Answer";
                }
            }
            public class User
            {
                public const string NAME = "User";
                public class Columns : _CommonColumns
                {
                    public const string USERID = "UserID";
                    public const string LOGIN = "Login";
                }
            }
            public class Score
            {
                public const string NAME = "Score";
                public class Columns : _CommonColumns
                {
                    public const string SCOREID = "ScoreID";
                    public const string USERID = "UserID";
                    public const string QUESTIONID = "QuestionID";
                    public const string SCORE = "Score";
                }
            }

        }
    }
}
