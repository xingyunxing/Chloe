using System;
using System.Collections.Generic;

namespace ChloeDemo
{
    public class TestData
    {
        public static List<Province> GetMockProvinces()
        {
            List<Province> provinces = new List<Province>();

            provinces.Add(CreateProvince("广东", "广州", "深圳", "东莞"));
            provinces.Add(CreateProvince("广西", "南宁", "柳州", "桂林", "河池"));
            provinces.Add(CreateProvince("湖南", "长沙", "衡阳", "张家界"));

            return provinces;
        }

        static int ProvinceIdSeq = 1;
        public static Province CreateProvince(string provinceName, params string[] cityNames)
        {
            Province province = new Province();
            province.Id = ProvinceIdSeq++;
            province.Name = provinceName;

            cityNames = cityNames ?? new string[0];
            foreach (var cityName in cityNames)
            {
                province.Cities.Add(TestData.CreateCity(cityName, province.Id));
            }

            return province;
        }

        static int CityIdSeq = 1;
        public static City CreateCity(string cityName, int provinceId)
        {
            City city = new City();
            city.Id = CityIdSeq++;
            city.Name = cityName;
            city.ProvinceId = provinceId;

            city.Persons.Add(new Person()
            {
                Name = $"{city.Name}-张三",
                Age = 30,
                Gender = Gender.Male,
                CreateTime = DateTime.Now,
                Profile = new PersonProfile()
                {
                    IdNumber = "452723197110211024",
                    BirthDay = new DateTime(1971, 10, 21),
                    Annexes = new List<ProfileAnnex>()
                    {
                        new ProfileAnnex() { FilePath = "1/2/3.txt" },
                        new ProfileAnnex() { FilePath = "a/b/c.txt" }
                    }
                }
            });
            city.Persons.Add(new Person()
            {
                Name = $"{city.Name}-李四",
                Age = 31,
                Gender = Gender.Male,
                CreateTime = DateTime.Now,
                Profile = new PersonProfile()
                {
                    IdNumber = "452723197110221024",
                    BirthDay = new DateTime(1971, 10, 22),
                    Annexes = new List<ProfileAnnex>()
                    {
                        new ProfileAnnex() { FilePath = "1/2/3.jpg" },
                        new ProfileAnnex() { FilePath = "a/b/c.pdf" }
                    }
                }
            });
            city.Persons.Add(new Person()
            {
                Name = $"{city.Name}-Chloe",
                Age = 18,
                Gender = Gender.Female,
                CreateTime = DateTime.Now,
                Profile = new PersonProfile()
                {
                    IdNumber = "452723197110231024",
                    BirthDay = new DateTime(1971, 10, 23),
                    Annexes = new List<ProfileAnnex>()
                    {
                        new ProfileAnnex() { FilePath = "1/2/3.jpg" },
                        new ProfileAnnex() { FilePath = "a/b/c.pdf" }
                    }
                }
            });
            city.Persons.Add(new Person()
            {
                Name = $"{city.Name}-东方不败",
                CreateTime = DateTime.Now,
                Profile = new PersonProfile()
                {
                    IdNumber = "452723197110241024",
                    BirthDay = new DateTime(1971, 10, 24),
                    Annexes = new List<ProfileAnnex>()
                    {
                        new ProfileAnnex() { FilePath = "1/2/3.jpg" },
                        new ProfileAnnex() { FilePath = "a/b/c.pdf" }
                    }
                }
            });

            return city;
        }
    }
}
