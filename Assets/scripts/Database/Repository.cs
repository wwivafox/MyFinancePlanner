using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SQLite4Unity3d;
using System.Linq;

public class Repository
{
    private SQLiteConnection conn;

    public Repository()
    {
        if (DatabaseManager.Instance == null)
            Debug.LogError("DatabaseManager.Instance == null Ч он не успел инициализироватьс€!");

        if (DatabaseManager.Instance.DB == null)
            Debug.LogError("DatabaseManager.Instance.DB == null Ч база не создана!");

        conn = DatabaseManager.Instance.DB.GetConnection();
    }

    // ƒл€ счетов

    public void AddAccount(Account acc)
    {
        conn.Insert(acc);
    }

    public List<Account> GetAccounts()
    {
        return conn.Table<Account>().ToList();
    }

    public Account GetAccountById(int id)
    {
        return conn.Table<Account>().FirstOrDefault(a => a.Id == id);
    }


    public void UpdateAccount(Account acc)
    {
        conn.Update(acc);
    }

    public void DeleteAccount(int id)
    {
        conn.Delete<Account>(id);
    }

    // ƒл€ категорий

    public void AddCategory(Category cat)
    {
        conn.Insert(cat);
    }

    public List<Category> GetCategories()
    {
        return conn.Table<Category>().ToList();
    }

    public Category GetCategoryById(int id)
    {
        return conn.Table<Category>().FirstOrDefault(c => c.Id == id);
    }


    public void DeleteCategory(int id)
    {
        conn.Delete<Category>(id);
    }

    // “ранзакции

    public void AddTransaction(Transaction t)
    {
        conn.Insert(t);
    }

    public List<Transaction> GetTransactions()
    {
        return conn.Table<Transaction>().ToList();
    }


    public List<Transaction> GetTransactionsByAccount(int accountId)
    {
        return conn.Table<Transaction>()
                   .Where(t => t.AccountId == accountId)
                   .ToList();
    }

    public List<Transaction> GetTransactionsByDate(string date)
    {
        return conn.Table<Transaction>()
                   .Where(t => t.Date == date)
                   .ToList();
    }

    public void DeleteTransaction(int id)
    {
        conn.Delete<Transaction>(id);
    }



    // “ип цветов
    public List<FlowerType> GetFlowerTypes()
    {
        return conn.Table<FlowerType>().ToList();
    }

    public FlowerType GetFlowerType(int id)
    {
        return conn.Table<FlowerType>().FirstOrDefault(f => f.Id == id);
    }

    // ÷ели

    public void AddGoal(Goal g)
    {
        conn.Insert(g);
    }

    public List<Goal> GetGoals()
    {
        return conn.Table<Goal>().ToList();
    }

    public void UpdateGoal(Goal g)
    {
        conn.Update(g);
    }

    public void DeleteGoal(int id)
    {
        conn.Delete<Goal>(id);
    }

    // ¬ыполненнве цели
    public void AddCompletedGoal(CompletedGoal g)
    {
        conn.Insert(g);
    }

    public List<CompletedGoal> GetCompletedGoals()
    {
        return conn.Table<CompletedGoal>().ToList();
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
