﻿using Chloe;
using Chloe.SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeDemo
{
    abstract class DemoBase : IDisposable
    {
        object _result;
        public DemoBase()
        {
            this.DbContext = this.CreateDbContext();

            /* context filter，仅对当前 DbContext 对象有效 */
            this.DbContext.HasQueryFilter<Person>(a => a.Id > -100);
        }

        /* WARNING: DbContext 是非线程安全的，不能设置为 static，并且用完务必要调用 Dispose 方法销毁对象 */
        public IDbContext DbContext { get; private set; }

        protected abstract IDbContext CreateDbContext();

        public virtual void Dispose()
        {
            this.DbContext.Dispose();
        }

        public virtual void Run()
        {
            this.InitDatabase();
            this.InitData();

            this.Crud();
            this.CrudAsync().GetAwaiter().GetResult();

            this.BasicQuery();
            this.JoinQuery();
            this.ExcludeFieldQuery();
            this.AggregateQuery();
            this.GroupQuery();
            this.ComplexQuery();
            this.QueryWithNavigation();
            this.Insert();
            this.Update();
            this.Delete();
            this.Method();
            this.ExecuteCommandText();
            this.DoWithTransaction();

            ConsoleHelper.WriteLineAndReadKey();
        }

        public virtual void InitDatabase()
        {

        }

        public virtual void InitData()
        {
            List<Province> provinces = new List<Province>();
            if (!this.DbContext.Query<Province>().Any())
            {
                provinces.Add(this.CreateProvince("广东", "广州", "深圳", "东莞"));
                provinces.Add(this.CreateProvince("广西", "南宁", "柳州", "桂林", "河池"));
                provinces.Add(this.CreateProvince("湖南", "长沙", "衡阳", "张家界"));

                foreach (var province in provinces)
                {
                    this.DbContext.Save(province);
                }
            }

            provinces = this.DbContext.Query<Province>().IncludeAll().ToList();

            //清空旧数据
            this.DbContext.Delete<TestEntity>(a => true);

            List<TestEntity> testEntities = new List<TestEntity>();
            for (int i = 0; i < 1.2 * 10000; i++)
            {
                TestEntity testEntity = new TestEntity()
                {
                    F_Byte = (byte)(i % 10),
                    F_Int16 = (Int16)(16 + i),
                    F_Int32 = 32 + i,
                    F_Int64 = 64 + i,
                    F_Double = 100.01 + i,
                    F_Float = 10.01f + i,
                    F_Decimal = 1000.01M + i,
                    F_Bool = (i % 2) == 0,
                    F_DateTime = DateTime.Now.AddMinutes(i),
                    F_Guid = Guid.NewGuid(),
                    F_String = "Chloe.ORM-" + i,
                    F_Enum = Gender.Male
                };

                testEntities.Add(testEntity);
            }

            this.DbContext.InsertRange(testEntities);

            testEntities = this.DbContext.Query<TestEntity>().ToList();

            ConsoleHelper.WriteLineAndReadKey("InitData over...");
        }
        public Province CreateProvince(string provinceName, params string[] cityNames)
        {
            Province province = new Province();
            province.Name = provinceName;

            cityNames = cityNames ?? new string[0];
            foreach (var cityName in cityNames)
            {
                province.Cities.Add(this.CreateCity(cityName));
            }

            return province;
        }
        public City CreateCity(string cityName)
        {
            City city = new City();
            city.Name = cityName;

            city.Persons.Add(new Person() { Name = $"{city.Name}-张三", Age = 30, Gender = Gender.Male, CreateTime = DateTime.Now, Ex = new PersonEx() { IdNumber = "452723197110211024", BirthDay = new DateTime(1971, 10, 21) } });
            city.Persons.Add(new Person() { Name = $"{city.Name}-李四", Age = 31, Gender = Gender.Male, CreateTime = DateTime.Now, Ex = new PersonEx() { IdNumber = "452723197110221024", BirthDay = new DateTime(1971, 10, 22) } });
            city.Persons.Add(new Person() { Name = $"{city.Name}-Chloe", Age = 18, Gender = Gender.Female, CreateTime = DateTime.Now, Ex = new PersonEx() { IdNumber = "452723197110231024", BirthDay = new DateTime(1971, 10, 23) } });
            city.Persons.Add(new Person() { Name = $"{city.Name}-东方不败", CreateTime = DateTime.Now, Ex = new PersonEx() { IdNumber = "452723197110241024", BirthDay = new DateTime(1971, 10, 24) } });

            return city;
        }

        public virtual void Crud()
        {
            Province province = this.CreateProvince("北京", "朝阳区", "海淀区");
            //保存数据到数据库，会将导航属性一起保存
            this.DbContext.Save(province);

            //查询数据并包含关联的导航属性一并查出
            province = this.DbContext.Query<Province>().IncludeAll().Where(a => a.Id == province.Id).First();

            Person person = null;
            var q = this.DbContext.Query<Person>();

            this._result = q.Where(a => a.Id > 0).ToList();
            this._result = q.Count();
            this._result = q.LongCount();
            this._result = q.Max(a => a.Id);
            this._result = q.Min(a => a.Id);
            this._result = q.Sum(a => a.Id);
            this._result = q.Average(a => a.Age);

            person = new Person() { Name = "chloe", Age = 18, Gender = Gender.Female, CityId = 1, CreateTime = DateTime.Now };

            //插入
            person = this.DbContext.Insert(person);

            //lambda 表达式更新
            this._result = this.DbContext.Update<Person>(a => a.Id == person.Id, a => new Person() { Age = a.Age + 1, EditTime = DateTime.Now });

            //查询
            person = this.DbContext.QueryByKey<Person>(person.Id);
            person.Age = person.Age + 1;
            person.EditTime = DateTime.Now;

            //更新
            this._result = this.DbContext.Update(person);

            //删除
            this._result = this.DbContext.Delete<Person>(person);

            //lambda 表达式插入，返回主键
            var insertedId = this.DbContext.Insert(() => new Person() { Name = "chloe", Age = 18, Gender = Gender.Female, CityId = 1, CreateTime = DateTime.Now, RowVersion = 0 });

            //根据主键删除
            this._result = this.DbContext.DeleteByKey<Person>(insertedId);

            ConsoleHelper.WriteLineAndReadKey(1);
        }
        public virtual async Task CrudAsync()
        {
            Province province = this.CreateProvince("上海", "黄浦区", "徐汇区");
            //保存数据到数据库，会将导航属性一起保存
            await this.DbContext.SaveAsync(province);

            //查询数据并包含关联的导航属性一并查出
            province = await this.DbContext.Query<Province>().IncludeAll().Where(a => a.Id == province.Id).FirstAsync();

            Person person = null;
            var q = this.DbContext.Query<Person>();

            this._result = await q.Where(a => a.Id > 0).ToListAsync();
            this._result = await q.CountAsync();
            this._result = await q.LongCountAsync();
            this._result = await q.MaxAsync(a => a.Id);
            this._result = await q.MinAsync(a => a.Id);
            this._result = await q.SumAsync(a => a.Id);
            this._result = await q.AverageAsync(a => a.Age);

            person = new Person() { Name = "Chloe", Age = 18, Gender = Gender.Female, CityId = 1, CreateTime = DateTime.Now };

            //插入
            person = await this.DbContext.InsertAsync(person);

            //lambda 表达式更新
            this._result = await this.DbContext.UpdateAsync<Person>(a => a.Id == person.Id, a => new Person() { Age = a.Age + 1, EditTime = DateTime.Now });

            //查询
            person = await this.DbContext.QueryByKeyAsync<Person>(person.Id);
            person.Age = person.Age + 1;
            person.EditTime = DateTime.Now;

            //更新
            this._result = await this.DbContext.UpdateAsync(person);

            //删除
            this._result = await this.DbContext.DeleteAsync<Person>(person);

            //lambda 表达式插入，返回主键
            var insertedId = await this.DbContext.InsertAsync(() => new Person() { Name = "Chloe", Age = 18, Gender = Gender.Female, CityId = 1, CreateTime = DateTime.Now, RowVersion = 0 });

            //根据主键删除
            this._result = await this.DbContext.DeleteByKeyAsync<Person>(insertedId);

            ConsoleHelper.WriteLineAndReadKey(1);
        }

        /// <summary>
        /// 基础查询
        /// </summary>
        public virtual void BasicQuery()
        {
            IQuery<Person> q = this.DbContext.Query<Person>();

            string name = "Chloe";
            this._result = q.Where(a => a.Name == name).ToList();

            this._result = q.Where(a => a.Id == 1).FirstOrDefault();
            /*
             * SELECT [Person].[Id] AS [Id],[Person].[Name] AS [Name],[Person].[Gender] AS [Gender],[Person].[Age] AS [Age],[Person].[CityId] AS [CityId],[Person].[EditTime] AS [EditTime] FROM [Person] AS [Person] WHERE [Person].[Id] = 1 LIMIT 1 OFFSET 0
             */


            //可以选取指定的字段
            this._result = q.Where(a => a.Id == 1).Select(a => new { a.Id, a.Name }).FirstOrDefault();
            /*
             * SELECT [Person].[Id] AS [Id],[Person].[Name] AS [Name] FROM [Person] AS [Person] WHERE [Person].[Id] = 1 LIMIT 1 OFFSET 0
             */


            //分页
            this._result = q.Where(a => a.Id > 0).OrderBy(a => a.Age).Skip(20).Take(10).ToList();
            /*
             * SELECT [Person].[Id] AS [Id],[Person].[Name] AS [Name],[Person].[Gender] AS [Gender],[Person].[Age] AS [Age],[Person].[CityId] AS [CityId],[Person].[EditTime] AS [EditTime] FROM [Person] AS [Person] WHERE [Person].[Id] > 0 ORDER BY [Person].[Age] ASC LIMIT 10 OFFSET 20
             */


            /* like 查询 */
            this._result = q.Where(a => a.Name.Contains("Chloe") || a.Name.StartsWith("C") || a.Name.EndsWith("o")).ToList();
            /*
             * SELECT 
             *      [Person].[Gender] AS [Gender],[Person].[Age] AS [Age],[Person].[CityId] AS [CityId],[Person].[EditTime] AS [EditTime],[Person].[Id] AS [Id],[Person].[Name] AS [Name] 
             * FROM [Person] AS [Person] 
             * WHERE ([Person].[Name] LIKE '%' || 'Chloe' || '%' OR [Person].[Name] LIKE 'C' || '%' OR [Person].[Name] LIKE '%' || 'o')
             */


            /* in 一个数组 */
            List<Person> persons = null;
            List<int> personIds = new List<int>() { 1, 2, 3 };
            persons = q.Where(a => personIds.Contains(a.Id)).ToList(); /* list.Contains() 方法组合就会生成 in一个数组 sql 语句 */
            /*
             * SELECT 
             *      [Person].[Gender] AS [Gender],[Person].[Age] AS [Age],[Person].[CityId] AS [CityId],[Person].[EditTime] AS [EditTime],[Person].[Id] AS [Id],[Person].[Name] AS [Name]
             * FROM [Person] AS [Person] 
             * WHERE [Person].[Id] IN (1,2,3)
             */


            /* in 子查询 */
            persons = q.Where(a => this.DbContext.Query<City>().Select(c => c.Id).ToList().Contains((int)a.CityId)).ToList(); /* IQuery<T>.ToList().Contains() 方法组合就会生成 in 子查询 sql 语句 */
            /*
             * SELECT 
             *      [Person].[Gender] AS [Gender],[Person].[Age] AS [Age],[Person].[CityId] AS [CityId],[Person].[EditTime] AS [EditTime],[Person].[Id] AS [Id],[Person].[Name] AS [Name]
             * FROM [Person] AS [Person] 
             * WHERE [Person].[CityId] IN (SELECT [City].[Id] AS [C] FROM [City] AS [City])
             */


            /* distinct 查询 */
            q.Select(a => new { a.Name }).Distinct().ToList();
            /*
             * SELECT DISTINCT [Person].[Name] AS [Name] FROM [Person] AS [Person]
             */

            ConsoleHelper.WriteLineAndReadKey();
        }

        /// <summary>
        /// 连接查询
        /// </summary>
        public virtual void JoinQuery()
        {
            var person_city_province = this.DbContext.Query<Person>()
                                     .InnerJoin<City>((person, city) => person.CityId == city.Id)
                                     .InnerJoin<Province>((person, city, province) => city.ProvinceId == province.Id);

            //查出一个用户及其隶属的城市和省份的所有信息
            var view = person_city_province.Select((person, city, province) => new { Person = person, City = city, Province = province }).Where(a => a.Person.Id > 1).ToList();
            /*
             * SELECT [Person].[Id] AS [Id],[Person].[Name] AS [Name],[Person].[Gender] AS [Gender],[Person].[Age] AS [Age],[Person].[CityId] AS [CityId],[Person].[EditTime] AS [EditTime],[City].[Id] AS [Id0],[City].[Name] AS [Name0],[City].[ProvinceId] AS [ProvinceId],[Province].[Id] AS [Id1],[Province].[Name] AS [Name1] FROM [Person] AS [Person] INNER JOIN [City] AS [City] ON [Person].[CityId] = [City].[Id] INNER JOIN [Province] AS [Province] ON [City].[ProvinceId] = [Province].[Id] WHERE [Person].[Id] > 1
             */

            //也可以只获取指定的字段信息：PersonId,PersonName,CityName,ProvinceName
            person_city_province.Select((person, city, province) => new { PersonId = person.Id, PersonName = person.Name, CityName = city.Name, ProvinceName = province.Name }).Where(a => a.PersonId > 1).ToList();
            /*
             * SELECT [Person].[Id] AS [PersonId],[Person].[Name] AS [PersonName],[City].[Name] AS [CityName],[Province].[Name] AS [ProvinceName] FROM [Person] AS [Person] INNER JOIN [City] AS [City] ON [Person].[CityId] = [City].[Id] INNER JOIN [Province] AS [Province] ON [City].[ProvinceId] = [Province].[Id] WHERE [Person].[Id] > 1
             */


            /* quick join and paging. */
            this.DbContext.JoinQuery<Person, City>((person, city) => new object[]
            {
                JoinType.LeftJoin, person.CityId == city.Id
            })
            .Select((person, city) => new { Person = person, City = city })
            .Where(a => a.Person.Id > -1)
            .OrderByDesc(a => a.Person.Age)
            .TakePage(1, 20)
            .ToList();

            this.DbContext.JoinQuery<Person, City, Province>((person, city, province) => new object[]
            {
                JoinType.LeftJoin, person.CityId == city.Id,          /* 表 Person 和 City 进行Left连接 */
                JoinType.LeftJoin, city.ProvinceId == province.Id   /* 表 City 和 Province 进行Left连接 */
            })
            .Select((person, city, province) => new { Person = person, City = city, Province = province })   /* 投影成匿名对象 */
            .Where(a => a.Person.Id > -1)     /* 进行条件过滤 */
            .OrderByDesc(a => a.Person.Age)   /* 排序 */
            .TakePage(1, 20)                /* 分页 */
            .ToList();

            ConsoleHelper.WriteLineAndReadKey();
        }

        /// <summary>
        /// 排除字段查询
        /// </summary>
        public virtual void ExcludeFieldQuery()
        {
            IQuery<Person> q = this.DbContext.Query<Person>();

            //排除指定的字段
            List<Person> persons = q.Exclude(a => a.Name).ToList();
            /*
             * 生成的 sql 语句中不包含 Name 字段
             * SELECT [Person].[Gender],[Person].[Age],[Person].[CityId],[Person].[CreateTime],[Person].[EditTime],[Person].[Id]
               FROM [Person] AS [Person] WHERE ([Person].[Id] > -100 AND [Person].[Id] > -1)
             */

            foreach (var person in persons)
            {
                Debug.Assert(person.Name == default(string));
            }


            //使用匿名类形式排除多个字段
            persons = q.Exclude(a => new { a.Name, a.Age, a.EditTime, a.CityId, a.CreateTime }).ToList();
            /*
             * SELECT [Person].[Gender],[Person].[Id] FROM [Person] AS [Person] WHERE ([Person].[Id] > -100 AND [Person].[Id] > -1)
             */
            foreach (var person in persons)
            {
                Debug.Assert(person.Name == default(string));
                Debug.Assert(person.Age == default(int?));
                Debug.Assert(person.EditTime == default(DateTime?));
                Debug.Assert(person.CityId == default(int));
                Debug.Assert(person.CreateTime == default(DateTime));
            }


            //使用 new object[] 形式排除多个字段
            persons = q.Exclude(a => new object[] { a.Name, a.Age, a.EditTime, a.CityId, a.CreateTime }).ToList();
            /*
             * SELECT [Person].[Gender],[Person].[Id] FROM [Person] AS [Person] WHERE ([Person].[Id] > -100 AND [Person].[Id] > -1)
             */

            foreach (var person in persons)
            {
                Debug.Assert(person.Name == default(string));
                Debug.Assert(person.Age == default(int?));
                Debug.Assert(person.EditTime == default(DateTime?));
                Debug.Assert(person.CityId == default(int));
                Debug.Assert(person.CreateTime == default(DateTime));
            }


            //在多表连接中排除指定的字段
            var joinResults = q.LeftJoin<City>((person, city) => person.CityId == city.Id).Select((person, city) => new { Person = person, City = city })
                .Exclude(a => new Object[] { a.Person.Name, a.City.Name }).ToList();
            /*
             * SELECT [Person].[Gender],[Person].[Age],[Person].[CityId],[Person].[CreateTime],[Person].[EditTime],[Person].[Id],[City].[ProvinceId],[City].[Id] AS [Id0] 
               FROM [Person] AS [Person] LEFT JOIN [City] AS [City] ON ([Person].[CityId] = [City].[Id] AND [City].[Id] > -2) 
               WHERE ([Person].[Id] > -100 AND [Person].[Id] > -1)
             */

            foreach (var joinResult in joinResults)
            {
                Debug.Assert(joinResult.Person.Name == default(string));
                Debug.Assert(joinResult.City.Name == default(string));
            }


            //在导航属性中排除指定的字段
            persons = q.Exclude(a => a.Name)
                .Include(a => a.City).ExcludeField(a => a.Name)
                .ThenInclude(a => a.Province).ExcludeField(a => a.Name)
                .ToList();
            /*
             * 生成的 sql 语句中不包含 Name 字段
             * SELECT [Person].[Gender],[Person].[Age],[Person].[CityId],[Person].[CreateTime],[Person].[EditTime],[Person].[Id],[City].[ProvinceId],[City].[Id] AS [Id0],[Province].[Id] AS [Id1] 
               FROM [Person] AS [Person] 
               INNER JOIN [City] AS [City] ON ([Person].[CityId] = [City].[Id] AND [City].[Id] > -2) 
               INNER JOIN [Province] AS [Province] ON ([City].[ProvinceId] = [Province].[Id] AND [Province].[Id] > -3) 
               WHERE ([Person].[Id] > -100 AND [Person].[Id] > -1)
             */

            foreach (var person in persons)
            {
                Debug.Assert(person.Name == default(string));
                Debug.Assert(person.City.Name == default(string));
                Debug.Assert(person.City.Province.Name == default(string));
            }


            var cities = this.DbContext.Query<City>().Exclude(a => a.Name)
                  .IncludeMany(a => a.Persons).ExcludeField(a => a.Name)
                  .Include(a => a.Province).ExcludeField(a => a.Name)
                  .ToList();
            /*
             * 生成的 sql 语句中不包含 Name 字段
             * SELECT [City].[ProvinceId],[City].[Id],[Province].[Id] AS [Id0],[Person].[Gender],[Person].[Age],[Person].[CityId],[Person].[CreateTime],[Person].[EditTime],[Person].[Id] AS [Id1] 
               FROM [City] AS [City] 
               INNER JOIN [Province] AS [Province] ON ([City].[ProvinceId] = [Province].[Id] AND [Province].[Id] > -3) 
               LEFT JOIN [Person] AS [Person] ON ([City].[Id] = [Person].[CityId] AND [Person].[Id] > -100 AND [Person].[Id] > -1) 
               WHERE [City].[Id] > -2 ORDER BY [City].[Id] ASC
             */

            foreach (var city in cities)
            {
                Debug.Assert(city.Name == default(string));
                Debug.Assert(city.Province.Name == default(string));

                foreach (var person in city.Persons)
                {
                    Debug.Assert(person.Name == default(string));
                }
            }

            ConsoleHelper.WriteLineAndReadKey();
        }

        /// <summary>
        /// 聚合查询
        /// </summary>
        public virtual void AggregateQuery()
        {
            IQuery<Person> q = this.DbContext.Query<Person>();

            //注：一定要 ToList()
            q.Select(a => Sql.Count()).ToList().First();
            /*
             * SELECT COUNT(1) AS [C] FROM [Person] AS [Person]
             */

            q.Select(a => new { Count = Sql.Count(), LongCount = Sql.LongCount(), Sum = Sql.Sum(a.Age), Max = Sql.Max(a.Age), Min = Sql.Min(a.Age), Average = Sql.Average(a.Age) }).ToList().First();
            /*
             * SELECT COUNT(1) AS [Count],COUNT(1) AS [LongCount],CAST(SUM([Person].[Age]) AS INTEGER) AS [Sum],MAX([Person].[Age]) AS [Max],MIN([Person].[Age]) AS [Min],CAST(AVG([Person].[Age]) AS REAL) AS [Average] FROM [Person] AS [Person]
             */

            var count = q.Count();
            /*
             * SELECT COUNT(1) AS [C] FROM [Person] AS [Person]
             */

            var longCount = q.LongCount();
            /*
             * SELECT COUNT(1) AS [C] FROM [Person] AS [Person]
             */

            var sum = q.Sum(a => a.Age);
            /*
             * SELECT CAST(SUM([Person].[Age]) AS INTEGER) AS [C] FROM [Person] AS [Person]
             */

            var max = q.Max(a => a.Age);
            /*
             * SELECT MAX([Person].[Age]) AS [C] FROM [Person] AS [Person]
             */

            var min = q.Min(a => a.Age);
            /*
             * SELECT MIN([Person].[Age]) AS [C] FROM [Person] AS [Person]
             */

            var avg = q.Average(a => a.Age);
            /*
             * SELECT CAST(AVG([Person].[Age]) AS REAL) AS [C] FROM [Person] AS [Person]
             */

            ConsoleHelper.WriteLineAndReadKey();
        }

        /// <summary>
        /// 分组查询
        /// </summary>
        public virtual void GroupQuery()
        {
            IQuery<Person> q = this.DbContext.Query<Person>();

            //用法1
            IGroupingQuery<Person> g = q.Where(a => a.Id > 0).GroupBy(a => a.Age);
            // g = g.AndBy(a => a.Id);  //多个字段分组

            g = g.Having(a => a.Age > 1 && Sql.Count() > 0);

            g.Select(a => new { a.Age, Count = Sql.Count(), Sum = Sql.Sum(a.Age), Max = Sql.Max(a.Age), Min = Sql.Min(a.Age), Avg = Sql.Average(a.Age) }).ToList();
            /*
             * SELECT [Person].[Age] AS [Age],COUNT(1) AS [Count],CAST(SUM([Person].[Age]) AS INTEGER) AS [Sum],CAST(MAX([Person].[Age]) AS INTEGER) AS [Max],CAST(MIN([Person].[Age]) AS INTEGER) AS [Min],CAST(AVG([Person].[Age]) AS REAL) AS [Avg] 
             * FROM [Person] AS [Person] WHERE [Person].[Id] > 0 
             * GROUP BY [Person].[Age] HAVING ([Person].[Age] > 1 AND COUNT(1) > 0)
             */


            //用法2
            g = q.Where(a => a.Id > 0).GroupBy(a => new { a.Age, a.Id });
            g = g.Having(a => a.Age > 1 && Sql.Count() > 0);
            g.Select(a => new { a.Age, Count = Sql.Count(), Sum = Sql.Sum(a.Age), Max = Sql.Max(a.Age), Min = Sql.Min(a.Age), Avg = Sql.Average(a.Age) }).ToList();
            /*
             * SELECT [Person].[Age] AS [Age],COUNT(1) AS [Count],CAST(SUM([Person].[Age]) AS INTEGER) AS [Sum],MAX([Person].[Age]) AS [Max],MIN([Person].[Age]) AS [Min],CAST(AVG([Person].[Age]) AS REAL) AS [Avg] 
             * FROM [Person] AS [Person] WHERE [Person].[Id] > 0
             * GROUP BY [Person].[Age],[Person].[Id] HAVING ([Person].[Age] > 1 AND COUNT(1) > 0)
             */

            ConsoleHelper.WriteLineAndReadKey();
        }

        /// <summary>
        /// 复杂查询
        /// </summary>
        public virtual void ComplexQuery()
        {
            /*
             * 支持 select * from Person where CityId in (1,2,3)    --in一个数组
             * 支持 select * from Person where CityId in (select Id from City)    --in子查询
             * 支持 select * from Person exists (select 1 from City where City.Id=Person.CityId)    --exists查询
             * 支持 select (select top 1 CityName from City where Person.CityId==City.Id) as CityName, Person.Id, Person.Name from Person    --select子查询
             * 支持 select 
             *            (select count(*) from Person where Person.CityId=City.Id) as PersonCount,     --总数
             *            (select max(Person.Age) from Person where Person.CityId=City.Id) as MaxAge,  --最大年龄
             *            (select avg(Person.Age) from Person where Person.CityId=City.Id) as AvgAge   --平均年龄
             *      from City
             *      --统计查询
             */

            IQuery<Person> personQuery = this.DbContext.Query<Person>();
            IQuery<City> cityQuery = this.DbContext.Query<City>();

            List<Person> persons = null;

            /* in 一个数组 */
            List<int> personIds = new List<int>() { 1, 2, 3 };
            persons = personQuery.Where(a => personIds.Contains(a.Id)).ToList();  /* list.Contains() 方法组合就会生成 in一个数组 sql 语句 */
            /*
             * SELECT 
             *      [Person].[Gender] AS [Gender],[Person].[Age] AS [Age],[Person].[CityId] AS [CityId],[Person].[EditTime] AS [EditTime],[Person].[Id] AS [Id],[Person].[Name] AS [Name] 
             * FROM [Person] AS [Person] 
             * WHERE [Person].[Id] IN (1,2,3)
             */


            /* in 子查询 */
            persons = personQuery.Where(a => cityQuery.Select(c => c.Id).ToList().Contains((int)a.CityId)).ToList();  /* IQuery<T>.ToList().Contains() 方法组合就会生成 in 子查询 sql 语句 */
            /*
             * SELECT 
             *      [Person].[Gender] AS [Gender],[Person].[Age] AS [Age],[Person].[CityId] AS [CityId],[Person].[EditTime] AS [EditTime],[Person].[Id] AS [Id],[Person].[Name] AS [Name] 
             * FROM [Person] AS [Person] 
             * WHERE [Person].[CityId] IN (SELECT [City].[Id] AS [C] FROM [City] AS [City])
             */


            /* IQuery<T>.Any() 方法组合就会生成 exists 子查询 sql 语句 */
            persons = personQuery.Where(a => cityQuery.Where(c => c.Id == a.CityId).Any()).ToList();
            /*
             * SELECT 
             *      [Person].[Gender] AS [Gender],[Person].[Age] AS [Age],[Person].[CityId] AS [CityId],[Person].[EditTime] AS [EditTime],[Person].[Id] AS [Id],[Person].[Name] AS [Name] 
             * FROM [Person] AS [Person] 
             * WHERE Exists (SELECT '1' AS [C] FROM [City] AS [City] WHERE [City].[Id] = [Person].[CityId])
             */


            /* select 子查询 */
            var result = personQuery.Select(a => new
            {
                CityName = cityQuery.Where(c => c.Id == a.CityId).First().Name,
                Person = a
            }).ToList();
            /*
             * SELECT 
             *      (SELECT [City].[Name] AS [C] FROM [City] AS [City] WHERE [City].[Id] = [Person].[CityId] LIMIT 1 OFFSET 0) AS [CityName],
             *      [Person].[Gender] AS [Gender],[Person].[Age] AS [Age],[Person].[CityId] AS [CityId],[Person].[EditTime] AS [EditTime],[Person].[Id] AS [Id],[Person].[Name] AS [Name] 
             * FROM [Person] AS [Person]
             */


            /* 统计 */
            var statisticsResult = cityQuery.Select(a => new
            {
                PersonCount = personQuery.Where(u => u.CityId == a.Id).Count(),
                MaxAge = personQuery.Where(u => u.CityId == a.Id).Max(c => c.Age),
                AvgAge = personQuery.Where(u => u.CityId == a.Id).Average(c => c.Age),
            }).ToList();
            /*
             * SELECT 
             *      (SELECT COUNT(1) AS [C] FROM [Person] AS [Person] WHERE [Person].[CityId] = [City].[Id]) AS [PersonCount],
             *      (SELECT MAX([Person].[Age]) AS [C] FROM [Person] AS [Person] WHERE [Person].[CityId] = [City].[Id]) AS [MaxAge],
             *      (SELECT CAST(AVG([Person].[Age]) AS REAL) AS [C] FROM [Person] AS [Person] WHERE [Person].[CityId] = [City].[Id]) AS [AvgAge] 
             * FROM [City] AS [City]
             */

            ConsoleHelper.WriteLineAndReadKey();
        }

        /// <summary>
        /// 导航属性查询
        /// </summary>
        public virtual void QueryWithNavigation()
        {
            object result = null;
            result = this.DbContext.Query<Person>().Include(a => a.City).ThenInclude(a => a.Province).ToList();

            //忽略所有过滤器
            result = this.DbContext.Query<Person>().IgnoreAllFilters().Include(a => a.City).ThenInclude(a => a.Province).ToList();

            List<City> cities = this.DbContext.Query<City>().Include(a => a.Province).IncludeMany(a => a.Persons).Filter(a => a.Age >= 18).ToList();

            cities.ForEach(city =>
            {
                city.Persons.ForEach(person =>
                {
                    Debug.Assert(person.Age >= 18);
                    Debug.Assert(person.City == null);
                });
            });

            List<Province> provinces = this.DbContext.Query<Province>().IncludeMany(a => a.Cities).ThenIncludeMany(a => a.Persons).ToList();

            provinces.ForEach(province =>
            {
                province.Cities.ForEach(city =>
                {
                    Debug.Assert(city.Province == null);

                    city.Persons.ForEach(person =>
                    {
                        Debug.Assert(person.City == null);
                    });
                });
            });

            provinces = this.DbContext.Query<Province>().IncludeMany(a => a.Cities).ThenIncludeMany(a => a.Persons).BindTwoWay().ToList(); //调用 BindTwoWay() 方法，子对象会将父对象设置到子对象对应的属性上

            provinces.ForEach(province =>
            {
                province.Cities.ForEach(city =>
                {
                    Debug.Assert(city.Province != null);

                    city.Persons.ForEach(person =>
                    {
                        Debug.Assert(person.City != null);
                    });
                });
            });

            //分页
            result = this.DbContext.Query<Province>().IncludeMany(a => a.Cities).ThenIncludeMany(a => a.Persons).Where(a => a.Id > 0).TakePage(1, 20).ToList();

            result = this.DbContext.Query<City>().IncludeMany(a => a.Persons).Filter(a => a.Age > 18).ToList();

            ConsoleHelper.WriteLineAndReadKey();
        }

        public virtual void Insert()
        {
            //lambda 插入
            //返回主键 Id
            int id = (int)this.DbContext.Insert<Person>(() => new Person() { Name = "Chloe", Age = 18, Gender = Gender.Female, CityId = 1, CreateTime = DateTime.Now, RowVersion = 0 });
            /*
             * INSERT INTO [Person]([Name],[Age],[Gender],[CityId],[CreateTime]) VALUES('Chloe',18,2,1,DATETIME('NOW','LOCALTIME'));SELECT LAST_INSERT_ROWID()
             */

            //实体插入
            Person person = new Person();
            person.Name = "Chloe";
            person.Age = 18;
            person.Gender = Gender.Female;
            person.CityId = 1;
            person.CreateTime = DateTime.Now;

            //会自动将自增 Id 设置到 person 的 Id 属性上
            person = this.DbContext.Insert(person);
            /*
             * String @P_0 = 'Chloe';
               Gender @P_1 = Female;
               Int32 @P_2 = 18;
               Int32 @P_3 = 1;
               DateTime @P_4 = '2016/8/6 22:03:42';
               INSERT INTO [Person]([Name],[Gender],[Age],[CityId],[CreateTime]) VALUES(@P_0,@P_1,@P_2,@P_3,@P_4);SELECT LAST_INSERT_ROWID()
             */


            //实体插入
            person = new Person();
            person.Name = "";
            person.Age = 18;
            person.Gender = Gender.Female;
            person.CityId = 1;
            person.CreateTime = DateTime.Now;
            person.EditTime = null;

            //设置属性值为 null 和字符串空值不参与插入
            (this.DbContext as DbContext).Options.InsertStrategy = InsertStrategy.IgnoreNull | InsertStrategy.IgnoreEmptyString;

            //插入时 Name 和 EditTime 属性不会生成到 sql 语句中
            person = this.DbContext.Insert(person);
            /*
             * Input Int32 @P_0 = 2;
               Input Int32 @P_1 = 18;
               Input Int32 @P_2 = 1;
               Input DateTime @P_3 = '2023/12/7 1:09:08';
               Input Int32 @P_4 = 0;
               INSERT INTO [Person]([Gender],[Age],[CityId],[CreateTime],[RowVersion]) VALUES(@P_0,@P_1,@P_2,@P_3,@P_4);SELECT LAST_INSERT_ROWID()
             */

            person = this.DbContext.Query<Person>().Where(a => a.Id == person.Id).First();
            Debug.Assert(person.Name == null && person.EditTime == null);

            ConsoleHelper.WriteLineAndReadKey();
        }
        public virtual void Update()
        {
            Person person = this.DbContext.Query<Person>().First();

            //lambda 更新
            this.DbContext.Update<Person>(a => a.Id == person.Id, a => new Person() { Name = a.Name, Age = a.Age + 1, Gender = Gender.Female, EditTime = DateTime.Now });
            /*
             * UPDATE [Person] SET [Name]=[Person].[Name],[Age]=([Person].[Age] + 1),[Gender]=1,[EditTime]=DATETIME('NOW','LOCALTIME') WHERE [Person].[Id] = 1
             */

            //批量更新
            //给所有女性年轻 1 岁
            this.DbContext.Update<Person>(a => a.Gender == Gender.Female, a => new Person() { Age = a.Age - 1, EditTime = DateTime.Now });
            /*
             * UPDATE [Person] SET [Age]=([Person].[Age] - 1),[EditTime]=DATETIME('NOW','LOCALTIME') WHERE [Person].[Gender] = 2
             */

            //复杂条件以及复杂 set 更新
            this.DbContext.Update<Person>(a => a.Id == person.Id && a.Id == this.DbContext.Query<City>().IgnoreAllFilters().Where(city => city.Id == a.Id).First().Id, a => new Person()
            {
                Name = this.DbContext.Query<City>().IgnoreAllFilters().Where(city => city.Id == a.Id).First().Name
            });
            /*
             * UPDATE [Person] 
               SET [Name]=(SELECT [City].[Name] AS [C] FROM [City] AS [City] WHERE [City].[Id] = [Person].[Id] LIMIT 1 OFFSET 0) 
               WHERE ([Person].[Id] = 1 AND [Person].[Id] = (SELECT [City].[Id] AS [C] FROM [City] AS [City] WHERE [City].[Id] = [Person].[Id] LIMIT 1 OFFSET 0))
             */

            //实体更新
            person = this.DbContext.Query<Person>().First();
            person.Name = "Chloe";
            person.Age = 28;
            person.Gender = Gender.Female;
            person.EditTime = DateTime.Now;

            this.DbContext.Update(person); //会更新所有映射的字段
            /*
             * Input String @P_0 = 'Chloe';
               Input Int32 @P_1 = 2;
               Input Int32 @P_2 = 28;
               Input Int32 @P_3 = 1;
               Input DateTime @P_4 = '2023/11/10 23:27:48';
               Input Int32 @P_5 = 0;
               UPDATE [Person] SET [Name]=@P_0,[Gender]=@P_1,[Age]=@P_2,[CityId]=@P_3,[EditTime]=@P_4,[RowVersion]=@P_3 WHERE ([Person].[Id] = @P_3 AND [Person].[RowVersion] = @P_5)
             */


            /*
             * 支持只更新属性值已变的属性
             */

            this.DbContext.TrackEntity(person);//在上下文中跟踪实体
            person.Name = person.Name + "1";
            this.DbContext.Update(person);//这时只会更新被修改的字段
            /*
             * Input String @P_0 = 'Chloe1';
               Input Int32 @P_1 = 2;
               Input Int32 @P_2 = 1;
               UPDATE [Person] SET [Name]=@P_0,[RowVersion]=@P_1 WHERE ([Person].[Id] = @P_2 AND [Person].[RowVersion] = @P_2)
             */

            ConsoleHelper.WriteLineAndReadKey();
        }
        public virtual void Delete()
        {
            Person person = this.DbContext.Query<Person>().First();

            this.DbContext.Delete<Person>(a => a.Id == person.Id);
            /*
             * DELETE FROM [Person] WHERE [Person].[Id] = 1
             */

            //批量删除
            //根据条件删除
            this.DbContext.Delete<Person>(a => a.Id < 0);
            /*
             * DELETE FROM [Person] WHERE [Person].[Id] < 0
             */


            //复杂条件删除
            this.DbContext.Delete<Person>(a => !this.DbContext.Query<City>().IgnoreAllFilters().Select(a => a.Id).ToList().Contains((int)a.CityId));
            /*
             * DELETE FROM [Person] WHERE NOT ([Person].[CityId] IN (SELECT [City].[Id] AS [C] FROM [City] AS [City]))
             */

            //实体删除
            person = this.DbContext.Query<Person>().First();
            this.DbContext.Delete(person);
            /*
             * Input Int32 @P_0 = 2;
               Input Int32 @P_1 = 0;
               DELETE FROM [Person] WHERE ([Person].[Id] = @P_0 AND [Person].[RowVersion] = @P_1)
             */

            ConsoleHelper.WriteLineAndReadKey(1);
        }

        /// <summary>
        /// 常用方法使用
        /// </summary>
        public virtual void Method()
        {
            IQuery<Person> q = this.DbContext.Query<Person>();

            var space = new char[] { ' ' };

            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now.AddDays(1);

            var ret = q.Select(a => new
            {
                Id = a.Id,

                String_Length = (int?)a.Name.Length,//LENGTH([Person].[Name])
                Substring = a.Name.Substring(0),//SUBSTR([Person].[Name],0 + 1)
                Substring1 = a.Name.Substring(1),//SUBSTR([Person].[Name],1 + 1)
                Substring1_2 = a.Name.Substring(1, 2),//SUBSTR([Person].[Name],1 + 1,2)
                ToLower = a.Name.ToLower(),//LOWER([Person].[Name])
                ToUpper = a.Name.ToUpper(),//UPPER([Person].[Name])
                IsNullOrEmpty = string.IsNullOrEmpty(a.Name),//CASE WHEN ([Person].[Name] IS NULL OR [Person].[Name] = '') THEN 1 ELSE 0 END = 1
                Contains = (bool?)a.Name.Contains("s"),//[Person].[Name] LIKE '%' || 's' || '%'
                StartsWith = (bool?)a.Name.StartsWith("s"),//[Person].[Name] LIKE 's' || '%'
                EndsWith = (bool?)a.Name.EndsWith("s"),//[Person].[Name] LIKE '%' || 's'
                Trim = a.Name.Trim(),//TRIM([Person].[Name])
                TrimStart = a.Name.TrimStart(space),//LTRIM([Person].[Name])
                TrimEnd = a.Name.TrimEnd(space),//RTRIM([Person].[Name])
                Replace = a.Name.Replace("l", "L"),

                DiffYears = Sql.DiffYears(startTime, endTime),//(CAST(STRFTIME('%Y',@P_0) AS INTEGER) - CAST(STRFTIME('%Y',@P_1) AS INTEGER))
                DiffMonths = Sql.DiffMonths(startTime, endTime),//((CAST(STRFTIME('%Y',@P_0) AS INTEGER) - CAST(STRFTIME('%Y',@P_1) AS INTEGER)) * 12 + (CAST(STRFTIME('%m',@P_0) AS INTEGER) - CAST(STRFTIME('%m',@P_1) AS INTEGER)))
                DiffDays = Sql.DiffDays(startTime, endTime),//CAST((JULIANDAY(@P_0) - JULIANDAY(@P_1)) AS INTEGER)
                DiffHours = Sql.DiffHours(startTime, endTime),//CAST((JULIANDAY(@P_0) - JULIANDAY(@P_1)) * 24 AS INTEGER)
                DiffMinutes = Sql.DiffMinutes(startTime, endTime),//CAST((JULIANDAY(@P_0) - JULIANDAY(@P_1)) * 1440 AS INTEGER)
                DiffSeconds = Sql.DiffSeconds(startTime, endTime),//CAST((JULIANDAY(@P_0) - JULIANDAY(@P_1)) * 86400 AS INTEGER)
                                                                  //DiffMilliseconds = Sql.DiffMilliseconds(startTime, endTime),//不支持 Millisecond
                                                                  //DiffMicroseconds = Sql.DiffMicroseconds(startTime, endTime),//不支持 Microseconds

                AddYears = startTime.AddYears(1),//DATETIME(@P_0,'+' || 1 || ' years')
                AddMonths = startTime.AddMonths(1),//DATETIME(@P_0,'+' || 1 || ' months')
                AddDays = startTime.AddDays(1),//DATETIME(@P_0,'+' || 1 || ' days')
                AddHours = startTime.AddHours(1),//DATETIME(@P_0,'+' || 1 || ' hours')
                AddMinutes = startTime.AddMinutes(2),//DATETIME(@P_0,'+' || 2 || ' minutes')
                AddSeconds = startTime.AddSeconds(120),//DATETIME(@P_0,'+' || 120 || ' seconds')
                                                       //AddMilliseconds = startTime.AddMilliseconds(2000),//不支持

                Now = DateTime.Now,//DATETIME('NOW','LOCALTIME')
                UtcNow = DateTime.UtcNow,//DATETIME()
                Today = DateTime.Today,//DATE('NOW','LOCALTIME')
                Date = DateTime.Now.Date,//DATE('NOW','LOCALTIME')
                Year = DateTime.Now.Year,//CAST(STRFTIME('%Y',DATETIME('NOW','LOCALTIME')) AS INTEGER)
                Month = DateTime.Now.Month,//CAST(STRFTIME('%m',DATETIME('NOW','LOCALTIME')) AS INTEGER)
                Day = DateTime.Now.Day,//CAST(STRFTIME('%d',DATETIME('NOW','LOCALTIME')) AS INTEGER)
                Hour = DateTime.Now.Hour,//CAST(STRFTIME('%H',DATETIME('NOW','LOCALTIME')) AS INTEGER)
                Minute = DateTime.Now.Minute,//CAST(STRFTIME('%M',DATETIME('NOW','LOCALTIME')) AS INTEGER)
                Second = DateTime.Now.Second,//CAST(STRFTIME('%S',DATETIME('NOW','LOCALTIME')) AS INTEGER)
                Millisecond = DateTime.Now.Millisecond,//@P_2 直接计算 DateTime.Now.Millisecond 的值 
                DayOfWeek = DateTime.Now.DayOfWeek,//CAST(STRFTIME('%w',DATETIME('NOW','LOCALTIME')) AS INTEGER)

                Byte_Parse = byte.Parse("1"),//CAST('1' AS INTEGER)
                Int_Parse = int.Parse("1"),//CAST('1' AS INTEGER)
                Int16_Parse = Int16.Parse("11"),//CAST('11' AS INTEGER)
                Long_Parse = long.Parse("2"),//CAST('2' AS INTEGER)
                Double_Parse = double.Parse("3.1"),//CAST('3.1' AS REAL)
                Float_Parse = float.Parse("4.1"),//CAST('4.1' AS REAL)
                                                 //Decimal_Parse = decimal.Parse("5"),//不支持
                                                 //Guid_Parse = Guid.Parse("D544BC4C-739E-4CD3-A3D3-7BF803FCE179"),//不支持 'D544BC4C-739E-4CD3-A3D3-7BF803FCE179'

                Bool_Parse = bool.Parse("1"),//CAST('1' AS INTEGER)
                DateTime_Parse = DateTime.Parse("2014-01-01"),//DATETIME('2014-01-01')

                B = a.Age == null ? false : a.Age > 1, //三元表达式
                CaseWhen = Case.When(a.Id > 100).Then(1).Else(0) //case when
            }).ToList();

            ConsoleHelper.WriteLineAndReadKey();
        }

        /// <summary>
        /// 执行原生sql
        /// </summary>
        public virtual void ExecuteCommandText()
        {
            List<Person> persons = this.DbContext.SqlQuery<Person>("select * from Person where Age > @age", DbParam.Create("@age", 1)).ToList();

            int rowsAffected = this.DbContext.Session.ExecuteNonQuery("update Person set name=@name where Id = 1", DbParam.Create("@name", "Chloe"));

            /* 
             * 执行存储过程:
             * Person person = this.DbContext.SqlQuery<Person>("Proc_GetPerson", CommandType.StoredProcedure, DbParam.Create("@id", 1)).FirstOrDefault();
             * rowsAffected = this.DbContext.Session.ExecuteNonQuery("Proc_UpdatePersonName", CommandType.StoredProcedure, DbParam.Create("@name", "Chloe"));
             */

            ConsoleHelper.WriteLineAndReadKey();
        }

        /// <summary>
        /// 事务
        /// </summary>
        public virtual void DoWithTransaction()
        {
            /*--------------1--------------*/
            using (ITransientTransaction tran = this.DbContext.BeginTransaction())
            {
                /* do some things here */
                this.DbContext.Update<Person>(a => a.Id == 1, a => new Person() { Name = a.Name, Age = a.Age + 1, Gender = Gender.Male, EditTime = DateTime.Now });
                this.DbContext.Delete<Person>(a => a.Id == 1024);

                tran.Commit();
            }
            ConsoleHelper.WriteLineAndReadKey();



            /*--------------2--------------*/
            this.DbContext.UseTransaction(() =>
            {
                this.DbContext.Update<Person>(a => a.Id == 1, a => new Person() { Name = a.Name, Age = a.Age + 1, Gender = Gender.Male, EditTime = DateTime.Now });
                this.DbContext.Delete<Person>(a => a.Id == 1024);
            });
            ConsoleHelper.WriteLineAndReadKey();



            /*--------------3--------------*/
            this.DbContext.Session.BeginTransaction();
            try
            {
                /* do some things here */
                this.DbContext.Update<Person>(a => a.Id == 1, a => new Person() { Name = a.Name, Age = a.Age + 1, Gender = Gender.Male, EditTime = DateTime.Now });
                this.DbContext.Delete<Person>(a => a.Id == 1024);

                this.DbContext.Session.CommitTransaction();
            }
            catch
            {
                if (this.DbContext.Session.IsInTransaction)
                    this.DbContext.Session.RollbackTransaction();
                throw;
            }
            ConsoleHelper.WriteLineAndReadKey();
        }

    }
}
