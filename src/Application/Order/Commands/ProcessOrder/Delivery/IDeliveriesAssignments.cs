public interface IDeliveriesAssignments
{
    DeliveryAssignmentJob CreateAssignmentJob(int orderId);
    DeliveryAssignmentJob? GetAssignmentJob(int orderId);
    void RemoveAssignmentJob(int orderId);
}
