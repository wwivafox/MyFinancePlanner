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
        conn = DatabaseManager.Instance.DB.GetConnection();
    }

    // Для счетов

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

    // Для категорий

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

    // Транзакции

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

    public void UpdateTransaction(Transaction t)
    {
        conn.Update(t);
    }




    // Тип цветов
    public List<FlowerType> GetFlowerTypes()
    {
        return conn.Table<FlowerType>().ToList();
    }

    public FlowerType GetFlowerType(int id)
    {
        return conn.Table<FlowerType>().FirstOrDefault(f => f.Id == id);
    }

    // Цели

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

    // Выполненнве цели
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

    void Update()
    {
        
    }
}
