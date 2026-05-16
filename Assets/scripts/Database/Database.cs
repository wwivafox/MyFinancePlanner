using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SQLite4Unity3d;

public class Database
{
    private SQLiteConnection connection;

    void Start()
    {
        
    }

    private void SeedData()
    {
        if (connection.Table<Category>().Count() == 0)
        {
            connection.Insert(new Category { Name = "Еда", IconName = "Еда", IsIncome = false });
            connection.Insert(new Category { Name = "Транспорт", IconName = "Транспорт", IsIncome = false });
            connection.Insert(new Category { Name = "Медицина", IconName = "Медицина", IsIncome = false });
            connection.Insert(new Category { Name = "Одежда", IconName = "Одежда", IsIncome = false });
            connection.Insert(new Category { Name = "Спорт", IconName = "Спорт", IsIncome = false });
            connection.Insert(new Category { Name = "Быт", IconName = "Быт", IsIncome = false });
            connection.Insert(new Category { Name = "Красота", IconName = "Красота", IsIncome = false });
            connection.Insert(new Category { Name = "Жилье", IconName = "Жилье", IsIncome = false });
            connection.Insert(new Category { Name = "Связь", IconName = "Связь", IsIncome = false });
            connection.Insert(new Category { Name = "Развлечения", IconName = "Развлечения", IsIncome = false });
            connection.Insert(new Category { Name = "Другое", IconName = "Другое", IsIncome = false });
            // Доходы
            connection.Insert(new Category { Name = "Зарплата", IconName = "Работа", IsIncome = true });
            connection.Insert(new Category { Name = "Долг", IconName = "Долг", IsIncome = true });
            connection.Insert(new Category { Name = "Премия", IconName = "Медаль", IsIncome = true });
            connection.Insert(new Category { Name = "Кэшбэк", IconName = "Деньги2", IsIncome = true });
            connection.Insert(new Category { Name = "Инвестиции", IconName = "Проценты", IsIncome = true });
            connection.Insert(new Category { Name = "Подработка", IconName = "Деньги", IsIncome = true });
            connection.Insert(new Category { Name = "Долг", IconName = "Долг", IsIncome = true });
            connection.Insert(new Category { Name = "Подарок", IconName = "ДеньгиПодарок", IsIncome = true });
           
        }


        if (connection.Table<FlowerType>().Count() == 0)
        {
            connection.Insert(new FlowerType
            {
                Name = "Роза",
                Stage1 = "rose_1",
                Stage2 = "rose_2",
                Stage3 = "rose_3",
                Stage4 = "rose_4",
                Stage5 = "rose_5",
                Stage6 = "rose_6",
                Stage7 = "rose_7",
                Stage8 = "rose_8",
                Stage9 = "rose_9"
            });

            connection.Insert(new FlowerType
            {
                Name = "Тюльпан",
                Stage1 = "tulip_1",
                Stage2 = "tulip_2",
                Stage3 = "tulip_3",
                Stage4 = "tulip_4",
                Stage5 = "tulip_5",
                Stage6 = "rose_6",
                Stage7 = "rose_7",
                Stage8 = "rose_8",
                Stage9 = "rose_9"
            });

            connection.Insert(new FlowerType
            {
                Name = "Пион",
                Stage1 = "tulip_1",
                Stage2 = "tulip_2",
                Stage3 = "tulip_3",
                Stage4 = "tulip_4",
                Stage5 = "tulip_5",
                Stage6 = "rose_6",
                Stage7 = "rose_7",
                Stage8 = "rose_8",
                Stage9 = "rose_9"
            });

            connection.Insert(new FlowerType
            {
                Name = "Лилия",
                Stage1 = "tulip_1",
                Stage2 = "tulip_2",
                Stage3 = "tulip_3",
                Stage4 = "tulip_4",
                Stage5 = "tulip_5",
                Stage6 = "rose_6",
                Stage7 = "rose_7",
                Stage8 = "rose_8",
                Stage9 = "rose_9"
            });

            connection.Insert(new FlowerType
            {
                Name = "Ромашка",
                Stage1 = "tulip_1",
                Stage2 = "tulip_2",
                Stage3 = "tulip_3",
                Stage4 = "tulip_4",
                Stage5 = "tulip_5",
                Stage6 = "rose_6",
                Stage7 = "rose_7",
                Stage8 = "rose_8",
                Stage9 = "rose_9"
            });

            connection.Insert(new FlowerType
            {
                Name = "Герберы",
                Stage1 = "tulip_1",
                Stage2 = "tulip_2",
                Stage3 = "tulip_3",
                Stage4 = "tulip_4",
                Stage5 = "tulip_5",
                Stage6 = "rose_6",
                Stage7 = "rose_7",
                Stage8 = "rose_8",
                Stage9 = "rose_9"
            });

            connection.Insert(new FlowerType
            {
                Name = "Калла",
                Stage1 = "tulip_1",
                Stage2 = "tulip_2",
                Stage3 = "tulip_3",
                Stage4 = "tulip_4",
                Stage5 = "tulip_5",
                Stage6 = "rose_6",
                Stage7 = "rose_7",
                Stage8 = "rose_8",
                Stage9 = "rose_9"
            });

            connection.Insert(new FlowerType
            {
                Name = "Васильки",
                Stage1 = "tulip_1",
                Stage2 = "tulip_2",
                Stage3 = "tulip_3",
                Stage4 = "tulip_4",
                Stage5 = "tulip_5",
                Stage6 = "rose_6",
                Stage7 = "rose_7",
                Stage8 = "rose_8",
                Stage9 = "rose_9"
            });

            connection.Insert(new FlowerType
            {
                Name = "Орхидея",
                Stage1 = "tulip_1",
                Stage2 = "tulip_2",
                Stage3 = "tulip_3",
                Stage4 = "tulip_4",
                Stage5 = "tulip_5",
                Stage6 = "rose_6",
                Stage7 = "rose_7",
                Stage8 = "rose_8",
                Stage9 = "rose_9"
            });

            connection.Insert(new FlowerType
            {
                Name = "Астра",
                Stage1 = "tulip_1",
                Stage2 = "tulip_2",
                Stage3 = "tulip_3",
                Stage4 = "tulip_4",
                Stage5 = "tulip_5",
                Stage6 = "rose_6",
                Stage7 = "rose_7",
                Stage8 = "rose_8",
                Stage9 = "rose_9"
            });

            connection.Insert(new FlowerType
            {
                Name = "Тюльпан",
                Stage1 = "tulip_1",
                Stage2 = "tulip_2",
                Stage3 = "tulip_3",
                Stage4 = "tulip_4",
                Stage5 = "tulip_5",
                Stage6 = "rose_6",
                Stage7 = "rose_7",
                Stage8 = "rose_8",
                Stage9 = "rose_9"
            });

            connection.Insert(new FlowerType
            {
                Name = "Георгин",
                Stage1 = "tulip_1",
                Stage2 = "tulip_2",
                Stage3 = "tulip_3",
                Stage4 = "tulip_4",
                Stage5 = "tulip_5",
                Stage6 = "rose_6",
                Stage7 = "rose_7",
                Stage8 = "rose_8",
                Stage9 = "rose_9"
            });

            connection.Insert(new FlowerType
            {
                Name = "Ирис",
                Stage1 = "tulip_1",
                Stage2 = "tulip_2",
                Stage3 = "tulip_3",
                Stage4 = "tulip_4",
                Stage5 = "tulip_5",
                Stage6 = "rose_6",
                Stage7 = "rose_7",
                Stage8 = "rose_8",
                Stage9 = "rose_9"
            });

            connection.Insert(new FlowerType
            {
                Name = "Подсолнух",
                Stage1 = "tulip_1",
                Stage2 = "tulip_2",
                Stage3 = "tulip_3",
                Stage4 = "tulip_4",
                Stage5 = "tulip_5",
                Stage6 = "rose_6",
                Stage7 = "rose_7",
                Stage8 = "rose_8",
                Stage9 = "rose_9"
            });

            connection.Insert(new FlowerType
            {
                Name = "Геоцин",
                Stage1 = "tulip_1",
                Stage2 = "tulip_2",
                Stage3 = "tulip_3",
                Stage4 = "tulip_4",
                Stage5 = "tulip_5",
                Stage6 = "rose_6",
                Stage7 = "rose_7",
                Stage8 = "rose_8",
                Stage9 = "rose_9"
            });

            connection.Insert(new FlowerType
            {
                Name = "Мак",
                Stage1 = "tulip_1",
                Stage2 = "tulip_2",
                Stage3 = "tulip_3",
                Stage4 = "tulip_4",
                Stage5 = "tulip_5",
                Stage6 = "rose_6",
                Stage7 = "rose_7",
                Stage8 = "rose_8",
                Stage9 = "rose_9"
            });

            connection.Insert(new FlowerType
            {
                Name = "Ранюкулюс",
                Stage1 = "tulip_1",
                Stage2 = "tulip_2",
                Stage3 = "tulip_3",
                Stage4 = "tulip_4",
                Stage5 = "tulip_5",
                Stage6 = "rose_6",
                Stage7 = "rose_7",
                Stage8 = "rose_8",
                Stage9 = "rose_9"
            });
        }
    }

    public Database()
    {
        string dbName = "finance.db";
        string dbPath = Path.Combine(Application.persistentDataPath, dbName);

        if (!File.Exists(dbPath))
        {
            File.Create(dbPath).Dispose();
           // Debug.Log("Создан новый файл базы: " + dbPath);
        }

    
        connection = new SQLiteConnection(
            dbPath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create
        );

        connection.CreateTable<Account>();
        connection.CreateTable<Category>();
        connection.CreateTable<Transaction>();
        connection.CreateTable<FlowerType>();
        connection.CreateTable<Goal>();
        connection.CreateTable<CompletedGoal>();

        SeedData();

        // Debug.Log("Все таблицы успешно созданы или уже существуют.");
    }

    public SQLiteConnection GetConnection()
    {
        return connection;
    }
    void Update()
    {
        
    }
}
