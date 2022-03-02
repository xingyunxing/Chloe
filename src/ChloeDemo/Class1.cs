//using MongoDB.Bson;
//using MongoDB.Bson.Serialization.Attributes;
//using MongoDB.Driver;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ChloeDemo
//{
//    internal class Class1
//    {
//        public static void Fn()
//        {
//            //与Mongodb建立连接
//            MongoClient client = new MongoClient("mongodb://127.0.0.1");
//            //获得数据库,没有则自动创建
//            IMongoDatabase db = client.GetDatabase("db1");
//            //拿到集合(表)
//            IMongoCollection<Student> student = db.GetCollection<Student>("Student");
//            var data = new Student();
//            data.id = 1;
//            data.name = "江北";
//            data.age = 22;
//            data.remarks = "暂无";
//            //添加一条数据
//            student.InsertOne(data);

//            var filter = Builders<Student>.Filter.Where(m => m.age > 21);
//            FindOptions<Student, Student> findOpt = new FindOptions<Student, Student>();
//            findOpt.Limit = 2;
//            findOpt.Skip = 1;
//            findOpt.Sort = Builders<Student>.Sort.Ascending(m => m.age).Descending(m => m.name);
//            var result = (student.FindAsync(filter, findOpt).Result).ToList();
//        }
//    }

//    public class School
//    {
//        public ObjectId id { get; set; }
//        public string name { get; set; }
//        public string address { get; set; }
//    }
//    public class Student
//    {
//        public int id { get; set; }
//        public string name { get; set; }
//        public int age { get; set; }
//        public string remarks { get; set; }
//        public School School { get; set; }
//    }
//}
