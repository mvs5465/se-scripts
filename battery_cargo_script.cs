readonly List<IMyTerminalBlock> _blocks = new List<IMyTerminalBlock>();
readonly StringBuilder _textBuffer = new StringBuilder(1024);
readonly IMyTextSurface _display;

public Program()
{
    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(_blocks, block => block.IsSameConstructAs(Me));

    _display = Me.GetSurface(0);
    _display.ContentType = ContentType.TEXT_AND_IMAGE;
    _display.TextPadding = 0.1f;
    _display.Alignment = TextAlignment.CENTER;
    _display.FontSize = 2;

    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Main(string argument, UpdateType updateSource)
{
    const string br = "\n";

    var batteryCapacity = CalculateBatteryCapacity(_blocks);
    var cargoSpace = CalculateCargoSpace(_blocks);
    _textBuffer.Clear();
    _textBuffer.Append($"{br}Charge: {br}{(batteryCapacity.UsageRatio * 100):N1} %{br}{br}");
    _textBuffer.Append($"Storage: {br}{(cargoSpace.UsageRatio * 100):N1} %");
    _display.WriteText(_textBuffer);
}

public BatteryCapacity CalculateBatteryCapacity(List<IMyTerminalBlock> blocks)
{
    double used = 0;
    double max = 0;
    foreach (var block in blocks)
    {
        if (block.CustomName.Contains("Battery"))
        {
            IMyBatteryBlock battery = block as IMyBatteryBlock;  
            Echo("Found a battery: " + battery.CustomName);
            used += battery.CurrentStoredPower;
            max += battery.MaxStoredPower;
        }
    }

    return new BatteryCapacity(used, max);
}



public CargoSpace CalculateCargoSpace(List<IMyTerminalBlock> blocks)
{
    double used = 0;
    double capacity = 0;

    var containerCount = 0;
    foreach (var block in blocks)
    {

          // If the block is an ejector or tank we skip it
          if (block.CustomName.Contains("Ejector") || block.CustomName.Contains("Tank")) {
              Echo("Skipping: " + block.CustomName);
              continue;
          }

        for (var i = 0; i < block.InventoryCount; i++)
        {
            containerCount++;
            var inventory = block.GetInventory(i);

            var currentVolume = (double) inventory.CurrentVolume;
            var maxVolume = (double) inventory.MaxVolume;

            used += currentVolume;
            capacity += maxVolume;
        }
    }

    return new CargoSpace(used, capacity, containerCount);
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

public struct CargoSpace
{

    public CargoSpace(double usedLiters, double capacityLiters, double containerCount)
    {
        UsedLiters = usedLiters;
        CapacityLiters = capacityLiters;
        UsageRatio = capacityLiters > 0 ? usedLiters / capacityLiters : 1.0;
        ContainerCount = containerCount;
    }

    public readonly double UsedLiters;

    public readonly double CapacityLiters;

    public readonly double UsageRatio;

    public readonly double ContainerCount;
}