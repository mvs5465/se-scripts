readonly List<IMyInventory> _inventories = new List<IMyInventory>();
readonly List<IMyAssembler> _assemblers = new List<IMyAssembler>();
readonly List<IMyInventory> _storageInventories = new List<IMyInventory>();
readonly IMyInteriorLight trashLight;
const string TYPE_COMPONENT = "MyObjectBuilder_Component";
const string TYPE_OXYGENBOTTLE = "MyObjectBuilder_OxygenContainerObject";
const string TYPE_HYDROGENBOTTLE = "MyObjectBuilder_GasContainerObject";
const string TYPE_TOOL = "MyObjectBuilder_PhysicalGunObject";

public Program()
{
    List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(allBlocks, block => block.IsSameConstructAs(Me));
    GridTerminalSystem.GetBlocksOfType<IMyAssembler>(_assemblers, assembler => assembler.IsSameConstructAs(Me));
    
    List<IMyInteriorLight> lights = new List<IMyInteriorLight>();
    GridTerminalSystem.GetBlocksOfType<IMyInteriorLight>(lights, light => light.IsSameConstructAs(Me));
    foreach (var light in lights) {
        if (light.CustomName.Contains("Trash Light")) {
            trashLight = light;
            break;
        }
    }

    // Populate our tracking lists
    foreach (var block in allBlocks)
    {
        if (
            block.CustomName.Contains("Control Seat") || 
            block.CustomName.Contains("Turret") ||
            block.CustomName.Contains("O2/H2 Generator") ||
            block.CustomName.Contains("Reactor")
        ) {
            continue;
        }
        else if (block.CustomName.Contains("Large Cargo Container")) {
            for (var i = 0; i < block.InventoryCount; i++) {
                _storageInventories.Add(block.GetInventory(i));
                _inventories.Add(block.GetInventory(i));
            }
        } 
        else if (block.CustomName.Contains("Assembler")) {
            _inventories.Add(((IMyAssembler) block).InputInventory);
        }
        else {
            for (var i = 0; i < block.InventoryCount; i++) {
                _inventories.Add(block.GetInventory(i));
            }
        }
    }
    Echo($"Found {_inventories.Count()} inventories");
    Echo($"Found {_storageInventories.Count()} storage inventories");
    Echo($"Found {_assemblers.Count()} assemblers");

    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Main(string argument, UpdateType updateSource)
{

    // Only run if the trash light is on
    if (!trashLight.Enabled) {
        foreach (var assembler in _assemblers) {
            assembler.Mode = MyAssemblerMode.Assembly;
        }
        return;
    } else {
        foreach (var assembler in _assemblers) {
            assembler.Mode = MyAssemblerMode.Disassembly;
        }
    }

    foreach (var inventory in _inventories) {

        if (inventory.ItemCount == 0) {  
            continue;
        }

        List<MyInventoryItem> items = new List<MyInventoryItem>();
        inventory.GetItems(items, null);
        
        for (int i = items.Count-1; i >= 0 ; i--) {
            if (
                items[i].Type.TypeId.ToString() == TYPE_COMPONENT ||
                items[i].Type.TypeId.ToString() == TYPE_OXYGENBOTTLE ||
                items[i].Type.TypeId.ToString() == TYPE_TOOL ||
                items[i].Type.TypeId.ToString() == TYPE_HYDROGENBOTTLE
            ) {
                inventory.TransferItemTo(_assemblers[0].OutputInventory, i, stackIfPossible: true, amount: (items[i].Amount.ToIntSafe()/2));
                var assemblersFull = inventory.TransferItemTo(_assemblers[1].OutputInventory, i, stackIfPossible: true);
                if (assemblersFull) {
                    inventory.TransferItemTo(_storageInventories[1], i, stackIfPossible: true);
                }
            } else {
                inventory.TransferItemTo(_storageInventories[0], i, stackIfPossible: true);
            }
        }
    }
}
