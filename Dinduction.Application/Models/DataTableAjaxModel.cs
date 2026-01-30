namespace Dinduction.Application.Models;

public class DataTableAjaxPostModel
{
    // Properties are not capital due to json mapping from DataTables
    public int draw { get; set; }
    public int start { get; set; }
    public int length { get; set; }

    public List<Column> columns { get; set; } = new();
    public Search search { get; set; } = new();
    public List<Order> order { get; set; } = new();
}

public class Column
{
    public string? data { get; set; }
    public string? name { get; set; }
    public bool searchable { get; set; }
    public bool orderable { get; set; }
    public Search search { get; set; } = new();
}

public class Search
{
    public string? value { get; set; }
    public string? regex { get; set; }
}

public class Order
{
    public int column { get; set; }
    public string? dir { get; set; }
}