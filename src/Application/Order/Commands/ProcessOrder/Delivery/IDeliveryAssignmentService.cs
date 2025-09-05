public interface IDeliveryAssignmentService
{
    Task<bool> InitiateDeliveryAssignment(Order order);
    Task AcceptDeliveryOffer(int driverId, int orderId);
    void RejectDeliveryOffer(int driverId, int orderId);
    void CancelOngoingAssignment(int orderId);
}
