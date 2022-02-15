readonly List<IMyTerminalBlock> _blocks = new List<IMyTerminalBlock>();
readonly StringBuilder _textBuffer = new StringBuilder(1024);
readonly IMyTextSurface _display;
readonly IMyTextSurface _lcd;

public Program()
{
    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(_blocks, block => block.IsSameConstructAs(Me));
    foreach(var block in _blocks)
    {
        if (block.CustomName.Contains("Inventory Panel"))
        {
            _lcd = block as IMyTextSurface;
            _lcd.ContentType = ContentType.TEXT_AND_IMAGE;
            _lcd.TextPadding = 5.0f;
            _lcd.Alignment = TextAlignment.CENTER;
            _lcd.FontSize = 1.0f;
            _lcd.Font = "DEBUG";
            break;
        }
    }

    _display = Me.GetSurface(0);
    _display.ContentType = ContentType.TEXT_AND_IMAGE;
    _display.TextPadding = 5.0f;
    _display.Alignment = TextAlignment.CENTER;
    _display.FontSize = 1.0f;
    _display.Font = "DEBUG";

    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Main(string argument, UpdateType updateSource)
{
    const string br = "\n";

    IDictionary<string, double> inventory = BuildShipInventory(_blocks);
    _textBuffer.Clear();

    foreach (var itemKey in inventory.Keys)
    {
        if ($"{itemKey}" == "Stone")
        {
            continue;
        }
        _textBuffer.Append($"{br}{itemKey}: {inventory[itemKey]}");
    }

    if (_lcd != null)
    {
        _lcd.WriteText(_textBuffer);
    }
    _display.WriteText(_textBuffer);
}

public IDictionary<string, double> BuildShipInventory(List<IMyTerminalBlock> blocks)
{
    Dictionary<string, double> itemList = new Dictionary<string, double>();
    foreach (var block in blocks)
    {
        for (var i = 0; i < block.InventoryCount; i++)
        {
            var inventory = block.GetInventory(i);

            List<MyInventoryItem> items = new List<MyInventoryItem>();
            inventory.GetItems(items, null);
            
            foreach (var item in items)
            {
                
                AddItemToList(new SimpleItem(item), itemList);
            }
        }
    }

    return itemList;
}

public void AddItemToList(SimpleItem itemToAdd, IDictionary<string, double> itemList)
{
    if (!itemList.ContainsKey(itemToAdd.Name))
    {
        Echo($"Creating item: {itemToAdd.Name}: {itemToAdd.Amount}");
        itemList[itemToAdd.Name] = itemToAdd.Amount;
    }
    else
    {
        Echo($"Updating item: {itemToAdd.Name}: {itemToAdd.Amount}");
        itemList[itemToAdd.Name] = itemList[itemToAdd.Name] + itemToAdd.Amount;
    }
}

public struct SimpleItem
{
    public SimpleItem(MyInventoryItem item)
    {

        Name = item.Type.SubtypeId.ToString();
        Amount = item.Amount.ToIntSafe();
    }

    public readonly string Name;
    public readonly double Amount;
}