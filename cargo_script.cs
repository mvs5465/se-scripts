readonly List<IMyTerminalBlock> t_blocks = new List<IMyTerminalBlock>();
readonly StringBuilder t_textBuffer = new StringBuilder(1024);
readonly IMyTextSurface t_display;

public MyProgram()
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

public void Main(string argument, UpdateType updateSource)
{
    // A constant for newline - just for readability
    const string br = "\n";

    // Get the cargo information
    var cargoSpace = CalculateCargoSpace(_blocks);

    // Write the cargo information to the programmable block display
    _textBuffer.Clear();
    _textBuffer.Append(br);
    _textBuffer.Append($"Cargo:{br}{(cargoSpace.UsageRatio * 100):N1} %{br}");
    _textBuffer.Append($"{br}Volume:{br}{cargoSpace.UsedLiters:N2} / {cargoSpace.CapacityLiters:N2} kL{br}");
    _display.WriteText(_textBuffer);
}

/// <summary>
///     Calculates the cargo space of the provided blocks
/// </summary>
/// <param name="blocks">A list of blocks to scan for inventories</param>
/// <returns>A value containing cargo space information</returns>
public CargoSpace CalculateCargoSpace(List<IMyTerminalBlock> blocks)
{
    double used = 0;
    double capacity = 0;
    // We iterate through all the blocks

    var containerCount = 0;
    foreach (var block in blocks)
    {

          // If the block is an ejector or tank we skip it
          if (block.CustomName.Contains("Ejector") || block.CustomName.Contains("Tank")) {
              Echo("Skipping: " + block.CustomName);
              continue;
          }

        // and then we iterate and retrieve all individual inventories for each of those blocks.
        for (var i = 0; i < block.InventoryCount; i++)
        {
            containerCount++;
            // If the block has no inventories, block.InventoryCount will be 0,
            // and we will never enter this code block.

            // Get the inventory manager at the index specified by i
            var inventory = block.GetInventory(i);

            // CurrentVolume and MaxVolume are both of the type MyFixedPoint. We will CAST
            // (convert the data type) to a double type instead, which is more useful to us.
            var currentVolume = (double) inventory.CurrentVolume;
            var maxVolume = (double) inventory.MaxVolume;

            // Now we add up the current- and max volumes to our total
            // tally.
            used += currentVolume;
            capacity += maxVolume;
        }
    }

    // When we get here, we've looped through all the inventories in the given blocks,
    // and we can create the final informational object - and return it to the user.
    return new CargoSpace(used, capacity, containerCount);
}

/// <summary>
///     A simple object to contain the final information about
///     the cargo space
/// </summary>
public struct CargoSpace
{
    /// <summary>
    ///     Create a new CargoSpace object with the given values
    /// </summary>
    /// <param name="usedLiters">The occupied volume in Liters</param>
    /// <param name="capacityLiters">The total available volume in Liters</param>
    public CargoSpace(double usedLiters, double capacityLiters, double containerCount)
    {
        UsedLiters = usedLiters;
        CapacityLiters = capacityLiters;
        UsageRatio = capacityLiters > 0 ? usedLiters / capacityLiters : 1.0;
        ContainerCount = containerCount;
    }

    /// <summary>
    ///     Contains the occupied inventory volume represented in Liters
    /// </summary>
    public readonly double UsedLiters;

    /// <summary>
    ///     Contains the total available space represented in Liters
    /// </summary>
    public readonly double CapacityLiters;

    /// <summary>
    ///     Contains the usage ratio (a value between 0.0 to 1.0)
    /// </summary>
    public readonly double UsageRatio;


    /// <summary>
    ///     Contains the number of containers to be monitored
    /// </summary>
    public readonly double ContainerCount;
}