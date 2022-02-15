var _blocks = new List<IMyTerminalBlock>();
var _textBuffer = new StringBuilder(1024);
IMyTextSurface _display;

void Program()
{
    // Retrieve all the blocks which can be found in this construct.
    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(_blocks, block => block.IsSameConstructAs(Me));

    // Configure the programmable block display to show simple text output
    _display = Me.GetSurface(0);
    _display.ContentType = ContentType.TEXT_AND_IMAGE;
    _display.TextPadding = 0.1f;
    _display.Alignment = TextAlignment.CENTER;
    _display.FontSize = 2;

    // We don't want to do this job too often, so let's just update once every 100 ticks
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

void Main(string argument, UpdateType updateSource)
{
    const string br = "\n";

    var batteryCapacity = CalculateBatteryCapacity(_blocks);
    _textBuffer.Clear();
    _textBuffer.Apped($"Charge: {br}{(batteryCapacity.UsageRatio * 100)}:N1 %");
}

BatteryCapacity CalculateBatteryCapacity(List<IMyTerminalBlock> blocks)
{
    double used = 0;
    double max = 0;
    foreach (var block in blocks)
    {
        if (block.CustomName.Contains("Battery"))
        {
            Echo("Found a battery: " + block.CustomName);
            used += block.CurrentStoredPower;
            max += block.MaxStoredPower;
        }
    }

    return new BatteryCapacity(used, max);
}

public struct BatteryCapacity
{
    public readonly double CurrentStoredPower;
    public readonly double MaxStoredPower;
    public readonly double UsageRatio;
    public BatteryCapacity(double currentStoredPower, double maxStoredPower)
    {
        CurrentStoredPower = currentStoredPower;
        MaxStoredPower = maxStoredPower;
        UsageRatio = maxStoredPower > 0 ? currentStoredPower / maxStoredPower : 1.0;
    }
}