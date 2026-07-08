using System.IO;
using UnityEngine;
using SQLite4Unity3d;

public class Database
{
    private SQLiteConnection connection;

    public SQLiteConnection GetConnection() => connection;

    public void Init()
    {
        string dbName = "finance1.db";
        string dbPath = Path.Combine(Application.persistentDataPath, dbName);

        Debug.Log($"DB Init, path = {dbPath}");

        if (!File.Exists(dbPath))
        {
            Debug.Log("DB missing — creating new empty DB");
        }



        try
        {
            connection = new SQLiteConnection(
                dbPath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create
            );

            Debug.Log("SQLite opened successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("SQLite OPEN ERROR: " + ex);
            return;
        }

        try
        {
            connection.CreateTable<Account>();
            connection.CreateTable<Category>();
            connection.CreateTable<Transaction>();
            connection.CreateTable<FlowerType>();
            connection.CreateTable<Goal>();
            connection.CreateTable<CompletedGoal>();

            Debug.Log("Tables created");

            SeedData();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("TABLE/SEED ERROR: " + ex);
        }
    }

//    private void CopyDatabaseFromStreamingAssets(string targetPath)
//    {
//        string sourcePath = Path.Combine(Application.streamingAssetsPath, "finance1.db");

//#if UNITY_ANDROID
//        var request = UnityEngine.Networking.UnityWebRequest.Get(sourcePath);
//        request.SendWebRequest();

//        while (!request.isDone) {}

//        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
//        {
//            File.WriteAllBytes(targetPath, request.downloadHandler.data);
//            Debug.Log("DB copied from StreamingAssets (Android)");
//        }
//        else
//        {
//            Debug.LogError("DB copy error: " + request.error);
//        }
//#else
//        File.Copy(sourcePath, targetPath, true);
//        Debug.Log("DB copied from StreamingAssets (Editor/PC)");
//#endif
//    }

    private void SeedData()
    {
        try
        {
            int catCount = connection.Table<Category>().Count();
            int flowerCount = connection.Table<FlowerType>().Count();

            Debug.Log($"SeedData start. Categories={catCount}, Flowers={flowerCount}");

            if (catCount == 0)
            {
                InsertDefaultCategories();
                Debug.Log("Categories inserted");
            }

            if (flowerCount == 0)
            {
                InsertDefaultFlowers();
                Debug.Log("FlowerTypes inserted");
            }

            Debug.Log("SeedData finished");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("SeedData FATAL ERROR: " + ex);
        }
    }

    private void InsertDefaultCategories()
    {
        connection.RunInTransaction(() =>
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

            connection.Insert(new Category { Name = "Зарплата", IconName = "Работа", IsIncome = true });
            connection.Insert(new Category { Name = "Долг", IconName = "Долг", IsIncome = true });
            connection.Insert(new Category { Name = "Премия", IconName = "Медаль", IsIncome = true });
            connection.Insert(new Category { Name = "Кэшбэк", IconName = "Деньги2", IsIncome = true });
            connection.Insert(new Category { Name = "Инвестиции", IconName = "Проценты", IsIncome = true });
            connection.Insert(new Category { Name = "Подработка", IconName = "Деньги", IsIncome = true });
            connection.Insert(new Category { Name = "Подарок", IconName = "ДеньгиПодарок", IsIncome = true });
        });
    }

    private void InsertDefaultFlowers()
    {
        connection.RunInTransaction(() =>
        {
            connection.Insert(new FlowerType { Name = "Роза", Stage1 = "Роза1", Stage2 = "Роза2", Stage3 = "Роза3", Stage4 = "Роза4", Stage5 = "Роза5", Stage6 = "Роза6", Stage7 = "Роза7", Stage8 = "Роза8", Stage9 = "Роза9" });
            connection.Insert(new FlowerType { Name = "Тюльпан", Stage1 = "Тюльпан1", Stage2 = "Тюльпан2", Stage3 = "Тюльпан3", Stage4 = "Тюльпан4", Stage5 = "Тюльпан5", Stage6 = "Тюльпан6", Stage7 = "Тюльпан7", Stage8 = "Тюльпан8", Stage9 = "Тюльпан9" });
            connection.Insert(new FlowerType { Name = "Пион", Stage1 = "Пион1", Stage2 = "Пион2", Stage3 = "Пион3", Stage4 = "Пион4", Stage5 = "Пион5", Stage6 = "Пион6" });
            connection.Insert(new FlowerType { Name = "Лилия", Stage1 = "Лилия1", Stage2 = "Лилия2", Stage3 = "Лилия3", Stage4 = "Лилия4", Stage5 = "Лилия5", Stage6 = "Лилия6" });
            connection.Insert(new FlowerType { Name = "Ромашка", Stage1 = "Ромашка1", Stage2 = "Ромашка2", Stage3 = "Ромашка3", Stage4 = "Ромашка4", Stage5 = "Ромашка5", Stage6 = "Ромашка6" });
            connection.Insert(new FlowerType { Name = "Гербера", Stage1 = "Гербера1", Stage2 = "Гербера2", Stage3 = "Гербера3", Stage4 = "Гербера4", Stage5 = "Гербера5", Stage6 = "Гербера6" });
            connection.Insert(new FlowerType { Name = "Калла", Stage1 = "Калла1", Stage2 = "Калла2", Stage3 = "Калла3", Stage4 = "Калла4", Stage5 = "Калла5", Stage6 = "Калла6" });
            connection.Insert(new FlowerType { Name = "Васильки", Stage1 = "Васильки1", Stage2 = "Васильки2", Stage3 = "Васильки3", Stage4 = "Васильки4", Stage5 = "Васильки5", Stage6 = "Васильки6" });
            connection.Insert(new FlowerType { Name = "Орхидея", Stage1 = "Орхидея1", Stage2 = "Орхидея2", Stage3 = "Орхидея3", Stage4 = "Орхидея4", Stage5 = "Орхидея5", Stage6 = "Орхидея6" });
            connection.Insert(new FlowerType { Name = "Астра", Stage1 = "Астра1", Stage2 = "Астра2", Stage3 = "Астра3", Stage4 = "Астра4", Stage5 = "Астра5", Stage6 = "Астра6" });
            connection.Insert(new FlowerType { Name = "Гладиолус", Stage1 = "Гладиолус1", Stage2 = "Гладиолус2", Stage3 = "Гладиолус3", Stage4 = "Гладиолус4", Stage5 = "Гладиолус5", Stage6 = "Гладиолус6" });
            connection.Insert(new FlowerType { Name = "Хризантема", Stage1 = "Хризантема1", Stage2 = "Хризантема2", Stage3 = "Хризантема3", Stage4 = "Хризантема4", Stage5 = "Хризантема5", Stage6 = "Хризантема6" });
            connection.Insert(new FlowerType { Name = "Ирис", Stage1 = "Ирис1", Stage2 = "Ирис2", Stage3 = "Ирис3", Stage4 = "Ирис4", Stage5 = "Ирис5", Stage6 = "Ирис6" });
            connection.Insert(new FlowerType { Name = "Подсолнух", Stage1 = "Подсолнух1", Stage2 = "Подсолнух2", Stage3 = "Подсолнух3", Stage4 = "Подсолнух4", Stage5 = "Подсолнух5", Stage6 = "Подсолнух6" });
            connection.Insert(new FlowerType { Name = "Гиацинт", Stage1 = "Гиацинт1", Stage2 = "Гиацинт2", Stage3 = "Гиацинт3", Stage4 = "Гиацинт4", Stage5 = "Гиацинт5", Stage6 = "Гиацинт6" });
            connection.Insert(new FlowerType { Name = "Мак", Stage1 = "Мак1", Stage2 = "Мак2", Stage3 = "Мак3", Stage4 = "Мак4", Stage5 = "Мак5", Stage6 = "Мак6" });
            connection.Insert(new FlowerType { Name = "Ранукулюс", Stage1 = "Ранукулюс1", Stage2 = "Ранукулюс2", Stage3 = "Ранукулюс3", Stage4 = "Ранукулюс4", Stage5 = "Ранукулюс5", Stage6 = "Ранукулюс6" });
        });
    }
}
