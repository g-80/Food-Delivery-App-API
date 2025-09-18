public class AddDriverLocationHistoryValidator
{
    public ValidationResult Validate(AddDriverLocationHistoryCommand command)
    {
        var errors = new List<string>();

        if (command.Location.Latitude < 49.0 || command.Location.Latitude > 59.0)
            errors.Add("Latitude must be between 49.0 and 59.0 degrees");

        if (command.Location.Longitude < -8.0 || command.Location.Longitude > 2.0)
            errors.Add("Longitude must be between -8.0 and 2.0 degrees");

        if (command.Accuracy <= 0)
            errors.Add("Accuracy must be greater than 0");
        else if (command.Accuracy > 100)
            errors.Add("Accuracy must be less than 100 meters");

        if (command.Speed < 0)
            errors.Add("Speed cannot be negative");
        else if (command.Speed > 55.56) // ~200 km/h in m/s
            errors.Add("Speed must be less than 200 km/h");

        if (command.Heading < 0 || command.Heading >= 360)
            errors.Add("Heading must be between 0 and 359.99 degrees");

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
