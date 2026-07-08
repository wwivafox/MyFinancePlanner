using SQLite4Unity3d;

public class Account
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Currency { get; set; }
    public float StartAmount { get; set; }
    public string ColorHex { get; set; }
}

public class Category
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
    public string IconName { get; set; }
    public bool IsIncome { get; set; }
}

public class Transaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public float Amount { get; set; }
    public string Date { get; set; }
    public string Description { get; set; }
}

public class FlowerType
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Stage1 { get; set; }
    public string Stage2 { get; set; }
    public string Stage3 { get; set; }
    public string Stage4 { get; set; }
    public string Stage5 { get; set; }
    public string Stage6 { get; set; }
    public string Stage7 { get; set; }
    public string Stage8 { get; set; }
    public string Stage9 { get; set; }
}


public class Goal
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Title { get; set; }
    public float TargetAmount { get; set; }
    public float CurrentAmount { get; set; }

    public int FlowerTypeId { get; set; }

    public string CreatedDate { get; set; }
    public string Deadline { get; set; }

    public int PageIndex { get; set; }   
    public int SlotIndex { get; set; }   
}



public class CompletedGoal
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Title { get; set; }
    public int FlowerTypeId { get; set; }
    public float FinalAmount { get; set; }
    public string CompletedDate { get; set; }
}
