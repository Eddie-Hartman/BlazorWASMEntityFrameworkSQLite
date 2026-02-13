namespace PWAExampleData.Data;

public class ToDo
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Notes { get; set; } = "";

    // 1 = Low, 2 = Medium, 3 = High
    public int Priority { get; set; } = 2;

    public bool IsCompleted { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? DueDateUtc { get; set; }
}
