readonly List<IMyInventory> _inventories = new List<IMyInventory>();
readonly List<IMyAssembler> _assemblers = new List<IMyAssembler>();
readonly List<IMyInventory> _storageInventories = new List<IMyInventory>();
readonly IMyLightingBlock trashLight;
const string TYPE_COMPONENT = "MyObjectBuilder_Component";
const string TYPE_OXYGENBOTTLE = "MyObjectBuilder_OxygenContainerObject";
const string TYPE_HYDROGENBOTTLE = "MyObjectBuilder_GasContainerObject";
const string TYPE_TOOL = "MyObjectBuilder_PhysicalGunObject";

public Program()
{
    List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(allBlocks, block => block.IsSameConstructAs(Me));
    GridTerminalSystem.GetBlocksOfType<IMyAssembler>(_assemblers, assembler => assembler.IsSameConstructAs(Me));
    
    List<IMyLightingBlock > lights = new List<IMyLightingBlock >();
    GridTerminalSystem.GetBlocksOfType<IMyLightingBlock >(lights, light => light.IsSameConstructAs(Me));
    foreach (var light in lights) {
        if (light.CustomName.Contains("Trash Light")) {
            trashLight = light;
            break;
        }
    }

    if (trashLight == null) {
        Echo("WARN: No Trash Light detected! Exiting...");
        return;
    }

    // Populate our tracking lists
    foreach (var block in allBlocks)
    {
        if ( 
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
                var assemblerTransferSuccessful = false;
                for (int j = _assemblers.Count-1; j >= 0; j--) {
                    assemblerTransferSuccessful = inventory.TransferItemTo(_assemblers[j].OutputInventory, i, stackIfPossible: true, amount: (items[i].Amount.ToIntSafe()/(j+1)));
                }
                if (!assemblerTransferSuccessful) {
                    for (int k = _storageInventories.Count-1; k >= 0; k--) {
                        if (inventory.TransferItemTo(_storageInventories[k], i, stackIfPossible: true)) {
                            break;
                        }
                    }
                }

            } else {
                if (!_storageInventories.Contains(inventory)) {
                    for (int k = 0; k < _storageInventories.Count; k++) {
                        inventory.TransferItemTo(_storageInventories[k], i, stackIfPossible: true);
                        // if (inventory.TransferItemTo(_storageInventories[k], i, stackIfPossible: true)) {
                        //     break;
                        // }
                    }
                }
            }
        }
    }
}