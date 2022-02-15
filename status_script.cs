readonly List<IMyTerminalBlock> _blocks = new List<IMyTerminalBlock>();
readonly List<IMyTerminalBlock> _lcd_status_panels = new List<IMyTerminalBlock>();
readonly StringBuilder _textBuffer = new StringBuilder(1024);
readonly IMyTextSurface _display;
readonly IMyTextSurface _lcd;

public Program()
{
    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(_blocks, block => block.IsSameConstructAs(Me));
    foreach(var block in _blocks)
    {
        if (block.CustomName.Contains("Status Panel"))
        {
            _lcd = block as IMyTextSurface;
            _lcd.ContentType = ContentType.TEXT_AND_IMAGE;
            _lcd.TextPadding = 5.0f;
            _lcd.Alignment = TextAlignment.CENTER;
            _lcd.FontSize = 2.0f;
            _lcd.Font = "DEBUG";
            break;
        }
    }

    _display = Me.GetSurface(0);
    _display.ContentType = ContentType.TEXT_AND_IMAGE;
    _display.TextPadding = 5.0f;
    _display.Alignment = TextAlignment.CENTER;
    _display.FontSize = 2.0f;
    _display.Font = "DEBUG";

    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Main(string argument, UpdateType updateSource)
{
    const string br = "\n";

    var shipStatus = BuildShipStatus(_blocks);
    _textBuffer.Clear();
    _textBuffer.Append($"C/E/H/O:{br}---------{br}");

    if (shipStatus.CargoRatio == -1) {
        _textBuffer.Append($"<none>{br}");
    } else {
        _textBuffer.Append($"{(shipStatus.CargoRatio*100):N1} %{br}");
    }

    if (shipStatus.EnergyRatio == -1) {
        _textBuffer.Append($"<none>{br}");
    } else {
        _textBuffer.Append($"{(shipStatus.EnergyRatio*100):N1} %{br}");
    }

    if (shipStatus.HydrogenRatio == -1) {
        _textBuffer.Append($"<none>{br}");
    } else {
        _textBuffer.Append($"{(shipStatus.HydrogenRatio*100):N1} %{br}");
    }

    if (shipStatus.OxygenRatio == -1) {
        _textBuffer.Append($"<none>{br}");
    } else {
        _textBuffer.Append($"{(shipStatus.OxygenRatio*100):N1} %{br}");
    }

    if (_lcd != null)
    {
        _lcd.WriteText(_textBuffer);
    }

    _display.WriteText(_textBuffer);
}

public ShipStatus BuildShipStatus(List<IMyTerminalBlock> blocks)
{
    double usedCargo = 0;
    double maxCargo = 0;
    double storedEnergy = 0;
    double maxEnergy = 0;
    double hydrogenRatio = 0;
    double numHydrogenTanks = 0;
    double oxygenRatio = 0;
    double numOxygenTanks = 0;

    foreach (var block in blocks)
    {

        // If the block has inventory
        if (block.InventoryCount > 0)
        {
            // If the block is in the ejector net or is a tank we skip it
            if (!block.CustomName.Contains("Ejector") && !block.CustomName.Contains("Tank")) {
                for (var i = 0; i < block.InventoryCount; i++)
                {
                    var inventory = block.GetInventory(i);

                    var currentVolume = (double) inventory.CurrentVolume;
                    var maxVolume = (double) inventory.MaxVolume;

                    usedCargo += currentVolume;
                    maxCargo += maxVolume;
                }
            }
        }

        // If the block is a battery
        if (block.CustomName.Contains("Battery"))
        {
            IMyBatteryBlock battery = block as IMyBatteryBlock;
            storedEnergy += battery.CurrentStoredPower;
            maxEnergy += battery.MaxStoredPower;
        }

        // If the block is an H2 tank
        if (block.CustomName.Contains("Hydrogen Tank"))
        {
            Echo($"Found an H2 tank: {block.CustomName}");
            IMyGasTank htank = block as IMyGasTank;
            hydrogenRatio += htank.FilledRatio;
            ++numHydrogenTanks;
        }

        // If the block is an O2 tank
        if (block.CustomName.Contains("Oxygen Tank"))
        {
            Echo($"Found an O2 tank: {block.CustomName}");
            IMyGasTank otank = block as IMyGasTank;
            oxygenRatio += otank.FilledRatio;
            ++numOxygenTanks;
        }
    }

    return new ShipStatus(
        usedCargo,
        maxCargo,
        storedEnergy,
        maxEnergy,
        hydrogenRatio,
        numHydrogenTanks,
        oxygenRatio,
        numOxygenTanks
    );
}


public struct ShipStatus
{
    public ShipStatus(
        double usedCargo,
        double maxCargo,
        double storedEnergy,
        double maxEnergy,
        double hydrogenRatio,
        double numHydrogenTanks,
        double oxygenRatio,
        double numOxygenTanks
    ) {
        CargoRatio = maxCargo > 0 ? usedCargo / maxCargo : -1;
        EnergyRatio = maxEnergy > 0 ? storedEnergy / maxEnergy : -1;
        HydrogenRatio = numHydrogenTanks > 0 ? hydrogenRatio / numHydrogenTanks : -1;
        OxygenRatio = numOxygenTanks > 0 ? oxygenRatio / numOxygenTanks : -1;
    }

    public readonly double CargoRatio;
    public readonly double EnergyRatio;
    public readonly double HydrogenRatio;
    public readonly double OxygenRatio;
}