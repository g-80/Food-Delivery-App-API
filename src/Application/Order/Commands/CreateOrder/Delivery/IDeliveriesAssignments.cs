public interface IDeliveriesAssignments
{
    DeliveryAssignmentJob GetOrCreateAssignmentJob(int orderId);
    DeliveryAssignmentJob GetAssignmentJob(int orderId);
    void RemoveAssignmentJob(int orderId);
}
