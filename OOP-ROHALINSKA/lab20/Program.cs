using System;
using System.Collections.Generic;

// ===== Order та статус =====
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Order(int id, string customerName, decimal totalAmount)
    {
        Id = id;
        CustomerName = customerName;
        TotalAmount = totalAmount;
        Status = OrderStatus.New;
    }
}

public enum OrderStatus
{
    New,
    PendingValidation,
    Processed,
    Cancelled
}

// ===== Інтерфейси (SRP) =====
public interface IOrderValidator
{
    bool IsValid(Order order);
}

public interface IOrderRepository
{
    void Save(Order order);
    Order GetById(int id);
}

public interface IEmailService
{
    void SendOrderConfirmation(Order order);
}

// ===== Реалізації =====
public class OrderValidator : IOrderValidator
{
    public bool IsValid(Order order)
    {
        return order.TotalAmount > 0;
    }
}

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly Dictionary<int, Order> orders = new();

    public void Save(Order order)
    {
        orders[order.Id] = order;
        Console.WriteLine("Замовлення збережено в базу даних");
    }

    public Order GetById(int id)
    {
        return orders.ContainsKey(id) ? orders[id] : null;
    }
}

public class ConsoleEmailService : IEmailService
{
    public void SendOrderConfirmation(Order order)
    {
        Console.WriteLine($"Email надіслано клієнту {order.CustomerName}");
    }
}

// ===== Сервіс-координатор =====
public class OrderService
{
    private readonly IOrderValidator validator;
    private readonly IOrderRepository repository;
    private readonly IEmailService emailService;

    public OrderService(
        IOrderValidator validator,
        IOrderRepository repository,
        IEmailService emailService)
    {
        this.validator = validator;
        this.repository = repository;
        this.emailService = emailService;
    }

    public void ProcessOrder(Order order)
    {
        Console.WriteLine($"\nОбробка замовлення #{order.Id}");
        order.Status = OrderStatus.PendingValidation;

        if (!validator.IsValid(order))
        {
            order.Status = OrderStatus.Cancelled;
            Console.WriteLine("❌ Замовлення невалідне");
            return;
        }

        repository.Save(order);
        emailService.SendOrderConfirmation(order);
        order.Status = OrderStatus.Processed;

        Console.WriteLine("✅ Замовлення успішно оброблено");
    }
}

// ===== Main =====
class Program
{
    static void Main()
    {
        IOrderValidator validator = new OrderValidator();
        IOrderRepository repository = new InMemoryOrderRepository();
        IEmailService emailService = new ConsoleEmailService();

        OrderService orderService = new OrderService(
            validator,
            repository,
            emailService);

        Order validOrder = new Order(1, "Олександра", 1500);
        Order invalidOrder = new Order(2, "Іван", -200);

        orderService.ProcessOrder(validOrder);
        orderService.ProcessOrder(invalidOrder);

        Console.ReadLine();
    }
}
